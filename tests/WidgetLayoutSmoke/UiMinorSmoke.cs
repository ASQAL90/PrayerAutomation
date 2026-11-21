extern alias ProductUi;

using System.Globalization;
using System.IO;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Fixture = WidgetSettingsInteractionSmoke.Fixture;

/// <summary>Smoke test verifying minor UI surfaces: widget placement, text selection rendering, and numeric editor sizing.</summary>
internal static class UiMinorSmoke
{
    /// <summary>Binding flags for instance members.</summary>
    private const BindingFlags I = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    /// <summary>Binding flags for static members.</summary>
    private const BindingFlags S = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    /// <summary>Consequential settings flags forced false for the synthetic fixture.</summary>
    private static readonly string[] SafetyFlags = ["RemindersEnabled", "AutoLockEnabled", "LockScreenEnabled", "LockScreenOptIn", "StartupEnabled"];
    /// <summary>Margin passed by the production widget to WidgetSettingsLogic.PlaceWidget.</summary>
    private const double WidgetMargin = 4;

    /// <summary>Runs all UI minor smoke phases in sequence.</summary>
    /// <param name="application">Application used to host the isolated fixtures.</param>
    public static void Run(Application application)
    {
        RunTop(application);
        RunSelection(application);
        RunSize(application);
    }

    /// <summary>Verifies widget placement at the top edge of the work area across modes, motions, and positions.</summary>
    /// <param name="application">Application used to host the isolated fixture.</param>
    public static void RunTop(Application application)
    {
        VerifySyntheticSettings();
        Assembly assembly = typeof(ProductUi::PrayerAutomation.UI.App).Assembly;
        Console.WriteLine($"UI_MINOR_ASSEMBLY production={assembly.FullName} path={assembly.Location} linked={typeof(PrayerAutomation.UI.WidgetSettingsLogic).Assembly.FullName}");
        var failures = new List<string>();
        int checks = 0;
        using (var fixture = new Fixture(application, preventActivation: true))
        {
            string saved = JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType);
            Window actual = fixture.Widget;
            Rect area = SystemParameters.WorkArea;
            // DpiScaleX is already the scale relative to 96 DPI (1.25 on this PC).
            double physicalScale = VisualTreeHelper.GetDpi(actual).DpiScaleX;
            double SnapToPhysicalPixel(double value) =>
                Math.Abs(physicalScale - 1d) < 0.001 ? value : Math.Round(value * physicalScale) / physicalScale;
            foreach (string mode in new[] { "minimalist", "essential", "detail" })
            foreach (string motion in mode == "essential" ? new[] { "static", "marquee" } : new[] { "static" })
            foreach (string position in new[] { "tc", "tl", "tr", "bl", "br" })
            {
                fixture.Select("WidgetModeBox", mode);
                fixture.Select("WidgetMotionBox", motion);
                fixture.Select("WidgetPositionBox", position);
                Pump(application);
                actual.UpdateLayout();
                string normalized = position;
                // Production computes tr/br anchors (and bl/br bottom) from Width/Height before
                // layout, while WPF re-centers tc against the realized ActualWidth. WPF then snaps
                // the final DIP value to a physical pixel, so at 125% DPI 1062 DIP becomes 1062.4
                // DIP (1327.5 -> 1328 px). Snap the expectation the same way instead of widening
                // the assertion tolerance.
                double expectedLeft = SnapToPhysicalPixel(normalized switch
                {
                    "tl" or "bl" => area.Left + WidgetMargin,
                    "tr" or "br" => area.Right - actual.Width - WidgetMargin,
                    _ => area.Left + (area.Width - actual.ActualWidth) / 2
                });
                double expectedTop = SnapToPhysicalPixel(
                    normalized is "bl" or "br" ? area.Bottom - actual.Height - WidgetMargin : area.Top);
                Console.WriteLine($"UI_MINOR_TOP mode={mode} motion={motion} requested={position} normalized={normalized} actual={actual.Left:0.###},{actual.Top:0.###},{actual.ActualWidth:0.###},{actual.ActualHeight:0.###} expected={expectedLeft:0.###},{expectedTop:0.###} workarea={area}");
                Assert(ReferenceEquals(actual, fixture.Widget) && actual.IsVisible && actual.ActualWidth > 0 && actual.ActualHeight > 0,
                    "The placement test must use the same visible realized production widget.");
                Assert(Math.Abs(actual.Left - expectedLeft) < 0.1,
                    $"Horizontal placement must preserve centering and side margins: {mode}/{position}.");
                if (Math.Abs(actual.Top - expectedTop) >= 0.1)
                    failures.Add($"UI_MINOR_TOP_FAILURE {mode}/{motion}/{position}: actual Top={actual.Top:0.###}, expected work-area edge/bottom margin={expectedTop:0.###}.");
                Assert(actual.ActualWidth <= area.Width - 2 * WidgetMargin + 0.1 &&
                       actual.ActualHeight <= area.Height - 2 * WidgetMargin + 0.1,
                    "Top alignment must preserve work-area dimension clamps.");
                Assert(JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == saved,
                    "Placement preview must not change persisted/scheduler settings.");
                checks++;
            }
            // A fresh placement restores the top edge without rewriting manual drag wiring.
            actual.Top = area.Top + 23;
            fixture.Select("WidgetModeBox", "minimalist");
            Pump(application);
            if (Math.Abs(actual.Top - SnapToPhysicalPixel(area.Top)) >= 0.1)
                failures.Add($"UI_MINOR_TOP_FAILURE preview after manual offset: actual={actual.Top}, expected={SnapToPhysicalPixel(area.Top)}.");
            fixture.InvokeMain("CancelSettings_Click", fixture.MainWindow, new RoutedEventArgs());
            Pump(application);
            Assert(fixture.WidgetMode == "essential" && JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == saved,
                "Cancel must preserve saved mode and settings after placement preview.");
        }
        Pump(application);
        checks += VerifyPurePlacement(assembly, failures);
        Assert(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Console.WriteLine($"UI_MINOR_TOP PASS: actual+pure checks={checks}; all top modes at WorkArea.Top, horizontal/bottom margins/clamps/preview/Cancel retained.");
    }

