using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PrayerAutomation.UI;

/// <summary>A prayer time trigger for the lock-screen schedule.</summary>
internal sealed record LockScreenPrayerTrigger(string Key, TimeSpan Time);

/// <summary>Schedules the temporary lock-screen image at prayer times and restores it afterwards, with safety backstops.</summary>
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

    /// <summary>Initializes the scheduler.</summary>
    /// <param name="isEnabled">Returns whether the lock-screen feature is enabled.</param>
    /// <param name="scheduleForDate">Returns prayer triggers for a local date.</param>
    /// <param name="timeZone">Resolves the configured time zone.</param>
    /// <param name="imagePath">Path of the temporary image to apply.</param>
    /// <param name="transaction">Transaction manager for apply/restore operations.</param>
    /// <param name="log">Optional error log sink.</param>
    /// <param name="durationMinutes">Optional override for the temporary image duration.</param>
    /// <param name="executionRequest">Optional execution-required power request helper.</param>
    /// <param name="trace">Optional trace sink for timeline events.</param>
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

    /// <summary>Raised when a user-facing status message becomes available.</summary>
    internal event Action<string>? StatusChanged;

    /// <summary>Starts the scheduler, recovering any pending transaction and processing the current tick.</summary>
    /// <param name="nowUtc">Current UTC time.</param>
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

    /// <summary>Re-evaluates schedule and recovery state after settings change.</summary>
    /// <param name="nowUtc">Current UTC time.</param>
    /// <returns>Results of the tick processing.</returns>
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

    /// <summary>Processes one scheduler tick: recovery check, prayer-trigger detection, apply/extend.</summary>
    /// <param name="nowUtc">Current UTC time.</param>
    /// <returns>Results of any transactions applied or extended on this tick.</returns>
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

    /// <summary>Stops scheduling and restores the previous lock-screen state; safe to call at shutdown.</summary>
    /// <returns>The outcome of the restore transaction.</returns>
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

    /// <summary>Disposes the scheduler and its power request; the timer is stopped.</summary>
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

    /// <summary>Starts the 1-second dispatcher timer if not already running.</summary>
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

    /// <summary>Stops and detaches the dispatcher timer.</summary>
    private void StopTimer()
    {
        if (_timer is null) return;
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _timer = null;
    }

    /// <summary>Timer handler; processes a tick and logs unexpected failures.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
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

    /// <summary>Runs recovery with tracing, status publishing and backoff interval selection.</summary>
    /// <param name="enabled">Whether the feature is enabled.</param>
    /// <param name="nowUtc">Current UTC time.</param>
    /// <returns>The outcome of the recovery transaction.</returns>
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

    /// <summary>Publishes a status message for apply/extend transaction results.</summary>
    /// <param name="result">The transaction result.</param>
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

    /// <summary>Publishes a status message for recovery transaction results.</summary>
    /// <param name="result">The recovery result.</param>
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

    /// <summary>Clears the cached per-day trigger list and processed triggers.</summary>
    private void ResetScheduleCache()
    {
        _trackedLocalDate = null;
        _cachedTriggers = Array.Empty<LockScreenPrayerTrigger>();
        _processedPrayerTriggers.Clear();
    }

    /// <summary>Raises <see cref="StatusChanged"/> with a message.</summary>
    /// <param name="message">Message to publish.</param>
    private void SetStatus(string message) => StatusChanged?.Invoke(message);

    /// <summary>Aligns the execution-required power request with a transaction result.</summary>
    /// <param name="result">The transaction result.</param>
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

    /// <summary>Requests the execution-required power state up to a deadline, never shrinking an existing lease.</summary>
    /// <param name="deadline">UTC deadline for the request.</param>
    /// <param name="acquireOnly">When true, does not extend an already-active lease.</param>
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

    /// <summary>Clears the execution-required power request and its tracked deadline.</summary>
    private void ClearExecutionRequest()
    {
        lock (_executionSync)
        {
            try { _executionRequest?.Clear(); }
            catch (Exception) { }
            _executionDeadlineUtc = null;
        }
    }

    /// <summary>Throws <see cref="ObjectDisposedException"/> when the scheduler is disposed.</summary>
    /// <exception cref="ObjectDisposedException">The scheduler has been disposed.</exception>
    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(LockScreenImageScheduler));
    }

    /// <summary>Reads the transaction restore deadline for diagnostics; null when unavailable.</summary>
    /// <returns>The diagnostic deadline, or null.</returns>
    private DateTimeOffset? ReadDiagnosticDeadline()
    {
        if (_trace is null) return null;
        try { return _transaction.RestoreAtUtc; }
        catch (Exception) { return null; }
    }

    /// <summary>Emits a privacy-safe timeline trace entry; failures are swallowed.</summary>
    /// <param name="action">Trace action name.</param>
    /// <param name="nowUtc">Current UTC time.</param>
    /// <param name="detail">Trace detail payload.</param>
    private void Trace(string action, DateTimeOffset nowUtc, string detail)
    {
        try { _trace?.Invoke("lock-screen.timeline." + action, $"utc={nowUtc.ToUniversalTime():O} {detail}"); }
        catch (Exception) { }
    }

    /// <summary>Maps an exception to a coarse, privacy-safe category for tracing.</summary>
    /// <param name="exception">The exception to classify.</param>
    /// <returns>The category string.</returns>
    private static string ErrorCategory(Exception exception) => exception switch
    {
        System.IO.IOException => "io",
        UnauthorizedAccessException => "denied",
        OperationCanceledException => "cancelled",
        _ => "operation"
    };
}
