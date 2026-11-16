using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PrayerAutomation.UI;

internal sealed record LockScreenPrayerTrigger(string Key, TimeSpan Time);

internal sealed class LockScreenImageScheduler : IDisposable
{
    private static readonly HashSet<string> EssentialPrayerKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha"
    };

    private static readonly TimeSpan TriggerWindow = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ActiveImagePollInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan RecoveryPollInterval = TimeSpan.FromSeconds(30);
    private readonly Func<bool> _isEnabled;
    private readonly Func<DateTime, IReadOnlyList<LockScreenPrayerTrigger>> _scheduleForDate;
    private readonly Func<TimeZoneInfo> _timeZone;
    private readonly Func<int> _durationMinutes;
    private readonly string _imagePath;
    private readonly LockScreenImageTransaction _transaction;
    private readonly Action<string, Exception>? _log;
    private readonly Action<string, string>? _trace;
    private DateTimeOffset? _lastTickUtc;
    private DateTimeOffset? _lastRecoveryTraceUtc;
    private LockScreenTransactionResult? _lastRecoveryResult;
    private readonly ILockScreenExecutionRequest? _executionRequest;
    private readonly object _executionSync = new();
    private DateTimeOffset? _executionDeadlineUtc;
    private readonly HashSet<string> _processedPrayerTriggers = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<LockScreenPrayerTrigger> _cachedTriggers = Array.Empty<LockScreenPrayerTrigger>();
    private DispatcherTimer? _timer;
    private DateTime? _trackedLocalDate;
    private DateTimeOffset _nextRecoveryCheckAtUtc = DateTimeOffset.MinValue;
    private bool _isProcessing;
    private bool _recoveryPending;
    private bool _restoreOnly;
    private TaskCompletionSource? _processingCompleted;
    private volatile bool _disposed;

    internal LockScreenImageScheduler(
        Func<bool> isEnabled,
        Func<DateTime, IReadOnlyList<LockScreenPrayerTrigger>> scheduleForDate,
        Func<TimeZoneInfo> timeZone,
        string imagePath,
        LockScreenImageTransaction transaction,
        Action<string, Exception>? log = null,
        Func<int>? durationMinutes = null,
        ILockScreenExecutionRequest? executionRequest = null,
        Action<string, string>? trace = null)
    {
        _isEnabled = isEnabled;
        _scheduleForDate = scheduleForDate;
        _timeZone = timeZone;
        _durationMinutes = durationMinutes ?? (() => 60);
        _imagePath = imagePath;
        _transaction = transaction;
        _log = log;
        _executionRequest = executionRequest;
        _trace = trace;
    }

    internal event Action<string>? StatusChanged;

    internal async Task StartAsync(DateTimeOffset nowUtc)
    {
        ThrowIfDisposed();
        ResetScheduleCache();
        LockScreenTransactionResult recovery;
        try
        {
            recovery = await RecoverWithBackoffAsync(_isEnabled(), nowUtc);
        }
        finally
        {
            if (_isEnabled() || _recoveryPending) StartTimer();
            else StopTimer();
        }
        if (!_isEnabled())
        {
            if (recovery == LockScreenTransactionResult.PendingVerification) StartTimer();
            else StopTimer();
            return;
        }

        StartTimer();
        await ProcessTickAsync(nowUtc);
    }

    internal async Task<IReadOnlyList<LockScreenTransactionResult>> RefreshAsync(DateTimeOffset nowUtc)
    {
        ThrowIfDisposed();
        if (_restoreOnly || !_isEnabled())
        {
            ClearExecutionRequest();
            StopTimer();
            try
            {
                await RecoverWithBackoffAsync(false, nowUtc);
            }
            finally
            {
                if (_recoveryPending) StartTimer();
                else StopTimer();
            }
            return Array.Empty<LockScreenTransactionResult>();
        }

        ResetScheduleCache();
        _nextRecoveryCheckAtUtc = DateTimeOffset.MinValue;
        StartTimer();
        return await ProcessTickAsync(nowUtc);
    }

    internal async Task<IReadOnlyList<LockScreenTransactionResult>> ProcessTickAsync(DateTimeOffset nowUtc)
    {
        ThrowIfDisposed();
        if (_isProcessing) return Array.Empty<LockScreenTransactionResult>();
        _isProcessing = true;
        var processingCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _processingCompleted = processingCompleted;
        try
        {
            if (_lastTickUtc.HasValue && nowUtc - _lastTickUtc.Value >= TriggerWindow)
                Trace("tick-gap", nowUtc, $"previousTickUtc={_lastTickUtc:O} gapSeconds={(nowUtc - _lastTickUtc.Value).TotalSeconds:F0}");
            _lastTickUtc = nowUtc;
            bool enabled = !_restoreOnly && _isEnabled();
            if (!enabled) ClearExecutionRequest();
            if (nowUtc >= _nextRecoveryCheckAtUtc)
            {
                await RecoverWithBackoffAsync(enabled, nowUtc);
            }
            if (_recoveryPending || !enabled || !_isEnabled()) return Array.Empty<LockScreenTransactionResult>();

            DateTime localNow = TimeZoneInfo.ConvertTime(nowUtc, _timeZone()).DateTime;
            DateTime localDate = localNow.Date;
            if (_trackedLocalDate != localDate)
            {
                _processedPrayerTriggers.Clear();
                _trackedLocalDate = localDate;
                _cachedTriggers = _scheduleForDate(localDate) ?? Array.Empty<LockScreenPrayerTrigger>();
            }

            var results = new List<LockScreenTransactionResult>();
            foreach (LockScreenPrayerTrigger trigger in _cachedTriggers)
            {
                if (!EssentialPrayerKeys.Contains(trigger.Key)) continue;
                DateTime triggerTime = localDate.Add(trigger.Time);
                if (Math.Abs((localNow - triggerTime).TotalSeconds) > TriggerWindow.TotalSeconds) continue;

                string triggerId = localDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "|" + trigger.Key;
                if (!_processedPrayerTriggers.Add(triggerId)) continue;
                if (_restoreOnly || _disposed || !_isEnabled()) return results;

                try
                {
                    TimeSpan duration = TimeSpan.FromMinutes(Math.Clamp(_durationMinutes(), 1, 120));
                    Trace("trigger", nowUtc, $"triggerUtc={nowUtc.Add(triggerTime - localNow):O} deadlineUtc={nowUtc.Add(duration):O}");
                    RequestExecutionUntil(nowUtc.Add(duration), acquireOnly: true);
                    LockScreenTransactionResult result = await _transaction.BeginOrExtendAsync(
                        _imagePath, nowUtc, CancellationToken.None, duration);
                    SynchronizeExecutionRequest(result);
                    Trace("apply-result", nowUtc, $"result={result} deadlineUtc={ReadDiagnosticDeadline():O} visual=unverified");
                    results.Add(result);
                    if (result is LockScreenTransactionResult.Applied or LockScreenTransactionResult.Extended)
                    {
                        _nextRecoveryCheckAtUtc = nowUtc.Add(ActiveImagePollInterval);
                    }
                    PublishTransactionStatus(result);
                }
                catch (Exception ex)
                {
                    Trace("apply-error", nowUtc, $"category={ErrorCategory(ex)} visual=unverified");
                    ClearExecutionRequest();
                    _processedPrayerTriggers.Remove(triggerId);
                    throw;
                }
            }
            return results;
        }
        finally
        {
            _isProcessing = false;
            processingCompleted.TrySetResult();
        }
    }

    internal async Task<LockScreenTransactionResult> StopAndRestoreAsync()
    {
        ThrowIfDisposed();
        _restoreOnly = true;
        ClearExecutionRequest();
        StopTimer();
        if (_isProcessing && _processingCompleted is not null) await _processingCompleted.Task;
        _recoveryPending = true;
        try
        {
            Trace("stop-restore-start", DateTimeOffset.UtcNow, "visual=unverified");
            LockScreenTransactionResult result = await _transaction.RestoreAsync(CancellationToken.None);
            Trace("stop-restore-result", DateTimeOffset.UtcNow, $"result={result} verification=config-only visual=unverified");
            _recoveryPending = result == LockScreenTransactionResult.PendingVerification;
            PublishRecoveryStatus(result);
            return result;
        }
        catch (Exception ex)
        {
            Trace("stop-restore-error", DateTimeOffset.UtcNow, $"category={ErrorCategory(ex)} visual=unverified");
            throw;
        }
        finally
        {
            _nextRecoveryCheckAtUtc = DateTimeOffset.UtcNow.Add(RecoveryPollInterval);
            if (_recoveryPending && !_disposed) StartTimer();
        }
    }

    public void Dispose()
    {
        lock (_executionSync)
        {
            if (_disposed) return;
            _disposed = true;
            try { _executionRequest?.Dispose(); }
            catch (Exception) { }
            _executionDeadlineUtc = null;
        }
        StopTimer();
    }

    private void StartTimer()
    {
        if (_disposed) return;
        if (_timer is null)
        {
            _timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;
        }
        if (!_timer.IsEnabled) _timer.Start();
    }

    private void StopTimer()
    {
        if (_timer is null) return;
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _timer = null;
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        try
        {
            await ProcessTickAsync(DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            _log?.Invoke("lock-screen.scheduler", ex);
        }
    }

    private async Task<LockScreenTransactionResult> RecoverWithBackoffAsync(bool enabled, DateTimeOffset nowUtc)
    {
        _recoveryPending = true;
        TimeSpan nextPollInterval = RecoveryPollInterval;
        try
        {
            DateTimeOffset? deadline = ReadDiagnosticDeadline();
            if (!enabled || (deadline.HasValue && nowUtc >= deadline.Value))
                Trace("restore-start", nowUtc, $"deadlineUtc={deadline:O} due={deadline.HasValue && nowUtc >= deadline.Value} enabled={enabled} visual=unverified");
            LockScreenTransactionResult result = await _transaction.RecoverAsync(enabled, nowUtc, CancellationToken.None);
            if (result != _lastRecoveryResult || !_lastRecoveryTraceUtc.HasValue || nowUtc - _lastRecoveryTraceUtc.Value >= RecoveryPollInterval)
            {
                Trace("recovery-result", nowUtc, $"result={result} deadlineUtc={deadline:O} verification=config-only visual=unverified");
                _lastRecoveryResult = result;
                _lastRecoveryTraceUtc = nowUtc;
            }
            if (enabled) SynchronizeExecutionRequest(result);
            else ClearExecutionRequest();
            _recoveryPending = result == LockScreenTransactionResult.PendingVerification;
            if (result == LockScreenTransactionResult.Active) nextPollInterval = ActiveImagePollInterval;
            if (!enabled && !_recoveryPending) StopTimer();
            PublishRecoveryStatus(result);
            return result;
        }
        catch (Exception ex)
        {
            Trace("recovery-error", nowUtc, $"category={ErrorCategory(ex)} visual=unverified");
            throw;
        }
        finally
        {
            _nextRecoveryCheckAtUtc = nowUtc.Add(nextPollInterval);
        }
    }

    private void PublishTransactionStatus(LockScreenTransactionResult result)
    {
        if (result is LockScreenTransactionResult.Applied or LockScreenTransactionResult.Extended)
        {
            DateTimeOffset? restoreAt = _transaction.RestoreAtUtc;
            SetStatus(restoreAt.HasValue
                ? $"Gambar lock screen sementara aktif; pemulihan dijadwalkan sekitar {restoreAt.Value.ToLocalTime():HH:mm}. Mode awal akan dipulihkan; fitur gambar tidak mengunci sesi."
                : "Gambar lock screen sementara aktif; waktu pemulihan belum dapat dibaca dari jurnal.");
        }
        else if (result == LockScreenTransactionResult.ExternalChangePreserved)
        {
            SetStatus("Perubahan lock screen dari pengguna dipertahankan; aplikasi tidak menimpanya.");
        }
        else if (result == LockScreenTransactionResult.PendingVerification)
        {
            SetStatus("Status lock screen belum dapat diverifikasi; perubahan baru ditahan demi keamanan.");
        }
    }

    private void PublishRecoveryStatus(LockScreenTransactionResult result)
    {
        if (result == LockScreenTransactionResult.Restored)
            SetStatus("Mode lock screen sebelumnya telah dipulihkan; gambar asal dipulihkan hanya untuk mode Picture.");
        else if (result == LockScreenTransactionResult.ExternalChangePreserved)
            SetStatus("Perubahan lock screen dari pengguna dipertahankan; aplikasi tidak menimpanya.");
        else if (result == LockScreenTransactionResult.PendingVerification)
            SetStatus("Pemulihan lock screen tertunda karena status gambar belum dapat diverifikasi.");
        else if (result == LockScreenTransactionResult.Active)
        {
            SetStatus("Gambar lock screen sementara masih aktif; pemulihan otomatis tetap berjalan.");
        }
    }

    private void ResetScheduleCache()
    {
        _trackedLocalDate = null;
        _cachedTriggers = Array.Empty<LockScreenPrayerTrigger>();
        _processedPrayerTriggers.Clear();
    }

    private void SetStatus(string message) => StatusChanged?.Invoke(message);

    private void SynchronizeExecutionRequest(LockScreenTransactionResult result)
    {
        if (result is LockScreenTransactionResult.Applied or LockScreenTransactionResult.Extended or LockScreenTransactionResult.Active)
        {
            // Optional support must not turn an unreadable diagnostic deadline into a transaction failure.
            try
            {
                if (_transaction.RestoreAtUtc is { } deadline) RequestExecutionUntil(deadline);
            }
            catch (Exception) { }
        }
        else if (result != LockScreenTransactionResult.PendingVerification)
        {
            ClearExecutionRequest();
        }
        // Pending/throwing recovery retains only the already bounded lease, never renews it.
    }

    private void RequestExecutionUntil(DateTimeOffset deadline, bool acquireOnly = false)
    {
        lock (_executionSync)
        {
            if (_disposed || _restoreOnly || _executionRequest is null || _executionDeadlineUtc == deadline) return;
            // Acquire before the first apply; an active scenario extends only from the verified journal.
            if (acquireOnly && _executionDeadlineUtc.HasValue) return;
            if (_executionDeadlineUtc.HasValue && deadline < _executionDeadlineUtc.Value) return;
            _executionDeadlineUtc = deadline;
            try { _executionRequest.RequestUntil(deadline); }
            catch (Exception ex) { Trace("power-error", DateTimeOffset.UtcNow, $"category={ErrorCategory(ex)}"); }
        }
    }

    private void ClearExecutionRequest()
    {
        lock (_executionSync)
        {
            try { _executionRequest?.Clear(); }
            catch (Exception) { }
            _executionDeadlineUtc = null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(LockScreenImageScheduler));
    }

    private DateTimeOffset? ReadDiagnosticDeadline()
    {
        if (_trace is null) return null;
        try { return _transaction.RestoreAtUtc; }
        catch (Exception) { return null; }
    }

    private void Trace(string action, DateTimeOffset nowUtc, string detail)
    {
        try { _trace?.Invoke("lock-screen.timeline." + action, $"utc={nowUtc.ToUniversalTime():O} {detail}"); }
        catch (Exception) { }
    }

    private static string ErrorCategory(Exception exception) => exception switch
    {
        System.IO.IOException => "io",
        UnauthorizedAccessException => "denied",
        OperationCanceledException => "cancelled",
        _ => "operation"
    };
}
