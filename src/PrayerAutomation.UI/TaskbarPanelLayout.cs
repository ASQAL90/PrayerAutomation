using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Point = System.Windows.Point;
using System.Windows.Automation;

namespace PrayerAutomation.UI;

internal sealed record TaskbarPanelSnapshot(IntPtr Parent, Rect ScreenBounds, int Dpi, Rect? Slot, string Message)
{
    public bool Compact { get; init; }
    public bool Vertical { get; init; }
    // No free-space proof; installed visibility is decided by independent host geometry.
    public bool OccupancyUnavailable { get; init; }
}
internal readonly record struct TaskbarPanelPlacement(Rect Bounds, bool Compact, bool Vertical);

internal static class TaskbarPanelLayout
{
    internal static Rect? FindSlot(double width, double height, double scale, IReadOnlyList<Rect> occupied)
        => FindPlacement(width, height, scale, occupied)?.Bounds;

    internal static TaskbarPanelPlacement? FindPlacement(double width, double height, double scale,
        IReadOnlyList<Rect> occupied, Rect? previous = null)
        => FindPlacementForAlignment(width, height, scale, occupied, previous, false);

    internal static TaskbarPanelPlacement? FindPlacementForAlignment(double width, double height, double scale,
        IReadOnlyList<Rect> occupied, Rect? previous, bool centered)
    {
        if (!double.IsFinite(width) || !double.IsFinite(height) || !double.IsFinite(scale) ||
            width <= 0 || height <= 0 || scale <= 0) return null;
        bool vertical = height > width;
        double length = vertical ? height : width, thickness = vertical ? width : height;
        double gap = Math.Ceiling(4 * scale);
        var intervals = occupied.Where(r => !r.IsEmpty && double.IsFinite(r.Left) && double.IsFinite(r.Top) &&
            double.IsFinite(r.Right) && double.IsFinite(r.Bottom) && r.Width > 0 && r.Height > 0 &&
            r.Bottom > 0 && r.Top < height && r.Right > 0 && r.Left < width)
            .Select(r => (Start: Math.Max(0, (vertical ? r.Top : r.Left) - gap),
                End: Math.Min(length, (vertical ? r.Bottom : r.Right) + gap)))
            .OrderBy(r => r.Start).ToArray();
        foreach (bool compact in new[] { false, true })
        {
            double along = Math.Ceiling((vertical ? compact ? 64 : 88 : compact ? 104 : 144) * scale);
            double across = Math.Ceiling((compact ? 32 : 40) * scale);
            if (thickness < across + 2 || length < along + 2 * gap) continue;
            double start = gap;
            if (vertical && previous is Rect old)
            {
                double oldStart = vertical ? old.Top : old.Left;
                if (oldStart >= gap && oldStart + along <= length - gap &&
                    intervals.All(i => oldStart + along <= i.Start || oldStart >= i.End)) start = oldStart;
                else start = FindGap();
            }
            else start = FindGap();
            if (start + along > length - gap) continue;
            double cross = Math.Floor((thickness - across) / 2);
            Rect bounds = vertical ? new Rect(cross, Math.Ceiling(start), across, along)
                : new Rect(Math.Ceiling(start), cross, along, across);
            return new(bounds, compact, vertical);

            double FindGap()
            {
                double candidate = gap;
                double? trailing = null;
                foreach (var interval in intervals)
                {
                    if (interval.Start - candidate >= along)
                    {
                        if (vertical || centered) return candidate;
                        trailing = Math.Floor(interval.Start - along);
                    }
                    candidate = Math.Max(candidate, interval.End);
                }
                if (length - gap - candidate >= along)
                    return vertical || centered ? candidate : Math.Floor(length - gap - along);
                return trailing ?? length;
            }
        }
        return null;
    }

    internal static bool IsCenteredAlignmentValue(object? value) => value is int alignment && alignment == 1;

