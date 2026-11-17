using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace PrayerAutomation.UI;

/// <summary>Frameless, topmost auto-closing reminder popup shown near the bottom-right of the work area.</summary>
internal sealed class ReminderPopup : Window
{
    private static readonly List<ReminderPopup> ActivePopups = [];
    private readonly DispatcherTimer _closeTimer;

    /// <summary>Initializes the popup UI for a prayer reminder.</summary>
    /// <param name="prayer">Prayer label shown in the title.</param>
    /// <param name="message">Reminder message body.</param>
    private ReminderPopup(string prayer, string message)
    {
        Title = "Pengingat waktu sholat";
        Width = 360;
        SizeToContent = SizeToContent.Height;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;
        ShowActivated = false;
        Background = Brushes.Transparent;
        ThemePalette palette = ThemeManager.Current;

        var closeButton = new Button
        {
            Content = "✕",
            Width = 26,
            MinWidth = 26,
            Height = 26,
            MinHeight = 26,
            Padding = new Thickness(0),
            Margin = new Thickness(10, 0, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = ThemeManager.ToBrush(palette.MutedText),
            FontSize = 12,
            ToolTip = "Tutup pengingat",
            IsTabStop = false
        };
        closeButton.Click += (_, _) => Close();
        AutomationProperties.SetAutomationId(closeButton, "ReminderPopupClose");
        AutomationProperties.SetName(closeButton, "Tutup pengingat waktu sholat");

        var header = new Grid();
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var title = new TextBlock
        {
            Text = $"Waktu {prayer}",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = ThemeManager.ToBrush(palette.Accent)
        };
        Grid.SetColumn(closeButton, 1);
        header.Children.Add(title);
        header.Children.Add(closeButton);

        var panel = new StackPanel { Margin = new Thickness(18, 14, 14, 16) };
        panel.Children.Add(header);
        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 0),
            Foreground = ThemeManager.ToBrush(palette.PrimaryText)
        });
        Content = new Border
        {
            Background = ThemeManager.ToBrush(palette.Surface),
            BorderBrush = ThemeManager.ToBrush(palette.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(palette.CornerRadius),
            Padding = new Thickness(4),
            Child = panel
        };
        AutomationProperties.SetName(this, $"Pengingat waktu {prayer}");
        Closed += (_, _) => ActivePopups.Remove(this);
        _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(12) };
        _closeTimer.Tick += (_, _) => { _closeTimer.Stop(); Close(); };
    }

    /// <summary>Creates and shows a reminder popup, stacking it above existing popups.</summary>
    /// <param name="prayer">Prayer label shown in the title.</param>
    /// <param name="message">Reminder message body.</param>
    public static void Show(string prayer, string message)
    {
        var popup = new ReminderPopup(prayer, message);
        ActivePopups.Add(popup);
        // Position after layout so the auto-sized height is known; hide the first frame.
        popup.Opacity = 0;
        popup.Show();
        popup.UpdateLayout();
        double bottom = SystemParameters.WorkArea.Bottom - 16;
        foreach (ReminderPopup other in ActivePopups)
        {
            if (ReferenceEquals(other, popup)) continue;
            bottom -= other.ActualHeight + 8;
        }
        popup.Left = SystemParameters.WorkArea.Right - popup.ActualWidth - 16;
        popup.Top = Math.Max(SystemParameters.WorkArea.Top + 16, bottom - popup.ActualHeight);
        popup.Opacity = 1;
        popup._closeTimer.Start();
    }
}
