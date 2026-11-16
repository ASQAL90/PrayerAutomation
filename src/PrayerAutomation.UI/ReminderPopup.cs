using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PrayerAutomation.UI;

internal sealed class ReminderPopup : Window
{
    private static readonly List<ReminderPopup> ActivePopups = [];
    private readonly DispatcherTimer _closeTimer;

    private ReminderPopup(string prayer, string message)
    {
        Title = "Pengingat waktu sholat";
        Width = 340;
        Height = 150;
        WindowStyle = WindowStyle.ToolWindow;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;
        ShowActivated = false;
        ThemePalette palette = ThemeManager.Current;
        Background = ThemeManager.ToBrush(palette.Surface);
        var panel = new StackPanel { Margin = new Thickness(18) };
        panel.Children.Add(new TextBlock { Text = $"Waktu {prayer}", FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = ThemeManager.ToBrush(palette.Accent) });
        panel.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0), Foreground = ThemeManager.ToBrush(palette.PrimaryText) });
        Content = new Border
        {
            Background = ThemeManager.ToBrush(palette.Surface),
            BorderBrush = ThemeManager.ToBrush(palette.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(palette.CornerRadius),
            Child = panel
        };
        Closed += (_, _) => ActivePopups.Remove(this);
        _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(12) };
        _closeTimer.Tick += (_, _) => { _closeTimer.Stop(); Close(); };
    }

    public static void Show(string prayer, string message)
    {
        var popup = new ReminderPopup(prayer, message);
        int index = ActivePopups.Count;
        popup.Left = SystemParameters.WorkArea.Right - popup.Width - 16;
        popup.Top = SystemParameters.WorkArea.Bottom - popup.Height - 16 - index * (popup.Height + 8);
        ActivePopups.Add(popup);
        popup.Show();
        popup._closeTimer.Start();
    }
}
