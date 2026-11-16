using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Binding = System.Windows.Data.Binding;
using Control = System.Windows.Controls.Control;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBoxBase = System.Windows.Controls.Primitives.TextBoxBase;
using ScrollBar = System.Windows.Controls.Primitives.ScrollBar;

namespace PrayerAutomation.UI;

// Keep the Windows/WPF template geometry, but prevent its hard-coded state
// colors from painting over the owner's theme-aware foreground/background.
internal static class ThemeControlChrome
{
    internal static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled", typeof(bool), typeof(ThemeControlChrome), new PropertyMetadata(false, OnEnabledChanged));

    public static bool GetEnabled(DependencyObject target) => (bool)target.GetValue(EnabledProperty);
    public static void SetEnabled(DependencyObject target, bool value) => target.SetValue(EnabledProperty, value);

    private static void OnEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (target is not Control control) return;
        control.Loaded -= OnLoaded;
        if (control is ComboBox comboBox) comboBox.DropDownOpened -= OnDropDownOpened;
        if (e.NewValue is not true) return;
        control.Loaded += OnLoaded;
        if (control is ComboBox combo) combo.DropDownOpened += OnDropDownOpened;
        if (control.IsLoaded) Apply(control);
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is Control control) Apply(control);
    }

    private static void OnDropDownOpened(object? sender, EventArgs e)
    {
        if (sender is ComboBox comboBox) Apply(comboBox);
    }

    private static void Apply(Control control)
    {
        control.ApplyTemplate();
        if (control is ComboBox comboBox)
        {
            // The editable template paints its own white Border when unfocused.
            foreach (Border border in Descendants(comboBox).OfType<Border>()
                .Where(border => ReferenceEquals(border.TemplatedParent, comboBox)))
                BindSurface(border, comboBox);
            if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is System.Windows.Controls.TextBox editor)
            {
                editor.SetResourceReference(TextBoxBase.SelectionBrushProperty, "AccentBrush");
                editor.SetResourceReference(TextBoxBase.SelectionTextBrushProperty, "AccentTextBrush");
            }
            foreach (ToggleButton toggle in Descendants(comboBox).OfType<ToggleButton>()
                .Where(toggle => ReferenceEquals(toggle.TemplatedParent, comboBox)))
            {
                toggle.ApplyTemplate();
                foreach (Border border in Descendants(toggle).OfType<Border>()
                    .Where(border => ReferenceEquals(border.TemplatedParent, toggle)))
                    BindSurface(border, comboBox);
                foreach (Path arrow in Descendants(toggle).OfType<Path>())
                    Bind(arrow, Shape.FillProperty, comboBox, nameof(Control.Foreground));
            }
            if (comboBox.Template.FindName("PART_Popup", comboBox) is Popup popup && popup.Child is DependencyObject root)
            {
                Border? popupSurface = Descendants(root).OfType<Border>().FirstOrDefault();
                if (popupSurface is not null) BindSurface(popupSurface, comboBox);
            }
        }
        else if (control is ScrollBar scrollBar)
        {
            foreach (Border border in Descendants(scrollBar).OfType<Border>()
                .Where(border => ReferenceEquals(border.TemplatedParent, scrollBar)))
                BindSurface(border, scrollBar);
            foreach (RepeatButton button in Descendants(scrollBar).OfType<RepeatButton>()
                .Where(button => ReferenceEquals(button.TemplatedParent, scrollBar)))
            {
                button.SetResourceReference(FrameworkElement.StyleProperty, "ThemedScrollRepeatButton");
                button.ApplyTemplate();
                foreach (Border border in Descendants(button).OfType<Border>()
                    .Where(border => ReferenceEquals(border.TemplatedParent, button)))
                    BindSurface(border, button);
            }
            foreach (Thumb thumb in Descendants(scrollBar).OfType<Thumb>()
                .Where(thumb => ReferenceEquals(thumb.TemplatedParent, scrollBar)))
            {
                thumb.SetResourceReference(FrameworkElement.StyleProperty, "ThemedScrollThumb");
                thumb.ApplyTemplate();
                foreach (Shape shape in Descendants(thumb).OfType<Shape>()
                    .Where(shape => ReferenceEquals(shape.TemplatedParent, thumb)))
                    Bind(shape, Shape.FillProperty, thumb, nameof(Control.Background));
            }
            // RepeatButton content is realized after its style/template is applied.
            scrollBar.UpdateLayout();
            foreach (Path arrow in Descendants(scrollBar).OfType<Path>())
                Bind(arrow, Shape.FillProperty, FindContentOwner(arrow) ?? scrollBar, nameof(Control.Foreground));
        }
        else if (control is ComboBoxItem item)
        {
            // Generated string content can resolve the application TextBlock
            // style instead of inheriting the highlighted item's foreground.
            foreach (TextBlock text in Descendants(item).OfType<TextBlock>())
                Bind(text, TextBlock.ForegroundProperty, item, nameof(Control.Foreground));
        }
        else
        {
            foreach (Border border in Descendants(control).OfType<Border>()
                .Where(border => ReferenceEquals(border.TemplatedParent, control)))
                BindSurface(border, control);
        }
        // A generated header/content TextBlock otherwise picks up the global
        // TextBlock style rather than the selected/pressed control foreground.
        foreach (TextBlock text in Descendants(control).OfType<TextBlock>().Where(text =>
            ReferenceEquals(FindContentOwner(text), control) &&
            text.ReadLocalValue(TextBlock.ForegroundProperty) == DependencyProperty.UnsetValue))
            Bind(text, TextBlock.ForegroundProperty, control, nameof(Control.Foreground));
    }

    private static Control? FindContentOwner(DependencyObject node)
    {
        while (node is Visual or System.Windows.Media.Media3D.Visual3D)
        {
            node = VisualTreeHelper.GetParent(node)!;
            if (node is Control owner) return owner;
        }
        return null;
    }

    private static void BindSurface(Border border, Control owner)
    {
        Bind(border, Border.BackgroundProperty, owner, nameof(Control.Background));
        Bind(border, Border.BorderBrushProperty, owner, nameof(Control.BorderBrush));
    }

    private static void Bind(DependencyObject target, DependencyProperty property, Control owner, string sourceProperty)
        => BindingOperations.SetBinding(target, property, new Binding(sourceProperty) { Source = owner, Mode = BindingMode.OneWay });

    private static IEnumerable<DependencyObject> Descendants(DependencyObject node)
    {
        if (node is not Visual && node is not System.Windows.Media.Media3D.Visual3D) yield break;
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(node, index);
            yield return child;
            foreach (DependencyObject descendant in Descendants(child)) yield return descendant;
        }
    }
}
