using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using Point = System.Windows.Point;
using System.Windows.Automation;

namespace PrayerAutomation.UI;

/// <summary>Result of capturing the taskbar layout: parent window, screen bounds, DPI, chosen slot and a message.</summary>
internal sealed record TaskbarPanelSnapshot(IntPtr Parent, Rect ScreenBounds, int Dpi, Rect? Slot, string Message)
{
    /// <summary>True when the chosen placement is the compact variant.</summary>
    public bool Compact { get; init; }
    /// <summary>True when the taskbar is vertical.</summary>
    public bool Vertical { get; init; }
    // No free-space proof; installed visibility is decided by independent host geometry.
    /// <summary>True when occupancy evidence is unavailable; not an authority to hide an installed panel.</summary>
    public bool OccupancyUnavailable { get; init; }
}
/// <summary>A placement candidate: its bounds, compact/vertical flags.</summary>
internal readonly record struct TaskbarPanelPlacement(Rect Bounds, bool Compact, bool Vertical);

/// <summary>Read-only discovery and placement math for the taskbar countdown panel; never modifies Explorer.</summary>
internal static class TaskbarPanelLayout
{
    /// <summary>Finds a free slot in the taskbar; shorthand for <see cref="FindPlacement"/> bounds.</summary>
    /// <param name="width">Taskbar client width.</param>
    /// <param name="height">Taskbar client height.</param>
    /// <param name="scale">DPI scale factor.</param>
    /// <param name="occupied">Rectangles known to be occupied.</param>
    /// <returns>The slot bounds, or null when no placement fits.</returns>
    internal static Rect? FindSlot(double width, double height, double scale, IReadOnlyList<Rect> occupied)
        => FindPlacement(width, height, scale, occupied)?.Bounds;

    /// <summary>Finds a placement without alignment preference.</summary>
    /// <param name="width">Taskbar client width.</param>
    /// <param name="height">Taskbar client height.</param>
    /// <param name="scale">DPI scale factor.</param>
    /// <param name="occupied">Rectangles known to be occupied.</param>
    /// <param name="previous">The previously installed slot, when known.</param>
    /// <returns>The placement, or null when none fits.</returns>
    internal static TaskbarPanelPlacement? FindPlacement(double width, double height, double scale,
        IReadOnlyList<Rect> occupied, Rect? previous = null)
        => FindPlacementForAlignment(width, height, scale, occupied, previous, false);

    /// <summary>Finds a placement; horizontal taskbars prefer the trailing (or centered) free gap, vertical taskbars may keep the previous slot.</summary>
    /// <param name="width">Taskbar client width.</param>
    /// <param name="height">Taskbar client height.</param>
    /// <param name="scale">DPI scale factor.</param>
    /// <param name="occupied">Rectangles known to be occupied.</param>
    /// <param name="previous">The previously installed slot, when known.</param>
    /// <param name="centered">Whether the taskbar is centered (TaskbarAl=1).</param>
    /// <returns>The placement, or null when none fits.</returns>
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

    /// <summary>Returns true when the TaskbarAl registry value indicates centered alignment.</summary>
    /// <param name="value">Raw registry value.</param>
    /// <returns>True for the centered alignment value.</returns>
    internal static bool IsCenteredAlignmentValue(object? value) => value is int alignment && alignment == 1;

