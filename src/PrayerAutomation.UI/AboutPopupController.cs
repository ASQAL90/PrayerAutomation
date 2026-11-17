namespace PrayerAutomation.UI;

// Owned by the tray, not App: closing a popup never changes app lifetime.
/// <summary>Controls the About popup window on behalf of the tray; closing the popup never changes app lifetime.</summary>
internal sealed class AboutPopupController
{
    private readonly Func<AboutWindow> _createWindow;
    private AboutWindow? _window;

    /// <summary>Initializes a controller that creates a real <see cref="AboutWindow"/>.</summary>
    public AboutPopupController() : this(() => new AboutWindow()) { }

    /// <summary>Initializes a controller with a custom window factory.</summary>
    /// <param name="createWindow">Factory used to create the About window on first use.</param>
    /// <exception cref="ArgumentNullException"><paramref name="createWindow"/> is null.</exception>
    internal AboutPopupController(Func<AboutWindow> createWindow)
    {
        _createWindow = createWindow ?? throw new ArgumentNullException(nameof(createWindow));
    }

    /// <summary>Shows the About window, creating it if needed, restoring it from minimized state, and activating it.</summary>
    public void Show()
    {
        if (_window is null)
        {
            _window = _createWindow();
            _window.Closed += (_, _) => _window = null;
        }
        if (_window.WindowState == System.Windows.WindowState.Minimized)
            _window.WindowState = System.Windows.WindowState.Normal;
        if (!_window.IsVisible) _window.Show();
        if (_window.ShowActivated) _window.Activate();
    }

    /// <summary>Closes the About window if it is open.</summary>
    public void Close() => _window?.Close();
}
