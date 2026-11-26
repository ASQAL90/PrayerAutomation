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
    private const int MaxVisiblePopups = 3;
    private const double PopupGap = 8;
    private const double ScreenMargin = 16;
    private static readonly List<ReminderPopup> ActivePopups = [];
    private static readonly Queue<ReminderPopup> PendingPrayerPopups = new();
    private static readonly Queue<ReminderPopup> PendingAdditionalPopups = new();
    private readonly DispatcherTimer? _closeTimer;
    private readonly string? _stopKey;
    private readonly Action<string>? _onStop;
    private readonly Action<string>? _onAgain;
    private readonly Action? _onNavigate;

    /// <summary>Initializes the popup UI for a prayer reminder.</summary>
    /// <param name="prayer">Prayer label shown in the title.</param>
    /// <param name="message">Reminder message body.</param>
    private ReminderPopup(string prayer, string message)
        : this($"Waktu {prayer}", message, null, null, null, null, null)
    {
        Title = "Pengingat waktu sholat";
        AutomationProperties.SetName(this, $"Pengingat waktu {prayer}");
    }

    /// <summary>Initializes a popup, optionally with stop/remind-again actions.</summary>
    /// <param name="heading">Heading shown in the popup.</param>
    /// <param name="message">Reminder message body.</param>
    /// <param name="stopKey">Schedule key passed to callbacks, or null for an auto-closing prayer reminder.</param>
    /// <param name="onStop">Callback for the permanent-stop button.</param>
    /// <param name="onAgain">Callback for the remind-again button.</param>
    /// <param name="onNavigate">Optional callback that opens the matching dzikir tab.</param>
    /// <param name="navigateLabel">Caption of the optional navigation button.</param>
    private ReminderPopup(string heading, string message, string? stopKey,
        Action<string>? onStop, Action<string>? onAgain,
        Action? onNavigate, string? navigateLabel)
    {
        _stopKey = stopKey;
        _onStop = onStop;
        _onAgain = onAgain;
        _onNavigate = onNavigate;
        Title = stopKey is null ? "Pengingat waktu sholat" : "Pengingat tambahan";
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
            ToolTip = stopKey is null ? "Tutup pengingat" : "Hentikan pengingat ini untuk hari ini",
            IsTabStop = false
        };
        closeButton.Click += (_, _) =>
        {
            if (stopKey is not null) _onStop?.Invoke(stopKey);
            Close();
        };
        AutomationProperties.SetAutomationId(closeButton, "ReminderPopupClose");
        AutomationProperties.SetName(closeButton,
            stopKey is null ? "Tutup pengingat waktu sholat" : "Hentikan pengingat");

        var header = new Grid();
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var title = new TextBlock
        {
            Text = heading,
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
        if (onNavigate is not null && !string.IsNullOrWhiteSpace(navigateLabel))
        {
            var navigateButton = new Button
            {
                Content = navigateLabel,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 12, 0, 0),
                Background = ThemeManager.ToBrush(palette.SurfaceAlt),
                BorderBrush = ThemeManager.ToBrush(palette.Border),
                Foreground = ThemeManager.ToBrush(palette.PrimaryText)
            };
            navigateButton.Click += (_, _) => { _onNavigate?.Invoke(); Close(); };
            AutomationProperties.SetAutomationId(navigateButton, "ReminderPopupNavigate");
            AutomationProperties.SetName(navigateButton, navigateLabel);
            panel.Children.Add(navigateButton);
        }
        if (stopKey is not null)
        {
            var againButton = new Button
            {
                Content = "Ingatkan lagi",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 12, 0, 0),
                Background = ThemeManager.ToBrush(palette.SurfaceAlt),
                BorderBrush = ThemeManager.ToBrush(palette.Border),
                Foreground = ThemeManager.ToBrush(palette.PrimaryText)
            };
            againButton.Click += (_, _) => { _onAgain?.Invoke(stopKey); Close(); };
            AutomationProperties.SetAutomationId(againButton, "ReminderPopupAgain");
            AutomationProperties.SetName(againButton, "Ingatkan lagi");
            panel.Children.Add(againButton);
        }
        Content = new Border
        {
            Background = ThemeManager.ToBrush(palette.Surface),
            BorderBrush = ThemeManager.ToBrush(palette.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(palette.CornerRadius),
            Padding = new Thickness(4),
            Child = panel
        };
        AutomationProperties.SetName(this, heading);
        Closed += (_, _) =>
        {
            ActivePopups.Remove(this);
            PumpQueue();
        };
        if (stopKey is null)
        {
            _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(12) };
            _closeTimer.Tick += (_, _) => { _closeTimer.Stop(); Close(); };
        }
    }

    /// <summary>Creates and shows a prayer reminder popup, stacking it above existing popups.</summary>
    /// <param name="prayer">Prayer label shown in the title.</param>
    /// <param name="message">Reminder message body.</param>
    public static void Show(string prayer, string message) => EnqueueOrShow(new ReminderPopup(prayer, message));

    /// <summary>Creates and shows a custom reminder with stop, repeat-again, and optional navigation.</summary>
    /// <param name="heading">Heading shown in the popup.</param>
    /// <param name="message">Reminder message body.</param>
    /// <param name="key">Schedule key passed to both callbacks.</param>
    /// <param name="onStop">Invoked when the user permanently stops the reminder.</param>
    /// <param name="onAgain">Invoked when the user asks to be reminded again.</param>
    /// <param name="onNavigate">Optional callback that opens the matching dzikir tab.</param>
    /// <param name="navigateLabel">Caption of the optional navigation button.</param>
    public static void ShowCustom(string heading, string message, string key,
        Action<string> onStop, Action<string> onAgain,
        Action? onNavigate = null, string? navigateLabel = null) =>
        EnqueueOrShow(new ReminderPopup(heading, message, key, onStop, onAgain, onNavigate, navigateLabel));

    /// <summary>Shows a popup now, or queues it when the screen has no room for another window.</summary>
    /// <param name="popup">Popup to show or queue.</param>
    private static void EnqueueOrShow(ReminderPopup popup)
    {
        if (ActivePopups.Count < MaxVisiblePopups && ShowCore(popup)) return;
        QueuePopup(popup);
    }

    /// <summary>Adds a popup to the queue matching its kind.</summary>
    /// <param name="popup">Popup to queue.</param>
    private static void QueuePopup(ReminderPopup popup)
    {
        if (popup._stopKey is null) PendingPrayerPopups.Enqueue(popup);
        else PendingAdditionalPopups.Enqueue(popup);
    }

    /// <summary>Tries to show a popup, refusing a slot when it would overlap the visible stack.</summary>
    /// <param name="popup">Popup to show.</param>
    /// <returns>True when the popup is visible; false when it was hidden and must wait in the queue.</returns>
    private static bool ShowCore(ReminderPopup popup)
    {
        if (ActivePopups.Count >= MaxVisiblePopups) return false;
        // Measure the auto-sized window before deciding whether it still fits.
        popup.Opacity = 0;
        if (!popup.IsVisible) popup.Show();
        popup.UpdateLayout();
        double availableHeight = SystemParameters.WorkArea.Height - (ScreenMargin * 2);
        double requiredHeight = popup.ActualHeight + (PopupGap * ActivePopups.Count) +
                                ActivePopups.Sum(other => other.ActualHeight);
        if (ActivePopups.Count > 0 && requiredHeight > availableHeight)
        {
            popup.Hide();
            return false;
        }
        ActivePopups.Add(popup);
        Place(popup);
        if (popup._stopKey is null) popup._closeTimer?.Start();
        return true;
    }

    /// <summary>Fills free slots after a popup closes, giving prayer popups priority.</summary>
    private static void PumpQueue()
    {
        while (ActivePopups.Count < MaxVisiblePopups)
        {
            if (PendingPrayerPopups.Count > 0)
            {
                ReminderPopup popup = PendingPrayerPopups.Dequeue();
                if (ShowCore(popup)) continue;
                PendingPrayerPopups.Enqueue(popup);
                break;
            }
            if (PendingAdditionalPopups.Count > 0)
            {
                ReminderPopup popup = PendingAdditionalPopups.Dequeue();
                if (ShowCore(popup)) continue;
                PendingAdditionalPopups.Enqueue(popup);
                break;
            }
            break;
        }
    }

    /// <summary>Places a visible popup in the bottom-right stack.</summary>
    /// <param name="popup">Popup to place.</param>
    private static void Place(ReminderPopup popup)
    {
        double bottom = SystemParameters.WorkArea.Bottom - ScreenMargin;
        foreach (ReminderPopup other in ActivePopups)
        {
            if (ReferenceEquals(other, popup)) continue;
            bottom -= other.ActualHeight + PopupGap;
        }
        popup.Left = SystemParameters.WorkArea.Right - popup.ActualWidth - ScreenMargin;
        popup.Top = Math.Max(SystemParameters.WorkArea.Top + ScreenMargin, bottom - popup.ActualHeight);
        popup.Opacity = 1;
    }
}
