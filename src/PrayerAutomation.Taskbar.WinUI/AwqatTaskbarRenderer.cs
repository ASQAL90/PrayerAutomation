// Hosting sequence and Native subset adapted from Khiro95/Awqat-Salaat
// pin c77a733efad9dc986901f07236e886b0bd6642ff, TaskBarWidget.cs:82-115,569-600
// and AwqatSalaat.Common/Interop/Native.cs. MIT (c) 2023 Khiro.
// See LICENSE-Awqat-Salaat.txt and PROVENANCE.md. Registration-free init
// follows Windows App SDK 2.6.4 UndockedRegFreeWinRT-AutoInitializer.cs;
// MIT (c) Microsoft Corporation and Contributors; see LICENSE-WindowsAppSDK-Source.txt.
// Other code is new.
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Content;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

namespace PrayerAutomation.Taskbar.WinUI;

/// <summary>STA-affine, app-local WinUI island. Coordinates/sizes are parent-local physical pixels.
/// Caller owns verified slot placement and a pumping UI thread. No Explorer discovery or mutation.</summary>
public sealed class AwqatTaskbarRenderer : IDisposable
{
    private readonly int threadId = Environment.CurrentManagedThreadId;
    private readonly IntPtr parent;
    private readonly Action openDetail;
    private readonly Native.WndProc windowProc;
    private readonly string className = "PrayerAutomation.AwqatIsland." + Guid.NewGuid().ToString("N");
    private readonly IntPtr module = Native.GetModuleHandle(null);
    private IntPtr hwnd;
    private GCHandle lifetimeRoot; // Native class/function pointer must outlive any managed caller reference.
    private bool registered, disposed, destroying;
    private DesktopWindowXamlSource? island;
    private AppWindow? appWindow;
    [ThreadStatic] private static ThreadRuntime? sharedRuntime;
    private ThreadRuntime? runtime;
    // WindowsXamlManager is shared by Windows App SDK 1.5+; share one lease per STA as well.
    private sealed class ThreadRuntime
    {
        internal int References;
        internal WindowsXamlManager? Manager;
        internal DispatcherQueueController? OwnedQueue;
        internal bool ShuttingDown;
        internal volatile bool Closed;
    }
    private Grid? root, textGrid, body;
    private Border? frame;
    private TextBlock? prayerText, countdownText;
    private Viewbox? cue;
    private string? cueKey;

    public IntPtr Handle => hwnd;
    public string? LastRenderError { get; private set; }

