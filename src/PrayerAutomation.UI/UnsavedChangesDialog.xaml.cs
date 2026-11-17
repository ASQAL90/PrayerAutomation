// Code-behind for the unsaved-changes confirmation dialog.
using System.Windows;

namespace PrayerAutomation.UI;

/// <summary>User's choice in the unsaved-changes dialog.</summary>
internal enum UnsavedChangesChoice { Cancel, Save, Discard }

/// <summary>Confirmation dialog shown when the user closes a window with unsaved changes.</summary>
internal partial class UnsavedChangesDialog : Window
{
    /// <summary>The choice the user made; defaults to Cancel until a button is clicked.</summary>
    internal UnsavedChangesChoice Choice { get; private set; } = UnsavedChangesChoice.Cancel;

    /// <summary>Initializes the dialog; in discard-only mode the save option is hidden.</summary>
    /// <param name="discardOnly">When true, only discard/cancel options are shown.</param>
    internal UnsavedChangesDialog(bool discardOnly = false)
    {
        InitializeComponent();
        Icon = App.LoadApplicationIcon();
        if (discardOnly)
        {
            Title = AppBranding.DisplayName + " — Batalkan perubahan";
            MessageText.Text = "Buang perubahan form dan kembali ke pengaturan tersimpan? Perubahan yang belum disimpan akan hilang.";
            SaveButton.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>Records the Save choice and closes the dialog.</summary>
    private void Save_Click(object sender, RoutedEventArgs e) { Choice = UnsavedChangesChoice.Save; Close(); }
    /// <summary>Records the Discard choice and closes the dialog.</summary>
    private void Discard_Click(object sender, RoutedEventArgs e) { Choice = UnsavedChangesChoice.Discard; Close(); }
    /// <summary>Records the Cancel choice and closes the dialog.</summary>
    private void Cancel_Click(object sender, RoutedEventArgs e) { Choice = UnsavedChangesChoice.Cancel; Close(); }
}