    /// <summary>Verifies the pure PlaceWidget logic across modes, positions, margins, and invalid inputs.</summary>
    /// <param name="assembly">Product UI assembly containing WidgetSettingsLogic.</param>
    /// <param name="failures">Collected failure messages.</param>
    /// <returns>Number of pure placement checks performed.</returns>
    private static int VerifyPurePlacement(Assembly assembly, List<string> failures)
    {
        Type helper = assembly.GetType("PrayerAutomation.UI.WidgetSettingsLogic", true)!;
        MethodInfo place = helper.GetMethod("PlaceWidget", S)!;
        int checks = 0;
        foreach (double margin in new[] { 12d, 0d, -3d })
        foreach (string mode in new[] { "detail", "essential", "minimalist", "minimal", "normal", "unknown" })
        foreach (string position in new[] { "tc", "tl", "tr", "bl", "br", "unknown" })
        {
            const double areaLeft = -800, areaTop = 48, areaWidth = 300, areaHeight = 220;
            object placement = place.Invoke(null, [mode, position, areaLeft, areaTop, areaWidth, areaHeight, 420d, 360d, margin])!;
            /// <summary>Gets a double property from the placement result.</summary>
            /// <param name="name">Property name.</param>
            /// <returns>The property value.</returns>
            double Value(string name) => (double)placement.GetType().GetProperty(name)!.GetValue(placement)!;
            double safeMargin = Math.Max(0, margin);
            double width = areaWidth - 2 * safeMargin, height = areaHeight - 2 * safeMargin;
            string normalized = (string)helper.GetMethod("NormalizePosition", S)!.Invoke(null, [position])!;
            double expectedLeft = normalized switch
            {
                "tl" or "bl" => areaLeft + safeMargin,
                "tr" or "br" => areaLeft + areaWidth - width - safeMargin,
                _ => areaLeft + (areaWidth - width) / 2
            };
            double expectedTop = normalized is "bl" or "br" ? areaTop + areaHeight - height - safeMargin : areaTop;
            Assert(Value("Width") == width && Value("Height") == height && Value("Left") == expectedLeft,
                "Production placement must retain finite bounds, dimension clamping and horizontal semantics.");
            if (Value("Top") != expectedTop)
                failures.Add($"UI_MINOR_TOP_FAILURE pure {mode}/{position}/margin={margin}: actual={Value("Top")}, expected={expectedTop}.");
            checks++;
        }
        try
        {
            place.Invoke(null, ["detail", "tc", 0d, double.NaN, 300d, 220d, 420d, 360d, 12d]);
            throw new InvalidOperationException("Non-finite work-area input must remain rejected.");
        }
        catch (TargetInvocationException exception) when (exception.InnerException is ArgumentOutOfRangeException) { checks++; }
        return checks;
    }

