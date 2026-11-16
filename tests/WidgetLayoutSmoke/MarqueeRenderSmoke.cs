extern alias ProductUi;

using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

internal static class MarqueeRenderSmoke
{
    private const BindingFlags I = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public static void Run()
    {
        Assembly assembly = typeof(ProductUi::PrayerAutomation.UI.App).Assembly;
        Type settingsType = assembly.GetType("PrayerAutomation.UI.AppSettings", true)!;
        Type widgetType = assembly.GetType("PrayerAutomation.UI.ScheduleWidgetWindow", true)!;
        object settings = JsonSerializer.Deserialize("""
            {"widget_mode":"essential","widget_motion":"marquee","widget_position":"tc",
             "enabled":false,"auto_lock_enabled":false,"lock_enabled":false,"lock_screen_opt_in":false,"startup":false}
            """, settingsType)!;
        foreach (string name in new[] { "RemindersEnabled", "AutoLockEnabled", "LockScreenEnabled", "LockScreenOptIn", "StartupEnabled" })
            Assert(settingsType.GetProperty(name, I)!.GetValue(settings) is false,
                $"Synthetic marquee settings must disable {name} before constructing any WPF window.");
        Console.WriteLine("MARQUEE_SETTINGS_GUARD PASS: all consequential flags are false before WPF window construction.");
        var window = (Window)Activator.CreateInstance(widgetType, I, null, [settings], null)!;
        window.Topmost = false;
        window.ShowActivated = false;
        window.Opacity = 0;
        window.IsHitTestVisible = false;
        try
        {
            window.Show();
            window.UpdateLayout();
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Loaded, new Action(window.UpdateLayout));
            foreach (string name in new[] { "_clockTimer", "_marqueeTimer", "_marqueeStartTimer" })
                ((DispatcherTimer)widgetType.GetField(name, I)!.GetValue(window)!).Stop();
            widgetType.GetField("_marqueeAnimationReady", I)!.SetValue(window, false);
            var viewport = (Grid)widgetType.GetField("_marqueeHost", I)!.GetValue(window)!;
            // Keep the fixture's local render origin stable when narrowing it.
            viewport.HorizontalAlignment = HorizontalAlignment.Left;
            var line = (TextBlock)widgetType.GetField("_marqueeLine", I)!.GetValue(window)!;
            var frame = (Border)widgetType.GetField("_frame", I)!.GetValue(window)!;
            Console.WriteLine($"WIDGET_SLIM height={window.Height:0.#} paddingTop={frame.Padding.Top:0.#} paddingBottom={frame.Padding.Bottom:0.#} viewport={viewport.ActualHeight:0.#} text={line.ActualHeight:0.#}");
            Assert(window.Height <= 40 && frame.Padding.Top <= 5 && frame.Padding.Bottom <= 5,
                "Essential one-line widget must be no taller than 40 DIP with at most 5 DIP vertical frame padding.");
            Assert(viewport.ActualHeight >= line.ActualHeight && viewport.ActualHeight >= 24,
                "Slimming must not clip the existing marquee text or reduce its viewport below 24 DIP.");
            Button hide = Descendants(frame).OfType<Button>().Single();
            Rect buttonBounds = hide.TransformToAncestor(frame).TransformBounds(new Rect(hide.RenderSize));
            Assert(hide.ActualHeight >= 28 && buttonBounds.Top >= 1 && buttonBounds.Bottom <= frame.ActualHeight - 1,
                "The 28-DIP hide button must remain fully inside the slim widget frame.");
            string productionContent = (string)widgetType.GetField("_marqueeContent", I)!.GetValue(window)!;
            Assert(productionContent.Contains("Masehi:", StringComparison.Ordinal), "Render fixture must contain a real offline production schedule.");
            double fullWidth = viewport.ActualWidth;
            var samples = new List<Sample>();
            CaptureCases("production", productionContent, fullWidth);
            CaptureCases("short", "Teks singkat", fullWidth);
            string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrayerAutomationSmoke", "scratch");
            string evidence = Path.Combine(root, "marquee-render-" + Guid.NewGuid().ToString("N") + ".json");
            File.WriteAllText(evidence, JsonSerializer.Serialize(samples, new JsonSerializerOptions { WriteIndented = true }));
            Sample[] empty = samples.Where(sample => sample.InkPixels == 0).ToArray();
            foreach (Sample sample in samples)
                Console.WriteLine($"MARQUEE_RENDER {sample.Content} width={sample.ViewportWidth:0.#} dpi={sample.RenderDpi} phase={sample.Phase:0.000} " +
                    $"ink={sample.InkPixels} rightQuarter={sample.RightQuarterInkPixels} line={sample.LineWidth:0.#} layoutSlot={sample.LayoutSlotWidth:0.#} clip={sample.ClipWidth:0.#}");
            Console.WriteLine("MARQUEE_RENDER_EVIDENCE " + evidence);
            Assert(samples.Where(sample => sample.Phase == 0).All(sample => sample.InkPixels > 0),
                "The render harness must detect visible text at the start of every case, not an opacity/background artifact.");
            Assert(empty.Length == 0,
                $"Marquee viewport must never become empty while the repeated text scrolls ({empty.Length}/{samples.Count} sampled frames are blank).");
            Assert(samples.All(sample => sample.RightQuarterInkPixels > 0),
                "Repeated marquee text must remain visible at the right side before, at, and after cycle wrap.");
            Console.WriteLine($"MARQUEE_RENDER PASS: frames={samples.Count} full/narrow short/production dpi=96/144 empty=0 right-side-entry verified.");

            void CaptureCases(string name, string text, double normalWidth)
            {
                foreach (double width in new[] { normalWidth, 180d })
                {
                    viewport.Width = width;
                    widgetType.GetField("_marqueeContent", I)!.SetValue(window, text);
                    widgetType.GetField("_marqueeConfiguredContent", I)!.SetValue(window, string.Empty);
                    widgetType.GetField("_marqueeOffset", I)!.SetValue(window, 0d);
                    window.UpdateLayout();
                    widgetType.GetMethod("ConfigureMarquee", I)!.Invoke(window, [true]);
                    window.UpdateLayout();
                    double cycleWidth = (double)widgetType.GetField("_marqueeCycleWidth", I)!.GetValue(window)!;
                    double[] offsets = [0, cycleWidth * 0.25, cycleWidth * 0.5, cycleWidth * 0.75,
                        Math.Max(0, cycleWidth - 1.25), Math.Max(0, cycleWidth - 0.1), cycleWidth, cycleWidth + 1.25];
                    foreach (double offset in offsets)
                    {
                        widgetType.GetField("_marqueeOffset", I)!.SetValue(window, offset % cycleWidth);
                        ((TranslateTransform)line.RenderTransform).X = -(offset % cycleWidth);
                        foreach (int dpi in new[] { 96, 144 })
                        {
                            (int ink, int rightInk) = RenderInk(viewport, dpi);
                            samples.Add(new Sample(name, viewport.ActualWidth, dpi, offset / cycleWidth, ink, rightInk,
                                line.ActualWidth, LayoutInformation.GetLayoutSlot(line).Width,
                                VisualTreeHelper.GetClip(line)?.Bounds.Width));
                        }
                    }
                }
            }
        }
        finally { window.Close(); }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject node)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(node, i);
            yield return child;
            foreach (DependencyObject descendant in Descendants(child)) yield return descendant;
        }
    }

    private static (int Ink, int RightInk) RenderInk(FrameworkElement viewport, int dpi)
    {
        var rect = new Rect(0, 0, viewport.ActualWidth, viewport.ActualHeight);
        var brush = new VisualBrush(viewport)
        {
            AutoLayoutContent = false,
            ViewboxUnits = BrushMappingMode.Absolute,
            Viewbox = rect,
            ViewportUnits = BrushMappingMode.Absolute,
            Viewport = rect,
            Stretch = Stretch.None,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top
        };
        var drawing = new DrawingVisual();
        using (DrawingContext context = drawing.RenderOpen()) context.DrawRectangle(brush, null, rect);
        int width = (int)Math.Ceiling(rect.Width * dpi / 96);
        int height = (int)Math.Ceiling(rect.Height * dpi / 96);
        var bitmap = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);
        bitmap.Render(drawing);
        int stride = width * 4;
        byte[] pixels = new byte[stride * height];
        bitmap.CopyPixels(pixels, stride, 0);
        int ink = 0, rightInk = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (pixels[y * stride + x * 4 + 3] >= 32)
                {
                    ink++;
                    if (x >= width * 3 / 4) rightInk++;
                }
        return (ink, rightInk);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed record Sample(string Content, double ViewportWidth, int RenderDpi, double Phase,
        int InkPixels, int RightQuarterInkPixels, double LineWidth, double LayoutSlotWidth, double? ClipWidth);
}