    private static bool ReadCenteredAlignment()
    {
        // Explorer preference, not a geometry API. Missing/unsupported values keep Left behavior.
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", writable: false);
            return IsCenteredAlignmentValue(key?.GetValue("TaskbarAl"));
        }
        catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or System.IO.IOException)
        {
            return false;
        }
    }

    // Read-only discovery. Never move, resize, subclass or inject into Explorer.
    internal static TaskbarPanelSnapshot Capture(IntPtr ownChild)
    {
        IntPtr parent = Native.FindWindow("Shell_TrayWnd", null);
        if (parent == IntPtr.Zero || !Native.GetWindowRect(parent, out Native.Bounds screen) ||
            !Native.GetClientRect(parent, out Native.Bounds client) || !Native.IsWindowVisible(parent))
            return Missing("Taskbar Windows belum tersedia; widget desktop tetap dapat digunakan.");
        int dpi = (int)Native.GetDpiForWindow(parent);
        if (dpi <= 0) dpi = 96;
        Rect screenBounds = screen.ToRect();
        if (!IsHostOnMonitor(parent, screenBounds))
            return new(parent, screenBounds, dpi, null, "Taskbar tersembunyi. Panel ikut disembunyikan.");
        var occupied = new List<Rect>();
        Native.EnumWindow callback = (window, _) =>
        {
            if (window == ownChild || Native.IsChild(ownChild, window) || !Native.IsWindowVisible(window)) return true;
            var name = new StringBuilder(256);
            Native.GetClassName(window, name, name.Capacity);
            string className = name.ToString();
            if (className is "Windows.UI.Composition.DesktopWindowContentBridge" or
                "Windows.UI.Input.InputSite.WindowClass" or "Windows.UI.Core.CoreWindow") return true;
            if (Native.GetWindowRect(window, out Native.Bounds bounds))
                Add(bounds.ToRect());
            return true;
        };
        Native.EnumChildWindows(parent, callback, IntPtr.Zero);
        GC.KeepAlive(callback);
        try
        {
            AutomationElement root = AutomationElement.FromHandle(parent);
            var interactive = new OrCondition(new[]
            {
                ControlType.Button, ControlType.MenuItem, ControlType.ListItem, ControlType.TabItem,
                ControlType.Edit, ControlType.ComboBox, ControlType.CheckBox, ControlType.Hyperlink,
                ControlType.Image, ControlType.Text
            }.Select(t => (System.Windows.Automation.Condition)new PropertyCondition(AutomationElement.ControlTypeProperty, t)).ToArray());
            AutomationElementCollection elements = root.FindAll(TreeScope.Descendants, interactive);
            if (elements.Count == 0)
                return Unavailable("Isi taskbar belum terdeteksi. Panel menunggu pembacaan baru.");
            foreach (AutomationElement element in elements)
            {
                var current = element.Current;
                if (current.ProcessId != Environment.ProcessId && !current.IsOffscreen)
                    Add(current.BoundingRectangle);
            }
        }
        catch (Exception ex) when (ex is ElementNotAvailableException or InvalidOperationException or COMException)
        {
            return Unavailable("Isi taskbar belum dapat dibaca. Panel menunggu pembacaan baru.");
        }
        Rect? previous = ownChild != IntPtr.Zero && Native.GetWindowRect(ownChild, out var oldBounds)
            ? new Rect(oldBounds.Left - screenBounds.Left, oldBounds.Top - screenBounds.Top,
                oldBounds.Right - oldBounds.Left, oldBounds.Bottom - oldBounds.Top) : null;
        TaskbarPanelPlacement? placement = FindPlacementForAlignment(client.Right - client.Left, client.Bottom - client.Top,
            dpi / 96d, occupied, previous, ReadCenteredAlignment());
        if (placement is not TaskbarPanelPlacement selected)
            return new(parent, screenBounds, dpi, null, "Taskbar penuh. Panel menunggu posisi baru.");
        Rect slot = selected.Bounds;
        return new(parent, screenBounds, dpi, slot, selected.Compact
            ? "Panel ringkas aktif. Klik untuk membuka Detail."
            : "Panel taskbar aktif. Klik untuk membuka Detail.") { Compact = selected.Compact, Vertical = selected.Vertical };

        TaskbarPanelSnapshot Unavailable(string message)
        {
            // Diagnostic occupancy evidence, not authority to hide an installed panel.
            Rect? old = ownChild != IntPtr.Zero && Native.GetWindowRect(ownChild, out var oldBounds)
                ? new Rect(oldBounds.Left - screenBounds.Left, oldBounds.Top - screenBounds.Top,
                    oldBounds.Right - oldBounds.Left, oldBounds.Bottom - oldBounds.Top) : null;
            bool overlap = old is Rect installed && occupied.Any(r => r.IntersectsWith(installed));
            return new(parent, screenBounds, dpi, null, message) { OccupancyUnavailable = !overlap };
        }

        void Add(Rect rectangle)
        {
            if (rectangle.IsEmpty || rectangle.Width <= 0 || rectangle.Height <= 0 ||
                !double.IsFinite(rectangle.Left) || !double.IsFinite(rectangle.Top)) return;
            rectangle.Offset(-screenBounds.Left, -screenBounds.Top);
            rectangle.Intersect(new Rect(0, 0, client.Right - client.Left, client.Bottom - client.Top));
            if (!rectangle.IsEmpty && rectangle.Width > 0 && rectangle.Height > 0) occupied.Add(rectangle);
        }
    }

    private static TaskbarPanelSnapshot Missing(string message) => new(IntPtr.Zero, Rect.Empty, 96, null, message);

    internal static bool IsHostOnMonitor(IntPtr host, Rect screenBounds)
    {
        if (screenBounds.IsEmpty || screenBounds.Width <= 0 || screenBounds.Height <= 0 ||
            !Native.GetMonitorInfo(Native.MonitorFromWindow(host, 2), out var monitor)) return false;
        Rect visible = screenBounds;
        visible.Intersect(monitor.Monitor.ToRect());
        return !visible.IsEmpty && visible.Width >= screenBounds.Width - 1 && visible.Height >= screenBounds.Height - 1;
    }

    internal enum CoverState { Clear, Covered, Unknown }

    // Use the caller's screen-coordinate/DPI context; do not sample outside the panel.
    internal static CoverState GetPanelCoverState(IntPtr host, Rect panel, IntPtr ownWindow)
    {
        if (host == IntPtr.Zero || host == ownWindow || !Native.IsWindowVisible(host) ||
            panel.IsEmpty || panel.Width <= 0 || panel.Height <= 0 ||
            !double.IsFinite(panel.Left) || !double.IsFinite(panel.Top) ||
            !double.IsFinite(panel.Right) || !double.IsFinite(panel.Bottom)) return CoverState.Unknown;
        var seen = new HashSet<IntPtr>();
        IntPtr window = Native.GetTopWindow(IntPtr.Zero);
        for (int count = 0; window != IntPtr.Zero && count < 1024; count++)
        {
            if (!seen.Add(window) || !Native.IsWindow(window)) return CoverState.Unknown;
            if (window == host) return CoverState.Clear;
            if (window != ownWindow && (ownWindow == IntPtr.Zero || !Native.IsChild(ownWindow, window)) &&
                Native.IsWindowVisible(window))
            {
                if (!Native.GetWindowRect(window, out var bounds)) return CoverState.Unknown;
                bool invalidBounds = bounds.Right <= bounds.Left || bounds.Bottom <= bounds.Top;
                // Edge contact is not overlap. Rectangles remain deliberately conservative.
                if (invalidBounds || (bounds.Left < panel.Right && bounds.Right > panel.Left &&
                    bounds.Top < panel.Bottom && bounds.Bottom > panel.Top))
                {
                    if (Native.DwmGetWindowAttribute(window, 14, out int cloaked, sizeof(int)) != 0)
                        return CoverState.Unknown;
                    if (cloaked == 0)
                    {
                        if (invalidBounds) return CoverState.Unknown;
                        // DWM frame bounds are physical; retain raw cover unless the caller is PerMonitorV2.
                        if (!Native.AreDpiAwarenessContextsEqual(Native.GetThreadDpiAwarenessContext(), new IntPtr(-4)))
                            return CoverState.Covered;
                        // GetWindowRect includes invisible resize borders. Refine only a raw-overlap candidate.
                        if (Native.DwmGetWindowAttribute(window, 9, out Native.Bounds frame, Marshal.SizeOf<Native.Bounds>()) != 0 ||
                            frame.Right <= frame.Left || frame.Bottom <= frame.Top ||
                            frame.Left < bounds.Left || frame.Top < bounds.Top ||
                            frame.Right > bounds.Right || frame.Bottom > bounds.Bottom)
                            return CoverState.Unknown;
                        if (frame.Left < panel.Right && frame.Right > panel.Left &&
                            frame.Top < panel.Bottom && frame.Bottom > panel.Top)
                            return CoverState.Covered;
                    }
                }
            }
            window = Native.GetWindow(window, 2); // GW_HWNDNEXT: continue past our own popup to the host.
        }
        return CoverState.Unknown;
    }

    internal static class Native
    {
        [StructLayout(LayoutKind.Sequential)] internal struct Bounds
        {
            internal int Left, Top, Right, Bottom;
            internal readonly Rect ToRect() => new(Left, Top, Math.Max(0, Right - Left), Math.Max(0, Bottom - Top));
        }
        [StructLayout(LayoutKind.Sequential)] internal readonly record struct Point(int X, int Y);
        [StructLayout(LayoutKind.Sequential)] internal struct MonitorInfo
        {
            internal int Size; internal Bounds Monitor, Work; internal uint Flags;
        }
        internal delegate bool EnumWindow(IntPtr window, IntPtr parameter);
        internal delegate void WinEvent(IntPtr hook, uint eventId, IntPtr window, int objectId, int childId, uint thread, uint time);
        [DllImport("user32.dll")] internal static extern IntPtr GetTopWindow(IntPtr window);
        [DllImport("user32.dll")] internal static extern IntPtr GetWindow(IntPtr window, uint command);
        [DllImport("dwmapi.dll")] internal static extern int DwmGetWindowAttribute(IntPtr window, uint attribute, out int value, int size);
        [DllImport("dwmapi.dll")] internal static extern int DwmGetWindowAttribute(IntPtr window, uint attribute, out Bounds value, int size);
        [DllImport("user32.dll")] internal static extern IntPtr GetThreadDpiAwarenessContext();
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AreDpiAwarenessContextsEqual(IntPtr first, IntPtr second);
        [DllImport("user32.dll")] internal static extern IntPtr GetAncestor(IntPtr window, uint flags);
        [DllImport("user32.dll")] internal static extern IntPtr WindowFromPoint(Point point);
        [DllImport("user32.dll")] internal static extern IntPtr MonitorFromWindow(IntPtr window, uint flags);
        [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")] private static extern bool ReadMonitorInfo(IntPtr monitor, ref MonitorInfo info);
        internal static bool GetMonitorInfo(IntPtr monitor, out MonitorInfo info)
        { info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() }; return ReadMonitorInfo(monitor, ref info); }
        [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
        [DllImport("user32.dll")] internal static extern IntPtr SetWinEventHook(uint start, uint end, IntPtr module, WinEvent callback, uint processId, uint threadId, uint flags);
        [DllImport("user32.dll")] internal static extern bool UnhookWinEvent(IntPtr hook);
        [DllImport("user32.dll")] internal static extern bool ShowWindow(IntPtr window, int command);
        [DllImport("user32.dll", EntryPoint = "FindWindowW", CharSet = CharSet.Unicode)] internal static extern IntPtr FindWindow(string name, string? title);
        [DllImport("user32.dll", EntryPoint = "GetClassNameW", CharSet = CharSet.Unicode)] internal static extern int GetClassName(IntPtr window, StringBuilder text, int capacity);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool EnumChildWindows(IntPtr window, EnumWindow callback, IntPtr parameter);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool GetWindowRect(IntPtr window, out Bounds bounds);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool GetClientRect(IntPtr window, out Bounds bounds);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool IsWindowVisible(IntPtr window);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool IsWindow(IntPtr window);
        [DllImport("user32.dll")] internal static extern uint GetDpiForWindow(IntPtr window);
        [DllImport("user32.dll")] internal static extern IntPtr GetParent(IntPtr window);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool IsChild(IntPtr parent, IntPtr window);
        [DllImport("user32.dll")] internal static extern IntPtr GetWindowDpiAwarenessContext(IntPtr window);
        [DllImport("user32.dll", SetLastError = true)] internal static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
        [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
    }
}
