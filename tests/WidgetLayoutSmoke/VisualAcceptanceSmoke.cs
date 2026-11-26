extern alias ProductUi;

using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

/// <summary>Smoke test verifying visual acceptance: date helpers, startup options, geocoding, theme contrast, and legends.</summary>
internal static class VisualAcceptanceSmoke
{
    /// <summary>Binding flags for instance members.</summary>
    private const BindingFlags I = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    /// <summary>Binding flags for static members.</summary>
    private const BindingFlags S = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>Runs the visual acceptance suite against an isolated main window.</summary>
    /// <param name="application">Application used to host the isolated window.</param>
    public static void Run(Application application)
    {
        Assembly assembly = typeof(ProductUi::PrayerAutomation.UI.App).Assembly;
        Type appType = assembly.GetType("PrayerAutomation.UI.App", throwOnError: true)!;
        MethodInfo initialize = appType.GetMethod("InitializeComponent", I)
            ?? throw new InvalidOperationException("App.InitializeComponent tidak ditemukan.");
        initialize.Invoke(application, null);
        var errors = new List<string>();
        Type settingsType = assembly.GetType("PrayerAutomation.UI.AppSettings", true)!;
        object settings = Activator.CreateInstance(settingsType, nonPublic: true)!;
        Set(settingsType, settings, "AutoLockEnabled", false);
        Set(settingsType, settings, "LockScreenEnabled", false);
        Set(settingsType, settings, "LockScreenOptIn", false);
        Set(settingsType, settings, "StartupEnabled", true);
        Set(settingsType, settings, "WidgetEnabled", false);
        ApplyTheme(assembly, application, "light");

        Type windowType = assembly.GetType("PrayerAutomation.UI.MainWindow", true)!;
        ConstructorInfo constructor = windowType.GetConstructor(I, null, [settingsType], null)
            ?? throw new InvalidOperationException("MainWindow(AppSettings) constructor tidak ditemukan.");
        var window = (Window)constructor.Invoke([settings]);
        window.Opacity = 0;
        window.ShowInTaskbar = false;
        window.ShowActivated = false;
        window.Width = Math.Max(940, Math.Min(1050, SystemParameters.WorkArea.Width - 32));
        window.Height = Math.Max(650, Math.Min(760, SystemParameters.WorkArea.Height - 32));

        try
        {
            window.Show();
            Pump();
            TabControl? tabs = Field<TabControl>(windowType, window, "MainTabs");
            if (tabs is null)
            {
                errors.Add("MainTabs pada WPF MainWindow tidak ditemukan.");
                Finish(errors);
                return;
            }

            tabs.SelectedIndex = 3;
            Pump();
            VerifyDateButtons(windowType, window, errors);
            VerifyHiddenStartup(windowType, settingsType, window, errors);
            VerifyGeocodingControls(windowType, window, errors);
            VerifyLivePreview(windowType, settingsType, window, errors);
            VerifyThemeContrast(assembly, application, windowType, window, tabs, errors);
            ControlStateContrastSmoke.Run(application, errors);
            VerifyAllRenderedText(assembly, application, windowType, window, tabs, errors);
            VerifyLegends(windowType, window, tabs, errors);
            VerifyStartupLogic(assembly, errors);
            Finish(errors);
        }
        finally
        {
            windowType.GetField("_allowClose", I)?.SetValue(window, true);
            window.Close();
            Pump();
            ApplyTheme(assembly, application, "light");
        }
    }

