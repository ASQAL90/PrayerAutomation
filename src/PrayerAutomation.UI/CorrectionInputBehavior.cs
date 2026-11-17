// Wires bounded numeric correction text boxes to decrease/increase buttons and input validation.
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;

namespace PrayerAutomation.UI;

/// <summary>Attaches bounded-integer editing behavior (validation and stepper buttons) to text boxes.</summary>
internal static class CorrectionInputBehavior
{
    private const int MinimumMinutes = -30;
    private const int MaximumMinutes = 30;
    private const int MinimumCalendarDays = -7;
    private const int MaximumCalendarDays = 7;

    /// <summary>Attaches bounded correction behavior using the prayer-time correction range (−30 to +30 minutes).</summary>
    /// <param name="textBox">The text box to validate.</param>
    /// <param name="decreaseButton">Stepper button that decrements the value.</param>
    /// <param name="increaseButton">Stepper button that increments the value.</param>
    public static void Attach(TextBox textBox, Button decreaseButton, Button increaseButton) =>
        AttachBounded(textBox, decreaseButton, increaseButton, MinimumMinutes, MaximumMinutes);

    /// <summary>Attaches bounded correction behavior using the calendar-day correction range (−7 to +7 days).</summary>
    /// <param name="textBox">The text box to validate.</param>
    /// <param name="decreaseButton">Stepper button that decrements the value.</param>
    /// <param name="increaseButton">Stepper button that increments the value.</param>
    public static void AttachDateCorrection(TextBox textBox, Button decreaseButton, Button increaseButton) =>
        AttachBounded(textBox, decreaseButton, increaseButton, MinimumCalendarDays, MaximumCalendarDays);

    /// <summary>Attaches bounded behavior for the minutes-after-sunrise setting (0 to 180).</summary>
    /// <param name="textBox">The text box to validate.</param>
    /// <param name="decreaseButton">Stepper button that decrements the value.</param>
    /// <param name="increaseButton">Stepper button that increments the value.</param>
    public static void AttachMinutesAfterSunrise(TextBox textBox, Button decreaseButton, Button increaseButton) =>
        AttachBounded(textBox, decreaseButton, increaseButton, 0, 180);

    /// <summary>Wires input filtering, paste handling, and stepper buttons for a bounded integer text box.</summary>
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

        /// <summary>Enables or disables the stepper buttons based on the current text value.</summary>
        void UpdateButtons()
        {
            bool valid = TryParseWithinBounds(textBox.Text, minimum, maximum, out int value);
            decreaseButton.IsEnabled = valid && value > minimum;
            increaseButton.IsEnabled = valid && value < maximum;
        }

        /// <summary>Applies a step change to the current value, clamped to the allowed range.</summary>
        /// <param name="delta">Signed step to apply.</param>
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

    /// <summary>Checks whether a replacement edit is allowed within the prayer-time correction range.</summary>
    /// <param name="currentText">Current text box content.</param>
    /// <param name="selectionStart">Start index of the current selection.</param>
    /// <param name="selectionLength">Length of the current selection.</param>
    /// <param name="replacementText">Text to insert in place of the selection.</param>
    /// <returns><c>true</c> if the resulting text stays within bounds.</returns>
    public static bool IsReplacementAllowed(string currentText, int selectionStart, int selectionLength, string replacementText)
        => IsReplacementAllowedWithinBounds(currentText, selectionStart, selectionLength, replacementText, MinimumMinutes, MaximumMinutes);

    /// <summary>Checks whether a replacement edit is allowed within the calendar-day correction range.</summary>
    /// <param name="currentText">Current text box content.</param>
    /// <param name="selectionStart">Start index of the current selection.</param>
    /// <param name="selectionLength">Length of the current selection.</param>
    /// <param name="replacementText">Text to insert in place of the selection.</param>
    /// <returns><c>true</c> if the resulting text stays within bounds.</returns>
    public static bool IsDateReplacementAllowed(string currentText, int selectionStart, int selectionLength, string replacementText)
        => IsReplacementAllowedWithinBounds(currentText, selectionStart, selectionLength, replacementText, MinimumCalendarDays, MaximumCalendarDays);

    /// <summary>Checks whether a replacement edit keeps the candidate text within the given bounds.</summary>
    private static bool IsReplacementAllowedWithinBounds(string currentText, int selectionStart, int selectionLength, string replacementText, int minimum, int maximum)
    {
        if (selectionStart < 0 || selectionStart > currentText.Length ||
            selectionLength < 0 || selectionLength > currentText.Length - selectionStart)
            return false;

        string candidate = currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, replacementText);
        if (candidate.Length == 0 || candidate is "+" or "-") return true;
        return TryParseBoundedCorrection(candidate, minimum, maximum, out _);
    }

    /// <summary>Parses the prayer-time correction text within the allowed range.</summary>
    /// <param name="text">Text to parse; null or whitespace counts as empty (0).</param>
    /// <param name="minutes">Parsed minutes on success.</param>
    /// <returns><c>true</c> if the text is empty or a signed integer within range.</returns>
    public static bool TryParseCorrection(string? text, out int minutes)
        => TryParseWithinBounds(text, MinimumMinutes, MaximumMinutes, out minutes);

    /// <summary>Parses the calendar-day correction text within the allowed range.</summary>
    /// <param name="text">Text to parse; null or whitespace counts as empty (0).</param>
    /// <param name="days">Parsed days on success.</param>
    /// <returns><c>true</c> if the text is empty or a signed integer within range.</returns>
    public static bool TryParseDateCorrection(string? text, out int days)
        => TryParseWithinBounds(text, MinimumCalendarDays, MaximumCalendarDays, out days);

    /// <summary>Parses empty text as 0, otherwise delegates to bounded parsing.</summary>
    private static bool TryParseWithinBounds(string? text, int minimum, int maximum, out int value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = 0;
            return true;
        }

        return TryParseBoundedCorrection(text, minimum, maximum, out value);
    }

    /// <summary>Parses a signed invariant-culture integer and checks it against the bounds.</summary>
    private static bool TryParseBoundedCorrection(string text, int minimum, int maximum, out int value) =>
        int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value) &&
        value >= minimum && value <= maximum;
}
