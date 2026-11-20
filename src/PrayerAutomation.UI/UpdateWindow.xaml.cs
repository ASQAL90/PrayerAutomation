// Small window that always gives visible feedback when updates are checked or downloaded.
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace PrayerAutomation.UI;

/// <summary>Shows update check results, download progress, and offers installation.</summary>
internal partial class UpdateWindow : Window
{
    private readonly App _app;
    private readonly UpdateService _service = new();
    private UpdateRelease? _release;
    private UpdatePreparation? _prepared;
    private bool _busy;
    private bool _waitingForRecovery;
    private CancellationTokenSource? _downloadCts;

    /// <summary>Creates the window, optionally showing a release that was already found.</summary>
    /// <param name="app">Application used for shutdown and installer launch.</param>
    /// <param name="info">Known release, or null to check immediately.</param>
    internal UpdateWindow(App app, UpdateRelease? info)
    {
        InitializeComponent();
        _app = app;
        InstalledVersionText.Text = $"Versi terpasang: {AppBranding.VersionLabel}";
        Closed += (_, _) =>
        {
            _downloadCts?.Cancel();
            if (!_waitingForRecovery) return;
            _waitingForRecovery = false;
            _ = _app.ResumeLockScreenAfterUpdateCancelAsync();
        };
        if (info is not null) ShowAvailable(info);
        else Loaded += async (_, _) => await CheckAsync();
    }

    /// <summary>Queries GitHub and updates the window with the result.</summary>
    private async Task CheckAsync()
    {
        if (_busy) return;
        _busy = true;
        SetBusy(true);
        _release = null;
        _prepared = null;
        _waitingForRecovery = false;
        RecoveryPanel.Visibility = Visibility.Collapsed;
        InstallButton.Visibility = Visibility.Collapsed;
        LaterButton.Visibility = Visibility.Collapsed;
        IgnoreButton.Visibility = Visibility.Collapsed;
        DownloadProgress.Visibility = Visibility.Collapsed;
        ProgressText.Text = string.Empty;
        StatusText.Text = "Memeriksa pembaruan ke GitHub…";
        try
        {
            UpdateRelease? release = await _service.CheckAsync(AppBranding.Version, IsInstalledCopy(),
                CancellationToken.None).ConfigureAwait(true);
            if (release is null)
            {
                StatusText.Text = $"Anda sudah memakai versi terbaru ({AppBranding.VersionLabel}). " +
                    "Tidak ada yang perlu diunduh.";
                ProgressText.Text = $"Diperiksa pukul {DateTime.Now:HH:mm:ss}.";
            }
            else if (string.Equals(_app.Settings.IgnoredUpdateVersion, release.Tag,
                         StringComparison.OrdinalIgnoreCase))
            {
                StatusText.Text = $"Versi {release.Tag} tersedia, tetapi sudah Anda pilih untuk diabaikan. " +
                    "Gunakan Periksa lagi bila ingin melihatnya kembali.";
                ProgressText.Text = $"Diperiksa pukul {DateTime.Now:HH:mm:ss}.";
            }
            else
            {
                ShowAvailable(release);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Write("update.check", ex);
            StatusText.Text = DescribeFailure(ex);
            ProgressText.Text = $"Diperiksa pukul {DateTime.Now:HH:mm:ss}.";
        }
        finally
        {
            if (IsLoaded)
            {
                _busy = false;
                SetBusy(false);
            }
        }
    }

    /// <summary>Fills the window with an available release.</summary>
    /// <param name="release">Release to offer.</param>
    private void ShowAvailable(UpdateRelease release)
    {
        _release = release;
        _prepared = null;
        _waitingForRecovery = false;
        RecoveryPanel.Visibility = Visibility.Collapsed;
        bool delta = release.Manifest is not null && release.Signature is not null;
        StatusText.Text = $"Versi {release.Tag} tersedia; Anda memakai {AppBranding.VersionLabel}.\r\n\r\n" +
            (delta
                ? "Aplikasi akan memeriksa berkas lokal Anda, lalu mengunduh hanya bagian yang berubah. " +
                  "Jika pembaruan kecil tidak tersedia, aplikasi mengunduh versi lengkap."
                : "Rilis ini belum menyediakan pembaruan kecil, jadi aplikasi akan mengunduh versi lengkap.") +
            "\r\n\r\nPengaturan, jadwal, dan data Anda tetap disimpan." +
            "\r\n\r\nTekan Nanti saja bila ingin diingatkan lagi setelah pemulihan lock screen selesai.";
        ProgressText.Text = string.Empty;
        InstallButton.Visibility = Visibility.Visible;
        LaterButton.Visibility = Visibility.Visible;
        IgnoreButton.Visibility = Visibility.Visible;
    }

    /// <summary>Starts installing a release that the reminder popup already confirmed.</summary>
    internal void BeginInstall()
    {
        if (!IsLoaded)
        {
            Loaded += OnBeginInstallLoaded;
            return;
        }
        _ = InstallAsync();
    }

    /// <summary>Runs the confirmed installation after the window finishes loading.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnBeginInstallLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnBeginInstallLoaded;
        await InstallAsync();
    }

