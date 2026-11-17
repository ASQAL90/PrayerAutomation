using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PrayerAutomation.UI;

/// <summary>About dialog window showing app branding, version, and allowed external links.</summary>
internal partial class AboutWindow : Window
{
    private readonly Action<Uri> _launchLink;
    private static readonly string[] AllowedLinks =
    [
        "mailto:madinalani@gmail.com",
        "https://linkedin.com/in/madinalani",
        "https://github.com/ASQAL90"
    ];

    /// <summary>Initializes the window using the default link launcher.</summary>
    public AboutWindow() : this(LaunchLink) { }

    /// <summary>Initializes the window with a custom link launcher.</summary>
    /// <param name="launchLink">Handler invoked to open an allowed link.</param>
    /// <exception cref="ArgumentNullException"><paramref name="launchLink"/> is null.</exception>
    internal AboutWindow(Action<Uri> launchLink)
    {
        _launchLink = launchLink ?? throw new ArgumentNullException(nameof(launchLink));
        InitializeComponent();
    }

    /// <summary>Opens a URI with the shell default handler.</summary>
    /// <param name="uri">The link to launch.</param>
    private static void LaunchLink(Uri uri) =>
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });

    /// <summary>Handles hyperlink requests, launching only allow-listed links and surfacing failures in the window.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Navigation event arguments; the URI is validated against the allow list.</param>
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

    /// <summary>Closes the window when the close button is clicked.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    /// <summary>Closes the window when Escape is pressed.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Key event arguments.</param>
    private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Escape) return;
        e.Handled = true;
        Close();
    }

    /// <summary>Starts a window drag when the header is pressed with the left mouse button.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse button event arguments.</param>
    private void OnHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
