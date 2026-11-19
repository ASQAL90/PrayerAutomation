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
    private UpdateInfo? _info;
    private bool _busy;
    private CancellationTokenSource? _downloadCts;

    /// <summary>Creates the window, optionally showing a release that was already found.</summary>
    /// <param name="app">Application used for shutdown and installer launch.</param>
    /// <param name="info">Known release, or null to check immediately.</param>
    internal UpdateWindow(App app, UpdateInfo? info)
    {
        InitializeComponent();
        _app = app;
        InstalledVersionText.Text = $"Versi terpasang: {AppBranding.VersionLabel}";
        Closed += (_, _) => _downloadCts?.Cancel();
        if (info is not null) ShowAvailable(info);
        else Loaded += async (_, _) => await CheckAsync();
    }

    /// <summary>Queries GitHub and updates the window with the result.</summary>
    private async Task CheckAsync()
    {
        if (_busy) return;
        _busy = true;
        SetBusy(true);
        _info = null;
        InstallButton.Visibility = Visibility.Collapsed;
        IgnoreButton.Visibility = Visibility.Collapsed;
        DownloadProgress.Visibility = Visibility.Collapsed;
        ProgressText.Text = string.Empty;
        StatusText.Text = "Memeriksa pembaruan ke GitHub…";
        try
        {
            UpdateInfo? info = await _service.CheckAsync(AppBranding.Version, CancellationToken.None);
            if (info is null)
            {
                StatusText.Text = $"Anda sudah memakai versi terbaru ({AppBranding.VersionLabel}). " +
                    "Tidak ada yang perlu diunduh.";
                ProgressText.Text = $"Diperiksa pukul {DateTime.Now:HH:mm:ss}.";
            }
            else
            {
                ShowAvailable(info);
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
    /// <param name="info">Release to offer.</param>
    private void ShowAvailable(UpdateInfo info)
    {
        _info = info;
        long megabytes = info.SizeBytes / (1024 * 1024);
        string size = megabytes > 0 ? $"± {megabytes} MB" : "ukuran tidak diketahui";
        StatusText.Text = $"Versi {info.Tag} tersedia; Anda memakai {AppBranding.VersionLabel}. " +
            $"Unduhan {size}.\r\n\r\n" +
            "Saat dipasang, aplikasi ditutup sebentar agar berkasnya bisa diganti. " +
            "Pengaturan, jadwal, dan data Anda tetap disimpan.";
        ProgressText.Text = string.Empty;
        InstallButton.Visibility = Visibility.Visible;
        IgnoreButton.Visibility = Visibility.Visible;
    }

    /// <summary>Downloads and verifies the release, then hands it to the installer.</summary>
    private async Task InstallAsync()
    {
        if (_info is null || _busy) return;
        _busy = true;
        SetBusy(true);
        InstallButton.IsEnabled = false;
        IgnoreButton.IsEnabled = false;
        DownloadProgress.Visibility = Visibility.Visible;
        DownloadProgress.Value = 0;
        ProgressText.Text = "Mengunduh 0%";
        StatusText.Text = $"Mengunduh pembaruan {_info.Tag}… Jendela ini boleh dibiarkan terbuka.";
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
            string? path = await _service.DownloadAsync(_info, folder, progress, _downloadCts.Token);
            if (_downloadCts.IsCancellationRequested) return;
            if (path is null)
            {
                StatusText.Text = "Unduhan tidak lolos pemeriksaan keutuhan (SHA-256), jadi tidak dipasang. " +
                    "Coba periksa dan unduh lagi.";
                ProgressText.Text = string.Empty;
                return;
            }
            DownloadProgress.Value = 100;
            ProgressText.Text = "Unduhan selesai dan sudah diperiksa.";
            StatusText.Text = "Aplikasi ditutup sebentar, lalu pembaruan dipasang otomatis.";
            await _app.InstallUpdateAsync(path);
            if (IsVisible)
            {
                StatusText.Text = "Pembaruan ditunda. Tidak ada berkas yang diubah.";
                ProgressText.Text = string.Empty;
            }
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
            if (IsVisible)
            {
                _busy = false;
                SetBusy(false);
            }
        }
    }

    /// <summary>Enables or disables the action buttons while work is in progress.</summary>
    /// <param name="busy">True while checking or downloading.</param>
    private void SetBusy(bool busy)
    {
        CheckAgainButton.IsEnabled = !busy;
        InstallButton.IsEnabled = !busy;
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

    /// <summary>Runs a check again on request.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnCheckAgainClick(object sender, RoutedEventArgs e) => await CheckAsync();

    /// <summary>Starts the download on request.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnInstallClick(object sender, RoutedEventArgs e) => await InstallAsync();

    /// <summary>Remembers the release as ignored and closes the window.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void OnIgnoreClick(object sender, RoutedEventArgs e)
    {
        if (_info is not null) _app.IgnoreUpdate(_info.Tag);
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
