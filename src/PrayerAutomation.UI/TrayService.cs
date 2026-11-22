// System tray icon and menu for the application, implemented with the native
// Shell_NotifyIcon API so the build does not need the WinForms runtime.
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Threading;

namespace PrayerAutomation.UI;

/// <summary>Hosts the native tray icon, its WPF context menu, and double-click handling.</summary>
internal sealed class TrayService : IDisposable
{
    private const int WmApp = 0x8000;
    private const int TrayCallbackMessage = WmApp + 1;
    private const int WmNull = 0x0000;
    private const int WmLButtonUp = 0x0202;
    private const int WmLButtonDoubleClick = 0x0203;
    private const int WmRButtonUp = 0x0205;
    private const int WmContextMenu = 0x007B;
    private const int ImageIcon = 1;
    private const uint LrLoadFromFile = 0x10;
    private const int SmCxSmIcon = 49;
    private const int SmCySmIcon = 50;
    private const uint NifMessage = 0x00000001;
    private const uint NifIcon = 0x00000002;
    private const uint NifTip = 0x00000004;
    private const uint NimAdd = 0x00000000;
    private const uint NimModify = 0x00000001;
    private const uint NimDelete = 0x00000002;
    private const uint TrayIconId = 1;

    private readonly App _app;
    private readonly Dispatcher _uiDispatcher;
    private readonly AboutPopupController _about = new();
    private readonly ContextMenu _menu = new();
    private readonly MenuItem _widgetItem = new() { Header = "Tampilkan widget jadwal" };
    private HwndSource? _source;
    private IntPtr _icon;
    private NotifyIconData _data;
    private uint _taskbarCreatedMessage;
    private bool _added;

    /// <summary>Creates the tray icon, loads the application icon, and wires menu actions.</summary>
    /// <param name="app">Application instance used to handle menu actions.</param>
    public TrayService(App app)
    {
        _app = app;
        _uiDispatcher = Application.Current.Dispatcher;

        var parameters = new HwndSourceParameters("AyoJamaahTrayWindow")
        {
            WindowStyle = 0,
            Width = 0,
            Height = 0
        };
        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);

