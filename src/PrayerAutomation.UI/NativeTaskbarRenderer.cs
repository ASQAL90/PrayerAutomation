using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace PrayerAutomation.UI;

// Clean-room implementation: owned native child and standard Win32/GDI only.
// Does not subclass, reparent, reserve space in, or reposition Explorer windows.
internal sealed class NativeTaskbarRenderer : IDisposable
{
    private readonly Action _openDetail;
    private readonly Native.WindowProc _windowProc;
    private readonly string _className = "PrayerAutomation.NativeTaskbarCountdown." + Guid.NewGuid().ToString("N");
    private readonly IntPtr _instance = Native.GetModuleHandle(null);
    private bool _registered, _disposed, _vertical, _compact;
    private string _prayer = "", _countdown = "--:--:--", _key = "";
    private string _background = "#FFFFFF", _foreground = "#000000", _border = "#000000";
    private int _dpi = 96;
    internal IntPtr Handle { get; private set; }
    internal string? LastRenderError { get; private set; }

    internal NativeTaskbarRenderer(IntPtr parent, Rect slot, Action openDetail)
    {
        _openDetail = openDetail;
        _windowProc = WindowMessage;
        if (parent == IntPtr.Zero || !TaskbarPanelLayout.Native.IsWindow(parent)) throw new ArgumentException("Host is unavailable.");
        var windowClass = new Native.WindowClass
        {
            Size = (uint)Marshal.SizeOf<Native.WindowClass>(),
            WindowProc = Marshal.GetFunctionPointerForDelegate(_windowProc),
            Instance = _instance, ClassName = _className,
            Cursor = Native.LoadCursor(IntPtr.Zero, new IntPtr(32512))
        };
        if (Native.RegisterClassEx(ref windowClass) == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        _registered = true;
        IntPtr previous = TaskbarPanelLayout.Native.SetThreadDpiAwarenessContext(
            TaskbarPanelLayout.Native.GetWindowDpiAwarenessContext(parent));
        try
        {
            if (previous == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            // Born as WS_CHILD | WS_CLIPSIBLINGS, initially hidden. Never SetParent.
            Handle = Native.CreateWindowEx(0x08000080, _className, "Ayo Jama'ah! — Taskbar Window",
                0x40000000 | 0x04000000, (int)slot.Left, (int)slot.Top, (int)slot.Width, (int)slot.Height,
                parent, IntPtr.Zero, _instance, IntPtr.Zero);
            if (Handle == IntPtr.Zero || TaskbarPanelLayout.Native.GetParent(Handle) != parent)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        catch { Dispose(); throw; }
        finally { if (previous != IntPtr.Zero) TaskbarPanelLayout.Native.SetThreadDpiAwarenessContext(previous); }
    }

    internal void Update(string prayer, string countdown, string key, ThemePalette palette,
        bool vertical, bool compact, int dpi, bool unavailable)
    {
        if (_disposed || Handle == IntPtr.Zero) return;
        _prayer = prayer; _countdown = countdown; _key = key;
        _background = palette.WidgetBackground; _foreground = unavailable ? palette.ErrorText : palette.WidgetText;
        _border = palette.Border; _vertical = vertical; _compact = compact; _dpi = Math.Max(96, dpi);
        string description = key.Length == 0 ? "Jadwal belum tersedia" : PrayerUiLogic.PresentationFor(key).AccessibleDescription;
        Native.SetWindowText(Handle, $"Ayo Jama'ah! — {prayer}, {countdown}; {description}; klik untuk membuka Detail");
        Native.InvalidateRect(Handle, IntPtr.Zero, false);
    }

    private IntPtr WindowMessage(IntPtr window, uint message, IntPtr wparam, IntPtr lparam)
    {
        try
        {
            switch (message)
            {
                case 0x000F: // WM_PAINT: render only our client area.
                    IntPtr dc = Native.BeginPaint(window, out Native.Paint paint);
                    try { if (dc != IntPtr.Zero) Paint(dc, window); }
                    finally { Native.EndPaint(window, ref paint); }
                    return IntPtr.Zero;
                case 0x0318: // WM_PRINTCLIENT: same buffered path, supplied owned DC.
                    if (wparam != IntPtr.Zero) Paint(wparam, window);
                    return IntPtr.Zero;
                case 0x0014: return new IntPtr(1); // Full opaque buffered background, no erase flicker.
                case 0x0021: return new IntPtr(3); // MA_NOACTIVATE; no focus or foreground calls.
                case 0x0202:
                    if (!_disposed && Handle != IntPtr.Zero) _openDetail();
                    return IntPtr.Zero;
                case 0x0082: Handle = IntPtr.Zero; break; // Parent destruction releases only owned HWND.
            }
        }
        catch (Exception ex)
        {
            // Exceptions must never unwind through an unmanaged window procedure.
            LastRenderError = ex.GetType().Name;
            return IntPtr.Zero;
        }
        return Native.DefWindowProc(window, message, wparam, lparam);
    }

    private void Paint(IntPtr destination, IntPtr window)
    {
        if (!TaskbarPanelLayout.Native.GetClientRect(window, out var bounds)) return;
        int width = bounds.Right, height = bounds.Bottom;
        if (width <= 0 || height <= 0) return;
        IntPtr memory = Native.CreateCompatibleDC(destination);
        if (memory == IntPtr.Zero) throw new Win32Exception();
        IntPtr bitmap = Native.CreateCompatibleBitmap(destination, width, height);
        IntPtr old = IntPtr.Zero;
        try
        {
            if (bitmap == IntPtr.Zero) throw new Win32Exception();
            old = Native.SelectObject(memory, bitmap);
            if (old == IntPtr.Zero || old == new IntPtr(-1)) throw new Win32Exception();
            var area = new Native.Bounds { Right = width, Bottom = height };
            Fill(memory, area, ColorRef(_background));
            IntPtr border = Native.CreateSolidBrush(ColorRef(_border));
            if (border == IntPtr.Zero) throw new InvalidOperationException("CreateSolidBrush failed for border.");
            try
            {
                if (Native.FrameRect(memory, ref area, border) == 0) throw new InvalidOperationException("FrameRect failed.");
            }
            finally { Native.DeleteObject(border); }
            double scale = _dpi / 96d;
            int Px(double value) => (int)Math.Ceiling(value * scale);
            int padding = Px(_vertical ? 3 : 5), icon = Px(_compact ? 18 : 24);
            int iconX = _vertical ? (width - icon) / 2 : padding;
            int iconY = _vertical ? Px(3) : (height - icon) / 2;
            DrawIcon(memory, iconX, iconY, icon);
            var text = new Native.Bounds { Left = _vertical ? padding : padding + icon + Px(7),
                Top = _vertical ? iconY + icon + Px(3) : Px(3), Right = width - padding, Bottom = height - Px(2) };
            if (!_compact)
            {
                var prayer = text;
                prayer.Bottom = prayer.Top + Px(_vertical ? 13 : 14);
                Text(memory, _prayer, prayer, Px(_vertical ? 9 : 11), false, _vertical);
                text.Top = prayer.Bottom;
            }
            if (_compact && !_vertical) text.Top = Math.Max(1, (height - Px(16)) / 2);
            Text(memory, _countdown, text, Px(_vertical ? _compact ? 8 : 10 : _compact ? 12 : 14), true, _vertical);
            if (!Native.BitBlt(destination, 0, 0, width, height, memory, 0, 0, 0x00CC0020)) throw new Win32Exception();
            LastRenderError = null;
        }
        finally
        {
            if (old != IntPtr.Zero && old != new IntPtr(-1)) Native.SelectObject(memory, old);
            if (bitmap != IntPtr.Zero) Native.DeleteObject(bitmap);
            Native.DeleteDC(memory);
        }
    }

    private void Text(IntPtr dc, string text, Native.Bounds bounds, int height, bool countdown, bool centered)
    {
        if (bounds.Right <= bounds.Left || bounds.Bottom <= bounds.Top || text.Length == 0) return;
        IntPtr font = Native.CreateFont(-height, 0, 0, 0, 600, 0, 0, 0, 1, 0, 0, 4, 0, countdown ? "Consolas" : "Segoe UI");
        if (font == IntPtr.Zero) throw new Win32Exception();
        IntPtr old = IntPtr.Zero;
        try
        {
            old = Native.SelectObject(dc, font);
            if (old == IntPtr.Zero || old == new IntPtr(-1)) throw new InvalidOperationException("SelectObject failed for font.");
            if (Native.SetBkMode(dc, 1) == 0) throw new InvalidOperationException("SetBkMode failed.");
            if (Native.SetTextColor(dc, ColorRef(_foreground)) == uint.MaxValue) throw new InvalidOperationException("SetTextColor failed.");
            uint flags = 0x0800 | 0x8000 | (centered ? 1u : 0u); // NOPREFIX | END_ELLIPSIS
            if (!centered || !countdown) flags |= 0x0020; // SINGLELINE
            if (Native.DrawText(dc, text, text.Length, ref bounds, flags) == 0) throw new InvalidOperationException("DrawText failed.");
        }
        finally
        {
            if (old != IntPtr.Zero && old != new IntPtr(-1)) Native.SelectObject(dc, old);
            Native.DeleteObject(font);
        }
    }

    private void DrawIcon(IntPtr dc, int x, int y, int size)
    {
        if (_key.Length == 0 || size <= 0) return;
        PrayerPresentation presentation = PrayerUiLogic.PresentationFor(_key);
        IntPtr brush = IntPtr.Zero, pen = IntPtr.Zero, cut = IntPtr.Zero;
        IntPtr oldBrush = IntPtr.Zero, oldPen = IntPtr.Zero;
        try
        {
            brush = Native.CreateSolidBrush(ColorRef(_foreground));
            if (brush == IntPtr.Zero) throw new InvalidOperationException("CreateSolidBrush failed for icon.");
            pen = Native.CreatePen(0, 1, ColorRef(_foreground));
            if (pen == IntPtr.Zero) throw new InvalidOperationException("CreatePen failed for icon.");
            oldBrush = Native.SelectObject(dc, brush);
            if (oldBrush == IntPtr.Zero || oldBrush == new IntPtr(-1)) throw new InvalidOperationException("SelectObject failed for icon brush.");
            oldPen = Native.SelectObject(dc, pen);
            if (oldPen == IntPtr.Zero || oldPen == new IntPtr(-1)) throw new InvalidOperationException("SelectObject failed for icon pen.");
            int inset = Math.Max(2, size / 5);
            if (!Native.Ellipse(dc, x + inset, y + inset, x + size - inset, y + size - inset)) throw new InvalidOperationException("Ellipse failed for icon.");
            if (presentation.IconKind == PrayerIconKind.Moon)
            {
                cut = Native.CreateSolidBrush(ColorRef(_background));
                if (cut == IntPtr.Zero) throw new InvalidOperationException("CreateSolidBrush failed for moon cutout.");
                IntPtr selected = Native.SelectObject(dc, cut);
                if (selected == IntPtr.Zero || selected == new IntPtr(-1)) throw new InvalidOperationException("SelectObject failed for moon cutout.");
                IntPtr nullPen = Native.GetStockObject(8); // NULL_PEN: borrowed, never deleted.
                if (nullPen == IntPtr.Zero) throw new InvalidOperationException("GetStockObject failed for NULL_PEN.");
                selected = Native.SelectObject(dc, nullPen);
                if (selected == IntPtr.Zero || selected == new IntPtr(-1)) throw new InvalidOperationException("SelectObject failed for NULL_PEN.");
                if (!Native.Ellipse(dc, x + size / 2, y + inset - 2, x + size, y + size - inset - 2)) throw new InvalidOperationException("Ellipse failed for moon cutout.");
                selected = Native.SelectObject(dc, brush);
                if (selected == IntPtr.Zero || selected == new IntPtr(-1)) throw new InvalidOperationException("SelectObject failed restoring icon brush.");
                selected = Native.SelectObject(dc, pen);
                if (selected == IntPtr.Zero || selected == new IntPtr(-1)) throw new InvalidOperationException("SelectObject failed restoring icon pen.");
            }
            else
            {
                for (int ray = 0; ray < 8; ray++)
                {
                    double angle = ray * Math.PI / 4;
                    int centerX = x + size / 2, centerY = y + size / 2;
                    if (!Native.MoveToEx(dc, centerX + (int)(Math.Cos(angle) * size * .38), centerY + (int)(Math.Sin(angle) * size * .38), IntPtr.Zero)) throw new InvalidOperationException("MoveToEx failed for icon ray.");
                    if (!Native.LineTo(dc, centerX + (int)(Math.Cos(angle) * size * .48), centerY + (int)(Math.Sin(angle) * size * .48))) throw new InvalidOperationException("LineTo failed for icon ray.");
                }
            }
        }
        finally
        {
            if (oldBrush != IntPtr.Zero && oldBrush != new IntPtr(-1)) Native.SelectObject(dc, oldBrush);
            if (oldPen != IntPtr.Zero && oldPen != new IntPtr(-1)) Native.SelectObject(dc, oldPen);
            if (cut != IntPtr.Zero) Native.DeleteObject(cut);
            if (brush != IntPtr.Zero) Native.DeleteObject(brush);
            if (pen != IntPtr.Zero) Native.DeleteObject(pen);
        }
    }

    private static uint ColorRef(string value)
    {
        var color = (Color)ColorConverter.ConvertFromString(value)!;
        return color.R | ((uint)color.G << 8) | ((uint)color.B << 16);
    }
    private static void Fill(IntPtr dc, Native.Bounds bounds, uint color)
    {
        if (bounds.Right <= bounds.Left || bounds.Bottom <= bounds.Top) return;
        IntPtr brush = Native.CreateSolidBrush(color);
        if (brush == IntPtr.Zero) throw new InvalidOperationException("CreateSolidBrush failed for fill.");
        try
        {
            if (Native.FillRect(dc, ref bounds, brush) == 0) throw new InvalidOperationException("FillRect failed.");
        }
        finally { Native.DeleteObject(brush); }
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        IntPtr window = Handle;
        if (window != IntPtr.Zero) Native.DestroyWindow(window);
        Handle = IntPtr.Zero;
        if (_registered) { Native.UnregisterClass(_className, _instance); _registered = false; }
        GC.KeepAlive(_windowProc);
    }

    private static class Native
    {
        internal delegate IntPtr WindowProc(IntPtr window, uint message, IntPtr wparam, IntPtr lparam);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] internal struct WindowClass
        {
            internal uint Size, Style; internal IntPtr WindowProc; internal int ClassExtra, WindowExtra;
            internal IntPtr Instance, Icon, Cursor, Background; internal string? MenuName, ClassName; internal IntPtr SmallIcon;
        }
        [StructLayout(LayoutKind.Sequential)] internal struct Bounds { internal int Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)] internal struct Paint
        {
            internal IntPtr Dc; internal int Erase; internal Bounds Bounds; internal int Restore, Update;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] internal byte[] Reserved;
        }
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode)] internal static extern IntPtr GetModuleHandle(string? name);
        [DllImport("user32.dll", EntryPoint = "RegisterClassExW", CharSet = CharSet.Unicode, SetLastError = true)] internal static extern ushort RegisterClassEx(ref WindowClass value);
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)] internal static extern IntPtr CreateWindowEx(uint ex, string cls, string title, uint style, int x, int y, int w, int h, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr parameter);
        [DllImport("user32.dll", EntryPoint = "UnregisterClassW", CharSet = CharSet.Unicode)] internal static extern bool UnregisterClass(string cls, IntPtr instance);
        [DllImport("user32.dll")] internal static extern bool DestroyWindow(IntPtr window);
        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")] internal static extern IntPtr DefWindowProc(IntPtr window, uint message, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll", EntryPoint = "LoadCursorW")] internal static extern IntPtr LoadCursor(IntPtr instance, IntPtr id);
        [DllImport("user32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode)] internal static extern bool SetWindowText(IntPtr window, string title);
        [DllImport("user32.dll")] internal static extern bool InvalidateRect(IntPtr window, IntPtr rect, bool erase);
        [DllImport("user32.dll")] internal static extern IntPtr BeginPaint(IntPtr window, out Paint paint);
        [DllImport("user32.dll")] internal static extern bool EndPaint(IntPtr window, ref Paint paint);
        [DllImport("user32.dll")] internal static extern int FillRect(IntPtr dc, ref Bounds bounds, IntPtr brush);
        [DllImport("user32.dll")] internal static extern int FrameRect(IntPtr dc, ref Bounds bounds, IntPtr brush);
        [DllImport("user32.dll", EntryPoint = "DrawTextW", CharSet = CharSet.Unicode)] internal static extern int DrawText(IntPtr dc, string text, int count, ref Bounds bounds, uint flags);
        [DllImport("gdi32.dll")] internal static extern IntPtr CreateCompatibleDC(IntPtr dc);
        [DllImport("gdi32.dll")] internal static extern IntPtr CreateCompatibleBitmap(IntPtr dc, int width, int height);
        [DllImport("gdi32.dll")] internal static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
        [DllImport("gdi32.dll")] internal static extern bool DeleteObject(IntPtr obj);
        [DllImport("gdi32.dll")] internal static extern bool DeleteDC(IntPtr dc);
        [DllImport("gdi32.dll")] internal static extern bool BitBlt(IntPtr dst, int x, int y, int w, int h, IntPtr src, int sx, int sy, uint mode);
        [DllImport("gdi32.dll")] internal static extern IntPtr CreateSolidBrush(uint color);
        [DllImport("gdi32.dll")] internal static extern IntPtr CreatePen(int style, int width, uint color);
        [DllImport("gdi32.dll")] internal static extern IntPtr GetStockObject(int index);
        [DllImport("gdi32.dll")] internal static extern bool Ellipse(IntPtr dc, int left, int top, int right, int bottom);
        [DllImport("gdi32.dll")] internal static extern bool MoveToEx(IntPtr dc, int x, int y, IntPtr previous);
        [DllImport("gdi32.dll")] internal static extern bool LineTo(IntPtr dc, int x, int y);
        [DllImport("gdi32.dll", EntryPoint = "CreateFontW", CharSet = CharSet.Unicode)] internal static extern IntPtr CreateFont(int height, int width, int escapement, int orientation, int weight, uint italic, uint underline, uint strike, uint charset, uint output, uint clip, uint quality, uint pitch, string face);
        [DllImport("gdi32.dll")] internal static extern int SetBkMode(IntPtr dc, int mode);
        [DllImport("gdi32.dll")] internal static extern uint SetTextColor(IntPtr dc, uint color);
    }
}
