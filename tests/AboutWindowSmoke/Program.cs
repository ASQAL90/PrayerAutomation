using System.Collections;
using System.IO;
using Expression = System.Linq.Expressions.Expression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml.Linq;

internal static class Program
{
    private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private static readonly List<string> Checks = [];
    private static readonly List<object> Layouts = [];
    private static readonly List<string> Links = [];
    private static readonly List<Window> Owned = [];
    private static readonly string[] ExpectedCopy =
    [
        "author : Muhammad Nashiruddin Al-Asqalani \"I develop as hobby\"",
        "email : madinalani@gmail.com",
        "linkedin : https://linkedin.com/in/madinalani",
        "github : github.com/ASQAL90",
        "co-author : dibantu beberapa mba-mba AI",
        "terimakasih pada mbak Luna yang membuat kerangka pondasi program dari awal membuat core perhitungan kalender & waktu sholat dengan Rust & C#,",
        "dan mbak Sol yang membantu menemukan dan memperbaiki Bug, celah- celah keamanan, unit test, serta meningkatkan fitur menjadi lebih baik."
    ];
    private static Application? Host;
    private static Assembly Product = null!;
    private static Type AboutType = null!;
    private static string Output = "";
    private static int StartupAborted;

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 3) { Console.Error.WriteLine("Usage: AboutWindowSmoke <product.dll> <App.xaml> <receipt-directory>"); return 2; }
        Output = Path.GetFullPath(args[2]);
        Directory.CreateDirectory(Output);
        string? failure = null;
        try
        {
            Product = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            // Only the generic framework Application is constructed. No product App,
            // MainWindow, NotifyIcon, profile, scheduler, native method or mutex is used.
            Host = CreateInertHost();
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
            var probe = new Border { Width = 20, Height = 20, Background = Brushes.Black };
            probe.Measure(new Size(20, 20)); probe.Arrange(new Rect(0, 0, 20, 20));
            Check(probe.ActualWidth == 20, "Actual WPF STA resource host/layout is active.");
            AboutType = Product.GetType("PrayerAutomation.UI.AboutWindow", false)!;
            Check(AboutType is not null && typeof(Window).IsAssignableFrom(AboutType),
                "Tray About feature must provide the actual WPF AboutWindow popup (feature missing).");
            LoadProductStyles(args[1]);
            VerifyPopupLifecycle();
            VerifyLinks();
            Check(Links.Count == 3, "Exactly the three deliberate mock link launches occurred.");
            Check(Host.GetType() == typeof(Application) && Host.Windows.Count == 0,
                "Generic host only; all owned About windows closed and no product lifecycle constructed.");
        }
        catch (Exception ex)
        {
            failure = (ex is TargetInvocationException tie && tie.InnerException is not null ? tie.InnerException : ex).ToString();
            Console.Error.WriteLine(failure);
        }
        finally
        {
            foreach (Window window in Owned.Where(w => w.IsVisible).ToArray()) window.Close();
            if (Host is not null) { Pump(); Host.Shutdown(); }
            string productHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[0]))).ToLowerInvariant();
            var receipt = new { passed = failure is null, checks = Checks, check_count = Checks.Count, failure,
                startup_aborted = StartupAborted, generic_framework_host = Host?.GetType().FullName,
                product_app_constructed = false, product_executable_launched = false, native_operations = false,
                notify_icon_constructed = false, profile_or_registry_writes = false, network_fetches = false,
                real_link_launches = false, mock_links = Links, layouts = Layouts,
                production_assembly = Path.GetFullPath(args[0]), production_sha256 = productHash,
                harness_sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Assembly.GetExecutingAssembly().Location))).ToLowerInvariant() };
            File.WriteAllText(Path.Combine(Output, "receipt.json"), JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
        }
        Console.WriteLine($"{(failure is null ? "PASS" : "FAIL")}: {Checks.Count} completed checks; receipt={Path.Combine(Output, "receipt.json")}");
        return failure is null ? 0 : 1;
    }

    private static Application CreateInertHost()
    {
        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        void Posted(object? sender, DispatcherHookEventArgs e)
        {
            Delegate callback = (Delegate)typeof(DispatcherOperation).GetField("_method", Members)!.GetValue(e.Operation)!;
            if (callback.Method.DeclaringType != typeof(Application)) return;
            Check(callback.Method.Name == "<.ctor>b__1_0" && e.Operation.Priority == DispatcherPriority.Send &&
                  e.Operation.Status == DispatcherOperationStatus.Pending && e.Operation.Abort(),
                "Exact generic Application constructor startup callback aborted before any pump.");
            StartupAborted++;
        }
        dispatcher.Hooks.OperationPosted += Posted;
        try
        {
            var host = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            Check(StartupAborted == 1, "One framework startup operation suppressed; product App never constructed.");
            return host;
        }
        finally { dispatcher.Hooks.OperationPosted -= Posted; }
    }

    private static void LoadProductStyles(string path)
    {
        // Read the unchanged production styles without constructing x:Class App.
        XDocument document = XDocument.Load(path);
        XNamespace p = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        var resources = document.Root!.Element(p + "Application.Resources")!;
        var dictionary = new XElement(p + "ResourceDictionary",
            document.Root!.Attributes().Where(attribute => attribute.IsNamespaceDeclaration),
            resources.Elements().Select(element => new XElement(element)));
        dictionary.SetAttributeValue(XNamespace.Xmlns + "local", "clr-namespace:PrayerAutomation.UI;assembly=AyoJamaah");
        Host!.Resources = (ResourceDictionary)XamlReader.Parse(dictionary.ToString());
    }

    private static Window CreateAbout(Action<Uri> launcher)
    {
        var constructor = AboutType.GetConstructor(Members, null, [typeof(Action<Uri>)], null);
        Check(constructor is not null, "Actual AboutWindow accepts a mock link launcher.");
        Window window = (Window)constructor!.Invoke([launcher]);
        window.ShowActivated = false;
        window.ShowInTaskbar = false;
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = -30000; window.Top = -30000;
        window.Topmost = false;
        Owned.Add(window);
        return window;
    }

    private static void VerifyPopupLifecycle()
    {
        Type controller = Product.GetType("PrayerAutomation.UI.AboutPopupController", true)!;
        Type factoryType = typeof(Func<>).MakeGenericType(AboutType);
        var factory = Expression.Lambda(factoryType,
            Expression.Convert(Expression.Call(typeof(Program).GetMethod(nameof(CreateAbout), BindingFlags.Static | BindingFlags.NonPublic)!,
                Expression.Constant((Action<Uri>)(uri => Links.Add(uri.AbsoluteUri)))), AboutType)).Compile();
        object popup = controller.GetConstructor(Members, null, [factoryType], null)!.Invoke([factory]);
        MethodInfo show = controller.GetMethod("Show", Members)!;
        show.Invoke(popup, null); Pump();
        Window first = Owned.Single();
        Check(first.IsVisible && Host!.Windows.Count == 1 && first.WindowStyle == WindowStyle.None && !first.ShowInTaskbar,
            "Tray-owned About is one actual visible offscreen popup without an OS title bar/taskbar button.");
        Check(Links.Count == 0, "Opening About performs no link launch.");
        show.Invoke(popup, null); Pump();
        Check(Owned.Count == 1 && first.IsVisible && Host!.Windows.Count == 1,
            "Repeated About command reuses the same window.");
        VerifyCopyLayoutThemes(first);
        Button close = ById<Button>(first, "AboutClose");
        var invoke = (IInvokeProvider)new ButtonAutomationPeer(close).GetPattern(PatternInterface.Invoke)!;
        invoke.Invoke(); Pump();
        Check(!first.IsVisible && Host!.Windows.Count == 0, "Actual close-button event closes only About.");
        show.Invoke(popup, null); Pump();
        Window second = Owned.Last();
        Check(Owned.Count == 2 && !ReferenceEquals(first, second) && second.IsVisible && Host!.Windows.Count == 1,
            "Closed resets the owner; reopening creates a fresh About popup.");
        controller.GetMethod("Close", Members)!.Invoke(popup, null); Pump();
        Check(!second.IsVisible && Host!.Windows.Count == 0, "Tray disposal closes its popup without application shutdown.");
        Check(Links.Count == 0, "Show/reuse/close/reopen do not launch URLs or email.");
    }

    private static void VerifyCopyLayoutThemes(Window window)
    {
        TextBlock[] blocks = Descendants(window).OfType<TextBlock>().Where(t => AutomationProperties.GetAutomationId(t).StartsWith("AboutCredit", StringComparison.Ordinal)).ToArray();
        Check(blocks.Length == ExpectedCopy.Length, "All seven credit lines/paragraphs are realized; no empty selector passes.");
        for (int i = 0; i < ExpectedCopy.Length; i++)
        {
            Check(InlineText(blocks[i].Inlines) == ExpectedCopy[i], "Verbatim credit text " + i + "; actual=" + JsonSerializer.Serialize(InlineText(blocks[i].Inlines)) + "; expected=" + JsonSerializer.Serialize(ExpectedCopy[i]));
        }
        Check(!Descendants(window).OfType<TabControl>().Any(), "About is a standalone popup, not a main-window tab.");
        MethodInfo apply = Product.GetType("PrayerAutomation.UI.ThemeManager", true)!.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static)!;
        IEnumerable palettes = (IEnumerable)Product.GetType("PrayerAutomation.UI.ThemeCatalog", true)!.GetProperty("All")!.GetValue(null)!;
        int themeCount = 0;
        foreach (object palette in palettes)
        {
            string id = (string)palette.GetType().GetProperty("Id")!.GetValue(palette)!;
            apply.Invoke(null, [Host, id]); Pump(); themeCount++;
            foreach ((double width, double height) in new[] { (640d, 540d), (440d, 320d) })
            {
                window.Width = width; window.Height = height; Pump(); window.UpdateLayout();
                var scroll = ById<ScrollViewer>(window, "AboutCreditsScroll");
                var frame = ById<Border>(window, "AboutFrame");
                Check(scroll.HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled && scroll.ViewportWidth > 0,
                    id + "/" + width + ": only vertical scrolling is needed.");
                foreach (TextBlock text in blocks)
                {
                    var measured = new TextBlock { Text = InlineText(text.Inlines),
                        FontFamily = text.FontFamily, FontSize = text.FontSize, FontWeight = text.FontWeight,
                        TextWrapping = TextWrapping.Wrap, LineHeight = text.LineHeight, LineStackingStrategy = text.LineStackingStrategy };
                    measured.Measure(new Size(text.ActualWidth + 1, double.PositiveInfinity));
                    Check(text.FontSize == 14 && text.LineHeight == 20 && text.TextWrapping == TextWrapping.Wrap &&
                          text.ActualWidth > 0 && text.ActualWidth <= scroll.ViewportWidth + 1 && text.ActualHeight + 1 >= measured.DesiredSize.Height,
                        id + "/" + width + ": readable/wrapped/unclipped " + AutomationProperties.GetAutomationId(text));
                    Check(Contrast(((SolidColorBrush)text.Foreground).Color, ((SolidColorBrush)frame.Background).Color) >= 4.5,
                        id + "/" + width + ": normal credit contrast >= 4.5:1.");
                }
                foreach (Hyperlink link in blocks.SelectMany(b => b.Inlines.OfType<Hyperlink>()))
                {
                    Check(Contrast(((SolidColorBrush)link.Foreground).Color, ((SolidColorBrush)frame.Background).Color) >= 4.5,
                        id + ": readable actual hyperlink foreground.");
                    Run run = link.Inlines.OfType<Run>().Single();
                    Rect glyph = run.ContentStart.GetCharacterRect(LogicalDirection.Forward);
                    Check(!string.IsNullOrEmpty(run.Text) && !glyph.IsEmpty && glyph.Height >= 10,
                        id + "/" + width + ": actual link glyph layout realized.");
                }
                scroll.ScrollToBottom(); Pump();
                TextBlock last = blocks.Last();
                Rect lastBounds = last.TransformToAncestor(scroll).TransformBounds(new Rect(last.RenderSize));
                Check(last.IsVisible && lastBounds.Bottom <= scroll.ActualHeight + 1 && lastBounds.Bottom > 0,
                    id + "/" + width + ": final credits reachable by scrolling.");
                Check(ById<Button>(window, "AboutClose").IsVisible, id + "/" + width + ": close remains reachable.");
                Layouts.Add(new { theme = id, width = window.ActualWidth, height = window.ActualHeight,
                    viewport_width = scroll.ViewportWidth, extent_height = scroll.ExtentHeight, final_bottom = lastBounds.Bottom });
            }
            if (id is "light" or "dark") Capture((FrameworkElement)window.Content, Path.Combine(Output, "about-" + id + "-minimum.png"));
        }
        Check(themeCount == 8 && Layouts.Count == 16, "All eight existing themes and normal/minimum sizes exercised on the same dynamic-resource popup.");
    }

    private static void VerifyLinks()
    {
        Window window = CreateAbout(uri => Links.Add(uri.AbsoluteUri));
        window.Show(); Pump();
        Hyperlink[] links = Descendants(window).OfType<TextBlock>().SelectMany(t => t.Inlines.OfType<Hyperlink>()).ToArray();
        string[] expected = ["mailto:madinalani@gmail.com", "https://linkedin.com/in/madinalani", "https://github.com/ASQAL90"];
        Check(links.Length == expected.Length, "Exactly three explicit links exist.");
        for (int i = 0; i < links.Length; i++)
        {
            Check(links[i].NavigateUri.AbsoluteUri == expected[i], "Exact mailto/HTTPS target " + i + ".");
            var e = new RequestNavigateEventArgs(links[i].NavigateUri, "") { RoutedEvent = Hyperlink.RequestNavigateEvent };
            links[i].RaiseEvent(e);
            Check(e.Handled && Links.Count == i + 1 && Links[i] == expected[i], "Actual routed hyperlink event invokes only the mock target " + i + ".");
        }
        foreach (string unsafeTarget in new[] { "file:///C:/Windows/notepad.exe", "http://github.com/ASQAL90", "https://example.invalid/", "mailto:other@example.invalid" })
        {
            var e = new RequestNavigateEventArgs(new Uri(unsafeTarget), "") { RoutedEvent = Hyperlink.RequestNavigateEvent };
            links[0].RaiseEvent(e);
            Check(e.Handled && Links.Count == 3, "Reject non-allowlisted routed link: " + unsafeTarget);
        }
        var source = PresentationSource.FromVisual(window)!;
        var otherKey = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, Key.A) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
        window.RaiseEvent(otherKey);
        Check(!otherKey.Handled && window.IsVisible, "A non-Escape routed key leaves About open.");
        var escape = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, Key.Escape) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
        window.RaiseEvent(escape); Pump();
        Check(escape.Handled && !window.IsVisible, "Actual synthetic Escape event closes only the owned About window; no input injection.");
        Window error = CreateAbout(_ => throw new System.ComponentModel.Win32Exception("Synthetic launcher failure"));
        error.Show(); Pump();
        Hyperlink errorLink = Descendants(error).OfType<TextBlock>().SelectMany(t => t.Inlines.OfType<Hyperlink>()).First();
        errorLink.RaiseEvent(new RequestNavigateEventArgs(errorLink.NavigateUri, "") { RoutedEvent = Hyperlink.RequestNavigateEvent });
        Check(ById<TextBlock>(error, "AboutLinkStatus").IsVisible && !string.IsNullOrWhiteSpace(ById<TextBlock>(error, "AboutLinkStatus").Text),
            "Unavailable link handler reports inline failure without external logging or app shutdown.");
        error.Close(); Pump();
    }

    private static T ById<T>(DependencyObject root, string id) where T : DependencyObject =>
        Descendants(root).OfType<T>().Single(item => AutomationProperties.GetAutomationId(item) == id);
    // TextBlock.Text/TextRange omit nested hyperlink text on this runtime;
    // read the actual realized Run tree rather than treating an empty range as copy.
    private static string InlineText(IEnumerable<Inline> inlines) => string.Concat(inlines.Select(inline =>
        inline is Run run ? run.Text : inline is Span span ? InlineText(span.Inlines) : inline is LineBreak ? "\n" : ""));
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            foreach (var child in Descendants(VisualTreeHelper.GetChild(root, i))) yield return child;
    }
    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }
    private static double Contrast(Color first, Color second)
    {
        static double Linear(byte value) { double c = value / 255d; return c <= .04045 ? c / 12.92 : Math.Pow((c + .055) / 1.055, 2.4); }
        static double L(Color c) => .2126 * Linear(c.R) + .7152 * Linear(c.G) + .0722 * Linear(c.B);
        double a = L(first), b = L(second); return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
    }
    private static void Capture(FrameworkElement element, string path)
    {
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen()) dc.DrawRectangle(new VisualBrush(element), null, new Rect(0, 0, element.ActualWidth, element.ActualHeight));
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(element.ActualWidth), (int)Math.Ceiling(element.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
        Check(pixels.Where((_, i) => i % 4 == 3).Any(a => a > 0), "Actual offscreen WPF raster populated: " + Path.GetFileName(path));
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); png.Save(stream);
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("ASSERTION FAILED: " + message);
        Checks.Add(message);
    }
}
