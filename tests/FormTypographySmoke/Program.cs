extern alias ProductUi;

using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

/// <summary>Smoke test verifying shared settings-form typography, spacing, focus, and dropdown contrast in a real WPF window.</summary>
internal static class Program
{
    /// <summary>Binding flags used for reflection access to non-public members.</summary>
    private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    /// <summary>Collected check failures for the final report.</summary>
    private static readonly List<string> Failures = [];
    /// <summary>Layout measurements captured for both form widths.</summary>
    private static readonly List<object> Layouts = [];
    /// <summary>Consequential settings flags forced false for the synthetic fixture.</summary>
    private static readonly string[] SafetyFlags = ["RemindersEnabled", "AutoLockEnabled", "LockScreenEnabled", "LockScreenOptIn", "StartupEnabled", "WidgetEnabled"];
    /// <summary>TextBlock fields expected to use the shared paragraph help typography.</summary>
    private static readonly string[] HelpFields = ["OverviewCalculationNote", "OverviewCalendarStatus", "SelectedDateDetail", "HijriSelectedDateDetail", "SettingsScheduleDateLabel", "SettingsSchedulePreviewStatus", "NetworkLocationStatus", "GpsLocationStatus", "OfflineReverseGeocodeStatus", "LockScreenSafetyText"];
    /// <summary>Production field-specific overrides of the shared paragraph margin.</summary>
    private static readonly Dictionary<string, Thickness> HelpFieldMargins = new()
    {
        ["SelectedDateDetail"] = new Thickness(0, 0, 10, 0),
        ["HijriSelectedDateDetail"] = new Thickness(0, 0, 10, 0),
        ["SettingsSchedulePreviewStatus"] = new Thickness(3, 12, 3, 8),
        ["NetworkLocationStatus"] = new Thickness(0, 0, 0, 8),
        ["OfflineReverseGeocodeStatus"] = new Thickness(0, 8, 0, 0)
    };
    /// <summary>CheckBox fields whose labels must wrap with the shared typography.</summary>
    private static readonly string[] CheckboxFields = ["ReminderEnabledCheckBox", "WidgetEnabledCheckBox", "StartupEnabledCheckBox", "StartHiddenCheckBox", "AutoLockEnabledCheckBox", "LockScreenCheckBox"];

