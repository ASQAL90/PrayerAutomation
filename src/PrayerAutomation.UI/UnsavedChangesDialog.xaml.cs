using System.Windows;

namespace PrayerAutomation.UI;

internal enum UnsavedChangesChoice { Cancel, Save, Discard }

internal partial class UnsavedChangesDialog : Window
{
    internal UnsavedChangesChoice Choice { get; private set; } = UnsavedChangesChoice.Cancel;

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

    private void Save_Click(object sender, RoutedEventArgs e) { Choice = UnsavedChangesChoice.Save; Close(); }
    private void Discard_Click(object sender, RoutedEventArgs e) { Choice = UnsavedChangesChoice.Discard; Close(); }
    private void Cancel_Click(object sender, RoutedEventArgs e) { Choice = UnsavedChangesChoice.Cancel; Close(); }
}
