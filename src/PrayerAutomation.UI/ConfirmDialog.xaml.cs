// Code-behind for the app-styled confirmation dialog.
using System.Windows;

namespace PrayerAutomation.UI;

/// <summary>Themed confirmation dialog shown instead of the native Windows message box.</summary>
internal partial class ConfirmDialog : Window
{
    /// <summary>Initializes the dialog with the given texts and button behavior.</summary>
    /// <param name="heading">Heading shown in the window and title bar.</param>
    /// <param name="message">Body text.</param>
    /// <param name="confirmText">Label of the primary button.</param>
    /// <param name="cancelText">Label of the cancel button.</param>
    /// <param name="confirmIsDefault">True when Enter should confirm instead of cancel.</param>
    internal ConfirmDialog(string heading, string message, string confirmText,
        string cancelText = "Batal", bool confirmIsDefault = false)
    {
        InitializeComponent();
        Icon = App.LoadApplicationIcon();
        Title = $"{AppBranding.DisplayName} — {heading}";
        HeadingText.Text = heading;
        MessageText.Text = message;
        ConfirmButton.Content = confirmText;
        CancelButton.Content = cancelText;
        if (confirmIsDefault)
            ConfirmButton.IsDefault = true;
        else
            CancelButton.IsDefault = true;
    }

    /// <summary>Shows the dialog modally and returns true when the user confirmed.</summary>
    /// <param name="owner">Owner window used for centering.</param>
    /// <param name="heading">Heading shown in the window and title bar.</param>
    /// <param name="message">Body text.</param>
    /// <param name="confirmText">Label of the primary button.</param>
    /// <param name="cancelText">Label of the cancel button.</param>
    /// <param name="confirmIsDefault">True when Enter should confirm instead of cancel.</param>
    /// <returns>True when the user pressed the primary button.</returns>
    internal static bool Ask(Window owner, string heading, string message, string confirmText,
        string cancelText = "Batal", bool confirmIsDefault = false)
    {
        var dialog = new ConfirmDialog(heading, message, confirmText, cancelText, confirmIsDefault)
        {
            Owner = owner
        };
        return dialog.ShowDialog() == true;
    }

    /// <summary>Confirms the dialog and closes it.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void Confirm_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