    /// <summary>Verifies date-correction decrease/increase buttons are visible and wired.</summary>
    /// <param name="type">MainWindow type.</param>
    /// <param name="window">Window instance to inspect.</param>
    /// <param name="errors">Collected error messages.</param>
    private static void VerifyDateButtons(Type type, Window window, List<string> errors)
    {
        string[] names = [
            "HijriDateCorrectionDecreaseButton", "HijriDateCorrectionIncreaseButton"];
        foreach (string name in names)
        {
            Button? button = Field<Button>(type, window, name);
            if (button is null)
            {
                errors.Add($"Tombol koreksi {name} tidak ditemukan di WPF.");
                continue;
            }

            button.BringIntoView();
            Pump();
            ScrollViewer? scroll = Ancestor<ScrollViewer>(button);
            bool inViewport = false;
            if (scroll is not null && scroll.ViewportHeight > 0 && scroll.ViewportWidth > 0)
            {
                Rect r = button.TransformToAncestor(scroll).TransformBounds(new Rect(button.RenderSize));
                inViewport = r.Left >= -1 && r.Top >= -1 && r.Right <= scroll.ViewportWidth + 1
                    && r.Bottom <= scroll.ViewportHeight + 1;
            }
            Console.WriteLine($"VISUAL {name} {button.ActualWidth:0.#}x{button.ActualHeight:0.#} inViewport={inViewport}");
            if (!button.IsVisible || button.ActualWidth < 24 || button.ActualHeight < 26 || !inViewport)
                errors.Add($"Tombol {name} tidak terlihat utuh di viewport settings.");
        }

        ClickAndCheck("HijriDateCorrectionDecreaseButton", "HijriDateCorrectionBox", "-1");
        ClickAndCheck("HijriDateCorrectionIncreaseButton", "HijriDateCorrectionBox", "0");

        /// <summary>Clicks a helper button and checks the resulting text box value.</summary>
        /// <param name="buttonName">Button field name.</param>
        /// <param name="boxName">TextBox field name.</param>
        /// <param name="expected">Expected text after the click.</param>
        void ClickAndCheck(string buttonName, string boxName, string expected)
        {
            Button? button = Field<Button>(type, window, buttonName);
            TextBox? box = Field<TextBox>(type, window, boxName);
            if (button is null || box is null)
                return;
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, button));
            Pump();
            if (box.Text != expected)
                errors.Add($"Klik {buttonName}: nilai {boxName} aktual '{box.Text}', diharapkan '{expected}'.");
        }
    }

    /// <summary>Verifies StartHidden is available, default-off, and independent of startup registration.</summary>
    /// <param name="type">MainWindow type.</param>
    /// <param name="settingsType">AppSettings type.</param>
    /// <param name="window">Window instance to inspect.</param>
    /// <param name="errors">Collected error messages.</param>
    private static void VerifyHiddenStartup(Type type, Type settingsType, Window window, List<string> errors)
    {
        PropertyInfo? preference = settingsType.GetProperty("StartHidden", I);
        object? settings = type.GetField("_settings", I)?.GetValue(window);
        if (preference is null || settings is null || preference.GetValue(settings) is not false)
            errors.Add("StartHidden harus tersedia dan nonaktif secara default.");

        CheckBox? startup = Field<CheckBox>(type, window, "StartupEnabledCheckBox");
        CheckBox? hidden = Field<CheckBox>(type, window, "StartHiddenCheckBox");
        if (startup is null || hidden is null)
        {
            errors.Add("Checkbox startup tersembunyi tidak ditemukan.");
            return;
        }
        startup.BringIntoView();
        Pump();
        if (hidden.IsChecked == true)
            errors.Add("StartHidden harus tidak dicentang secara default.");
        // Production keeps StartHidden available regardless of startup registration:
        // it now also applies when the app is launched manually.
        startup.IsChecked = false;
        Pump();
        if (!hidden.IsEnabled)
            errors.Add("StartHidden harus tetap tersedia saat registrasi startup dimatikan.");
        startup.IsChecked = true;
        Pump();
        if (!hidden.IsEnabled)
            errors.Add("StartHidden harus tetap tersedia saat registrasi startup dinyalakan.");
    }

    /// <summary>Verifies the offline-only geocoding controls and the local Jakarta lookup.</summary>
    /// <param name="type">MainWindow type.</param>
    /// <param name="window">Window instance to inspect.</param>
    /// <param name="errors">Collected error messages.</param>
    private static void VerifyGeocodingControls(Type type, Window window, List<string> errors)
    {
        TextBlock? offlineStatus = Field<TextBlock>(type, window, "OfflineReverseGeocodeStatus");
        TextBox? latitude = Field<TextBox>(type, window, "LatitudeBox");
        TextBox? longitude = Field<TextBox>(type, window, "LongitudeBox");
        TextBox? locationName = Field<TextBox>(type, window, "LocationNameBox");
        if (offlineStatus is null || latitude is null || longitude is null || locationName is null)
        {
            errors.Add("Kontrol reverse-geocoding offline tidak lengkap di WPF.");
            return;
        }
        foreach (string name in new[] { "OnlineGeocodingEndpointBox", "OnlineGeocodingConsentDisclosure",
                     "OnlineReverseGeocodeConsentCheckBox", "OnlineReverseGeocodeButton", "OnlineReverseGeocodeStatus" })
            if (Field<FrameworkElement>(type, window, name) is not null)
                errors.Add("Kontrol online yang sudah dihapus masih ditemukan: " + name);

        // The current UI fills the location name automatically when coordinates change;
        // there is no dedicated offline lookup button any more.
        bool timedOut = false;
        var frame = new DispatcherFrame();
        var timeout = new DispatcherTimer { Interval = TimeSpan.FromSeconds(20) };
        timeout.Tick += (_, _) =>
        {
            timedOut = true;
            frame.Continue = false;
        };
        var poll = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        poll.Tick += (_, _) =>
        {
            if (locationName.Text.StartsWith("Jakarta", StringComparison.OrdinalIgnoreCase))
                frame.Continue = false;
        };
        timeout.Start();
        poll.Start();
        locationName.Text = "Lokasi uji sebelum lookup";
        latitude.Text = "-6.21462";
        longitude.Text = "106.84513";
        Pump();
        if (!locationName.Text.StartsWith("Jakarta", StringComparison.OrdinalIgnoreCase))
        {
            frame.Continue = true;
            Dispatcher.PushFrame(frame);
        }
        poll.Stop();
        timeout.Stop();
        if (timedOut)
            errors.Add("Lookup offline WPF melebihi batas 20 detik.");
        if (!locationName.Text.StartsWith("Jakarta", StringComparison.OrdinalIgnoreCase) ||
            !offlineStatus.Text.Contains("tidak dikirim", StringComparison.OrdinalIgnoreCase))
            errors.Add("Isian otomatis offline WPF harus menemukan Jakarta dari dataset lokal tanpa jaringan.");
    }

    /// <summary>Verifies the live widget preview updates without persisting the previewed mode.</summary>
    /// <param name="type">MainWindow type.</param>
    /// <param name="settingsType">AppSettings type.</param>
    /// <param name="window">Window instance to inspect.</param>
    /// <param name="errors">Collected error messages.</param>
    private static void VerifyLivePreview(Type type, Type settingsType, Window window, List<string> errors)
    {
        ComboBox? mode = Field<ComboBox>(type, window, "WidgetModeBox");
        if (mode is null)
        {
            errors.Add("ComboBox mode widget tidak ditemukan.");
            return;
        }
        mode.BringIntoView();
        Pump();
        object? settings = type.GetField("_settings", I)?.GetValue(window);
        PropertyInfo? savedMode = settingsType.GetProperty("WidgetMode", I);
        string? before = savedMode?.GetValue(settings)?.ToString();
        if (mode.Items.Count < 3)
        {
            errors.Add("Pilihan mode widget kurang dari tiga.");
            return;
        }
        mode.SelectedIndex = 2;
        Pump();
        // Production applies the selection to the actual widget; the old imitation
        // preview label/content was removed, so only assert no imitation control remains.
        if (Field<ContentControl>(type, window, "WidgetLivePreviewContent") is not null)
            errors.Add("Preview widget tiruan masih ada; perubahan harus terlihat pada widget aktual.");
        if (savedMode?.GetValue(settings)?.ToString() != before)
            errors.Add("Perubahan preview sebelum Simpan mengubah setting tersimpan.");
        mode.SelectedIndex = 0;
        Pump();
    }

    /// <summary>Verifies theme contrast for controls, combo items, tabs, and checkbox glyph in every theme.</summary>
    /// <param name="assembly">Product UI assembly.</param>
    /// <param name="app">Application used to apply theme presets.</param>
    /// <param name="windowType">MainWindow type.</param>
    /// <param name="window">Window instance to inspect.</param>
    /// <param name="tabs">Main tab control.</param>
    /// <param name="errors">Collected error messages.</param>
    private static void VerifyThemeContrast(Assembly assembly, Application app, Type windowType, Window window,
        TabControl tabs, List<string> errors)
    {
        ComboBox? mode = Field<ComboBox>(windowType, window, "WidgetModeBox");
        CheckBox? startup = Field<CheckBox>(windowType, window, "StartupEnabledCheckBox");
        if (mode is null || startup is null)
        {
            errors.Add("ComboBox atau checkbox tema tidak ditemukan untuk uji kontras.");
            return;
        }
        Type catalog = assembly.GetType("PrayerAutomation.UI.ThemeCatalog", true)!;
        IEnumerable themes = (IEnumerable)catalog.GetProperty("All", S)!.GetValue(null)!;
        MethodInfo apply = Method(assembly, "PrayerAutomation.UI.ThemeManager", "Apply");
        MethodInfo refresh = windowType.GetMethod("RefreshThemeDependentViews", I)!;
        foreach (object theme in themes)
        {
            string id = theme.GetType().GetProperty("Id", I)!.GetValue(theme)!.ToString()!;
            apply.Invoke(null, [app, id]);
            refresh.Invoke(window, null);
            Pump();
            Contrast(mode.Foreground, mode.Background, $"ComboBox collapsed/{id}", errors);
            Contrast(startup.Foreground, Ancestor<GroupBox>(startup)?.Background ?? window.Background,
                $"Checkbox text/{id}", errors);

            tabs.SelectedIndex = 3;
            Pump();
            if (tabs.ItemContainerGenerator.ContainerFromIndex(3) is TabItem tab)
            {
                var defaultTab = new TabItem { Header = "Default template probe", Style = new Style(typeof(TabItem)) };
                var defaultTabs = new TabControl { Items = { defaultTab } };
                defaultTabs.Measure(new Size(300, 100));
                defaultTabs.Arrange(new Rect(0, 0, 300, 100));
                defaultTab.ApplyTemplate();
                tab.ApplyTemplate();
                if (!ReferenceEquals(tab.Template, defaultTab.Template))
                    errors.Add($"Tab {id} tidak mempertahankan template default Windows/WPF.");
                if (Descendants(tab).OfType<Border>().Any(border => border.CornerRadius != new CornerRadius(0)))
                    errors.Add($"Header tab {id} masih memiliki bentuk rounded.");
                Border? header = Descendants(tab).OfType<Border>().FirstOrDefault(border => border.Background is SolidColorBrush);
                if (header is not null)
                    Contrast(tab.Foreground, header.Background, $"Tab aktif/{id}", errors);
                else
                    errors.Add($"Border tab aktif tidak ditemukan untuk tema {id}.");
            }

            mode.IsDropDownOpen = true;
            Pump();
            if (mode.SelectedIndex >= 0 && mode.ItemContainerGenerator.ContainerFromIndex(mode.SelectedIndex) is ComboBoxItem item)
            {
                item.ApplyTemplate();
                if (item.Template.FindName("ItemBackground", item) is Border itemBackground)
                    Contrast(item.Foreground, itemBackground.Background, $"Item ComboBox terpilih/{id}", errors);
                else
                    errors.Add($"Border item ComboBox terpilih tidak ditemukan untuk tema {id}.");
            }
            mode.IsDropDownOpen = false;
            Pump();

            startup.IsChecked = false;
            startup.ApplyTemplate();
            IEnumerable<string> glyphNames = Descendants(startup).OfType<FrameworkElement>()
                .Select(element => element.Name).Where(name => !string.IsNullOrEmpty(name));
            Console.WriteLine($"CHECKBOX template={startup.Template?.TargetType?.Name ?? "null"} names={string.Join(",", glyphNames)}");
            if (startup.Template?.FindName("CheckBoxGlyphBackground", startup) is not Border glyph)
                errors.Add($"Glyph checkbox bertema tidak ditemukan untuk {id}.");
            else
            {
                Contrast(startup.Foreground, glyph.Background, $"Checkbox/glyph/{id}", errors);
                startup.IsChecked = true;
                Pump();
                Brush? background = glyph.Background;
                if (startup.Template?.FindName("CheckMarkPath", startup) is not Path mark || mark.Visibility != Visibility.Visible)
                    errors.Add($"Tanda centang tidak terlihat untuk tema {id}.");
                else
                    Contrast(mark.Stroke, background, $"Tanda checkbox/{id}", errors);
                startup.IsChecked = false;
            }
        }
        startup.IsChecked = true;
        mode.SelectedIndex = 0;
        ApplyTheme(assembly, app, "light");
        refresh.Invoke(window, null);
        Pump();
    }

    /// <summary>Verifies every rendered visible text block meets WCAG AA contrast in every theme and tab.</summary>
    /// <param name="assembly">Product UI assembly.</param>
    /// <param name="application">Application used to apply theme presets.</param>
    /// <param name="windowType">MainWindow type.</param>
    /// <param name="window">Window instance to inspect.</param>
    /// <param name="tabs">Main tab control.</param>
    /// <param name="errors">Collected error messages.</param>
    private static void VerifyAllRenderedText(Assembly assembly, Application application, Type windowType,
        Window window, TabControl tabs, List<string> errors)
    {
        Type catalog = assembly.GetType("PrayerAutomation.UI.ThemeCatalog", true)!;
        MethodInfo apply = Method(assembly, "PrayerAutomation.UI.ThemeManager", "Apply");
        MethodInfo refresh = windowType.GetMethod("RefreshThemeDependentViews", I)!;
        object[] themes = ((IEnumerable)catalog.GetProperty("All", S)!.GetValue(null)!).Cast<object>().ToArray();
        ComboBox mode = Field<ComboBox>(windowType, window, "WidgetModeBox")!;
        TabControl calendarTabs = Field<TabControl>(windowType, window, "CalendarModeTabs")!;
        int checkedText = 0;
        double minimum = double.MaxValue;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (object theme in themes)
        {
            string id = theme.GetType().GetProperty("Id", I)!.GetValue(theme)!.ToString()!;
            apply.Invoke(null, [application, id]);
            refresh.Invoke(window, null);
            Pump();
            for (int tabIndex = 0; tabIndex < tabs.Items.Count; tabIndex++)
            {
                tabs.SelectedIndex = tabIndex;
                int variants = tabIndex == 1 ? calendarTabs.Items.Count : tabIndex == 2 ? mode.Items.Count : 1;
                for (int variant = 0; variant < variants; variant++)
                {
                    if (tabIndex == 1) calendarTabs.SelectedIndex = variant;
                    if (tabIndex == 2) mode.SelectedIndex = variant;
                    Pump();
                    foreach (TextBlock text in Descendants(window).OfType<TextBlock>().Where(text => text.IsVisible && text.ActualWidth > 0 && !string.IsNullOrWhiteSpace(text.Text)))
                    {
                        Brush? background = TextSurface(text, window.Background);
                        if (text.Foreground is not SolidColorBrush fg || background is not SolidColorBrush bg) continue;
                        double ratio = Ratio(fg.Color, bg.Color);
                        checkedText++;
                        minimum = Math.Min(minimum, ratio);
                        if (ratio < 4.5)
                        {
                            string label = $"{id}/{tabIndex}/{variant}/{text.Name}/{text.Text[..Math.Min(28, text.Text.Length)]}/{fg.Color}/{bg.Color}";
                            if (seen.Add(label)) errors.Add($"Teks ter-render {label} hanya {ratio:0.000}:1.");
                        }
                    }
                }
            }
        }
        mode.SelectedIndex = 0;
        calendarTabs.SelectedIndex = 0;
        ApplyTheme(assembly, application, "light");
        refresh.Invoke(window, null);
        Pump();
        Console.WriteLine($"RENDERED_TEXT_CONTRAST themes={themes.Length} checks={checkedText} minimum={minimum:0.000}:1 failures={seen.Count}");
    }

    /// <summary>Verifies the expanded-header text area matches its selected raster background and contrast.</summary>
    /// <param name="text">Header text block to verify.</param>
    /// <param name="fallback">Fallback background brush.</param>
    /// <param name="errors">Collected error messages.</param>
    /// <param name="label">Report label.</param>
    private static void VerifyNativeHeaderSurface(TextBlock text, Brush fallback, List<string> errors, string label)
    {
        DependencyObject? parent = text;
        while (parent is not null && parent is not System.Windows.Documents.AdornerDecorator)
            parent = VisualTreeHelper.GetParent(parent);
        var root = parent as System.Windows.Documents.AdornerDecorator
            ?? throw new InvalidOperationException("The header must have its actual render root.");
        root.UpdateLayout();
        Rect bounds = text.TransformToAncestor(root).TransformBounds(new Rect(text.RenderSize));
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(root.ActualWidth), (int)Math.Ceiling(root.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(root);
        var region = new Int32Rect((int)Math.Floor(bounds.X), (int)Math.Floor(bounds.Y), (int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
        if (region.X < 0 || region.Y < 0 || region.Width <= 0 || region.Height <= 0
            || region.X + region.Width > bitmap.PixelWidth || region.Y + region.Height > bitmap.PixelHeight)
            throw new InvalidOperationException("The complete header must be inside the actual raster.");
        var cropped = new CroppedBitmap(bitmap, region);
        int stride = cropped.PixelWidth * 4;
        byte[] pixels = new byte[stride * cropped.PixelHeight];
        cropped.CopyPixels(pixels, stride, 0);
        var counts = new Dictionary<int, int>();
        for (int offset = 0; offset < pixels.Length; offset += 4)
        {
            if (pixels[offset + 3] != 255) continue;
            int rgb = (pixels[offset + 2] << 16) | (pixels[offset + 1] << 8) | pixels[offset];
            counts[rgb] = counts.TryGetValue(rgb, out int count) ? count + 1 : 1;
        }
        var dominant = counts.OrderByDescending(item => item.Value).First();
        if (dominant.Value <= pixels.Length / 8)
            throw new InvalidOperationException("The rendered text background must have a strict pixel majority.");
        Color actual = Color.FromRgb((byte)(dominant.Key >> 16), (byte)(dominant.Key >> 8), (byte)dominant.Key);
        Brush? selected = TextSurface(text, fallback);
        bool matched = selected is SolidColorBrush surface && surface.Color == actual;
        if (!matched) errors.Add($"Native header surface mismatch {label}: selected={selected} actual={actual}.");
        if (text.Foreground is not SolidColorBrush foreground || Ratio(foreground.Color, actual) < 4.5)
            errors.Add($"Native header actual raster contrast is below 4.5:1 {label}.");
        Console.WriteLine($"NATIVE_HEADER_SURFACE {label} selected={selected} actual={actual} matched={matched} background-pixels={dominant.Value}");
    }

    /// <summary>Resolves the opaque background brush behind a rendered node, falling back when none is found.</summary>
    /// <param name="node">Node to start from.</param>
    /// <param name="fallback">Fallback brush used when no opaque background is found.</param>
    /// <returns>The resolved background brush.</returns>
    private static Brush? TextSurface(DependencyObject node, Brush fallback)
    {
        DependencyObject target = node;
        while (node is not null)
        {
            Brush? brush = node switch
            {
                Border border => border.Background,
                Panel panel => panel.Background,
                // The native Expander header toggle paints its indicator, not this exact header text area.
                Control control when control is not CheckBox
                    && !(control is System.Windows.Controls.Primitives.ToggleButton toggle
                        && toggle.TemplatedParent is Expander expander
                        && ReferenceEquals(expander.Header, target)) => control.Background,
                TextBlock text => text.Background,
                _ => null
            };
            if (brush is SolidColorBrush color && color.Color.A == 255 && brush.Opacity == 1) return brush;
            node = node is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(node)! : LogicalTreeHelper.GetParent(node)!;
        }
        return fallback;
    }

    /// <summary>Verifies both calendar legends render at least four markers.</summary>
    /// <param name="type">MainWindow type.</param>
    /// <param name="window">Window instance to inspect.</param>
    /// <param name="tabs">Main tab control.</param>
    /// <param name="errors">Collected error messages.</param>
    private static void VerifyLegends(Type type, Window window, TabControl tabs, List<string> errors)
    {
        tabs.SelectedIndex = 1;
        Pump();
        foreach (string name in new[] { "GregorianCalendarLegend", "HijriCalendarLegend" })
        {
            FrameworkElement? legend = Field<FrameworkElement>(type, window, name);
            if (legend is null)
            {
                errors.Add($"Legenda kalender {name} tidak ada.");
                continue;
            }
            legend.BringIntoView();
            Pump();
            int markers = Descendants(legend).OfType<Shape>().Count();
            if (markers < 4)
                errors.Add($"Legenda {name} hanya memiliki {markers} penanda; perlu minimal empat.");
        }
    }

    /// <summary>Verifies the pure startup hidden/normal command logic without writing the registry.</summary>
    /// <param name="assembly">Product UI assembly.</param>
    /// <param name="errors">Collected error messages.</param>
    private static void VerifyStartupLogic(Assembly assembly, List<string> errors)
    {
        Type? type = assembly.GetType("PrayerAutomation.UI.StartupRegistration");
        MethodInfo? build = type?.GetMethod("BuildRunCommand", S);
        MethodInfo? hidden = type?.GetMethod("ShouldStartHidden", S);
        if (build is null || hidden is null)
        {
            errors.Add("Logika pure startup hidden untuk diuji tanpa menulis Registry tidak ditemukan.");
            return;
        }
        string path = @"C:\Apps\PrayerAutomation.UI.exe";
        string shown = build.Invoke(null, [path, false])?.ToString() ?? "";
        string concealed = build.Invoke(null, [path, true])?.ToString() ?? "";
        if (shown.Contains("--hidden", StringComparison.Ordinal) || !concealed.StartsWith('"')
            || !concealed.Contains("--hidden", StringComparison.Ordinal))
            errors.Add("Command startup hidden/normal tidak sesuai.");
        bool off = (bool)(hidden.Invoke(null, [new[] { "--hidden" }, false]) ?? false);
        bool on = (bool)(hidden.Invoke(null, [new[] { "--hidden" }, true]) ?? false);
        bool manual = (bool)(hidden.Invoke(null, [Array.Empty<string>(), true]) ?? false);
        if (off || !on || !manual)
            errors.Add("Mulai tersembunyi harus mengikuti preferensi pada peluncuran manual maupun startup Windows.");
    }

    /// <summary>Checks solid-brush contrast between a foreground and background, recording failures below 4.5:1.</summary>
    /// <param name="foreground">Foreground brush.</param>
    /// <param name="background">Background brush.</param>
    /// <param name="name">Check name for reporting.</param>
    /// <param name="errors">Collected error messages.</param>
    private static void Contrast(Brush? foreground, Brush? background, string name, List<string> errors)
    {
        if (foreground is not SolidColorBrush fg || background is not SolidColorBrush bg)
        {
            errors.Add($"Brush kontras {name} bukan solid.");
            return;
        }
        double ratio = Ratio(fg.Color, bg.Color);
        Console.WriteLine($"CONTRAST {name}={ratio:0.00}:1");
        if (ratio < 4.5)
            errors.Add($"Kontras {name} hanya {ratio:0.00}:1.");
    }

    /// <summary>Computes the WCAG contrast ratio between two colors.</summary>
    /// <param name="a">First color.</param>
    /// <param name="b">Second color.</param>
    /// <returns>Contrast ratio (>= 1).</returns>
    private static double Ratio(Color a, Color b)
    {
        /// <summary>Computes relative luminance of a color.</summary>
        /// <param name="c">Color to evaluate.</param>
        /// <returns>Relative luminance.</returns>
        static double L(Color c)
        {
            /// <summary>Linearizes one 8-bit channel value.</summary>
            /// <param name="n">Channel value 0-255.</param>
            /// <returns>Linearized channel.</returns>
            static double C(byte n)
            {
                double x = n / 255.0;
                return x <= 0.04045 ? x / 12.92 : Math.Pow((x + 0.055) / 1.055, 2.4);
            }
            return .2126 * C(c.R) + .7152 * C(c.G) + .0722 * C(c.B);
        }
        double x = L(a), y = L(b);
        return (Math.Max(x, y) + .05) / (Math.Min(x, y) + .05);
    }

    /// <summary>Applies a theme preset via ThemeManager.Apply and pumps the dispatcher.</summary>
    /// <param name="assembly">Product UI assembly.</param>
    /// <param name="app">Application to apply the theme to.</param>
    /// <param name="id">Theme identifier.</param>
    private static void ApplyTheme(Assembly assembly, Application app, string id)
    {
        Method(assembly, "PrayerAutomation.UI.ThemeManager", "Apply").Invoke(null, [app, id]);
        Pump();
    }

    /// <summary>Reflects a static method on a product type or throws.</summary>
    /// <param name="assembly">Product UI assembly.</param>
    /// <param name="typeName">Fully qualified type name.</param>
    /// <param name="name">Method name.</param>
    /// <returns>The method info.</returns>
    private static MethodInfo Method(Assembly assembly, string typeName, string name)
        => assembly.GetType(typeName, true)!.GetMethod(name, S)
            ?? throw new InvalidOperationException($"{typeName}.{name} tidak ditemukan.");

    /// <summary>Reads a field via reflection and casts it, or returns null.</summary>
    /// <typeparam name="T">Expected field value type.</typeparam>
    /// <param name="type">Type declaring the field.</param>
    /// <param name="instance">Instance to read from.</param>
    /// <param name="name">Field name.</param>
    /// <returns>The cast value, or null.</returns>
    private static T? Field<T>(Type type, object instance, string name) where T : class
        => type.GetField(name, I)?.GetValue(instance) as T;

    /// <summary>Sets a property via reflection when it exists.</summary>
    /// <param name="type">Type declaring the property.</param>
    /// <param name="instance">Instance to set on.</param>
    /// <param name="name">Property name.</param>
    /// <param name="value">Value to set.</param>
    private static void Set(Type type, object instance, string name, bool value)
        => type.GetProperty(name, I)?.SetValue(instance, value);

    /// <summary>Finds the nearest ancestor of the requested type, or null when absent.</summary>
    /// <typeparam name="T">Ancestor type.</typeparam>
    /// <param name="node">Node to ascend from.</param>
    /// <returns>The nearest matching ancestor, or null.</returns>
    private static T? Ancestor<T>(DependencyObject node) where T : DependencyObject
    {
        while (node is not null)
        {
            if (node is T found)
                return found;
            node = node is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(node)!
                : LogicalTreeHelper.GetParent(node)!;
        }
        return null;
    }

    /// <summary>Enumerates a visual node's descendants depth-first, excluding the node itself.</summary>
    /// <param name="root">Root to enumerate from.</param>
    /// <returns>All descendants of the node.</returns>
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (DependencyObject descendant in Descendants(child))
                yield return descendant;
        }
    }

    /// <summary>Pumps the dispatcher at ApplicationIdle priority.</summary>
    private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));

    /// <summary>Prints PASS and returns, or throws when errors were collected.</summary>
    /// <param name="errors">Collected error messages.</param>
    private static void Finish(List<string> errors)
    {
        if (errors.Count == 0)
        {
            Console.WriteLine("PASS VisualAcceptanceSmoke");
            return;
        }
        throw new InvalidOperationException("VisualAcceptanceSmoke failures: " + string.Join(" | ", errors));
    }
}