    /// <summary>Reads the TaskbarAl Explorer preference; missing/unsupported values keep Left behavior.</summary>
    /// <returns>True when the taskbar is configured centered.</returns>
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
    /// <summary>Reads the taskbar host and its occupied regions; returns a placement snapshot or an explanatory message.</summary>
    /// <param name="ownChild">The app-owned popup handle to exclude from occupied regions, or zero.</param>
    /// <returns>The capture result.</returns>
    internal static TaskbarPanelSnapshot Capture(IntPtr ownChild)
    {
        IntPtr parent = Native.FindWindow("Shell_TrayWnd", null);
        if (parent == IntPtr.Zero)
            return Missing("Taskbar Windows belum tersedia; widget desktop tetap dapat digunakan.");
        if (!Native.GetWindowRect(parent, out Native.Bounds screen) ||
            !Native.GetClientRect(parent, out Native.Bounds client))
            return Missing("Taskbar Windows belum tersedia; widget desktop tetap dapat digunakan.");
        int dpi = (int)Native.GetDpiForWindow(parent);
        if (dpi <= 0) dpi = 96;
        Rect screenBounds = screen.ToRect();
        if (!Native.IsWindowVisible(parent))
            return new(parent, screenBounds, dpi, null, "Taskbar tersembunyi. Panel ikut disembunyikan.");
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

    /// <summary>Builds a snapshot for the case where the taskbar host is unavailable.</summary>
    /// <param name="message">User-facing explanation.</param>
    /// <returns>The snapshot.</returns>
    private static TaskbarPanelSnapshot Missing(string message) => new(IntPtr.Zero, Rect.Empty, 96, null, message);

    /// <summary>Checks whether the host is on a monitor and its bounds match that monitor.</summary>
    /// <param name="host">The host window.</param>
    /// <param name="screenBounds">Host screen bounds to verify.</param>
    /// <returns>True when the host fully fits on one monitor.</returns>
    internal static bool IsHostOnMonitor(IntPtr host, Rect screenBounds)
    {
        if (screenBounds.IsEmpty || screenBounds.Width <= 0 || screenBounds.Height <= 0 ||
            !Native.GetMonitorInfo(Native.MonitorFromWindow(host, 2), out var monitor)) return false;
        Rect visible = screenBounds;
        visible.Intersect(monitor.Monitor.ToRect());
        return !visible.IsEmpty && visible.Width >= screenBounds.Width - 1 && visible.Height >= screenBounds.Height - 1;
    }

    /// <summary>Whether the taskbar host covers a panel rectangle.</summary>
    internal enum CoverState { Clear, Covered, Unknown }

    // Use the caller's screen-coordinate/DPI context; do not sample outside the panel.
    /// <summary>Determines whether the host is covered by another window; used to decide panel visibility.</summary>
    /// <param name="host">The taskbar host window.</param>
    /// <param name="panel">Panel rectangle in screen coordinates.</param>
    /// <param name="ownWindow">The app-owned popup to skip, or zero.</param>
    /// <returns>Clear, Covered, or Unknown.</returns>
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

    /// <summary>P/Invoke declarations and structures for taskbar discovery.</summary>
    internal static class Native
    {
        /// <summary>Native RECT structure.</summary>
        [StructLayout(LayoutKind.Sequential)] internal struct Bounds
        {
            internal int Left, Top, Right, Bottom;
            /// <summary>Converts the rect to a WPF Rect with non-negative size.</summary>
            /// <returns>The converted rectangle.</returns>
            internal readonly Rect ToRect() => new(Left, Top, Math.Max(0, Right - Left), Math.Max(0, Bottom - Top));
        }
        /// <summary>Simple integer point used by WindowFromPoint.</summary>
        [StructLayout(LayoutKind.Sequential)] internal readonly record struct Point(int X, int Y);
        /// <summary>Native MONITORINFO structure.</summary>
        [StructLayout(LayoutKind.Sequential)] internal struct MonitorInfo
        {
            /// <summary>Structure size (filled before calling), monitor bounds, work-area bounds, flags.</summary>
            internal int Size; internal Bounds Monitor, Work; internal uint Flags;
        }
        /// <summary>Callback for EnumChildWindows.</summary>
        internal delegate bool EnumWindow(IntPtr window, IntPtr parameter);
        /// <summary>Callback for SetWinEventHook.</summary>
        internal delegate void WinEvent(IntPtr hook, uint eventId, IntPtr window, int objectId, int childId, uint thread, uint time);
        /// <summary>Gets the top-most child window of a parent.</summary>
        [DllImport("user32.dll")] internal static extern IntPtr GetTopWindow(IntPtr window);
        /// <summary>Gets a window handle relative to another window.</summary>
        [DllImport("user32.dll")] internal static extern IntPtr GetWindow(IntPtr window, uint command);
        /// <summary>Reads a DWM window attribute.</summary>
        [DllImport("dwmapi.dll")] internal static extern int DwmGetWindowAttribute(IntPtr window, uint attribute, out int value, int size);
        /// <summary>Reads a DWM window attribute.</summary>
        [DllImport("dwmapi.dll")] internal static extern int DwmGetWindowAttribute(IntPtr window, uint attribute, out Bounds value, int size);
        /// <summary>Gets the current thread DPI awareness context.</summary>
        [DllImport("user32.dll")] internal static extern IntPtr GetThreadDpiAwarenessContext();
        /// <summary>Compares two DPI awareness contexts.</summary>
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AreDpiAwarenessContextsEqual(IntPtr first, IntPtr second);
        /// <summary>Gets the ancestor of a window.</summary>
        [DllImport("user32.dll")] internal static extern IntPtr GetAncestor(IntPtr window, uint flags);
        /// <summary>Gets the window handle at a point.</summary>
        [DllImport("user32.dll")] internal static extern IntPtr WindowFromPoint(Point point);
        /// <summary>Gets the monitor handle nearest a window.</summary>
        [DllImport("user32.dll")] internal static extern IntPtr MonitorFromWindow(IntPtr window, uint flags);
        /// <summary>Reads monitor information (see GetMonitorInfo wrapper).</summary>
        [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")] private static extern bool ReadMonitorInfo(IntPtr monitor, ref MonitorInfo info);
        /// <summary>Reads monitor information, filling in the structure size first.</summary>
        /// <param name="monitor">Monitor handle.</param>
        /// <param name="info">The monitor information.</param>
        /// <returns>True on success.</returns>
        internal static bool GetMonitorInfo(IntPtr monitor, out MonitorInfo info)
        { info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() }; return ReadMonitorInfo(monitor, ref info); }
        /// <summary>Gets the thread and process IDs owning a window.</summary>
        [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
        /// <summary>Registers a WinEvent hook.</summary>
        [DllImport("user32.dll")] internal static extern IntPtr SetWinEventHook(uint start, uint end, IntPtr module, WinEvent callback, uint processId, uint threadId, uint flags);
        /// <summary>Removes a WinEvent hook.</summary>
        [DllImport("user32.dll")] internal static extern bool UnhookWinEvent(IntPtr hook);
        /// <summary>Shows/hides a window.</summary>
        [DllImport("user32.dll")] internal static extern bool ShowWindow(IntPtr window, int command);
        /// <summary>Finds a top-level window by class name and title.</summary>
        [DllImport("user32.dll", EntryPoint = "FindWindowW", CharSet = CharSet.Unicode)] internal static extern IntPtr FindWindow(string name, string? title);
        /// <summary>Reads a window class name.</summary>
        [DllImport("user32.dll", EntryPoint = "GetClassNameW", CharSet = CharSet.Unicode)] internal static extern int GetClassName(IntPtr window, StringBuilder text, int capacity);
        /// <summary>Enumerates child windows of a window.</summary>
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool EnumChildWindows(IntPtr window, EnumWindow callback, IntPtr parameter);
        /// <summary>Gets the window bounds in screen coordinates.</summary>
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool GetWindowRect(IntPtr window, out Bounds bounds);
        /// <summary>Gets the client-area bounds of a window.</summary>
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool GetClientRect(IntPtr window, out Bounds bounds);
        /// <summary>Checks whether a window is visible.</summary>
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool IsWindowVisible(IntPtr window);
        /// <summary>Checks whether a handle is a valid window.</summary>
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool IsWindow(IntPtr window);
        /// <summary>Gets the DPI of a window.</summary>
        [DllImport("user32.dll")] internal static extern uint GetDpiForWindow(IntPtr window);
        /// <summary>Gets the parent of a window.</summary>
        [DllImport("user32.dll")] internal static extern IntPtr GetParent(IntPtr window);
        /// <summary>Checks whether a window is a child of another.</summary>
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool IsChild(IntPtr parent, IntPtr window);
        /// <summary>Gets the DPI awareness context of a window.</summary>
        [DllImport("user32.dll")] internal static extern IntPtr GetWindowDpiAwarenessContext(IntPtr window);
        /// <summary>Sets the DPI awareness context for the calling thread.</summary>
        [DllImport("user32.dll", SetLastError = true)] internal static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
        /// <summary>Sets window position, size, and Z-order.</summary>
        [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
        /// <summary>Native APPBARDATA structure used to query taskbar state.</summary>
        [StructLayout(LayoutKind.Sequential)] internal struct AppBarData
        {
            internal int Size;
            internal IntPtr Window;
            internal uint CallbackMessage;
            internal uint Edge;
            internal Bounds Rect;
            internal IntPtr Parameter;
        }
        /// <summary>Sends an appbar message such as ABM_GETSTATE.</summary>
        [DllImport("shell32.dll")] internal static extern IntPtr SHAppBarMessage(uint message, ref AppBarData data);
        /// <summary>Queries whether a fullscreen or presentation application is active.</summary>
        [DllImport("shell32.dll")] internal static extern int SHQueryUserNotificationState(out int state);
        /// <summary>Finds the monitor nearest a rectangle.</summary>
        [DllImport("user32.dll")] internal static extern IntPtr MonitorFromRect(ref Bounds rect, uint flags);
    }
}
