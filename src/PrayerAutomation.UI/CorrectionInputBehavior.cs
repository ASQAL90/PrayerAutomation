using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;

namespace PrayerAutomation.UI;

internal static class CorrectionInputBehavior
{
    private const int MinimumMinutes = -30;
    private const int MaximumMinutes = 30;
    private const int MinimumCalendarDays = -7;
    private const int MaximumCalendarDays = 7;

    public static void Attach(TextBox textBox, Button decreaseButton, Button increaseButton) =>
        AttachBounded(textBox, decreaseButton, increaseButton, MinimumMinutes, MaximumMinutes);

    public static void AttachDateCorrection(TextBox textBox, Button decreaseButton, Button increaseButton) =>
        AttachBounded(textBox, decreaseButton, increaseButton, MinimumCalendarDays, MaximumCalendarDays);

    public static void AttachMinutesAfterSunrise(TextBox textBox, Button decreaseButton, Button increaseButton) =>
        AttachBounded(textBox, decreaseButton, increaseButton, 0, 180);

    private static void AttachBounded(TextBox textBox, Button decreaseButton, Button increaseButton, int minimum, int maximum)
    {
        ArgumentNullException.ThrowIfNull(textBox);
        ArgumentNullException.ThrowIfNull(decreaseButton);
        ArgumentNullException.ThrowIfNull(increaseButton);

        textBox.PreviewTextInput += (_, e) =>
        {
            if (!IsReplacementAllowedWithinBounds(textBox.Text, textBox.SelectionStart, textBox.SelectionLength, e.Text, minimum, maximum))
                e.Handled = true;
        };
        System.Windows.DataObject.AddPastingHandler(textBox, (sender, e) =>
        {
            if (sender is not TextBox editor || !e.DataObject.GetDataPresent(System.Windows.DataFormats.UnicodeText))
            {
                e.CancelCommand();
                return;
            }

            string replacement = e.DataObject.GetData(System.Windows.DataFormats.UnicodeText) as string ?? string.Empty;
            if (!IsReplacementAllowedWithinBounds(editor.Text, editor.SelectionStart, editor.SelectionLength, replacement, minimum, maximum))
                e.CancelCommand();
        });

        void UpdateButtons()
        {
            bool valid = TryParseWithinBounds(textBox.Text, minimum, maximum, out int value);
            decreaseButton.IsEnabled = valid && value > minimum;
            increaseButton.IsEnabled = valid && value < maximum;
        }

        void Adjust(int delta)
        {
            if (!TryParseWithinBounds(textBox.Text, minimum, maximum, out int value)) return;
            textBox.Text = Math.Clamp(value + delta, minimum, maximum)
                .ToString(CultureInfo.InvariantCulture);
            textBox.CaretIndex = textBox.Text.Length;
            textBox.Focus();
        }

        textBox.TextChanged += (_, _) => UpdateButtons();
        decreaseButton.Click += (_, _) => Adjust(-1);
        increaseButton.Click += (_, _) => Adjust(1);
        UpdateButtons();
    }

    public static bool IsReplacementAllowed(string currentText, int selectionStart, int selectionLength, string replacementText)
        => IsReplacementAllowedWithinBounds(currentText, selectionStart, selectionLength, replacementText, MinimumMinutes, MaximumMinutes);

    public static bool IsDateReplacementAllowed(string currentText, int selectionStart, int selectionLength, string replacementText)
        => IsReplacementAllowedWithinBounds(currentText, selectionStart, selectionLength, replacementText, MinimumCalendarDays, MaximumCalendarDays);

    private static bool IsReplacementAllowedWithinBounds(string currentText, int selectionStart, int selectionLength, string replacementText, int minimum, int maximum)
    {
        if (selectionStart < 0 || selectionStart > currentText.Length ||
            selectionLength < 0 || selectionLength > currentText.Length - selectionStart)
            return false;

        string candidate = currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, replacementText);
        if (candidate.Length == 0 || candidate is "+" or "-") return true;
        return TryParseBoundedCorrection(candidate, minimum, maximum, out _);
    }

    public static bool TryParseCorrection(string? text, out int minutes)
        => TryParseWithinBounds(text, MinimumMinutes, MaximumMinutes, out minutes);

    public static bool TryParseDateCorrection(string? text, out int days)
        => TryParseWithinBounds(text, MinimumCalendarDays, MaximumCalendarDays, out days);

    private static bool TryParseWithinBounds(string? text, int minimum, int maximum, out int value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = 0;
            return true;
        }

        return TryParseBoundedCorrection(text, minimum, maximum, out value);
    }

    private static bool TryParseBoundedCorrection(string text, int minimum, int maximum, out int value) =>
        int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value) &&
        value >= minimum && value <= maximum;
}