    /// <summary>Runs the form typography smoke suite; returns 0 on success, 1 on failure.</summary>
    /// <returns>Process exit code.</returns>
    [STAThread]
    private static int Main()
    {
        string scratch = Environment.GetEnvironmentVariable("SMOKE_SCRATCH") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrayerAutomationSmoke", "scratch");
        string output = Path.Combine(scratch, "form-typography-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);
        Application? application = null;
        Window? window = null;
        bool isolated = false;
        try
        {
            if (System.Security.Principal.WindowsIdentity.GetCurrent().Owner is null)
                throw new InvalidOperationException("The isolated test requires a Windows user session.");
            application = CreateApplicationWithoutStartup();
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
            application.DispatcherUnhandledException += (_, e) => { Failures.Add("Dispatcher: " + e.Exception.GetType().Name + ": " + e.Exception.Message); e.Handled = true; };
            Type appType = typeof(ProductUi::PrayerAutomation.UI.App);
            appType.GetMethod("InitializeComponent", Members)!.Invoke(application, null);
            Assembly assembly = appType.Assembly;
            Type settingsType = assembly.GetType("PrayerAutomation.UI.AppSettings", true)!;
            object settings = Activator.CreateInstance(settingsType, nonPublic: true)!;
            foreach (string name in SafetyFlags) settingsType.GetProperty(name, Members)!.SetValue(settings, false);
            foreach (string name in SafetyFlags)
                if (settingsType.GetProperty(name, Members)!.GetValue(settings) is not false)
                    throw new InvalidOperationException("Fail closed: synthetic settings must disable " + name + ".");
            appType.GetProperty("Settings", Members)!.SetValue(application, settings);
            Type themeType = assembly.GetType("PrayerAutomation.UI.ThemeManager", true)!;
            themeType.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [application, "light"]);
            Type windowType = assembly.GetType("PrayerAutomation.UI.MainWindow", true)!;
            window = (Window)windowType.GetConstructor(Members, null, [settingsType], null)!.Invoke([settings]);
            foreach (FieldInfo field in windowType.GetFields(Members))
                if (field.GetValue(window) is DispatcherTimer timer) timer.Stop();
            window.Opacity = 0;
            window.ShowInTaskbar = false;
            window.ShowActivated = false;
            window.IsHitTestVisible = false;
            window.Topmost = false;
            window.Show();
            Pump();
            VerifyStartupAbsent(application);
            isolated = true;
            TabControl tabs = Field<TabControl>(window, "MainTabs");
            Check(window.TryFindResource("FormHelpText") is Style, "Descriptions need one shared FormHelpText style.");
            Check(window.TryFindResource("FormBodyText") is Style, "Labels and paragraphs need a shared readable body style.");
            foreach (string field in HelpFields)
            {
                TextBlock text = Field<TextBlock>(window, field);
                Check(text.FontSize == 14 && text.LineHeight == 20 && text.LineStackingStrategy == LineStackingStrategy.BlockLineHeight,
                    field + " must use 14-DIP text with a fixed 20-DIP line step, not " + text.FontSize + "/" + text.LineHeight + ".");
                Check(text.TextAlignment == TextAlignment.Left && text.TextWrapping == TextWrapping.Wrap, field + " must wrap and align left.");
                Thickness expectedMargin = HelpFieldMargins.TryGetValue(field, out Thickness configured)
                    ? configured
                    : new Thickness(3, 4, 3, 8);
                Check(text.Margin == expectedMargin,
                    field + " must keep its production paragraph inset/gap; actual " + text.Margin + ".");
            }
            foreach (string field in CheckboxFields)
            {
                CheckBox check = Field<CheckBox>(window, field);
                Check(check.Content is TextBlock label && label.TextWrapping == TextWrapping.Wrap && label.LineHeight == 20 && label.FontSize == 14,
                    field + " needs a short, wrapped label without replacing the default checkbox template.");
            }
            tabs.SelectedIndex = 2;
            Pump();
            FrameworkElement correctionsRow = Field<FrameworkElement>(window, "CorrectionsRow");
            // The production row starts collapsed; realize its visual children before measuring them.
            correctionsRow.Visibility = Visibility.Visible;
            correctionsRow.UpdateLayout();
            Pump();
            TextBlock? correctionHelp = Descendants(correctionsRow).OfType<TextBlock>()
                .FirstOrDefault(text => text.Text.Contains("Koreksi tiap waktu", StringComparison.Ordinal));
            Check(correctionHelp is not null && correctionHelp.FontSize == 14 && correctionHelp.LineHeight == 20 &&
                  correctionHelp.TextWrapping == TextWrapping.Wrap && correctionHelp.TextAlignment == TextAlignment.Left &&
                  IsAllowedDescriptionMargin(correctionHelp.Margin),
                "Prayer correction guidance must share the paragraph typography and a production paragraph margin.");
            MethodInfo? errorTextFactory = windowType.GetMethod("CreateScheduleErrorText", Members);
            if (errorTextFactory is null)
                Check(false, "Schedule error paragraphs need the actual shared production text builder so both error paths can be checked without logging or inducing a native failure.");
            else
            {
                const string syntheticError = "Synthetic schedule error: no native operation or log write performed.";
                TextBlock errorText = (TextBlock)errorTextFactory.Invoke(window, [syntheticError])!;
                Check(errorText.Text == syntheticError && errorText.FontSize == 14 && errorText.LineHeight == 20 &&
                      errorText.LineStackingStrategy == LineStackingStrategy.BlockLineHeight && errorText.TextWrapping == TextWrapping.Wrap &&
                      errorText.TextAlignment == TextAlignment.Left && errorText.Margin == new Thickness(3, 4, 3, 8),
                    "The actual schedule error text builder must preserve the message and share readable paragraph typography.");
                object palette = assembly.GetType("PrayerAutomation.UI.ThemeManager", true)!.GetProperty("Current", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
                string expectedInk = (string)palette.GetType().GetProperty("ErrorText")!.GetValue(palette)!;
                Check(errorText.Foreground is SolidColorBrush errorInk && errorInk.Color == (Color)ColorConverter.ConvertFromString(expectedInk),
                    "Shared error typography must preserve the theme's error foreground.");
            }
            Type probeType = assembly.GetType("PrayerAutomation.UI.LockScreenSafetyProbe", true)!;
            object safety = probeType.GetMethod("Inspect", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, null)!;
            string safetyMessage = (string)safety.GetType().GetProperty("Message")!.GetValue(safety)!;
            TextBlock safetyText = Field<TextBlock>(window, "LockScreenSafetyText");
            TextBlock statusText = Field<TextBlock>(window, "LockScreenStatus");
            Check(safetyText.Text.Contains(safetyMessage, StringComparison.Ordinal), "Keep the current Windows safety reason, not only a generic recovery description.");
            Check(safetyText.Visibility == Visibility.Visible && statusText.Visibility == Visibility.Visible &&
                  !statusText.Text.Contains(safetyMessage, StringComparison.Ordinal),
                "Do not display the same default Windows safety message twice.");
            const string syntheticStatus = "Synthetic scheduler status: no native operation performed.";
            windowType.GetMethod("SetLockScreenStatus", Members)!.Invoke(window, [syntheticStatus]);
            Check(Field<TextBlock>(window, "LockScreenStatus").Text == syntheticStatus && Field<TextBlock>(window, "LockScreenStatus").Visibility == Visibility.Visible && safetyText.Text.Contains(safetyMessage, StringComparison.Ordinal),
                "A scheduler status must not hide the current mode/API/recovery safety reason.");
            windowType.GetMethod("RefreshLockScreenSafety", Members)!.Invoke(window, [null]);
            string settingsBefore = JsonSerializer.Serialize(settings, settingsType);
            foreach (double width in new[] { 940d, 1180d })
            {
                window.Width = width;
                window.Height = width == 940 ? 650 : 820;
                tabs.SelectedIndex = 2;
                Pump();
                FrameworkElement settingsRoot = (FrameworkElement)((TabItem)tabs.SelectedItem).Content;
                ScrollViewer settingsScroll = Descendants(settingsRoot).OfType<ScrollViewer>().First();
                FrameworkElement content = (FrameworkElement)settingsScroll.Content;
                content.UpdateLayout();
                TextBlock[] descriptions = Descendants(content).OfType<TextBlock>().Where(t => t.TextWrapping == TextWrapping.Wrap && t.IsVisible && !string.IsNullOrWhiteSpace(t.Text) && !ReferenceEquals(t.Style, window.TryFindResource("FieldLabel"))).ToArray();
                foreach (TextBlock text in descriptions)
                {
                    Check(text.FontSize == 14 && text.LineHeight == 20 && text.TextAlignment == TextAlignment.Left,
                        "Settings paragraph must share typography: " + Short(text.Text));
                    Check(text.Text.Length <= 300, "Split the long settings explanation into short paragraphs: " + Short(text.Text));
                    Check(IsAllowedDescriptionMargin(text.Margin) || text.Parent is CheckBox,
                        "Description indentation is inconsistent: " + Short(text.Text) + " actual " + text.Margin + ".");
                    if (text.Parent is FrameworkElement parent)
                    {
                        Rect bounds = text.TransformToAncestor(parent).TransformBounds(new Rect(text.RenderSize));
                        Check(bounds.Left >= -1 && bounds.Right <= parent.ActualWidth + 1, "Description extends outside its parent: " + Short(text.Text));
                    }
                }
                var checkboxGaps = new List<object>();
                foreach (string field in CheckboxFields)
                {
                    CheckBox checkbox = Field<CheckBox>(window, field);
                    checkbox.ApplyTemplate();
                    if (checkbox.Template.FindName("CheckBoxGlyphBackground", checkbox) is Border glyph && checkbox.Content is TextBlock label)
                    {
                        AutomationPeer? peer = UIElementAutomationPeer.CreatePeerForElement(checkbox);
                        Check(peer is not null && peer.GetName() == label.Text, field + " must retain an accessible label after text wrapping.");
                        Rect glyphBounds = glyph.TransformToAncestor(checkbox).TransformBounds(new Rect(glyph.RenderSize));
                        Rect labelBounds = label.TransformToAncestor(checkbox).TransformBounds(new Rect(label.RenderSize));
                        double gap = labelBounds.Left - glyphBounds.Right;
                        Check(gap >= 14, field + " needs at least 14 DIP between the checkbox glyph and label; actual gap=" + gap + ".");
                        checkboxGaps.Add(new { field, gap, glyph_margin = glyph.Margin.ToString() });
                        bool wasEnabled = checkbox.IsEnabled;
                        foreach (bool enabled in new[] { true, false })
                        {
                            checkbox.IsEnabled = enabled;
                            Pump();
                            Check(label.Foreground is SolidColorBrush labelInk && checkbox.Foreground is SolidColorBrush controlInk && labelInk.Color == controlInk.Color,
                                field + " label must follow the checkbox foreground in normal and disabled states.");
                        }
                        checkbox.IsEnabled = wasEnabled;
                        Pump();
                    }
                    else Check(false, field + " must retain the existing application checkbox template.");
                }
                string allText = string.Join("\n", Descendants(content).OfType<TextBlock>().Select(t => t.Text));
                TextBlock? imageHelp = Descendants(content).OfType<TextBlock>().FirstOrDefault(text => text.Text.Contains("Gambar dipasang dua menit sebelum waktu sholat", StringComparison.OrdinalIgnoreCase));
                Check(imageHelp is not null && imageHelp.Text.Contains("waktu sholat", StringComparison.OrdinalIgnoreCase) &&
                      allText.Contains("Kunci Windows saat lima waktu sholat", StringComparison.OrdinalIgnoreCase) &&
                      allText.Contains("tidak mengubah gambar lock screen", StringComparison.OrdinalIgnoreCase),
                    "Image-change help must retain the prayer trigger and distinguish the image feature from optional session auto-lock.");
                Check(allText.Contains("1–60 menit", StringComparison.OrdinalIgnoreCase) &&
                      allText.Contains("default 15", StringComparison.OrdinalIgnoreCase) &&
                      allText.Contains("Dibatasi sisa waktu menuju sholat berikutnya", StringComparison.OrdinalIgnoreCase) &&
                      allText.Contains("berlaku untuk gambar berikutnya", StringComparison.OrdinalIgnoreCase),
                    "Keep the selectable restoration interval, previous default, and next-change limitation.");
                Check(allText.Contains("Mengunci sesi", StringComparison.OrdinalIgnoreCase) &&
                      allText.Contains("tidak mengubah gambar lock screen", StringComparison.OrdinalIgnoreCase),
                    "Changing a picture must remain distinct from locking a session or lock-screen wallpaper.");
                Check(allText.Contains("jika tidak bisa dipastikan", StringComparison.OrdinalIgnoreCase) &&
                      allText.Contains("gambar tidak diubah", StringComparison.OrdinalIgnoreCase),
                    "Retain the fail-closed recovery prerequisite.");
                TextBlock[] fieldLabels = Descendants(content).OfType<TextBlock>().Where(t => ReferenceEquals(t.Style, window.TryFindResource("FieldLabel"))).ToArray();
                foreach (TextBlock label in fieldLabels)
                    Check(label.TextAlignment == TextAlignment.Left && label.TextWrapping == TextWrapping.Wrap && IsAllowedFieldLabelMargin(label.Margin), "Field labels need a known production inset and room before their input: " + Short(label.Text) + " actual " + label.Margin + ".");
                object spacing = VerifyFormSpacing(content, window);
                Capture(content, Path.Combine(output, "settings-" + width + ".png"));
                Capture(Field<CheckBox>(window, "ReminderEnabledCheckBox"), Path.Combine(output, "checkbox-" + width + ".png"));
                Capture(Field<ComboBox>(window, "MethodBox"), Path.Combine(output, "method-" + width + ".png"));
                Capture(Field<FrameworkElement>(window, "CorrectionsRow"), Path.Combine(output, "corrections-" + width + ".png"));
                settingsScroll.ScrollToEnd();
                Pump();
                Button save = Field<Button>(window, "SaveSettingsButton");
                Rect saveBounds = save.TransformToAncestor(window).TransformBounds(new Rect(save.RenderSize));
                Check(save.IsVisible && saveBounds.Bottom <= window.ActualHeight + 1, "Save must remain reachable at the minimum form size.");
                Layouts.Add(new { width, window_height = window.ActualHeight, content_width = content.ActualWidth, content_height = content.ActualHeight, help_paragraphs = descriptions.Length, field_labels = fieldLabels.Length, checkbox_gaps = checkboxGaps, spacing, actual_dpi = VisualTreeHelper.GetDpi(window).PixelsPerInchX, bottom_reached = settingsScroll.VerticalOffset >= settingsScroll.ScrollableHeight - 1 });
                Check(settingsScroll.VerticalOffset >= settingsScroll.ScrollableHeight - 1, "The complete settings content must remain reachable by scrolling.");
            }
            Check(JsonSerializer.Serialize(settings, settingsType) == settingsBefore, "Layout/copy tests must not change synthetic saved settings.");
            VerifyDropdownText(window, application, assembly, tabs, output);
            VerifyStartupAbsent(application);
        }
        catch (Exception ex)
        {
            while (ex is TargetInvocationException { InnerException: not null }) ex = ex.InnerException;
            Failures.Add(ex.GetType().Name + ": " + ex.Message);
        }
        finally
        {
            if (window is not null)
            {
                window.GetType().GetMethod("AllowClose", Members)!.Invoke(window, null);
                window.Close();
            }
        }
        var report = new { complete = Failures.Count == 0, isolated_startup = isolated, main_app_run = false, synthetic_safety_flags_disabled = SafetyFlags, layouts = Layouts, failures = Failures.Distinct().ToArray(), screenshots = Directory.GetFiles(output, "*.png") };
        File.WriteAllText(Path.Combine(output, "results.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("FORM_TYPOGRAPHY_EVIDENCE " + Path.Combine(output, "results.json"));
        Console.WriteLine(JsonSerializer.Serialize(new { complete = Failures.Count == 0, isolated_startup = isolated, layouts = Layouts.Count, failures = Failures.Distinct().Count() }));
        foreach (string failure in Failures.Distinct().Take(15)) Console.WriteLine("FAIL: " + failure);
        return Failures.Count == 0 ? 0 : 1;
    }

    /// <summary>Accepts the shared paragraph inset plus the field-specific production overrides.</summary>
    /// <param name="margin">Margin measured on a settings description.</param>
    /// <returns>True when the margin is one of the known production values.</returns>
    private static bool IsAllowedDescriptionMargin(Thickness margin) =>
        margin == new Thickness(3, 4, 3, 8) ||
        margin == new Thickness(3, 0, 3, 0) ||
        margin == new Thickness(6, 4, 6, 2) ||
        margin == new Thickness(5, 0, 0, 0) ||
        margin == new Thickness(3, 12, 3, 8) ||
        margin == new Thickness(3, 0, 3, 4) ||
        margin == new Thickness(0, 0, 10, 0) ||
        margin == new Thickness(0, 0, 0, 8) ||
        margin == new Thickness(0, 8, 0, 0) ||
        margin == new Thickness(0, 6, 0, 0) ||
        margin == new Thickness(0, 2, 0, 0) ||
        margin == new Thickness(0, 4, 0, 6);

    /// <summary>Accepts the shared field-label inset plus the production overrides used by grouped settings.</summary>
    /// <param name="margin">Margin measured on a field label.</param>
    /// <returns>True when the margin is one of the known production values.</returns>
    private static bool IsAllowedFieldLabelMargin(Thickness margin) =>
        margin == new Thickness(3, 4, 12, 6) ||
        margin == new Thickness(0, 8, 0, 0) ||
        margin == new Thickness(3, 0, 3, 2) ||
        margin == new Thickness(0, 12, 0, 2);

    /// <summary>Verifies inline and stacked label/input spacing and the correction-group gaps; returns measurements.</summary>
    /// <param name="content">Settings tab content root to inspect.</param>
    /// <param name="window">Owning window used for the shared FieldLabel resource lookup.</param>
    /// <returns>Spacing measurements and containment counts.</returns>
    private static object VerifyFormSpacing(FrameworkElement content, Window window)
    {
        var gaps = new List<object>();
        int containmentChecks = 0;
        foreach (StackPanel row in Descendants(content).OfType<StackPanel>().Where(p => p.Orientation == Orientation.Horizontal && p.IsVisible))
        {
            FrameworkElement[] children = row.Children.OfType<FrameworkElement>().Where(c => c.IsVisible).ToArray();
            for (int i = 0; i < children.Length; i++)
            {
                FrameworkElement child = children[i];
                Rect bounds = Bounds(child, row);
                containmentChecks++;
                Check(bounds.Left >= -0.5 && bounds.Right <= row.ActualWidth + 0.5,
                    "Inline controls must fit their row without a clipped button: " + (child.Name.Length > 0 ? child.Name : child.GetType().Name));
                if (i == 0) continue;
                FrameworkElement previous = children[i - 1];
                if (previous is TextBlock text && IsInput(child))
                {
                    double gap = bounds.Left - TextInkBounds(text, row).Right;
                    Check(gap >= 10, "Leave at least 10 DIP from a field label to its input: " + Short(text.Text));
                    gaps.Add(new { kind = "inline-label-input", label = Short(text.Text), gap });
                }
                else if (IsInput(previous) && IsInput(child))
                {
                    double gap = bounds.Left - Bounds(previous, row).Right;
                    Check(gap >= 4, "Grouped input buttons/boxes need at least 4 DIP between controls.");
                    gaps.Add(new { kind = "grouped-controls", label = child.Name, gap });
                }
            }
        }
        foreach (TextBlock label in Descendants(content).OfType<TextBlock>().Where(t => ReferenceEquals(t.Style, window.TryFindResource("FieldLabel")) && t.IsVisible))
        {
            if (label.Parent is StackPanel stack)
            {
                int index = stack.Children.IndexOf(label);
                if (index + 1 < stack.Children.Count && stack.Children[index + 1] is FrameworkElement next && IsInput(next) && next.IsVisible)
                {
                    double gap = Bounds(next, stack).Top - Bounds(label, stack).Bottom;
                    Check(gap >= 8, "Leave at least 8 DIP between a stacked label and selection/input box: " + Short(label.Text) + " actual " + gap.ToString("0.#") + ".");
                    gaps.Add(new { kind = "stacked-label-input", label = Short(label.Text), gap });
                }
            }
            else if (label.Parent is Grid grid && Grid.GetColumnSpan(label) == 1)
            {
                FrameworkElement? adjacent = grid.Children.OfType<FrameworkElement>().FirstOrDefault(c => c.IsVisible && Grid.GetRow(c) == Grid.GetRow(label) && Grid.GetColumn(c) > Grid.GetColumn(label));
                FrameworkElement? input = adjacent is not null ? Descendants(adjacent).OfType<FrameworkElement>().Where(IsInput).Where(c => c.IsVisible).OrderBy(c => Bounds(c, grid).Left).FirstOrDefault() : null;
                if (input is not null)
                {
                    double gap = Bounds(input, grid).Left - TextInkBounds(label, grid).Right;
                    Check(gap >= 10, "Leave at least 10 DIP between a side label and selection/input box: " + Short(label.Text));
                    gaps.Add(new { kind = "side-label-input", label = Short(label.Text), gap });
                }
            }
        }
        StackPanel[] groups = Descendants(Field<FrameworkElement>(window, "CorrectionsRow")).OfType<StackPanel>()
            .Where(p => p.Orientation == Orientation.Horizontal && p.Children.OfType<Button>().Count() == 2 && p.Children.OfType<TextBox>().Any()).ToArray();
        for (int i = 0; i < groups.Length; i++)
            for (int j = i + 1; j < groups.Length; j++)
            {
                Rect left = Bounds(groups[i], content), right = Bounds(groups[j], content);
                if (Math.Abs(left.Top - right.Top) > 1) continue;
                if (left.Left > right.Left) (left, right) = (right, left);
                double gap = right.Left - left.Right;
                Check(gap >= 12, "Separate neighboring prayer-correction groups by at least 12 DIP, or wrap them to another row.");
                gaps.Add(new { kind = "correction-groups", label = "adjacent groups", gap });
            }
        ComboBox method = Field<ComboBox>(window, "MethodBox");
        string selectedName = (string)method.SelectedItem!.GetType().GetProperty("Name")!.GetValue(method.SelectedItem)!;
        TextBlock? selectedText = Descendants(method).OfType<TextBlock>().FirstOrDefault(t => t.Text == selectedName);
        Check(method.SelectedValuePath == "Id", "Display layout must preserve the method selection Id.");
        double? methodExpectedHeight = selectedText is null ? null : RealizedTextHeight(selectedText);
        Check(selectedText is not null && selectedText.TextWrapping == TextWrapping.Wrap && selectedText.ActualHeight >= methodExpectedHeight - 1,
            "The full calculation-method name must wrap inside the closed selection box, not be clipped; actual=" + selectedText?.ActualHeight + ", WPF measured=" + methodExpectedHeight + ", width=" + selectedText?.ActualWidth + ".");
        Check(groups.Length >= 7 && containmentChecks > 20 && gaps.Count > 20, "Spacing checks must cover the seven prayer-correction rows and real field/input pairs.");
        return new { containment_checks = containmentChecks, correction_groups = groups.Length, gaps, method_name_wraps = selectedText?.TextWrapping == TextWrapping.Wrap,
            method_width = selectedText?.ActualWidth, method_height = selectedText?.ActualHeight, method_expected_height = methodExpectedHeight };
    }

    /// <summary>Measures the wrapped height of the realized text using actual WPF layout.</summary>
    /// <param name="text">TextBlock to measure.</param>
    /// <returns>Measured height in DIP.</returns>
    private static double RealizedTextHeight(TextBlock text)
    {
        var measured = new TextBlock
        {
            Text = text.Text, FontFamily = text.FontFamily, FontSize = text.FontSize,
            FontStyle = text.FontStyle, FontWeight = text.FontWeight, FontStretch = text.FontStretch,
            FlowDirection = text.FlowDirection, TextWrapping = text.TextWrapping,
            TextAlignment = text.TextAlignment, LineHeight = text.LineHeight,
            LineStackingStrategy = text.LineStackingStrategy, UseLayoutRounding = text.UseLayoutRounding
        };
        // Use the actual WPF text layout; FormattedText can wrap one DIP earlier
        // than a rounded TextBlock at the exact end of a word.
        measured.Measure(new Size(text.ActualWidth + 1, double.PositiveInfinity));
        return measured.DesiredSize.Height;
    }

    /// <summary>Verifies dropdown popup item contrast across all themes, selectors, and item states.</summary>
    /// <param name="window">Main window under test.</param>
    /// <param name="application">Application used to apply theme presets.</param>
    /// <param name="assembly">Product UI assembly.</param>
    /// <param name="tabs">Main tab control used to switch tabs.</param>
    /// <param name="output">Output directory for captured screenshots.</param>
    private static void VerifyDropdownText(Window window, Application application, Assembly assembly, TabControl tabs, string output)
    {
        Type manager = assembly.GetType("PrayerAutomation.UI.ThemeManager", true)!;
        MethodInfo apply = manager.GetMethod("Apply", BindingFlags.Public | BindingFlags.Static)!;
        var catalog = (IEnumerable)assembly.GetType("PrayerAutomation.UI.ThemeCatalog", true)!.GetProperty("All", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
        DependencyPropertyKey highlighted = (DependencyPropertyKey)typeof(ComboBoxItem).GetField("IsHighlightedPropertyKey", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        window.GetType().GetField("_loadingSettings", Members)!.SetValue(window, true);
        int themes = 0, checks = 0;
        double minimum = double.MaxValue;
        foreach (object theme in catalog)
        {
            string id = (string)theme.GetType().GetProperty("Id")!.GetValue(theme)!;
            apply.Invoke(null, [application, id]);
            themes++;
            foreach ((string field, int tab) in new[] { ("CalendarYearBox", 1), ("HijriYearBox", 1), ("MethodBox", 2), ("WidgetModeBox", 2), ("WidgetMotionBox", 2), ("WidgetPositionBox", 2), ("ThemeBox", 2) })
            {
                tabs.SelectedIndex = tab;
                if (tab == 1) Field<TabControl>(window, "CalendarModeTabs").SelectedIndex = field == "HijriYearBox" ? 1 : 0;
                ComboBox combo = Field<ComboBox>(window, field);
                int savedIndex = combo.SelectedIndex;
                bool wasEnabled = combo.IsEnabled;
                combo.IsEnabled = true;
                combo.BringIntoView();
                combo.IsDropDownOpen = true;
                Pump();
                try
                {
                    int[] indices = new[] { 0, Math.Min(1, combo.Items.Count - 1), Math.Max(0, savedIndex) }.Distinct().ToArray();
                    foreach (int index in indices)
                    {
                        var item = combo.ItemContainerGenerator.ContainerFromIndex(index) as ComboBoxItem;
                        Check(item is not null, field + " must realize actual popup items for contrast checks.");
                        if (item is null) continue;
                        item.ApplyTemplate();
                        foreach (string state in new[] { "normal", "highlighted", "selected", "disabled" })
                        {
                            item.IsSelected = state == "selected";
                            item.SetValue(highlighted, state == "highlighted");
                            item.IsEnabled = state != "disabled";
                            Pump();
                            Border? surface = Descendants(item).OfType<Border>().FirstOrDefault(b => b.Background is SolidColorBrush ink && ink.Color.A == 255);
                            TextBlock[] texts = Descendants(item).OfType<TextBlock>().Where(t => !string.IsNullOrWhiteSpace(t.Text)).ToArray();
                            Check(texts.Length > 0 && surface is not null, field + "/" + state + " requires rendered text and item background.");
                            foreach (TextBlock text in texts)
                            {
                                checks++;
                                if (text.Foreground is not SolidColorBrush fg || surface?.Background is not SolidColorBrush bg) { Check(false, "Popup brushes must be solid."); continue; }
                                double ratio = Contrast(fg.Color, bg.Color);
                                minimum = Math.Min(minimum, ratio);
                                Check(ratio >= 4.5, "Rendered dropdown text contrast " + field + "/" + state + "/" + id + ": " + ratio.ToString("0.000", CultureInfo.InvariantCulture) + ":1.");
                            }
                            if (index == 1 && state == "highlighted" && field is "ThemeBox" or "CalendarYearBox" && id is "light" or "dark")
                                Capture(item, Path.Combine(output, "dropdown-" + field + "-" + id + ".png"));
                        }
                        item.SetValue(highlighted, false);
                        item.IsEnabled = true;
                    }
                }
                finally
                {
                    combo.SelectedIndex = savedIndex;
                    combo.IsDropDownOpen = false;
                    combo.IsEnabled = wasEnabled;
                    Pump();
                }
            }
        }
        apply.Invoke(null, [application, "light"]);
        window.GetType().GetField("_loadingSettings", Members)!.SetValue(window, false);
        Check(themes == 8 && checks >= 7 * 8 * 4, "Dropdown contrast must cover seven real selectors in all eight themes and four states.");
        Console.WriteLine("FORM_DROPDOWN_TEXT_CONTRAST themes=" + themes + " selectors=7 checks=" + checks + " minimum=" + minimum.ToString("0.000", CultureInfo.InvariantCulture) + ":1.");
    }

    /// <summary>Computes the WCAG contrast ratio between two colors.</summary>
    /// <param name="a">First color.</param>
    /// <param name="b">Second color.</param>
    /// <returns>Contrast ratio (>= 1).</returns>
    private static double Contrast(Color a, Color b)
    {
        /// <summary>Computes relative luminance of a color.</summary>
        /// <param name="c">Color to evaluate.</param>
        /// <returns>Relative luminance.</returns>
        static double L(Color c)
        {
            /// <summary>Linearizes one 8-bit channel value.</summary>
            /// <param name="value">Channel value 0-255.</param>
            /// <returns>Linearized channel.</returns>
            static double C(byte value) { double x = value / 255d; return x <= .04045 ? x / 12.92 : Math.Pow((x + .055) / 1.055, 2.4); }
            return .2126 * C(c.R) + .7152 * C(c.G) + .0722 * C(c.B);
        }
        double x = L(a), y = L(b);
        return (Math.Max(x, y) + .05) / (Math.Min(x, y) + .05);
    }

    /// <summary>Determines whether an element is treated as an input control for spacing checks.</summary>
    /// <param name="element">Element to test.</param>
    /// <returns>True for buttons, text boxes, and combo boxes.</returns>
    private static bool IsInput(FrameworkElement element) => element is Button or TextBox or ComboBox;
    /// <summary>Bounds of an element relative to an ancestor visual.</summary>
    /// <param name="element">Element to measure.</param>
    /// <param name="ancestor">Ancestor visual used as the coordinate origin.</param>
    /// <returns>Element bounds in ancestor coordinates.</returns>
    private static Rect Bounds(FrameworkElement element, Visual ancestor) => element.TransformToAncestor(ancestor).TransformBounds(new Rect(element.RenderSize));
    /// <summary>Builds formatted text metrics matching the TextBlock's typography.</summary>
    /// <param name="text">Source text block.</param>
    /// <returns>Formatted text for measurement.</returns>
    private static FormattedText TextMetrics(TextBlock text)
    {
        double pixelsPerDip = VisualTreeHelper.GetDpi(text).PixelsPerDip;
        if (pixelsPerDip <= 0) pixelsPerDip = 1;
        var formatted = new FormattedText(text.Text, CultureInfo.CurrentCulture, text.FlowDirection,
            new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, text.Foreground, pixelsPerDip)
        { MaxTextWidth = Math.Max(1, text.ActualWidth), TextAlignment = text.TextAlignment };
        if (!double.IsNaN(text.LineHeight)) formatted.LineHeight = text.LineHeight;
        return formatted;
    }
    /// <summary>Bounds of the rendered ink of a TextBlock relative to an ancestor visual.</summary>
    /// <param name="text">Text block to measure.</param>
    /// <param name="ancestor">Ancestor visual used as the coordinate origin.</param>
    /// <returns>Ink bounds in ancestor coordinates.</returns>
    private static Rect TextInkBounds(TextBlock text, Visual ancestor) => text.TransformToAncestor(ancestor).TransformBounds(TextMetrics(text).BuildGeometry(new Point()).Bounds);

    /// <summary>Creates the product application with its implicit WPF startup callback aborted exactly once.</summary>
    /// <returns>The created application, with startup suppressed.</returns>
    private static Application CreateApplicationWithoutStartup()
    {
        FieldInfo callbackField = typeof(DispatcherOperation).GetField("_method", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Fail closed: WPF startup-callback inspection is unavailable.");
        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        DispatcherOperation? startup = null;
        int aborted = 0;
        dispatcher.Hooks.OperationPosted += AbortStartup;
        Application application;
        try { application = (Application)Activator.CreateInstance(typeof(ProductUi::PrayerAutomation.UI.App))!; }
        finally { dispatcher.Hooks.OperationPosted -= AbortStartup; }
        if (aborted != 1 || startup?.Status != DispatcherOperationStatus.Aborted)
            throw new InvalidOperationException("Fail closed: implicit startup must be suppressed exactly once before pumping WPF.");
        VerifyStartupAbsent(application);
        return application;

        /// <summary>Aborts the single implicit WPF startup callback while it is still pending; throws on any unexpected shape.</summary>
        /// <param name="sender">Event sender (unused).</param>
        /// <param name="e">Dispatcher hook event carrying the posted operation.</param>
        void AbortStartup(object? sender, DispatcherHookEventArgs e)
        {
            if (callbackField.GetValue(e.Operation) is not Delegate callback || callback.Method.DeclaringType != typeof(Application) || callback.Method.Name != "<.ctor>b__1_0") return;
            if (e.Operation.Priority != DispatcherPriority.Send || e.Operation.Status != DispatcherOperationStatus.Pending || !e.Operation.Abort())
                throw new InvalidOperationException("Fail closed: startup must be aborted while pending.");
            startup = e.Operation;
            aborted++;
        }
    }

    /// <summary>Fails closed when any consequential startup object was created on the application.</summary>
    /// <param name="application">Application to inspect.</param>
    private static void VerifyStartupAbsent(Application application)
    {
        foreach (string name in new[] { "_singleInstanceMutex", "_tray", "_reminders", "_autoLock", "_lockScreenScheduler", "_widget" })
            if (typeof(ProductUi::PrayerAutomation.UI.App).GetField(name, Members)!.GetValue(application) is not null)
                throw new InvalidOperationException("Fail closed: no startup/session actions may run (" + name + ").");
    }

    /// <summary>Reflects a field from the window, cast to the requested type.</summary>
    /// <typeparam name="T">Field value type.</typeparam>
    /// <param name="window">Window instance to inspect.</param>
    /// <param name="name">Field name.</param>
    /// <returns>The field value.</returns>
    private static T Field<T>(Window window, string name) => (T)window.GetType().GetField(name, Members)!.GetValue(window)!;
    /// <summary>Records a failure message when the condition does not hold.</summary>
    /// <param name="condition">Condition that must hold.</param>
    /// <param name="failure">Failure message recorded when false.</param>
    private static void Check(bool condition, string failure) { if (!condition) Failures.Add(failure); }
    /// <summary>Truncates a string to 70 characters for reports.</summary>
    /// <param name="value">Value to shorten.</param>
    /// <returns>The original or truncated value.</returns>
    private static string Short(string value) => value.Length <= 70 ? value : value[..70] + "…";
    /// <summary>Pumps the dispatcher at ContextIdle priority.</summary>
    private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
    /// <summary>Enumerates a dependency object and its descendants depth-first.</summary>
    /// <param name="root">Root to enumerate.</param>
    /// <returns>The root followed by all descendants.</returns>
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            foreach (DependencyObject child in Descendants(VisualTreeHelper.GetChild(root, i))) yield return child;
    }
    /// <summary>Renders an element to a PNG file with the themed background brush.</summary>
    /// <param name="element">Element to capture.</param>
    /// <param name="path">Destination PNG path.</param>
    private static void Capture(FrameworkElement element, string path)
    {
        element.UpdateLayout();
        int width = (int)Math.Ceiling(element.ActualWidth);
        int height = (int)Math.Ceiling(element.ActualHeight);
        if (width <= 0 || height <= 0) return;
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        var background = new DrawingVisual();
        using (DrawingContext context = background.RenderOpen())
            context.DrawRectangle((Brush)element.FindResource("AppBackgroundBrush"), null, new Rect(element.RenderSize));
        bitmap.Render(background);
        bitmap.Render(element);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream file = File.Create(path);
        encoder.Save(file);
    }
}
