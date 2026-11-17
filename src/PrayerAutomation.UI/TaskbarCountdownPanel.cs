using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;
using FontFamily = System.Windows.Media.FontFamily;
using Size = System.Windows.Size;

namespace PrayerAutomation.UI;

internal sealed class TaskbarCountdownPanel : IDisposable
{
    private const int PopupStyle = unchecked((int)0x90000000);
    private const int NoActivateToolWindow = 0x08000000 | 0x00000080 | 0x00000008;
    private readonly AppSettings _settings;
    private readonly Action _openDetail;
    private readonly Action<string>? _publishStatus;
    private readonly Func<IntPtr, TaskbarPanelSnapshot> _capture;
    private readonly Func<DateTime>? _clock;
    private readonly Func<DateTime, IReadOnlyList<PrayerEntry>>? _schedule;
    internal string RendererMode { get; }
    private IntPtr PanelHandle => _source?.Handle ?? IntPtr.Zero;
    private string _iconKey = "";
    private readonly DispatcherTimer _timer;
    private readonly Dictionary<DateTime, IReadOnlyList<PrayerEntry>> _schedules = new();
    private readonly Border _root;
    private readonly Image _icon;
    private readonly TextBlock _prayer;
    private readonly TextBlock _countdown;
    private readonly Grid _grid;
    private readonly TaskbarPanelLayout.Native.WinEvent _layoutChanged;
    private IntPtr _trackedHost, _eventHook, _foregroundHook;
    private int _layoutVersion, _captureVersion;
    private bool _vertical, _compact, _layoutConfigured;
    private bool _hostGeometryKnown;
    private long _lastLayoutVerifiedTicks;
    private Rect _hostScreenBounds;
    private Size _hostClientSize;
    private int _hostDpi;
    private HwndSource? _source;
    private Task<TaskbarPanelSnapshot>? _captureTask;
    private long _captureStarted;
    private DateTime _scheduleDay;
    private string? _lastStatus;
    private bool _disposed;

    internal TaskbarCountdownPanel(AppSettings settings, Action openDetail, Action<string>? publishStatus,
        Func<IntPtr, TaskbarPanelSnapshot>? capture = null)
        : this(settings, openDetail, publishStatus, capture, null, null, true) { }

    internal TaskbarCountdownPanel(AppSettings settings, Action openDetail, Action<string>? publishStatus,
        Func<IntPtr, TaskbarPanelSnapshot>? capture, Func<DateTime>? clock,
        Func<DateTime, IReadOnlyList<PrayerEntry>>? schedule, bool start)
    {
        _settings = settings;
        _openDetail = openDetail;
        _publishStatus = publishStatus;
        _capture = capture ?? TaskbarPanelLayout.Capture;
        _clock = clock;
        _schedule = schedule;
        RendererMode = "popup";
        _icon = new Image { Width = 24, Height = 24, Margin = new Thickness(0, 0, 7, 0), VerticalAlignment = VerticalAlignment.Center };
        _prayer = new TextBlock { FontSize = 11, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis, TextAlignment = TextAlignment.Center };
        _countdown = new TextBlock { FontFamily = new FontFamily("Consolas"), FontSize = 16, FontWeight = FontWeights.SemiBold, TextAlignment = TextAlignment.Center };
        var grid = _grid = new Grid { VerticalAlignment = VerticalAlignment.Center };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetRowSpan(_icon, 2);
        Grid.SetColumn(_prayer, 1);
        Grid.SetColumn(_countdown, 1);
        Grid.SetRow(_countdown, 1);
        grid.Children.Add(_icon);
        grid.Children.Add(_prayer);
        grid.Children.Add(_countdown);
        _root = new Border { Child = grid, Padding = new Thickness(5, 2, 5, 2), BorderThickness = new Thickness(1),
            SnapsToDevicePixels = true, ToolTip = "Klik untuk membuka jadwal Detail " + AppBranding.DisplayName };
        _root.MouseLeftButtonUp += (_, e) => { if (!_disposed) _openDetail(); e.Handled = true; };
        AutomationProperties.SetAutomationId(_root, "EmbeddedTaskbarMinimalist");
        AutomationProperties.SetAutomationId(_prayer, "EmbeddedTaskbarNextPrayer");
        AutomationProperties.SetAutomationId(_countdown, "EmbeddedTaskbarCountdown");
        _layoutChanged = (_, eventId, window, objectId, childId, _, _) =>
        {
            // WinUI creates descendant HWNDs; their own layout events must not invalidate the taskbar.
            if (window != IntPtr.Zero && PanelHandle != IntPtr.Zero &&
                (window == PanelHandle || TaskbarPanelLayout.Native.IsChild(PanelHandle, window))) return;
            if (eventId == 3) // EVENT_SYSTEM_FOREGROUND: restore the Shell-covered popup without waiting for discovery.
            {
                if (!_disposed && _timer?.Dispatcher.HasShutdownStarted == false && _trackedHost != IntPtr.Zero &&
                    RendererMode == "popup" && TaskbarPanelLayout.Native.IsWindow(window))
                    RestorePopupVisibility();
                return;
            }
            // Focus/selection and accessible button state do not move the taskbar.
            if (eventId is >= 0x8005 and <= 0x8009 ||
                eventId == 0x800A && (window != _trackedHost || objectId != 0 || childId != 0)) return;
            if (_disposed || _trackedHost == IntPtr.Zero || (window != _trackedHost &&
                TaskbarPanelLayout.Native.GetAncestor(window, 2) != _trackedHost)) return;
            // Caret/cursor objects have no occupied taskbar surface.
            if (objectId is -8 or -9) return;
            _layoutVersion++;
            // Shell subtree relayout invalidates discovery, not an independently safe
            // installed placement. Actual host visibility/geometry loss still hides.
            if (PanelHandle != IntPtr.Zero) HideForPendingLayout();
            RequestCapture();
        };
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += (_, _) => Tick();
        RefreshSchedule();
        if (start) { _timer.Start(); Tick(); }
    }