    /// <summary>Verifies selected-text rendering, focus, clipboard, and disabled-state behavior across themes.</summary>
    /// <param name="application">Application used to host the isolated fixture.</param>
    public static void RunSelection(Application application)
    {
        VerifySyntheticSettings();
        Console.WriteLine($"WPF_SELECTION_FRAMEWORK {typeof(FrameworkCompatibilityPreferences).Assembly.FullName} {typeof(FrameworkCompatibilityPreferences).Assembly.Location}");
        foreach (PropertyInfo property in typeof(FrameworkCompatibilityPreferences).GetProperties(S)
            .Where(property => property.Name.Contains("Selection", StringComparison.OrdinalIgnoreCase)))
            Console.WriteLine($"WPF_SELECTION_COMPATIBILITY {property.Name}={property.GetValue(null)}");
        foreach (Type type in typeof(FrameworkCompatibilityPreferences).Assembly.GetTypes().Where(type => type.Name.Contains("AppContext", StringComparison.OrdinalIgnoreCase)))
        {
            foreach (PropertyInfo property in type.GetProperties(S).Where(property => property.Name.Contains("Selection", StringComparison.OrdinalIgnoreCase)))
                Console.WriteLine($"WPF_SELECTION_SWITCH {type.FullName}.{property.Name}={property.GetValue(null)}");
            foreach (FieldInfo field in type.GetFields(S).Where(field => field.Name.Contains("Selection", StringComparison.OrdinalIgnoreCase)))
                Console.WriteLine($"WPF_SELECTION_SWITCH_FIELD {type.FullName}.{field.Name}={field.GetValue(null)}");
        }
        int cases = 0;
        var failures = new List<string>();
        string output = Path.Combine(Environment.GetEnvironmentVariable("SMOKE_SCRATCH")!, "selection-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);
        using (var fixture = new Fixture(application, isolateSave: true))
        {
            string saved = JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType);
            string[] themes = fixture.Field<ComboBox>("ThemeBox").Items.OfType<ComboBoxItem>().Select(item => (string)item.Tag).ToArray();
            foreach (string theme in themes)
            {
                fixture.Select("ThemeBox", theme);
                foreach (string name in new[] { "LocationNameBox", "CorrectionFajrBox", "HijriDateCorrectionBox" })
                {
                    TextBox box = fixture.Field<TextBox>(name);
                    string original = box.Text;
                    box.Text = name == "LocationNameBox" ? "HgM888" : "+7";
                    box.BringIntoView();
                    Pump(application);
                    box.ApplyTemplate();
                    box.UpdateLayout();
                    ControlTemplate template = box.Template;
                    double width = box.ActualWidth, height = box.ActualHeight;
                    foreach (string state in new[] { "focused", "inactive", "disabled" })
                    {
                        box.IsEnabled = true;
                        box.IsInactiveSelectionHighlightEnabled = state != "focused";
                        if (state == "focused")
                        {
                            IInputElement? focused = Keyboard.Focus(box);
                            Console.WriteLine($"WPF_SELECTION_FOCUS target={name} returned={ReferenceEquals(focused, box)} keyboard={box.IsKeyboardFocused} active={box.IsSelectionActive}");
                            Assert(ReferenceEquals(focused, box) && box.IsKeyboardFocused && box.IsSelectionActive,
                                "The selected-text fixture must exercise actual WPF keyboard focus, not fake a dependency property.");
                        }
                        else
                        {
                            Keyboard.ClearFocus();
                            if (state == "disabled") box.IsEnabled = false;
                        }
                        box.Select(0, 0);
                        Pump(application);
                        PixelSample unselected = CaptureBox(box);
                        box.SelectAll();
                        Pump(application);
                        Assert(box.SelectionLength == box.Text.Length && box.SelectedText == box.Text,
                            "The real TextBox selection must retain selected data.");
                        PixelSample selected = CaptureBox(box);
                        SavePng(selected.Bitmap, Path.Combine(output, theme + "-" + name + "-" + state + ".png"));
                        Color foreground = ((SolidColorBrush)box.SelectionTextBrush).Color;
                        Color background = ((SolidColorBrush)box.SelectionBrush).Color;
                        double intendedContrast = Contrast(foreground, background);
                        int backgroundPixels = 0, glyphPixels = 0, changedPixels = 0;
                        double bestContrast = 1;
                        Rect first = box.GetRectFromCharacterIndex(0, false);
                        Rect last = box.GetRectFromCharacterIndex(box.Text.Length - 1, true);
                        int minX = Math.Max(0, (int)Math.Floor(first.Left * 2));
                        int maxX = Math.Min(selected.Bitmap.PixelWidth, (int)Math.Ceiling(last.Left * 2));
                        int minY = Math.Max(0, (int)Math.Floor(first.Top * 2));
                        int maxY = Math.Min(selected.Bitmap.PixelHeight, (int)Math.Ceiling(first.Bottom * 2));
                        Assert(maxX > minX && maxY > minY, "The realized selected character range must have a nonzero text-only pixel ROI.");
                        for (int y = minY; y < maxY; y++)
                        for (int x = minX; x < maxX; x++)
                        {
                            int index = (y * selected.Bitmap.PixelWidth + x) * 4;
                            Color pixel = Color.FromArgb(selected.Pixels[index + 3], selected.Pixels[index + 2], selected.Pixels[index + 1], selected.Pixels[index]);
                            if (Close(pixel, background, 3)) backgroundPixels++;
                            if (Close(pixel, foreground, 12) && Contrast(pixel, background) >= 4.5) glyphPixels++;
                            if (selected.Pixels.AsSpan(index, 4).SequenceEqual(unselected.Pixels.AsSpan(index, 4))) continue;
                            changedPixels++;
                            if (Close(pixel, foreground, 60)) bestContrast = Math.Max(bestContrast, Contrast(pixel, background));
                        }
                        // Rendered glyphs, not configured brushes, establish readability.
                        Console.WriteLine($"UI_MINOR_SELECTION theme={theme} box={name} state={state} active={box.IsSelectionActive} opacity={box.SelectionOpacity} size={width:0.###}x{height:0.###} selection-bg={backgroundPixels} glyph-core={glyphPixels} changed={changedPixels} intended-contrast={intendedContrast:0.###} rendered-best={bestContrast:0.###}");
                        Assert(intendedContrast >= 4.5,
                            "The intended selected foreground/background palette must retain >=4.5 contrast.");
                        if (state == "disabled")
                            Assert(!box.IsEnabled && changedPixels == 0 && backgroundPixels == 0,
                                "Disabled TextBoxes must retain native no-highlight rendering, not falsely show an editable selection.");
                        else if (backgroundPixels < 4 || glyphPixels < 4 || changedPixels < 4 || bestContrast < 4.5)
                            failures.Add($"UI_MINOR_SELECTION_FAILURE {theme}/{name}/{state}: rendered background={backgroundPixels}, glyph core={glyphPixels}, changed={changedPixels}, contrast={bestContrast:0.###}.");
                        Assert(ReferenceEquals(box.Template, template) && box.ActualWidth == width && box.ActualHeight == height,
                            "Selection rendering must not replace TextBox templates or change control geometry.");
                        if (state == "focused")
                        {
                            bool copyObserved = false;
                            DataObjectCopyingEventHandler copying = (_, args) =>
                            {
                                Assert((string)args.DataObject.GetData(DataFormats.UnicodeText) == box.SelectedText,
                                    "The real Copy command must expose the exact selected text.");
                                copyObserved = true;
                                args.CancelCommand(); // Observe WPF copy without changing the user's clipboard.
                            };
                            DataObject.AddCopyingHandler(box, copying);
                            try
                            {
                                Assert(ApplicationCommands.Copy.CanExecute(null, box), "Copy must remain available for a focused editable selection.");
                                ApplicationCommands.Copy.Execute(null, box);
                                Assert(copyObserved && box.SelectedText == box.Text, "Copy must preserve the selection and text.");
                            }
                            finally { DataObject.RemoveCopyingHandler(box, copying); }
                        }
                        cases++;
                    }
                    box.IsEnabled = true;
                    box.IsInactiveSelectionHighlightEnabled = false;
                    Keyboard.Focus(box);
                    string edited = name == "LocationNameBox" ? "Edit" : "0";
                    string beforeEdit = box.Text;
                    box.SelectAll();
                    box.SelectedText = edited;
                    Pump(application);
                    Assert(box.Text == edited && box.IsKeyboardFocused,
                        "Replacing a real selected typed/numeric value must remain editable and focused.");
                    box.Undo();
                    Pump(application);
                    Assert(box.Text == beforeEdit, "Undo must restore the selected-text edit without changing control behavior.");
                    box.Text = original;
                }
            }
            fixture.InvokeMain("CancelSettings_Click", fixture.MainWindow, new RoutedEventArgs());
            Pump(application);
            Assert(JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == saved,
                "Selected-text tests must not save or alter settings/scheduler inputs.");
        }
        Pump(application);
        Console.WriteLine("UI_MINOR_SELECTION_IMAGES " + output);
        Assert(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Console.WriteLine($"UI_MINOR_SELECTION PASS: cases={cases}; actual selected glyph/background pixels >=4.5 contrast across eight themes, typed/numeric, focused/inactive; disabled native no-highlight behavior retained.");
    }

    /// <summary>Rasterized selected-text capture: bitmap plus raw pixel buffer.</summary>
    private sealed record PixelSample(RenderTargetBitmap Bitmap, byte[] Pixels);

    /// <summary>Captures the full adorner decorator around a TextBox, cropped to the TextBox bounds.</summary>
    /// <param name="box">TextBox to capture.</param>
    /// <returns>The cropped bitmap and pixel buffer.</returns>
    private static PixelSample CaptureBox(TextBox box)
    {
        DependencyObject? node = box;
        AdornerDecorator? root = null;
        while ((node = VisualTreeHelper.GetParent(node)) is not null)
            if (node is AdornerDecorator decorator) root = decorator;
        Assert(root is not null, "The realized TextBox needs its real adorner decorator for complete selected-text raster capture.");
        const double scale = 2;
        var whole = new RenderTargetBitmap((int)Math.Ceiling(root!.ActualWidth * scale), (int)Math.Ceiling(root.ActualHeight * scale),
            96 * scale, 96 * scale, PixelFormats.Pbgra32);
        whole.Render(root);
        Rect bounds = box.TransformToAncestor(root).TransformBounds(new Rect(box.RenderSize));
        var crop = new Int32Rect((int)Math.Round(bounds.Left * scale), (int)Math.Round(bounds.Top * scale),
            (int)Math.Round(bounds.Width * scale), (int)Math.Round(bounds.Height * scale));
        Assert(crop.X >= 0 && crop.Y >= 0 && crop.X + crop.Width <= whole.PixelWidth && crop.Y + crop.Height <= whole.PixelHeight,
            "The selected TextBox must remain wholly reachable in the real settings viewport.");
        var cropped = new CroppedBitmap(whole, crop);
        byte[] pixels = new byte[crop.Width * crop.Height * 4];
        cropped.CopyPixels(pixels, crop.Width * 4, 0);
        var bitmap = new RenderTargetBitmap(crop.Width, crop.Height, 96 * scale, 96 * scale, PixelFormats.Pbgra32);
        var drawing = new DrawingVisual();
        using (DrawingContext context = drawing.RenderOpen()) context.DrawImage(cropped, new Rect(0, 0, crop.Width / scale, crop.Height / scale));
        bitmap.Render(drawing);
        return new PixelSample(bitmap, pixels);
    }

    /// <summary>Writes a bitmap to a PNG file.</summary>
    /// <param name="bitmap">Bitmap to save.</param>
    /// <param name="path">Destination file path.</param>
    private static void SavePng(RenderTargetBitmap bitmap, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream file = File.Create(path);
        encoder.Save(file);
    }

    /// <summary>Checks whether a pixel is opaque and within tolerance of an expected color.</summary>
    /// <param name="actual">Sampled color.</param>
    /// <param name="expected">Expected color.</param>
    /// <param name="tolerance">Per-channel tolerance.</param>
    /// <returns>True when the pixel matches.</returns>
    private static bool Close(Color actual, Color expected, int tolerance) => actual.A >= 250 &&
        Math.Abs(actual.R - expected.R) <= tolerance && Math.Abs(actual.G - expected.G) <= tolerance && Math.Abs(actual.B - expected.B) <= tolerance;

    /// <summary>Computes the WCAG contrast ratio between two colors.</summary>
    /// <param name="first">First color.</param>
    /// <param name="second">Second color.</param>
    /// <returns>Contrast ratio (>= 1).</returns>
    private static double Contrast(Color first, Color second)
    {
        /// <summary>Computes relative luminance of a color.</summary>
        /// <param name="color">Color to evaluate.</param>
        /// <returns>Relative luminance.</returns>
        static double Luminance(Color color)
        {
            /// <summary>Linearizes one 8-bit channel value.</summary>
            /// <param name="channel">Channel value 0-255.</param>
            /// <returns>Linearized channel.</returns>
            static double Linear(byte channel) { double value = channel / 255d; return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4); }
            return 0.2126 * Linear(color.R) + 0.7152 * Linear(color.G) + 0.0722 * Linear(color.B);
        }
        double a = Luminance(first), b = Luminance(second);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    /// <summary>Verifies numeric date/prayer correction editors retain sizing, bounds, and save/cancel behavior.</summary>
    /// <param name="application">Application used to host the isolated fixture.</param>
    public static void RunSize(Application application)
    {
        VerifySyntheticSettings();
        var failures = new List<string>();
        int cases = 0;
        string output = Path.Combine(Environment.GetEnvironmentVariable("SMOKE_SCRATCH")!, "numeric-size-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);
        using (var fixture = new Fixture(application, isolateSave: true))
        {
            string saved = JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType);
            string[] prayers = ["Fajr", "Sunrise", "Dhuha", "Dhuhr", "Asr", "Maghrib", "Isha"];
            string[] dates = ["HijriDateCorrection"];
            TextBox reference = fixture.Field<TextBox>("CorrectionFajrBox");
            double baselineWidth = reference.ActualWidth, baselineHeight = reference.ActualHeight;
            Assert(baselineWidth > 0 && baselineHeight > 0 && reference.Width == 54 && reference.MinHeight == 28,
                "The comparison must use a realized original prayer-minute TextBox with its established sizing.");
            foreach (string prefix in dates)
                Assert(fixture.Field<TextBox>(prefix + "Box").Text == "0",
                    "Date correction defaults must remain zero before draft editing.");
            string[] themes = fixture.Field<ComboBox>("ThemeBox").Items.OfType<ComboBoxItem>().Select(item => (string)item.Tag).ToArray();
            foreach (string theme in themes)
            foreach ((double width, double height) in new[] { (1180d, 820d), (940d, 650d) })
            {
                fixture.MainWindow.Width = width;
                fixture.MainWindow.Height = height;
                fixture.Select("ThemeBox", theme);
                Pump(application);
                fixture.MainWindow.UpdateLayout();
                foreach (string prayer in prayers)
                {
                    TextBox prayerBox = fixture.Field<TextBox>("Correction" + prayer + "Box");
                    Assert(prayerBox.ActualWidth == baselineWidth && prayerBox.ActualHeight == baselineHeight,
                        $"All seven prayer correction boxes must retain their original realized dimensions: {prayer}/{theme}/{width}.");
                }
                foreach (string prefix in dates)
                {
                    TextBox box = fixture.Field<TextBox>(prefix + "Box");
                    box.BringIntoView();
                    Pump(application);
                    box.UpdateLayout();
                    Grid group = (Grid)VisualTreeHelper.GetParent(box);
                    Button decrease = fixture.Field<Button>(prefix + "DecreaseButton");
                    Button increase = fixture.Field<Button>(prefix + "IncreaseButton");
                    Rect boxBounds = Bounds(box, group), leftBounds = Bounds(decrease, group), rightBounds = Bounds(increase, group);
                    Console.WriteLine($"UI_MINOR_SIZE theme={theme} window={width}x{height} box={prefix} actual={box.ActualWidth:0.###}x{box.ActualHeight:0.###} prayer={baselineWidth:0.###}x{baselineHeight:0.###} min-height={box.MinHeight} group={group.ActualWidth:0.###} left-gap={boxBounds.Left-leftBounds.Right:0.###} right-gap={rightBounds.Left-boxBounds.Right:0.###}");
                    if (Math.Abs(box.ActualWidth - baselineWidth) >= 0.1 || Math.Abs(box.ActualHeight - baselineHeight) >= 0.1)
                        failures.Add($"UI_MINOR_SIZE_FAILURE {theme}/{width}/{prefix}: date={box.ActualWidth:0.###}x{box.ActualHeight:0.###}, prayer={baselineWidth:0.###}x{baselineHeight:0.###}.");
                    if (group.ActualWidth > baselineWidth + 74 || boxBounds.Left - leftBounds.Right > 6 || rightBounds.Left - boxBounds.Right > 6)
                        failures.Add($"UI_MINOR_SIZE_FAILURE compact {theme}/{width}/{prefix}: group={group.ActualWidth:0.###}, gaps={boxBounds.Left-leftBounds.Right:0.###}/{rightBounds.Left-boxBounds.Right:0.###}.");
                    Assert(box.TextAlignment == TextAlignment.Center && box.VerticalContentAlignment == VerticalAlignment.Center,
                        "Date numeric values must retain centered text and content alignment.");
                    Assert(leftBounds.Right <= boxBounds.Left && boxBounds.Right <= rightBounds.Left &&
                           decrease.ActualWidth == 25 && increase.ActualWidth == 25 && decrease.ActualHeight == 28 && increase.ActualHeight == 28,
                        "Date helper buttons must retain original 25x28 geometry and never overlap the TextBox.");
                    Assert(Math.Abs(leftBounds.Top + leftBounds.Height / 2 - boxBounds.Top - boxBounds.Height / 2) <= 1 &&
                           Math.Abs(rightBounds.Top + rightBounds.Height / 2 - boxBounds.Top - boxBounds.Height / 2) <= 1,
                        "The compact numeric group must keep both helpers vertically centered with its TextBox.");
                    ScrollContentPresenter viewport = Ancestor<ScrollContentPresenter>(box);
                    foreach (FrameworkElement element in new FrameworkElement[] { decrease, box, increase })
                    {
                        Rect visible = Bounds(element, viewport);
                        Assert(visible.Left >= -0.1 && visible.Top >= -0.1 && visible.Right <= viewport.ActualWidth + 0.1 && visible.Bottom <= viewport.ActualHeight + 0.1,
                            "Every date control must be reachable, unclipped, by normal scrolling at normal and minimum window sizes.");
                    }
                    SavePng(CaptureBox(box).Bitmap, Path.Combine(output, theme + "-" + width + "-" + prefix + ".png"));
                    cases++;
                }
                Assert(JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == saved,
                    "Theme/layout drafts must not alter saved settings.");
            }
            VerifyBoundedEditor(application, fixture, "HijriDateCorrection", 7);
            VerifyBoundedEditor(application, fixture, "CorrectionFajr", 30);
            fixture.InvokeMain("RefreshSettingsSchedulePreview");
            Pump(application);
            string preview = PreviewText(fixture);
            Assert(preview.Length > 0, "The real native-pure prayer preview must be realized before checking calendar-only corrections.");
            fixture.Field<TextBox>("HijriDateCorrectionBox").Text = "-7";
            fixture.InvokeMain("RefreshSettingsSchedulePreview");
            Pump(application);
            Assert(PreviewText(fixture) == preview, "Calendar-day drafts must not change the underlying local-date prayer preview.");
            Assert(JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == saved,
                "Date/minute helper edits must remain unsaved until Save.");
            fixture.InvokeMain("CancelSettings_Click", fixture.MainWindow, new RoutedEventArgs());
            Pump(application);
            Assert(fixture.Field<TextBox>("HijriDateCorrectionBox").Text == "0" &&
                   JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == saved,
                "Cancel must restore date correction defaults and keep saved settings unchanged.");
            fixture.Field<TextBox>("HijriDateCorrectionBox").Text = "-1";
            fixture.Field<TextBox>("CorrectionFajrBox").Text = "1";
            fixture.InvokeMain("SaveSettings_Click", fixture.MainWindow, new RoutedEventArgs());
            Pump(application);
            Assert(fixture.SavePath is not null && File.Exists(fixture.SavePath), "The real Save handler must write through the isolated scratch saver.");
            object restored = JsonSerializer.Deserialize(File.ReadAllText(fixture.SavePath!), fixture.SettingsType)!;
            foreach (string name in SafetyFlags)
                Assert(fixture.SettingsType.GetProperty(name, I)!.GetValue(restored) is false,
                    "Fail closed: saved/deserialized fixture consequential flags must stay false.");
            Assert((int)fixture.SettingsType.GetProperty("HijriDateCorrectionDays", I)!.GetValue(restored)! == -1 &&
                   fixture.Field<TabControl>("MainTabs").SelectedIndex == 2 && !fixture.StartupValue,
                "Save must preserve the existing date values, safe startup choice and selected settings tab.");
            Console.WriteLine("UI_MINOR_SIZE_SAVE_READBACK " + fixture.SavePath);
        }
        Pump(application);
        Console.WriteLine("UI_MINOR_SIZE_IMAGES " + output);
        Assert(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Console.WriteLine($"UI_MINOR_SIZE PASS: date cases={cases}; all seven prayer boxes unchanged; eight themes at normal/minimum size, compact reachable groups, date +/-7/prayer +/-30, real helpers/input/paste/preview/Cancel/Save preserved.");
    }

    /// <summary>Verifies the bounded numeric editor: helper buttons, preview input, paste, and bound states.</summary>
    /// <param name="application">Application used to pump the dispatcher.</param>
    /// <param name="fixture">Isolated fixture holding the window under test.</param>
    /// <param name="prefix">Control name prefix (e.g. HijriDateCorrection).</param>
    /// <param name="bound">Absolute inclusive bound for the editor.</param>
    private static void VerifyBoundedEditor(Application application, Fixture fixture, string prefix, int bound)
    {
        TextBox box = fixture.Field<TextBox>(prefix + "Box");
        Button decrease = fixture.Field<Button>(prefix + "DecreaseButton"), increase = fixture.Field<Button>(prefix + "IncreaseButton");
        box.Text = "0";
        Assert(decrease.IsEnabled && increase.IsEnabled, "Both original helpers must be enabled at zero.");
        increase.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert(box.Text == "1", "The real plus helper must increment its attached numeric TextBox.");
        decrease.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert(box.Text == "0", "The real minus helper must decrement its attached numeric TextBox.");
        box.SelectAll();
        Assert(!InputBlocked(box, bound.ToString(CultureInfo.InvariantCulture)) && InputBlocked(box, (bound + 1).ToString(CultureInfo.InvariantCulture)) && InputBlocked(box, "x"),
            "Original PreviewTextInput handlers must accept the bound and reject overflow/non-numeric text.");
        var paste = new DataObjectPastingEventArgs(new DataObject(DataFormats.UnicodeText, (bound + 1).ToString(CultureInfo.InvariantCulture)), false, DataFormats.UnicodeText)
        { RoutedEvent = DataObject.PastingEvent };
        box.RaiseEvent(paste);
        Assert(paste.CommandCancelled, "The real date/minute paste handler must reject overflow without touching the clipboard.");
        box.Text = bound.ToString(CultureInfo.InvariantCulture);
        Assert(!increase.IsEnabled && decrease.IsEnabled, "The upper-bound helper must remain disabled.");
        decrease.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert(box.Text == (bound - 1).ToString(CultureInfo.InvariantCulture), "The lower helper must still work at the upper bound.");
        box.Text = (-bound).ToString(CultureInfo.InvariantCulture);
        Assert(!decrease.IsEnabled && increase.IsEnabled, "The lower-bound helper must remain disabled.");
        increase.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert(box.Text == (-bound + 1).ToString(CultureInfo.InvariantCulture), "The upper helper must still work at the lower bound.");
        box.Text = "0";
        Pump(application);
        Console.WriteLine($"UI_MINOR_SIZE_BOUNDS PASS: {prefix} range={-bound}..{bound}, actual helpers/PreviewTextInput/paste.");
    }

    /// <summary>Raises a PreviewTextInput event and reports whether it was handled.</summary>
    /// <param name="box">TextBox receiving the input.</param>
    /// <param name="text">Input text.</param>
    /// <returns>True when the input was blocked.</returns>
    private static bool InputBlocked(TextBox box, string text)
    {
        var input = new TextCompositionEventArgs(Keyboard.PrimaryDevice, new TextComposition(InputManager.Current, box, text))
        { RoutedEvent = TextCompositionManager.PreviewTextInputEvent };
        box.RaiseEvent(input);
        return input.Handled;
    }

    /// <summary>Bounds of an element relative to an ancestor visual.</summary>
    /// <param name="element">Element to measure.</param>
    /// <param name="ancestor">Ancestor visual used as the coordinate origin.</param>
    /// <returns>Element bounds in ancestor coordinates.</returns>
    private static Rect Bounds(FrameworkElement element, Visual ancestor) => element.TransformToAncestor(ancestor).TransformBounds(new Rect(element.RenderSize));

    /// <summary>Finds the nearest ancestor of the requested type or throws.</summary>
    /// <typeparam name="T">Ancestor type.</typeparam>
    /// <param name="element">Element to ascend from.</param>
    /// <returns>The nearest matching ancestor.</returns>
    private static T Ancestor<T>(DependencyObject element) where T : DependencyObject
    {
        DependencyObject? node = element;
        while ((node = VisualTreeHelper.GetParent(node)) is not null)
            if (node is T target) return target;
        throw new InvalidOperationException("The real control is missing its expected viewport ancestor.");
    }

    /// <summary>Collects all TextBlock texts under the settings preview grid, joined by pipes.</summary>
    /// <param name="fixture">Isolated fixture holding the window under test.</param>
    /// <returns>The joined text content.</returns>
    private static string PreviewText(Fixture fixture)
    {
        /// <summary>Enumerates the text of a node and its visual descendants.</summary>
        /// <param name="node">Node to enumerate.</param>
        /// <returns>Text of the node, if any, followed by descendants' texts.</returns>
        static IEnumerable<string> Text(DependencyObject node)
        {
            if (node is TextBlock block) yield return block.Text;
            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
                foreach (string text in Text(VisualTreeHelper.GetChild(node, index))) yield return text;
        }
        return string.Join("|", Text(fixture.Field<Grid>("SettingsSchedulePreviewGrid")));
    }

    /// <summary>Fails closed unless deserialized synthetic consequential flags remain false and widget is enabled.</summary>
    private static void VerifySyntheticSettings()
    {
        Type type = typeof(ProductUi::PrayerAutomation.UI.App).Assembly.GetType("PrayerAutomation.UI.AppSettings", true)!;
        object settings = Activator.CreateInstance(type, nonPublic: true)!;
        foreach (string name in SafetyFlags) type.GetProperty(name, I)!.SetValue(settings, false);
        type.GetProperty("WidgetEnabled", I)!.SetValue(settings, true);
        string json = JsonSerializer.Serialize(settings, type);
        object restored = JsonSerializer.Deserialize(json, type)!;
        foreach (string name in SafetyFlags)
            Assert(type.GetProperty(name, I)!.GetValue(restored) is false,
                $"Fail closed: deserialized synthetic {name} must be false before any test Window.");
        Assert(type.GetProperty("WidgetEnabled", I)!.GetValue(restored) is true,
            "The synthetic widget must be enabled explicitly, independently of consequential flags.");
        Console.WriteLine("UI_MINOR_FIXTURE_FLAGS PASS: serialized/deserialized reminders/auto-lock/lock-screen/consent/startup=false widget=true before Window.");
    }

    /// <summary>Pumps the dispatcher and re-verifies WPF isolation invariants.</summary>
    /// <param name="application">Application to verify.</param>
    private static void Pump(Application application)
    {
        Assert(SynchronizationContext.Current is DispatcherSynchronizationContext,
            "Fail closed: actual UI handlers require a DispatcherSynchronizationContext.");
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
        WpfApplicationIsolation.Verify(application);
    }

    /// <summary>Throws <see cref="InvalidOperationException"/> when the condition does not hold.</summary>
    /// <param name="condition">Condition that must be true.</param>
    /// <param name="message">Failure message used when the condition is false.</param>
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
