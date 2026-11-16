using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PrayerAutomation.UI;

internal partial class AboutWindow : Window
{
    private readonly Action<Uri> _launchLink;
    private static readonly string[] AllowedLinks =
    [
        "mailto:madinalani@gmail.com",
        "https://linkedin.com/in/madinalani",
        "https://github.com/ASQAL90"
    ];

    public AboutWindow() : this(LaunchLink) { }

    internal AboutWindow(Action<Uri> launchLink)
    {
        _launchLink = launchLink ?? throw new ArgumentNullException(nameof(launchLink));
        InitializeComponent();
    }

    private static void LaunchLink(Uri uri) =>
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });

    private void OnLinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        if (!e.Uri.IsAbsoluteUri || !AllowedLinks.Contains(e.Uri.AbsoluteUri, StringComparer.Ordinal)) return;
        LinkStatus.Visibility = Visibility.Collapsed;
        try { _launchLink(e.Uri); }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or NotSupportedException)
        {
            LinkStatus.Text = "Tautan tidak dapat dibuka. Periksa aplikasi browser atau email bawaan Windows.";
            LinkStatus.Visibility = Visibility.Visible;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Escape) return;
        e.Handled = true;
        Close();
    }

    private void OnHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
