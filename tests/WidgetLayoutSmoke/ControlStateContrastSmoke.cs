// Smoke test verifying WCAG-style foreground/background contrast of themed WPF controls in normal, hover, pressed, selected, disabled and popup states across all themes.
extern alias ProductUi;

using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

/// <summary>Verifies theme-aware control state contrast against the product's theme catalog.</summary>
internal static class ControlStateContrastSmoke
{
    private static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    private static readonly DependencyPropertyKey MouseOverKey = (DependencyPropertyKey)typeof(UIElement)
        .GetField("IsMouseOverPropertyKey", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
    private static readonly DependencyPropertyKey PressedKey = (DependencyPropertyKey)typeof(ButtonBase)
        .GetField("IsPressedPropertyKey", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
    private static readonly DependencyPropertyKey HighlightedKey = (DependencyPropertyKey)typeof(ComboBoxItem)
        .GetField("IsHighlightedPropertyKey", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

    /// <summary>Applies every theme in turn, measures control state contrast, and reports results via console and the errors list.</summary>
    /// <param name="application">Application whose themes are applied.</param>
    /// <param name="errors">Accumulator for contrast failures.</param>
    internal static void Run(Application application, List<string> errors)
    {
        Assembly assembly = typeof(ProductUi::PrayerAutomation.UI.App).Assembly;
        Type catalog = assembly.GetType("PrayerAutomation.UI.ThemeCatalog", true)!;
        Type themeManager = assembly.GetType("PrayerAutomation.UI.ThemeManager", true)!;
        MethodInfo apply = themeManager.GetMethod("Apply", Flags)!;
        var themes = ((IEnumerable)catalog.GetProperty("All", Flags)!.GetValue(null)!).Cast<object>().ToArray();
        int checks = 0;
        int initialErrors = errors.Count;
        double minimum = double.MaxValue;
        foreach (object theme in themes)
        {
            string id = theme.GetType().GetProperty("Id", Flags)!.GetValue(theme)!.ToString()!;
            apply.Invoke(null, [application, id]);
            var button = new Button { Content = "Button" };
            var textBox = new TextBox { Text = "TextBox selected text" };
            var comboBox = new ComboBox { Items = { "First", "Second", "Disabled" }, SelectedIndex = 0 };
            var checkBox = new CheckBox { Content = "CheckBox" };
            var listBox = new ListBox { Items = { "First", "Second" }, Height = 80, SelectedIndex = 0 };
            var tab = new TabItem { Header = "Selected tab" };
            var otherTab = new TabItem { Header = "Other tab" };
            var tabs = new TabControl { Items = { tab, otherTab }, Height = 60 };
            var toolTip = new ToolTip { Content = "ToolTip" };
            var group = new GroupBox { Header = "GroupBox", Content = new TextBlock { Text = "Notice" } };
            button.ToolTip = toolTip;
            var panel = new StackPanel { Children = { button, textBox, comboBox, checkBox, listBox, tabs, group } };
            var window = new Window { Content = panel, Width = 420, Height = 520, Opacity = 0, ShowInTaskbar = false, ShowActivated = false };
            window.SetResourceReference(Control.BackgroundProperty, "SurfaceBrush");
            window.SetResourceReference(Control.ForegroundProperty, "PrimaryTextBrush");
            try
            {
                window.Show();
                Pump();
                foreach (string state in new[] { "normal", "hover", "pressed", "disabled" })
                {
                    button.SetValue(MouseOverKey, state is "hover" or "pressed");
                    button.SetValue(PressedKey, state == "pressed");
                    button.IsEnabled = state != "disabled";
                    Pump();
                    Check(button.Foreground, Surface(button, window.Background), $"Button/{state}/{id}");
                }
                button.SetValue(MouseOverKey, false);
                button.SetValue(PressedKey, false);
                button.IsEnabled = true;
                Check(textBox.Foreground, Surface(textBox, window.Background), $"TextBox/normal/{id}");
                textBox.Focus();
                textBox.SelectAll();
                Pump();
                Check(textBox.SelectionTextBrush, textBox.SelectionBrush, $"TextBox/selection/{id}");
                textBox.IsEnabled = false;
                Pump();
                Check(textBox.Foreground, Surface(textBox, window.Background), $"TextBox/disabled/{id}");
                Check(comboBox.Foreground, Surface(comboBox, window.Background), $"ComboBox/collapsed/{id}");
                comboBox.IsDropDownOpen = true;
                Pump();
                if (comboBox.Template.FindName("PART_Popup", comboBox) is Popup popup && popup.Child is DependencyObject popupRoot)
                {
                    Border? popupSurface = Descendants(popupRoot).OfType<Border>().FirstOrDefault(border => Opaque(border.Background));
                    Check(comboBox.Foreground, popupSurface?.Background, $"ComboBox/popup/{id}");
                }
                else errors.Add($"ComboBox popup tidak terwujud/{id}.");
                for (int index = 0; index < comboBox.Items.Count; index++)
                {
                    if (comboBox.ItemContainerGenerator.ContainerFromIndex(index) is not ComboBoxItem item)
                    {
                        errors.Add($"ComboBox item {index} tidak terwujud/{id}.");
                        continue;
                    }
                    item.ApplyTemplate();
                    foreach (string state in new[] { "normal", "highlighted", "selected", "disabled" })
                    {
                        item.IsSelected = state == "selected";
                        item.SetValue(HighlightedKey, state == "highlighted");
                        item.IsEnabled = state != "disabled";
                        Pump();
                        Check(item.Foreground, Surface(item, window.Background), $"ComboBoxItem/{state}/{id}/{index}");
                    }
                    item.SetValue(HighlightedKey, false);
                    item.IsEnabled = true;
                }
                comboBox.IsDropDownOpen = false;
                comboBox.IsEnabled = false;
                Pump();
                Check(comboBox.Foreground, Surface(comboBox, window.Background), $"ComboBox/disabled/{id}");
                foreach (bool enabled in new[] { true, false })
                {
                    checkBox.IsEnabled = enabled;
                    foreach (bool isChecked in new[] { false, true })
                    {
                        checkBox.IsChecked = isChecked;
                        Pump();
                        Check(checkBox.Foreground, window.Background, $"CheckBox/label/{enabled}/{isChecked}/{id}");
                        if (checkBox.Template.FindName("CheckBoxGlyphBackground", checkBox) is Border glyph &&
                            checkBox.Template.FindName("CheckMarkPath", checkBox) is Path mark && isChecked)
                            Check(mark.Stroke, glyph.Background, $"CheckBox/mark/{enabled}/{id}");
                    }
                }
                foreach (TabItem item in new[] { tab, otherTab })
                {
                    foreach (string state in new[] { "normal", "hover", "selected", "disabled" })
                    {
                        item.IsSelected = state == "selected";
                        item.SetValue(MouseOverKey, state == "hover");
                        item.IsEnabled = state != "disabled";
                        Pump();
                        Check(item.Foreground, Surface(item, window.Background), $"TabItem/{state}/{id}");
                    }
                }
                foreach (ListBoxItem item in listBox.Items.Cast<object>().Select(value => listBox.ItemContainerGenerator.ContainerFromItem(value)).OfType<ListBoxItem>())
                {
                    foreach (string state in new[] { "normal", "hover", "selected", "disabled" })
                    {
                        item.IsSelected = state == "selected";
                        item.SetValue(MouseOverKey, state == "hover");
                        item.IsEnabled = state != "disabled";
                        Pump();
                        Check(item.Foreground, Surface(item, window.Background), $"ListBoxItem/{state}/{id}");
                    }
                }
                Check(group.Foreground, Surface(group, window.Background), $"GroupBox/{id}");
                Check(toolTip.Foreground, Surface(toolTip, window.Background), $"ToolTip/{id}");
            }
            finally
            {
                window.Close();
                Pump();
            }
        }
        apply.Invoke(null, [application, "light"]);
        Console.WriteLine($"CONTROL_STATE_CONTRAST themes={themes.Length} checks={checks} minimum={minimum:0.000}:1 failures={errors.Count - initialErrors}");

        /// <summary>Checks one foreground/background pair, records the ratio, and appends an error when below 4.5:1.</summary>
        /// <param name="foreground">Measured foreground brush.</param>
        /// <param name="background">Measured background brush.</param>
        /// <param name="label">Check label used in error messages.</param>
        void Check(Brush? foreground, Brush? background, string label)
        {
            checks++;
            if (foreground is not SolidColorBrush fg || background is not SolidColorBrush bg)
            {
                errors.Add($"Brush belum solid: {label}.");
                return;
            }
            double ratio = Ratio(fg.Color, bg.Color);
            minimum = Math.Min(minimum, ratio);
            if (ratio < 4.5) errors.Add($"Kontras {label}: {ratio:0.000}:1 fg={fg.Color} bg={bg.Color}.");
        }
    }

    /// <summary>Returns the brush of the first opaque descendant border, falling back to the control or window background.</summary>
    /// <param name="control">Control to inspect.</param>
    /// <param name="fallback">Fallback brush when neither yields an opaque surface.</param>
    /// <returns>The surface brush, or null.</returns>
    private static Brush? Surface(Control control, Brush fallback)
    {
        control.ApplyTemplate();
        return Descendants(control).OfType<Border>().FirstOrDefault(border => Opaque(border.Background))?.Background
            ?? (Opaque(control.Background) ? control.Background : fallback);
    }

    /// <summary>Returns true when the brush is fully opaque solid color.</summary>
    /// <param name="brush">Brush to test.</param>
    /// <returns>True when opaque solid color.</returns>
    private static bool Opaque(Brush? brush) => brush is SolidColorBrush color && color.Color.A == 255 && brush.Opacity == 1;

    /// <summary>Enumerates visual descendants of a node.</summary>
    /// <param name="node">Root visual.</param>
    /// <returns>Descendant dependency objects in depth-first order.</returns>
    private static IEnumerable<DependencyObject> Descendants(DependencyObject node)
    {
        if (node is not Visual && node is not System.Windows.Media.Media3D.Visual3D) yield break;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(node, i);
            yield return child;
            foreach (DependencyObject descendant in Descendants(child)) yield return descendant;
        }
    }

    /// <summary>Pumps the dispatcher with context-idle priority.</summary>
    private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));

    /// <summary>Computes the WCAG contrast ratio between two colors.</summary>
    /// <param name="a">First color.</param>
    /// <param name="b">Second color.</param>
    /// <returns>Contrast ratio (1.0 or higher).</returns>
    private static double Ratio(Color a, Color b)
    {
        /// <summary>Relative luminance of a color (WCAG formula).</summary>
        /// <param name="c">Color to evaluate.</param>
        /// <returns>Relative luminance between 0 and 1.</returns>
        static double L(Color c)
        {
            /// <summary>Linearizes one sRGB channel (WCAG formula).</summary>
            /// <param name="value">Channel value 0-255.</param>
            /// <returns>Linearized channel value.</returns>
            static double C(byte value)
            {
                double x = value / 255.0;
                return x <= .04045 ? x / 12.92 : Math.Pow((x + .055) / 1.055, 2.4);
            }
            return .2126 * C(c.R) + .7152 * C(c.G) + .0722 * C(c.B);
        }
        double x = L(a), y = L(b);
        return (Math.Max(x, y) + .05) / (Math.Min(x, y) + .05);
    }
}
