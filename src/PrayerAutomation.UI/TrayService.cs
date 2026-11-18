// System tray icon and menu for the application.
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WpfApplication = System.Windows.Application;

namespace PrayerAutomation.UI;

/// <summary>Hosts the NotifyIcon tray icon, its context menu, and double-click handling.</summary>
internal sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly App _app;
    private readonly AboutPopupController _about = new();

    /// <summary>Creates the tray icon, loads the application icon, and wires menu actions.</summary>
    /// <param name="app">Application instance used to handle menu actions.</param>
    public TrayService(App app)
    {
        _app = app;
        string iconPath = Path.Combine(AppContext.BaseDirectory, "AyoJamaah.ico");
        Icon icon = File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Application;
        var menu = new ContextMenuStrip();
        menu.Items.Add("Buka aplikasi", null, (_, _) => Dispatch(() => _app.ShowMainWindow()));
        menu.Items.Add("Pengaturan", null, (_, _) => Dispatch(() => _app.OpenSettings()));
        menu.Items.Add("Buka kalender", null, (_, _) => Dispatch(() => _app.OpenCalendar()));
        menu.Items.Add("Tampilkan widget jadwal", null, (_, _) => Dispatch(() => _app.ToggleWidget()));
        menu.Items.Add("Tentang", null, (_, _) => Dispatch(_about.Show));
        menu.Items.Add("Periksa pembaruan", null, (_, _) => Dispatch(() => _app.CheckForUpdatesNow()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Keluar", null, (_, _) => Dispatch(() => _app.ShutdownApplication()));
        _icon = new NotifyIcon { Icon = icon, Text = AppBranding.DisplayName, ContextMenuStrip = menu, Visible = true };
        _icon.DoubleClick += (_, _) => Dispatch(() => _app.ShowMainWindow());
    }

    /// <summary>Shows a tray notification with the application name and message.</summary>
    /// <param name="message">Text to display in the balloon.</param>
    internal void Notify(string message)
    {
        _icon.ShowBalloonTip(5000, AppBranding.DisplayName, message, ToolTipIcon.Info);
    }

    /// <summary>Runs an action on the WPF dispatcher, marshaling to it when called off the UI thread.</summary>
    /// <param name="action">Action to execute on the UI thread.</param>
    private static void Dispatch(Action action)
    {
        if (WpfApplication.Current.Dispatcher.CheckAccess()) action();
        else WpfApplication.Current.Dispatcher.BeginInvoke(action);
    }

    /// <summary>Closes the about popup and removes and disposes the tray icon.</summary>
    public void Dispose()
    {
        _about.Close();
        _icon.Visible = false;
        _icon.Dispose();
    }
}
