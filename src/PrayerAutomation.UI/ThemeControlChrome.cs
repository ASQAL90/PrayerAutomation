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
/// <summary>Applies theme-aware surfaces to control template parts by binding them to owner control properties.</summary>
internal static class ThemeControlChrome
{
    /// <summary>Attached property; set true to opt a control into themed chrome.</summary>
    internal static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled", typeof(bool), typeof(ThemeControlChrome), new PropertyMetadata(false, OnEnabledChanged));

    /// <summary>Gets the attached Enabled value of a target element.</summary>
    /// <param name="target">The dependency object to read.</param>
    /// <returns>True when the control opted in.</returns>
    public static bool GetEnabled(DependencyObject target) => (bool)target.GetValue(EnabledProperty);
    /// <summary>Sets the attached Enabled value of a target element.</summary>
    /// <param name="target">The dependency object to update.</param>
    /// <param name="value">True to opt in.</param>
    public static void SetEnabled(DependencyObject target, bool value) => target.SetValue(EnabledProperty, value);

    /// <summary>Handles Enabled changes: attaches/detaches Loaded and DropDownOpened handlers and applies immediately when loaded.</summary>
    /// <param name="target">The control whose attached value changed.</param>
    /// <param name="e">Change event arguments.</param>
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

    /// <summary>Loaded handler; applies themed chrome once the template is available.</summary>
    /// <param name="sender">The loaded control.</param>
    /// <param name="e">Routed event arguments.</param>
    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is Control control) Apply(control);
    }

    /// <summary>DropDownOpened handler; re-applies chrome when a ComboBox popup opens.</summary>
    /// <param name="sender">The ComboBox.</param>
    /// <param name="e">Event arguments.</param>
    private static void OnDropDownOpened(object? sender, EventArgs e)
    {
        if (sender is ComboBox comboBox) Apply(comboBox);
    }

    /// <summary>Applies the themed chrome to a control's template parts (Borders, ToggleButtons, popups, TextBlocks).</summary>
    /// <param name="control">The control to theme.</param>
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

    /// <summary>Walks up the visual tree to the nearest containing Control.</summary>
    /// <param name="node">The node to start from.</param>
    /// <returns>The owning control, or null.</returns>
    private static Control? FindContentOwner(DependencyObject node)
    {
        while (node is Visual or System.Windows.Media.Media3D.Visual3D)
        {
            node = VisualTreeHelper.GetParent(node)!;
            if (node is Control owner) return owner;
        }
        return null;
    }

    /// <summary>Binds a Border's Background and BorderBrush to the owner control's properties.</summary>
    /// <param name="border">The border to bind.</param>
    /// <param name="owner">The owner control providing the source values.</param>
    private static void BindSurface(Border border, Control owner)
    {
        Bind(border, Border.BackgroundProperty, owner, nameof(Control.Background));
        Bind(border, Border.BorderBrushProperty, owner, nameof(Control.BorderBrush));
    }

    /// <summary>Sets a one-way binding on a target property from an owner control property.</summary>
    /// <param name="target">The target element.</param>
    /// <param name="property">The property to bind.</param>
    /// <param name="owner">The owner control providing the source.</param>
    /// <param name="sourceProperty">The source property name on the owner.</param>
    private static void Bind(DependencyObject target, DependencyProperty property, Control owner, string sourceProperty)
        => BindingOperations.SetBinding(target, property, new Binding(sourceProperty) { Source = owner, Mode = BindingMode.OneWay });

    /// <summary>Enumerates all visual-tree descendants of a node.</summary>
    /// <param name="node">The root node.</param>
    /// <returns>Depth-first descendant sequence.</returns>
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