    // NoInlining on InitializeIsland keeps WinUI activation after app-local WinRT bootstrap.
    public AwqatTaskbarRenderer(IntPtr parent, int width, int height, Action openDetail)
    {
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            throw new InvalidOperationException("Awqat renderer requires a pumping STA UI thread.");
        if (parent == IntPtr.Zero || !Native.IsWindow(parent))
            throw new ArgumentException("A live parent HWND is required; no desktop fallback.", nameof(parent));
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        this.parent = parent;
        this.openDetail = openDetail ?? throw new ArgumentNullException(nameof(openDetail));
        windowProc = WndProc;
        try
        {
            // Supported self-contained registration-free activation, no package installer/bootstrap MSIX.
            Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);
            Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY_PID", Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
            Marshal.ThrowExceptionForHR(Native.WindowsAppRuntime_EnsureIsLoaded());
            WinRT.ComWrappersSupport.InitializeComWrappers();
            InitializeIsland(width, height);
        }
        catch (Exception e)
        {
            LastRenderError = e.ToString();
            Cleanup();
            disposed = true;
            throw;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void InitializeIsland(int width, int height)
    {
        if (sharedRuntime?.ShuttingDown == true)
        {
            if (!sharedRuntime.Closed) throw new InvalidOperationException("Owned WinUI DispatcherQueue is shutting down; create again on a later dispatcher turn.");
            sharedRuntime = null;
        }
        runtime = sharedRuntime ??= new ThreadRuntime();
        runtime.References++;
        if (runtime.Manager is null)
        {
            if (DispatcherQueue.GetForCurrentThread() is null)
                runtime.OwnedQueue = DispatcherQueueController.CreateOnCurrentThread();
            runtime.Manager = WindowsXamlManager.InitializeForCurrentThread();
        }
        // Actual Awqat pipeline: DesktopWindowXamlSource BEFORE owned popup HWND creation.
        island = new DesktopWindowXamlSource();
        var wc = new Native.WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf<Native.WNDCLASSEX>(),
            lpfnWndProc = windowProc,
            hInstance = module,
            lpszClassName = className
        };
        if (Native.RegisterClassEx(ref wc) == 0) throw Native.Error("RegisterClassEx");
        registered = true;
        lifetimeRoot = GCHandle.Alloc(this);
        hwnd = Native.CreateWindowEx(Native.WS_EX_LAYERED | Native.WS_EX_NOACTIVATE | Native.WS_EX_TOOLWINDOW,
            className, "Prayer taskbar XAML island", Native.WS_POPUP,
            -32000, -32000, 0, 0, parent, IntPtr.Zero, module, IntPtr.Zero);
        if (hwnd == IntPtr.Zero) throw Native.Error("CreateWindowEx");
        var id = Win32Interop.GetWindowIdFromWindow(hwnd);
        appWindow = AppWindow.GetFromWindowId(id) ?? throw new InvalidOperationException("No AppWindow for owned HWND.");
        appWindow.IsShownInSwitchers = false;
        appWindow.Destroying += OnDestroying;
        appWindow.ResizeClient(new SizeInt32(width, height));
        island.Initialize(id);
        island.SiteBridge.ResizePolicy = ContentSizePolicy.ResizeContentToParentWindow;
        root = new Grid { Background = new SolidColorBrush(Colors.Transparent) };
        frame = new Border { CornerRadius = new CornerRadius(4), BorderThickness = new Thickness(1), Padding = new Thickness(5, 1, 5, 1) };
        body = new Grid();
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        cue = new Viewbox { Width = 24, Height = 24, Stretch = Stretch.Uniform, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 7, 0) };
        textGrid = new Grid { VerticalAlignment = VerticalAlignment.Center };
        prayerText = new TextBlock { FontSize = 11, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis };
        countdownText = new TextBlock { FontFamily = new FontFamily("Consolas"), FontSize = 14, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis };
        textGrid.Children.Add(prayerText);
        textGrid.Children.Add(countdownText);
        Grid.SetColumn(textGrid, 1);
        body.Children.Add(cue);
        body.Children.Add(textGrid);
        frame.Child = body;
        root.Children.Add(frame);
        root.PointerReleased += OnPointerReleased;
        island.Content = root;
        Marshal.SetLastPInvokeError(0);
        IntPtr previous = Native.SetParent(hwnd, parent);
        int error = Marshal.GetLastPInvokeError();
        if (previous == IntPtr.Zero && error != 0) throw new Win32Exception(error, "SetParent");
        RequireParent();
        RequirePolicy();
        Update("", "", "sun", "#FF202020", "#FFFFFFFF", "#FF606060", false, false, 96, "Prayer countdown");
        // Construction stays hidden; only SetBounds(show:true) can show a verified slot.
        appWindow.Hide();
    }

    public bool HasParent(IntPtr expectedParent) => !disposed && hwnd != IntPtr.Zero &&
        expectedParent != IntPtr.Zero && Native.IsWindow(expectedParent) && Native.IsWindow(hwnd) &&
        Native.GetAncestor(hwnd, 1) == expectedParent;

    public void Update(string prayer, string countdown, string iconKey, string background, string foreground,
        string border, bool vertical, bool compact, int dpi, string accessibleDescription,
        double cornerRadius = 4, string? countdownForeground = null)
    {
        CheckThread();
        try
        {
            RequireParent();
            if (dpi <= 0) throw new ArgumentOutOfRangeException(nameof(dpi));
            // XAML units are DIP; its XamlRoot handles monitor scale. HWND bounds are physical pixels.
            frame!.Background = new SolidColorBrush(ParseColor(background));
            frame.BorderBrush = new SolidColorBrush(ParseColor(border));
            frame.CornerRadius = new CornerRadius(Math.Max(0, cornerRadius));
            frame.Padding = vertical ? new Thickness(compact ? 1 : 2, 1, compact ? 1 : 2, 1) : new Thickness(5, 1, 5, 1);
            var brush = new SolidColorBrush(ParseColor(foreground));
            prayerText!.Foreground = brush;
            countdownText!.Foreground = new SolidColorBrush(ParseColor(countdownForeground ?? foreground));
            prayerText.Text = prayer ?? "";
            countdownText.Text = countdown ?? "";
            UpdateCue(iconKey, brush);
            prayerText.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
            prayerText.FontSize = vertical ? 9 : 11;
            prayerText.TextAlignment = countdownText.TextAlignment = vertical ? TextAlignment.Left : TextAlignment.Center;
            countdownText.FontSize = vertical ? compact ? 8 : 10 : compact ? 13 : 16;
            cue!.Width = cue.Height = compact ? 18 : 24;
            body!.RowDefinitions.Clear();
            body.ColumnDefinitions.Clear();
            Grid.SetRow(cue, 0); Grid.SetColumn(cue, 0);
            if (vertical)
            {
                body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                body.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                Grid.SetRow(textGrid!, 1); Grid.SetColumn(textGrid!, 0);
                cue.HorizontalAlignment = HorizontalAlignment.Center;
                cue.Margin = new Thickness(0, 0, 0, 3);
            }
            else
            {
                body.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Grid.SetRow(textGrid!, 0); Grid.SetColumn(textGrid!, 1);
                cue.HorizontalAlignment = HorizontalAlignment.Stretch;
                cue.Margin = new Thickness(0, 0, 7, 0);
            }
            textGrid!.RowDefinitions.Clear();
            textGrid.ColumnDefinitions.Clear();
            bool stacked = vertical || !compact;
            if (stacked)
            {
                textGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                textGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetRow(prayerText, 0); Grid.SetColumn(prayerText, 0);
                Grid.SetRow(countdownText, 1); Grid.SetColumn(countdownText, 0);
                countdownText.Margin = new Thickness(0);
            }
            else
            {
                textGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Grid.SetRow(prayerText, 0); Grid.SetColumn(prayerText, 0);
                Grid.SetRow(countdownText, 0); Grid.SetColumn(countdownText, 0);
                countdownText.Margin = new Thickness(0);
            }
            AutomationProperties.SetName(root!, accessibleDescription ?? "");
            AutomationProperties.SetHelpText(root!, accessibleDescription ?? "");
            if (!Native.SetWindowText(hwnd, accessibleDescription ?? "")) throw Native.Error("SetWindowText");
            LastRenderError = null;
        }
        catch (Exception e) { FailClosed(e); throw; }
    }

    private void UpdateCue(string iconKey, SolidColorBrush foreground)
    {
        cue!.Visibility = string.IsNullOrEmpty(iconKey) ? Visibility.Collapsed : Visibility.Visible;
        if (!string.Equals(cueKey, iconKey, StringComparison.Ordinal))
        {
            var canvas = new Canvas { Width = PrayerAutomation.Shared.PrayerIconPaths.Size, Height = PrayerAutomation.Shared.PrayerIconPaths.Size };
            foreach (string data in PrayerAutomation.Shared.PrayerIconPaths.ForKey(iconKey))
            {
                // Data is from the linked internal geometry constants, never external XAML.
                var path = (Microsoft.UI.Xaml.Shapes.Path)Microsoft.UI.Xaml.Markup.XamlReader.Load(
                    $"<Path xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Data='{data}' />");
                canvas.Children.Add(path);
            }
            cue.Child = canvas;
            cueKey = iconKey;
        }
        if (cue.Child is Canvas content)
            foreach (Microsoft.UI.Xaml.Shapes.Path path in content.Children)
                path.Fill = foreground;
    }

    public void SetBounds(int x, int y, int width, int height, bool show)
    {
        CheckThread();
        try
        {
            RequireParent();
            RequirePolicy();
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (!show) appWindow!.Hide();
            // Preserve Awqat AppWindow Move/MoveAndResize, with parent-local physical slot coordinates.
            appWindow!.MoveAndResize(new RectInt32(x, y, width, height));
            RequireParent();
            RequirePolicy();
            if (show) appWindow.Show(false);
            RequireParent();
            RequirePolicy();
            LastRenderError = null;
        }
        catch (Exception e) { FailClosed(e); throw; }
    }

    private void RequireParent()
    {
        if (!HasParent(parent)) throw new InvalidOperationException("Owned island lost its expected parent; hidden, no fallback.");
    }
    private void RequirePolicy()
    {
        uint ex = Native.GetWindowLong(hwnd, -20);
        uint required = Native.WS_EX_NOACTIVATE | Native.WS_EX_TOOLWINDOW;
        // A popup owned by the topmost taskbar inherits its z-order; never change the host.
        bool inheritedTopmost = (Native.GetWindowLong(parent, -20) & 8) != 0;
        if ((ex & required) != required || (ex & 8) != 0 && !inheritedTopmost)
            throw new InvalidOperationException("No-activation/tool-window/host-inherited z-order policy changed; hidden.");
    }
    private void CheckThread()
    {
        if (Environment.CurrentManagedThreadId != threadId) throw new InvalidOperationException("Call on the constructing STA UI thread.");
        ObjectDisposedException.ThrowIf(disposed, this);
    }
    private void FailClosed(Exception e)
    {
        LastRenderError = e.ToString();
        if (hwnd != IntPtr.Zero && Native.IsWindow(hwnd)) Native.ShowWindow(hwnd, 0);
    }
    private void OnDestroying(AppWindow sender, object args) => destroying = true;
    private void OnPointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        try
        {
            if (!disposed && e.GetCurrentPoint(root).Properties.PointerUpdateKind == Microsoft.UI.Input.PointerUpdateKind.LeftButtonReleased)
            { RequireParent(); openDetail(); e.Handled = true; }
        }
        catch (Exception error) { FailClosed(error); }
    }
    private IntPtr WndProc(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (message == 0x21) return new IntPtr(3); // WM_MOUSEACTIVATE => MA_NOACTIVATE
            if (message == 0x82 && hwnd == handle) { hwnd = IntPtr.Zero; destroying = true; }
            return Native.DefWindowProc(handle, message, wParam, lParam);
        }
        catch (Exception e)
        {
            LastRenderError = e.ToString();
            Native.ShowWindow(handle, 0);
            return IntPtr.Zero; // Never propagate managed exceptions through unmanaged WndProc.
        }
    }
    public void Dispose()
    {
        if (disposed) return;
        CheckThread();
        disposed = true;
        Cleanup();
        GC.SuppressFinalize(this);
    }
    private void Cleanup()
    {
        // Unwind partial creation too. Never destroy the supplied parent or touch foreign objects.
        void Try(Action action) { try { action(); } catch (Exception e) { LastRenderError ??= e.ToString(); } }
        if (hwnd != IntPtr.Zero) Native.ShowWindow(hwnd, 0);
        if (root is not null) Try(() => root.PointerReleased -= OnPointerReleased);
        if (island is not null)
        {
            // After external HWND destruction, Content touches a dead SiteBridge; close only.
            if (hwnd != IntPtr.Zero && Native.IsWindow(hwnd)) Try(() => island.Content = null);
            Try(island.Dispose);
            island = null;
        }
        if (appWindow is not null)
        {
            Try(() => appWindow.Destroying -= OnDestroying);
            if (!destroying && hwnd != IntPtr.Zero && Native.IsWindow(hwnd)) Try(appWindow.Destroy);
            appWindow = null;
        }
        if (hwnd != IntPtr.Zero && Native.IsWindow(hwnd))
            Try(() => { if (!Native.DestroyWindow(hwnd)) throw Native.Error("DestroyWindow"); });
        // Do NOT release retained class/delegate if native destruction actually failed.
        if (hwnd != IntPtr.Zero && Native.IsWindow(hwnd)) return;
        hwnd = IntPtr.Zero;
        if (registered) Try(() => { if (!Native.UnregisterClass(className, module)) throw Native.Error("UnregisterClass"); registered = false; });
        if (!registered && lifetimeRoot.IsAllocated) lifetimeRoot.Free();
        root = null; frame = null; textGrid = null; body = null; prayerText = null; countdownText = null; cue = null;
        if (runtime is not null)
        {
            var lease = runtime; runtime = null;
            if (--lease.References == 0)
            {
                lease.ShuttingDown = true;
                if (lease.Manager is not null) Try(lease.Manager.Dispose);
                if (lease.OwnedQueue is null) lease.Closed = true;
                else
                {
                    // Do not block the WPF thread. Keep pumping after last Dispose for WinUI shutdown.
                    Try(() => { var shutdown = lease.OwnedQueue.ShutdownQueueAsync(); shutdown.Completed = (_, _) => lease.Closed = true; });
                }
            }
        }
        GC.KeepAlive(windowProc);
    }
    private static Windows.UI.Color ParseColor(string value)
    {
        if (string.IsNullOrEmpty(value) || value[0] != '#' || value.Length is not (7 or 9))
            throw new ArgumentException("Colors must be #RRGGBB or #AARRGGBB.", nameof(value));
        uint n = uint.Parse(value.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return Windows.UI.Color.FromArgb(value.Length == 9 ? (byte)(n >> 24) : (byte)255,
            (byte)(n >> 16), (byte)(n >> 8), (byte)n);
    }

    private static class Native
    {
        internal const uint WS_POPUP = 0x80000000, WS_EX_LAYERED = 0x00080000,
            WS_EX_NOACTIVATE = 0x08000000, WS_EX_TOOLWINDOW = 0x00000080;
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WNDCLASSEX
        {
            internal uint cbSize, style;
            [MarshalAs(UnmanagedType.FunctionPtr)] internal WndProc lpfnWndProc;
            internal int cbClsExtra, cbWndExtra;
            internal IntPtr hInstance, hIcon, hCursor, hbrBackground;
            internal string? lpszMenuName;
            internal string lpszClassName;
            internal IntPtr hIconSm;
        }
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] internal static extern ushort RegisterClassEx(ref WNDCLASSEX wc);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool UnregisterClass(string name, IntPtr instance);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] internal static extern IntPtr CreateWindowEx(uint exStyle, string className, string title, uint style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);
        [DllImport("user32.dll", SetLastError = true)] internal static extern IntPtr SetParent(IntPtr child, IntPtr parent);
        [DllImport("user32.dll")] internal static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern IntPtr DefWindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool DestroyWindow(IntPtr hwnd);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool IsWindow(IntPtr hwnd);
        [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool ShowWindow(IntPtr hwnd, int show);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern uint GetWindowLong(IntPtr hwnd, int index);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool SetWindowText(IntPtr hwnd, string text);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] internal static extern IntPtr GetModuleHandle(string? name);
        [DllImport("Microsoft.WindowsAppRuntime.dll", ExactSpelling = true)] internal static extern int WindowsAppRuntime_EnsureIsLoaded();
        internal static Win32Exception Error(string operation) => new(Marshal.GetLastPInvokeError(), operation);
    }
}