        _icon = LoadAppIcon();
        BuildMenu();
        _taskbarCreatedMessage = RegisterWindowMessageW("TaskbarCreated");
        AddIcon();
    }

    /// <summary>Updates the tray tooltip used by the deferred-update countdown.</summary>
    /// <param name="text">Tooltip text; long text is truncated to the Windows limit.</param>
    internal void SetTooltip(string text)
    {
        Dispatch(() =>
        {
            _data.szTip = TruncateTip(text);
            _data.uFlags = NifTip;
            Shell_NotifyIconW(NimModify, ref _data);
        });
    }

    private void BuildMenu()
    {
        AddMenuItem("Buka aplikasi", () => _app.ShowMainWindow());
        AddMenuItem("Pengaturan", () => _app.OpenSettings());
        AddMenuItem("Buka kalender", () => _app.OpenCalendar());
        _widgetItem.Click += (_, _) => Dispatch(() => _app.ToggleWidget());
        _menu.Items.Add(_widgetItem);
        _menu.Items.Add(new Separator());
        AddMenuItem("Periksa pembaruan", () => _app.CheckForUpdatesNow());
        AddMenuItem("Tentang", _about.Show);
        _menu.Items.Add(new Separator());
        AddMenuItem("Keluar", () => _app.ShutdownApplication());
        _menu.Opened += (_, _) => RefreshWidgetItem();
        _menu.Closed += (_, _) =>
        {
            if (_source is not null) PostMessageW(_source.Handle, WmNull, IntPtr.Zero, IntPtr.Zero);
        };
    }

    private void AddMenuItem(string header, Action action)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) => Dispatch(action);
        _menu.Items.Add(item);
    }

    private void AddIcon()
    {
        if (_source is null) return;
        _data = new NotifyIconData
        {
            cbSize = Marshal.SizeOf<NotifyIconData>(),
            hWnd = _source.Handle,
            uID = TrayIconId,
            uFlags = NifMessage | NifIcon | NifTip,
            uCallbackMessage = TrayCallbackMessage,
            hIcon = _icon,
            szTip = TruncateTip(AppBranding.DisplayName)
        };
        _added = Shell_NotifyIconW(NimAdd, ref _data);
    }

    private void ShowMenu()
    {
        if (_source is null) return;
        SyncTheme();
        // Give the hidden window foreground rights so the menu closes when the
        // user clicks elsewhere, which is the documented tray-menu pattern.
        SetForegroundWindow(_source.Handle);
        _menu.Placement = PlacementMode.MousePoint;
        _menu.IsOpen = true;
    }

    /// <summary>Copies current theme resources onto the menu so popup DynamicResources stay in sync.</summary>
    private void SyncTheme()
    {
        if (Application.Current?.Resources is not { } appRes) return;
        foreach (var key in new[]
        {
            "AppBackgroundBrush", "SurfaceBrush", "SurfaceAltBrush", "PrimaryTextBrush",
            "MutedTextBrush", "AppBorderBrush", "AccentBrush", "AccentTextBrush",
            "ErrorTextBrush", "SuccessTextBrush", "WarningTextBrush", "NoteTextBrush",
            "CardCornerRadius"
        })
        {
            if (appRes.Contains(key)) _menu.Resources[key] = appRes[key];
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (_taskbarCreatedMessage != 0 && unchecked((uint)msg) == _taskbarCreatedMessage)
        {
            // Explorer restarted: the shell forgot every tray icon.
            _added = false;
            AddIcon();
            handled = true;
            return IntPtr.Zero;
        }

        if (msg != TrayCallbackMessage) return IntPtr.Zero;

        int code = (int)(lParam.ToInt64() & 0xFFFF);
        switch (code)
        {
            case WmLButtonDoubleClick:
                Dispatch(() => _app.ShowMainWindow());
                break;
            case WmRButtonUp:
            case WmContextMenu:
                Dispatch(ShowMenu);
                break;
            case WmLButtonUp:
                // Single click intentionally does nothing; parity with the
                // previous NotifyIcon behavior that only handled DoubleClick.
                break;
        }
        handled = true;
        return IntPtr.Zero;
    }

    private static IntPtr LoadAppIcon()
    {
        string iconPath = Path.Combine(AppContext.BaseDirectory, "AyoJamaah.ico");
        if (File.Exists(iconPath))
        {
            int cx = GetSystemMetrics(SmCxSmIcon);
            int cy = GetSystemMetrics(SmCySmIcon);
            IntPtr loaded = LoadImageW(IntPtr.Zero, iconPath, ImageIcon, cx, cy, LrLoadFromFile);
            if (loaded != IntPtr.Zero) return loaded;
        }
        return LoadIconW(IntPtr.Zero, (IntPtr)32512);
    }

    private static string TruncateTip(string text) => text.Length <= 127 ? text : text[..126] + "…";

    /// <summary>Shows the widget entry only when the feature is on, and labels it by current visibility.</summary>
    private void RefreshWidgetItem()
    {
        _widgetItem.Visibility = _app.IsWidgetMenuAvailable ? Visibility.Visible : Visibility.Collapsed;
        _widgetItem.Header = _app.IsScheduleWidgetVisible
            ? "Sembunyikan widget jadwal"
            : "Tampilkan widget jadwal";
    }

    /// <summary>Runs an action on the WPF dispatcher, marshaling to it when called off the UI thread.</summary>
    /// <param name="action">Action to execute on the UI thread.</param>
    private void Dispatch(Action action)
    {
        void Run()
        {
            try { action(); }
            catch (Exception ex) { AppLogger.Write("tray.action", ex); }
        }
        if (_uiDispatcher.CheckAccess()) Run();
        else _uiDispatcher.BeginInvoke(Run);
    }

    /// <summary>Closes the about popup and removes and disposes the tray icon.</summary>
    public void Dispose()
    {
        _about.Close();
        _menu.IsOpen = false;
        if (_added) Shell_NotifyIconW(NimDelete, ref _data);
        _added = false;
        if (_icon != IntPtr.Zero)
        {
            DestroyIcon(_icon);
            _icon = IntPtr.Zero;
        }
        _source?.RemoveHook(WndProc);
        _source?.Dispose();
        _source = null;
    }

    // Minimal V1 NOTIFYICONDATAW - only the fields the app uses (message,
    // icon, tip). Keeping the struct small avoids marshalling the large V2+
    // tail and keeps cbSize/layout predictable across architectures.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NotifyIconData
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Shell_NotifyIconW(uint dwMessage, ref NotifyIconData lpData);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadImageW(IntPtr hInst, string lpszName, int uType,
        int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr LoadIconW(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessageW(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint RegisterWindowMessageW(string lpString);
}
