namespace PrayerAutomation.UI;

// Owned by the tray, not App: closing a popup never changes app lifetime.
internal sealed class AboutPopupController
{
    private readonly Func<AboutWindow> _createWindow;
    private AboutWindow? _window;

    public AboutPopupController() : this(() => new AboutWindow()) { }

    internal AboutPopupController(Func<AboutWindow> createWindow)
    {
        _createWindow = createWindow ?? throw new ArgumentNullException(nameof(createWindow));
    }

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

    public void Close() => _window?.Close();
}