    /// <summary>Downloads, verifies, and installs the smallest available payload.</summary>
    private async Task InstallAsync()
    {
        if (_release is null || _busy) return;
        _busy = true;
        SetBusy(true);
        InstallButton.IsEnabled = false;
        LaterButton.IsEnabled = false;
        IgnoreButton.IsEnabled = false;
        DownloadProgress.Visibility = Visibility.Visible;
        DownloadProgress.Value = 0;
        ProgressText.Text = "Menyiapkan 0%";
        StatusText.Text = "Memeriksa berkas yang perlu diganti…";
        try
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PrayerAutomation", "Update");
            var progress = new Progress<double>(value =>
            {
                DownloadProgress.Value = value * 100;
                ProgressText.Text = $"Mengunduh {value * 100:0}%";
            });
            _downloadCts?.Dispose();
            _downloadCts = new CancellationTokenSource();

            UpdatePreparation? prepared = await _service.PrepareAsync(_release, AppBranding.Version,
                ApplicationRoot(), folder, progress, _downloadCts.Token).ConfigureAwait(true);
            if (_downloadCts.IsCancellationRequested) return;
            if (prepared is null)
            {
                StatusText.Text = "Pembaruan tidak lolos pemeriksaan keutuhan, jadi tidak dipasang. " +
                    "Coba periksa dan unduh lagi.";
                ProgressText.Text = string.Empty;
                return;
            }

            _prepared = prepared;
            DownloadProgress.Value = 100;
            ProgressText.Text = prepared.Summary;
            await LaunchPreparedAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException) when (_downloadCts?.IsCancellationRequested == true)
        {
            if (IsVisible)
            {
                StatusText.Text = "Unduhan dibatalkan. Jendela ditutup; pembaruan belum dipasang.";
                ProgressText.Text = string.Empty;
            }
        }
        catch (Exception ex)
        {
            AppLogger.Write("update.install", ex);
            StatusText.Text = DescribeDownloadFailure(ex);
            ProgressText.Text = string.Empty;
        }
        finally
        {
            if (!_waitingForRecovery && IsVisible)
            {
                _busy = false;
                SetBusy(false);
            }
        }
    }

    /// <summary>Runs the already-downloaded payload through the lock-screen readiness gate.</summary>
    private async Task LaunchPreparedAsync()
    {
        if (_prepared is null || _downloadCts is null) return;
        _busy = true;
        SetBusy(true);
        InstallButton.IsEnabled = false;
        LaterButton.IsEnabled = false;
        IgnoreButton.IsEnabled = false;
        DownloadProgress.Visibility = Visibility.Collapsed;
        RetryRecoveryButton.IsEnabled = false;
        CancelRecoveryButton.IsEnabled = true;
        StatusText.Text = "Memeriksa kesiapan lock screen sebelum memasang pembaruan…";
        try
        {
            UpdateLaunchResult result = _prepared.Mode == UpdateInstallMode.Installer
                ? await _app.InstallUpdateAsync(_prepared.LocalPath, ReportRecoveryProgress, _downloadCts.Token)
                : await _app.InstallDeltaAsync(_prepared.Plan!, _prepared.LocalPath, ReportRecoveryProgress, _downloadCts.Token);
            if (result == UpdateLaunchResult.LockScreenPending)
            {
                ShowRecoveryPrompt();
                return;
            }
            _waitingForRecovery = false;
            RecoveryPanel.Visibility = Visibility.Collapsed;
            if (IsVisible)
            {
                StatusText.Text = result == UpdateLaunchResult.Started
                    ? "Pembaruan dimulai. Aplikasi akan ditutup sebentar."
                    : "Pembaruan dibatalkan. Tidak ada berkas yang diubah.";
                ProgressText.Text = string.Empty;
            }
        }
        catch (OperationCanceledException) when (_downloadCts.IsCancellationRequested)
        {
            _waitingForRecovery = false;
            if (IsVisible)
            {
                StatusText.Text = "Pembaruan dibatalkan. Tidak ada berkas yang diubah.";
                ProgressText.Text = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _waitingForRecovery = false;
            AppLogger.Write("update.install", ex);
            if (IsVisible)
            {
                StatusText.Text = DescribeDownloadFailure(ex);
                ProgressText.Text = string.Empty;
            }
        }
        finally
        {
            if (!_waitingForRecovery && IsVisible)
            {
                _busy = false;
                SetBusy(false);
            }
        }
    }

    /// <summary>Shows a bounded restore countdown; the journal deadline is informational only.</summary>
    /// <param name="progress">Latest restore wait progress.</param>
    private void ReportRecoveryProgress(LockScreenRecoveryProgress progress)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => ReportRecoveryProgress(progress));
            return;
        }
        if (!IsVisible) return;
        RecoveryPanel.Visibility = Visibility.Visible;
        string schedule = progress.RestoreAtUtc is { } deadline
            ? $" Jadwal otomatis: {deadline.ToLocalTime():HH:mm}."
            : string.Empty;
        RecoveryText.Text = $"Sedang memulihkan gambar lock screen… sisa {progress.RemainingSeconds} detik.{schedule}";
        ProgressText.Text = string.Empty;
    }

    /// <summary>Offers another restore attempt after the wait budget expired.</summary>
    private void ShowRecoveryPrompt()
    {
        _waitingForRecovery = true;
        if (!IsVisible) return;
        RecoveryPanel.Visibility = Visibility.Visible;
        RecoveryText.Text = "Pemulihan lock screen belum selesai dalam 60 detik. Pembaruan dibatalkan dan catatan pemulihan " +
            "dibiarkan utuh. Tekan Pulihkan sekarang untuk mencoba lagi, atau Batal.";
        RetryRecoveryButton.IsEnabled = true;
        CancelRecoveryButton.IsEnabled = true;
    }

    /// <summary>Enables or disables the action buttons while work is in progress.</summary>
    /// <param name="busy">True while checking or downloading.</param>
    private void SetBusy(bool busy)
    {
        CheckAgainButton.IsEnabled = !busy;
        InstallButton.IsEnabled = !busy;
        LaterButton.IsEnabled = !busy;
        IgnoreButton.IsEnabled = !busy;
    }

    /// <summary>Maps check failures to a short, non-technical sentence.</summary>
    /// <param name="exception">Failure raised by the check.</param>
    /// <returns>Message shown in the window.</returns>
    private static string DescribeFailure(Exception exception) => exception switch
    {
        HttpRequestException => "Tidak dapat menghubungi GitHub. Periksa koneksi internet, lalu tekan Periksa lagi.",
        TaskCanceledException => "Pemeriksaan melebihi batas waktu. Periksa koneksi internet, lalu coba lagi.",
        _ => "Pemeriksaan pembaruan gagal. Coba lagi nanti."
    };

    /// <summary>Maps download failures to a short message.</summary>
    /// <param name="exception">Failure raised by the download.</param>
    /// <returns>Message shown in the window.</returns>
    private static string DescribeDownloadFailure(Exception exception) => exception switch
    {
        HttpRequestException => "Unduhan terputus karena koneksi ke GitHub bermasalah. Coba lagi.",
        TaskCanceledException => "Unduhan melebihi batas waktu. Coba lagi dengan koneksi yang lebih stabil.",
        UnauthorizedAccessException => "Berkas pembaruan tidak bisa ditulis ke folder aplikasi. Periksa izin folder.",
        IOException => "Penyimpanan unduhan gagal. Periksa ruang disk, lalu coba lagi.",
        InvalidOperationException => exception.Message,
        _ => "Unduhan gagal. Coba lagi nanti."
    };

    private static string ApplicationRoot()
    {
        string executable = Environment.ProcessPath ?? string.Empty;
        return string.IsNullOrWhiteSpace(executable) ? AppContext.BaseDirectory
            : Path.GetDirectoryName(executable) ?? AppContext.BaseDirectory;
    }

    private static bool IsInstalledCopy() => StartupRegistration.IsInstalledPath(ApplicationRoot());

    /// <summary>Runs a check again on request.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnCheckAgainClick(object sender, RoutedEventArgs e) => await CheckAsync();

    /// <summary>Starts the download on request.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnInstallClick(object sender, RoutedEventArgs e) => await InstallAsync();

    /// <summary>Retries the restore with the same downloaded payload.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnRetryRecoveryClick(object sender, RoutedEventArgs e)
    {
        if (_prepared is null) return;
        _waitingForRecovery = false;
        await LaunchPreparedAsync();
    }

    /// <summary>Closes the window without changing installed files or the restore journal.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnCancelRecoveryClick(object sender, RoutedEventArgs e)
    {
        bool needsResume = _waitingForRecovery;
        _waitingForRecovery = false;
        _prepared = null;
        RecoveryPanel.Visibility = Visibility.Collapsed;
        _downloadCts?.Cancel();
        if (needsResume) await _app.ResumeLockScreenAfterUpdateCancelAsync();
        Close();
    }

    /// <summary>Closes the window and starts the tray countdown until lock-screen recovery finishes.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void OnLaterClick(object sender, RoutedEventArgs e)
    {
        if (_release is null) return;
        _app.SnoozeUpdate(_release);
        Close();
    }

    /// <summary>Remembers the release as ignored and closes the window.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void OnIgnoreClick(object sender, RoutedEventArgs e)
    {
        if (_release is not null) _app.IgnoreUpdate(_release.Tag);
        Close();
    }

    /// <summary>Closes the window.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    /// <summary>Allows dragging the borderless window by its header.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse button arguments.</param>
    private void OnHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    /// <summary>Closes the window with Escape.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Key arguments.</param>
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }
}
