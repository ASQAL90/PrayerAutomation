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

internal static class UiMinorSmoke
{
    private const BindingFlags I = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags S = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    private static readonly string[] SafetyFlags = ["RemindersEnabled", "AutoLockEnabled", "LockScreenEnabled", "LockScreenOptIn", "StartupEnabled"];

    public static void Run(Application application)
    {
        RunTop(application);
        RunSelection(application);
        RunSize(application);
    }

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
                double expectedLeft = normalized switch
                {
                    "tl" or "bl" => area.Left + 12,
                    "tr" or "br" => area.Right - actual.Width - 12,
                    _ => area.Left + (area.Width - actual.Width) / 2
                };
                double expectedTop = normalized is "bl" or "br" ? area.Bottom - actual.Height - 12 : area.Top;
                Console.WriteLine($"UI_MINOR_TOP mode={mode} motion={motion} requested={position} normalized={normalized} actual={actual.Left:0.###},{actual.Top:0.###},{actual.ActualWidth:0.###},{actual.ActualHeight:0.###} expected={expectedLeft:0.###},{expectedTop:0.###} workarea={area}");
                Assert(ReferenceEquals(actual, fixture.Widget) && actual.IsVisible && actual.ActualWidth > 0 && actual.ActualHeight > 0,
                    "The placement test must use the same visible realized production widget.");
                Assert(Math.Abs(actual.Left - expectedLeft) < 0.1,
                    $"Horizontal placement must preserve centering and side margins: {mode}/{position}.");
                if (Math.Abs(actual.Top - expectedTop) >= 0.1)
                    failures.Add($"UI_MINOR_TOP_FAILURE {mode}/{motion}/{position}: actual Top={actual.Top:0.###}, expected work-area edge/bottom margin={expectedTop:0.###}.");
                Assert(actual.Width <= area.Width - 24 + 0.1 && actual.Height <= area.Height - 24 + 0.1,
                    "Top alignment must preserve work-area dimension clamps.");
                Assert(JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == saved,
                    "Placement preview must not change persisted/scheduler settings.");
                checks++;
            }
            // A fresh placement restores the top edge without rewriting manual drag wiring.
            actual.Top = area.Top + 23;
            fixture.Select("WidgetModeBox", "minimalist");
            Pump(application);
            if (Math.Abs(actual.Top - area.Top) >= 0.1)
                failures.Add($"UI_MINOR_TOP_FAILURE preview after manual offset: actual={actual.Top}, expected={area.Top}.");
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
                foreach (string name in new[] { "LocationNameBox", "CorrectionFajrBox", "CalendarDateCorrectionBox", "HijriDateCorrectionBox" })
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

    private sealed record PixelSample(RenderTargetBitmap Bitmap, byte[] Pixels);

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

    private static void SavePng(RenderTargetBitmap bitmap, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream file = File.Create(path);
        encoder.Save(file);
    }

    private static bool Close(Color actual, Color expected, int tolerance) => actual.A >= 250 &&
        Math.Abs(actual.R - expected.R) <= tolerance && Math.Abs(actual.G - expected.G) <= tolerance && Math.Abs(actual.B - expected.B) <= tolerance;

    private static double Contrast(Color first, Color second)
    {
        static double Luminance(Color color)
        {
            static double Linear(byte channel) { double value = channel / 255d; return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4); }
            return 0.2126 * Linear(color.R) + 0.7152 * Linear(color.G) + 0.0722 * Linear(color.B);
        }
        double a = Luminance(first), b = Luminance(second);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

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
            string[] dates = ["CalendarDateCorrection", "HijriDateCorrection"];
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
            VerifyBoundedEditor(application, fixture, "CalendarDateCorrection", 7);
            VerifyBoundedEditor(application, fixture, "HijriDateCorrection", 7);
            VerifyBoundedEditor(application, fixture, "CorrectionFajr", 30);
            fixture.InvokeMain("RefreshSettingsSchedulePreview");
            Pump(application);
            string preview = PreviewText(fixture);
            Assert(preview.Length > 0, "The real native-pure prayer preview must be realized before checking calendar-only corrections.");
            fixture.Field<TextBox>("CalendarDateCorrectionBox").Text = "7";
            fixture.Field<TextBox>("HijriDateCorrectionBox").Text = "-7";
            fixture.InvokeMain("RefreshSettingsSchedulePreview");
            Pump(application);
            Assert(PreviewText(fixture) == preview, "Calendar-day drafts must not change the underlying local-date prayer preview.");
            Assert(JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == saved,
                "Date/minute helper edits must remain unsaved until Save.");
            fixture.InvokeMain("CancelSettings_Click", fixture.MainWindow, new RoutedEventArgs());
            Pump(application);
            Assert(fixture.Field<TextBox>("CalendarDateCorrectionBox").Text == "0" && fixture.Field<TextBox>("HijriDateCorrectionBox").Text == "0" &&
                   JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == saved,
                "Cancel must restore date correction defaults and keep saved settings unchanged.");
            fixture.Field<TextBox>("CalendarDateCorrectionBox").Text = "1";
            fixture.Field<TextBox>("HijriDateCorrectionBox").Text = "-1";
            fixture.Field<TextBox>("CorrectionFajrBox").Text = "1";
            fixture.InvokeMain("SaveSettings_Click", fixture.MainWindow, new RoutedEventArgs());
            Pump(application);
            Assert(fixture.SavePath is not null && File.Exists(fixture.SavePath), "The real Save handler must write through the isolated scratch saver.");
            object restored = JsonSerializer.Deserialize(File.ReadAllText(fixture.SavePath!), fixture.SettingsType)!;
            foreach (string name in SafetyFlags)
                Assert(fixture.SettingsType.GetProperty(name, I)!.GetValue(restored) is false,
                    "Fail closed: saved/deserialized fixture consequential flags must stay false.");
            Assert((int)fixture.SettingsType.GetProperty("CalendarDateCorrectionDays", I)!.GetValue(restored)! == 1 &&
                   (int)fixture.SettingsType.GetProperty("HijriDateCorrectionDays", I)!.GetValue(restored)! == -1 &&
                   fixture.Field<TabControl>("MainTabs").SelectedIndex == 2 && !fixture.StartupValue,
                "Save must preserve the existing date values, safe startup choice and selected settings tab.");
            Console.WriteLine("UI_MINOR_SIZE_SAVE_READBACK " + fixture.SavePath);
        }
        Pump(application);
        Console.WriteLine("UI_MINOR_SIZE_IMAGES " + output);
        Assert(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Console.WriteLine($"UI_MINOR_SIZE PASS: date cases={cases}; all seven prayer boxes unchanged; eight themes at normal/minimum size, compact reachable groups, date +/-7/prayer +/-30, real helpers/input/paste/preview/Cancel/Save preserved.");
    }

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

    private static bool InputBlocked(TextBox box, string text)
    {
        var input = new TextCompositionEventArgs(Keyboard.PrimaryDevice, new TextComposition(InputManager.Current, box, text))
        { RoutedEvent = TextCompositionManager.PreviewTextInputEvent };
        box.RaiseEvent(input);
        return input.Handled;
    }

    private static Rect Bounds(FrameworkElement element, Visual ancestor) => element.TransformToAncestor(ancestor).TransformBounds(new Rect(element.RenderSize));

    private static T Ancestor<T>(DependencyObject element) where T : DependencyObject
    {
        DependencyObject? node = element;
        while ((node = VisualTreeHelper.GetParent(node)) is not null)
            if (node is T target) return target;
        throw new InvalidOperationException("The real control is missing its expected viewport ancestor.");
    }

    private static string PreviewText(Fixture fixture)
    {
        static IEnumerable<string> Text(DependencyObject node)
        {
            if (node is TextBlock block) yield return block.Text;
            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
                foreach (string text in Text(VisualTreeHelper.GetChild(node, index))) yield return text;
        }
        return string.Join("|", Text(fixture.Field<Grid>("SettingsSchedulePreviewGrid")));
    }

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

    private static void Pump(Application application)
    {
        Assert(SynchronizationContext.Current is DispatcherSynchronizationContext,
            "Fail closed: actual UI handlers require a DispatcherSynchronizationContext.");
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
        WpfApplicationIsolation.Verify(application);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
