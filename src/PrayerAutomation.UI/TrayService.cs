using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WpfApplication = System.Windows.Application;

namespace PrayerAutomation.UI;

internal sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly App _app;
    private readonly AboutPopupController _about = new();

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
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Keluar", null, (_, _) => Dispatch(() => _app.ShutdownApplication()));
        _icon = new NotifyIcon { Icon = icon, Text = AppBranding.DisplayName, ContextMenuStrip = menu, Visible = true };
        _icon.DoubleClick += (_, _) => Dispatch(() => _app.ShowMainWindow());
    }

    private static void Dispatch(Action action)
    {
        if (WpfApplication.Current.Dispatcher.CheckAccess()) action();
        else WpfApplication.Current.Dispatcher.BeginInvoke(action);
    }

    public void Dispose()
    {
        _about.Close();
        _icon.Visible = false;
        _icon.Dispose();
    }
}
