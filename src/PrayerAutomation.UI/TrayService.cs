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
    private readonly ToolStripMenuItem _widgetItem;
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
        _widgetItem = new ToolStripMenuItem("Tampilkan widget jadwal", null,
            (_, _) => Dispatch(() => _app.ToggleWidget()));
        menu.Items.Add(_widgetItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Periksa pembaruan", null, (_, _) => Dispatch(() => _app.CheckForUpdatesNow()));
        menu.Items.Add("Tentang", null, (_, _) => Dispatch(_about.Show));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Keluar", null, (_, _) => Dispatch(() => _app.ShutdownApplication()));
        menu.Opening += (_, _) => RefreshWidgetItem();
        _icon = new NotifyIcon { Icon = icon, Text = AppBranding.DisplayName, ContextMenuStrip = menu, Visible = true };
        _icon.DoubleClick += (_, _) => Dispatch(() => _app.ShowMainWindow());
    }

    /// <summary>Updates the tray tooltip used by the deferred-update countdown.</summary>
    /// <param name="text">Tooltip text; long text is truncated to the Windows limit.</param>
    internal void SetTooltip(string text)
    {
        Dispatch(() => _icon.Text = text.Length <= 63 ? text : text[..60] + "…");
    }

    /// <summary>Shows the widget entry only when the feature is on, and labels it by current visibility.</summary>
    private void RefreshWidgetItem()
    {
        _widgetItem.Visible = _app.IsWidgetMenuAvailable;
        _widgetItem.Text = _app.IsScheduleWidgetVisible
            ? "Sembunyikan widget jadwal"
            : "Tampilkan widget jadwal";
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