    internal void RefreshSchedule()
    {
        if (_disposed) return;
        _schedules.Clear();
        RefreshCountdown(GetLocationNow());
    }

    private DateTime GetLocationNow() => _clock?.Invoke() ?? TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,
        PrayerScheduleService.ResolveTimeZone(_settings.TimeZoneId)).DateTime;

    private void RefreshCountdown(DateTime now)
    {
        if (now.Date != _scheduleDay) { _schedules.Clear(); _scheduleDay = now.Date; }
        ThemePalette palette = ThemeManager.Current;
        _root.Background = ThemeManager.ToBrush(palette.WidgetBackground);
        _root.BorderBrush = ThemeManager.ToBrush(palette.Border);
        _root.CornerRadius = new CornerRadius(Math.Max(0, palette.CornerRadius));
        _prayer.Foreground = ThemeManager.ToBrush(palette.WidgetText);
        _countdown.Foreground = ThemeManager.ToBrush(palette.WidgetText);
        try
        {
            PrayerCountdown? next = PrayerUiLogic.NextPrayerAfter(now, GetSchedule);
            if (next is not PrayerCountdown countdown) throw new InvalidOperationException("Jadwal berikutnya belum tersedia.");
            _prayer.Text = $"{countdown.PrayerLabel} · {countdown.LocalTime.ToString("HH:mm", CultureInfo.InvariantCulture)}";
            string remaining = PrayerUiLogic.FormatCountdown(countdown.Remaining);
            int separator = remaining.LastIndexOf(':');
            _countdown.Text = _vertical && separator > 0
                ? remaining[..separator] + "\n" + remaining[(separator + 1)..] + "s" : remaining;
            string key = GetSchedule(countdown.LocalTime.Date).FirstOrDefault(e => e.Label == countdown.PrayerLabel)?.Key ?? "Fajr";
            _iconKey = key;
            _icon.Source = PrayerWidgetIcons.Create(key, _prayer.Foreground);
            AutomationProperties.SetName(_icon, PrayerUiLogic.PresentationFor(key, countdown.PrayerLabel).AccessibleDescription);
            AutomationProperties.SetName(_root, $"{_prayer.Text}, {_countdown.Text}; buka jadwal Detail");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or TimeZoneNotFoundException or
            InvalidTimeZoneException or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or TypeInitializationException)
        {
            _prayer.Text = "Jadwal belum tersedia";
            _countdown.Text = "--:--:--";
            _countdown.Foreground = ThemeManager.ToBrush(palette.ErrorText);
            _icon.Source = null;
            _iconKey = "";
        }
    }

    private IReadOnlyList<PrayerEntry> GetSchedule(DateTime date)
    {
        if (!_schedules.TryGetValue(date.Date, out IReadOnlyList<PrayerEntry>? entries))
        {
            entries = _schedule?.Invoke(date.Date) ?? PrayerScheduleService.ForDate(date.Date, _settings);
            _schedules[date.Date] = entries;
        }
        return entries;
    }

    private void Tick()
    {
        if (_disposed) return;
        RefreshCountdown(GetLocationNow());
        if (_captureTask is null)
        {
            RequestCapture();
            return;
        }
        CompleteCapture();
    }

    private void RequestCapture()
    {
        if (_disposed || _captureTask is not null) return;
        if (_hostGeometryKnown) HideForPendingLayout();
        IntPtr own = PanelHandle;
        _captureStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        _captureVersion = _layoutVersion;
        _captureTask = Task.Run(() => _capture(own));
        _ = _captureTask.ContinueWith(completed =>
        {
            Dispatcher dispatcher = _timer.Dispatcher;
            if (_disposed || dispatcher.HasShutdownStarted)
            {
                _ = completed.Exception;
                return;
            }
            try
            {
                dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!_disposed && ReferenceEquals(_captureTask, completed)) CompleteCapture();
                }));
            }
            catch (InvalidOperationException) when (dispatcher.HasShutdownStarted) { _ = completed.Exception; }
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    private void CompleteCapture()
    {
        if (_hostGeometryKnown) HideForPendingLayout();
        if (_captureTask is null) return;
        if (!_captureTask.IsCompleted)
        {
            if (System.Diagnostics.Stopwatch.GetElapsedTime(_captureStarted) > TimeSpan.FromSeconds(3))
            {
                if (!HasUnchangedHostGeometry()) HideForPendingLayout();
                Status("Taskbar sedang tidak merespons. Panel ditahan; widget desktop tetap tersedia.");
            }
            return;
        }
        Task<TaskbarPanelSnapshot> completed = _captureTask;
        _captureTask = null;
        if (_captureVersion != _layoutVersion)
        {
            _ = completed.Exception;
            RequestCapture();
            return;
        }
        if (System.Diagnostics.Stopwatch.GetElapsedTime(_captureStarted) > TimeSpan.FromSeconds(3))
        {
            if (!HasUnchangedHostGeometry()) HideForPendingLayout();
            Status("Posisi taskbar belum diperbarui. Panel menunggu pembacaan baru.");
            return;
        }
        try { ApplySnapshot(completed.GetAwaiter().GetResult()); }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception or COMException or ArgumentException)
        {
            RemoveChild();
            Status("Panel taskbar belum dapat ditampilkan. Taskbar Windows dan konfigurasi tidak diubah.");
        }
    }

    private void ApplySnapshot(TaskbarPanelSnapshot snapshot)
    {
        if (_disposed) return;
        if (snapshot.Slot is not Rect slot || snapshot.Parent == IntPtr.Zero || !TaskbarPanelLayout.Native.IsWindow(snapshot.Parent))
        {
            Status(HideForPendingLayout() ? "Panel tetap aktif. Posisi taskbar menunggu pembacaan baru." : snapshot.Message);
            return;
        }
        // A same-epoch result may still describe old geometry or an auto-hidden host.
        // Reject it before changing the tracked host or moving/showing either renderer.
        if (!TaskbarPanelLayout.Native.IsWindowVisible(snapshot.Parent) ||
            snapshot.Dpi <= 0 || TaskbarPanelLayout.Native.GetDpiForWindow(snapshot.Parent) != snapshot.Dpi ||
            !TaskbarPanelLayout.Native.GetWindowRect(snapshot.Parent, out var screen) || screen.ToRect() != snapshot.ScreenBounds ||
            !TaskbarPanelLayout.IsHostOnMonitor(snapshot.Parent, snapshot.ScreenBounds) ||
            !TaskbarPanelLayout.Native.GetClientRect(snapshot.Parent, out var client) ||
            !double.IsFinite(slot.Left) || !double.IsFinite(slot.Top) || !double.IsFinite(slot.Right) || !double.IsFinite(slot.Bottom) ||
            slot.Left < 0 || slot.Top < 0 ||
            slot.Right > client.Right || slot.Bottom > client.Bottom || slot.Width <= 0 || slot.Height <= 0)
        {
            Status(HideForPendingLayout() ? "Panel tetap aktif. Posisi taskbar menunggu pembacaan baru."
                : "Posisi taskbar berubah atau tersembunyi. Panel menunggu posisi baru.");
            return;
        }
        WatchHost(snapshot.Parent);
        if (TaskbarPanelLayout.Native.GetAncestor(snapshot.Parent, 2) != snapshot.Parent)
        {
            RemoveChild();
            Status("Host taskbar bukan jendela tingkat atas. Panel menunggu host baru.");
            return;
        }
        if (_source is not null && !HasPopupOwnership(snapshot.Parent)) RemoveChild();
        if (_source is null)
        {
            // Owned popup follows a verified free rectangle; Explorer is never modified.
            IntPtr context = TaskbarPanelLayout.Native.GetWindowDpiAwarenessContext(snapshot.Parent);
            IntPtr previous = TaskbarPanelLayout.Native.SetThreadDpiAwarenessContext(context);
            if (previous == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            try
            {
                var parameters = new HwndSourceParameters("PrayerAutomation.TaskbarCountdown")
                {
                    ParentWindow = snapshot.Parent, WindowStyle = PopupStyle, ExtendedWindowStyle = NoActivateToolWindow,
                    PositionX = (int)(snapshot.ScreenBounds.Left + slot.Left), PositionY = (int)(snapshot.ScreenBounds.Top + slot.Top),
                    Width = (int)slot.Width, Height = (int)slot.Height,
                    AdjustSizingForNonClientArea = false, UsesPerPixelTransparency = true
                };
                _source = new HwndSource(parameters);
                _source.AddHook(WindowMessage);
                _source.RootVisual = _root;
                // Exclude only our independent HWND from fading during another window's Peek.
                if (!HasPopupOwnership(snapshot.Parent)) throw new Win32Exception(Marshal.GetLastWin32Error());
                int excludedFromPeek = 1; // Win32 BOOL, not a one-byte managed bool.
                Marshal.ThrowExceptionForHR(PopupNative.DwmSetWindowAttribute(_source.Handle, 12,
                    ref excludedFromPeek, sizeof(int))); // DWMWA_EXCLUDED_FROM_PEEK
                int transitionsForcedDisabled = 1; // Disable transitions only on our independent popup.
                Marshal.ThrowExceptionForHR(PopupNative.DwmSetWindowAttribute(_source.Handle, 3,
                    ref transitionsForcedDisabled, sizeof(int))); // DWMWA_TRANSITIONS_FORCEDISABLED
            }
            finally { TaskbarPanelLayout.Native.SetThreadDpiAwarenessContext(previous); }
        }
        IntPtr own = _source.Handle;
        // Only our HWND moves; neither taskbar nor application buttons are repositioned.
        if (!TaskbarPanelLayout.Native.SetWindowPos(own, new IntPtr(-1),
            (int)(snapshot.ScreenBounds.Left + slot.Left), (int)(snapshot.ScreenBounds.Top + slot.Top),
            (int)slot.Width, (int)slot.Height, 0x0010 | 0x0040 | 0x0200) || !HasPopupOwnership(snapshot.Parent))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        Matrix transform = _source.CompositionTarget.TransformFromDevice;
        var size = new Size(slot.Width * transform.M11, slot.Height * transform.M22);
        _root.Width = size.Width;
        _root.Height = size.Height;
        ConfigureLayout(snapshot.Vertical, snapshot.Compact);
        _root.Measure(size);
        _root.Arrange(new Rect(size));
        _hostScreenBounds = snapshot.ScreenBounds;
        _hostDpi = snapshot.Dpi;
        _lastLayoutVerifiedTicks = System.Diagnostics.Stopwatch.GetTimestamp();
        _hostGeometryKnown = TaskbarPanelLayout.Native.GetClientRect(snapshot.Parent, out var hostClient);
        _hostClientSize = new Size(Math.Max(0, hostClient.Right - hostClient.Left),
            Math.Max(0, hostClient.Bottom - hostClient.Top));
        _root.UpdateLayout();
        Status(snapshot.Message);
    }

    private void ConfigureLayout(bool vertical, bool compact)
    {
        bool changed = _vertical != vertical || _compact != compact;
        if (_layoutConfigured && !changed) return;
        _layoutConfigured = true;
        _vertical = vertical;
        _compact = compact;
        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();
        _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = vertical ? new GridLength(1, GridUnitType.Star) : GridLength.Auto });
        if (!vertical) _grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (int row = 0; row < (vertical ? 3 : 2); row++)
            _grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        Grid.SetColumn(_icon, 0); Grid.SetRow(_icon, 0); Grid.SetRowSpan(_icon, vertical ? 1 : 2);
        Grid.SetColumn(_prayer, vertical ? 0 : 1); Grid.SetRow(_prayer, vertical ? 1 : 0);
        Grid.SetColumn(_countdown, vertical ? 0 : 1); Grid.SetRow(_countdown, vertical ? 2 : 1);
        _prayer.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        _prayer.FontSize = vertical ? 9 : 11;
        _prayer.TextAlignment = _countdown.TextAlignment = TextAlignment.Center;
        _icon.Width = _icon.Height = compact ? 18 : 24;
        _icon.Margin = vertical ? new Thickness(0, 0, 0, 3) : new Thickness(0, 0, 7, 0);
        _countdown.FontSize = vertical ? compact ? 8 : 10 : compact ? 13 : 16;
        _root.Padding = vertical ? new Thickness(3, 2, 3, 2) : new Thickness(5, 2, 5, 2);
        if (changed) RefreshCountdown(GetLocationNow());
    }

    private bool HasPopupOwnership(IntPtr host)
    {
        IntPtr own = _source?.Handle ?? IntPtr.Zero;
        if (host == IntPtr.Zero || own == IntPtr.Zero ||
            !TaskbarPanelLayout.Native.IsWindow(host) || !TaskbarPanelLayout.Native.IsWindow(own) ||
            TaskbarPanelLayout.Native.GetAncestor(host, 2) != host ||
            TaskbarPanelLayout.Native.GetAncestor(own, 2) != own ||
            TaskbarPanelLayout.Native.IsChild(host, own)) return false;
        // Shell flyouts can clear GW_OWNER without removing the live taskbar.
        // Keep our independent popup; a different nonzero owner is still invalid.
        IntPtr owner = TaskbarPanelLayout.Native.GetWindow(own, 4);
        if (owner != IntPtr.Zero && owner != host) return false;
        TaskbarPanelLayout.Native.GetWindowThreadProcessId(own, out uint process);
        if (process != (uint)Environment.ProcessId) return false;
        uint style = unchecked((uint)PopupNative.GetWindowLongPtrW(own, -16).ToInt64());
        uint extended = unchecked((uint)PopupNative.GetWindowLongPtrW(own, -20).ToInt64());
        const uint requiredExtended = 0x08000000 | 0x00000080 | 0x00080000 | 0x00000008;
        return (style & 0x80000000) != 0 && (style & 0x40000000) == 0 &&
            (extended & requiredExtended) == requiredExtended;
    }

    private static class PopupNative
    {
        [DllImport("dwmapi.dll")]
        internal static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

        internal static IntPtr GetWindowLongPtrW(IntPtr window, int index) => IntPtr.Size == 8
            ? GetWindowLongPtr64(window, index) : new IntPtr(GetWindowLong32(window, index));

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr window, int index);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
        private static extern int GetWindowLong32(IntPtr window, int index);
    }

    private bool HasUnchangedHostGeometry()
    {
        if (!_hostGeometryKnown || _lastLayoutVerifiedTicks == 0 || PanelHandle == IntPtr.Zero ||
            !TaskbarPanelLayout.Native.IsWindow(_trackedHost) ||
            !TaskbarPanelLayout.Native.IsWindow(PanelHandle) ||
            !HasPopupOwnership(_trackedHost) ||
            !TaskbarPanelLayout.Native.IsWindowVisible(_trackedHost) ||
            TaskbarPanelLayout.Native.GetDpiForWindow(_trackedHost) != _hostDpi ||
            !TaskbarPanelLayout.Native.GetWindowRect(_trackedHost, out var screen) ||
            new Rect(screen.Left, screen.Top, screen.Right - screen.Left, screen.Bottom - screen.Top) != _hostScreenBounds ||
            !TaskbarPanelLayout.IsHostOnMonitor(_trackedHost, _hostScreenBounds) ||
            !TaskbarPanelLayout.Native.GetClientRect(_trackedHost, out var client) ||
            new Size(client.Right - client.Left, client.Bottom - client.Top) != _hostClientSize ||
            !TaskbarPanelLayout.Native.GetWindowRect(PanelHandle, out var panel)) return false;
        if (panel.Right <= panel.Left || panel.Bottom <= panel.Top ||
            panel.Left < screen.Left || panel.Top < screen.Top || panel.Right > screen.Right ||
            panel.Bottom > screen.Bottom) return false;
        return true;
    }

    private void WatchHost(IntPtr host)
    {
        if (_trackedHost == host) return;
        _hostGeometryKnown = false;
        RemoveChild();
        if (_eventHook != IntPtr.Zero) TaskbarPanelLayout.Native.UnhookWinEvent(_eventHook);
        if (_foregroundHook != IntPtr.Zero) TaskbarPanelLayout.Native.UnhookWinEvent(_foregroundHook);
        _eventHook = _foregroundHook = IntPtr.Zero;
        _trackedHost = host;
        if (host == IntPtr.Zero) return;
        TaskbarPanelLayout.Native.GetWindowThreadProcessId(host, out uint process);
        if (process != 0)
        {
            _eventHook = TaskbarPanelLayout.Native.SetWinEventHook(0x8000, 0x800B, IntPtr.Zero,
                _layoutChanged, process, 0, 2);
            if (RendererMode == "popup")
                _foregroundHook = TaskbarPanelLayout.Native.SetWinEventHook(3, 3, IntPtr.Zero, _layoutChanged, process, 0, 2);
        }
    }

    private IntPtr WindowMessage(IntPtr hwnd, int message, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        if (message == 0x0021) { handled = true; return new IntPtr(3); } // MA_NOACTIVATE
        return IntPtr.Zero;
    }

    private void Status(string text)
    {
        if (_lastStatus == text) return;
        _lastStatus = text;
        _publishStatus?.Invoke(text);
    }

    private bool RestorePopupVisibility()
    {
        if (_disposed || RendererMode != "popup" || !HasUnchangedHostGeometry()) return false;
        // Keep a visible layered surface installed; repeated ShowWindow can churn Shell transitions.
        // A genuinely hidden popup still recovers without requiring a desktop click.
        if (!TaskbarPanelLayout.Native.IsWindowVisible(PanelHandle))
            TaskbarPanelLayout.Native.ShowWindow(PanelHandle, 8); // SW_SHOWNA: no activation.
        return TaskbarPanelLayout.Native.SetWindowPos(PanelHandle, new IntPtr(-1), 0, 0, 0, 0,
            0x0001 | 0x0002 | 0x0010 | 0x0200) && TaskbarPanelLayout.Native.IsWindowVisible(PanelHandle);
    }

    private bool HideForPendingLayout()
    {
        // Discovery/cover rejection does not hide an independently valid placement.
        // Popup retention re-pins without replaying ShowWindow on an already-visible surface.
        if (HasUnchangedHostGeometry())
        {
            if (RendererMode == "popup") return RestorePopupVisibility();
            TaskbarPanelLayout.Native.ShowWindow(PanelHandle, 8);
            return TaskbarPanelLayout.Native.IsWindowVisible(PanelHandle);
        }
        // Both renderers keep their HWND while the actual host is hidden or moving.
        // A lost host/parent still releases the renderer and its resources.
        if (TaskbarPanelLayout.Native.IsWindow(_trackedHost) &&
            _source is not null && HasPopupOwnership(_trackedHost))
        {
            TaskbarPanelLayout.Native.ShowWindow(PanelHandle, 0);
            return false;
        }
        RemoveChild();
        return false;
    }

    private void RemoveChild()
    {
        HwndSource? source = _source;
        _source = null;
        _hostGeometryKnown = false;
        if (source is null || source.IsDisposed) return;
        source.RootVisual = null;
        source.RemoveHook(WindowMessage);
        source.Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        RemoveChild();
        if (_eventHook != IntPtr.Zero) TaskbarPanelLayout.Native.UnhookWinEvent(_eventHook);
        if (_foregroundHook != IntPtr.Zero) TaskbarPanelLayout.Native.UnhookWinEvent(_foregroundHook);
        _eventHook = _foregroundHook = _trackedHost = IntPtr.Zero;
    }
}
