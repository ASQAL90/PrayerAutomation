extern alias ProductUi;

using System.Reflection;
using System.IO;
using System.Linq.Expressions;
using Expression = System.Linq.Expressions.Expression;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

internal static class WidgetSettingsInteractionSmoke
{
    private const BindingFlags I = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public static void Run(Application application)
    {
        using (var fixture = new Fixture(application))
        {
            VerifyAppearanceChangesOnActualWidget(fixture);
            VerifyCancelRestoresSavedAppearance(fixture);
            VerifyCloseToTrayRestoresSavedAppearance(fixture);
            VerifyTemporaryVisibilityRestoresPriorVisibility(fixture);
            VerifyHelperButtonConsistency(fixture);
        }
        using (var fixture = new Fixture(application, isolateSave: true))
            VerifySaveRetainsTabAndEstablishesBaseline(fixture);
    }

    private static void VerifyAppearanceChangesOnActualWidget(Fixture fixture)
    {
        string savedSettings = JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType);
        Window widget = fixture.Widget;
        fixture.Select("WidgetModeBox", "detail");
        fixture.Select("WidgetPositionBox", "br");
        Pump();

        Assert(ReferenceEquals(widget, fixture.Widget),
            "Changing widget appearance must update the existing actual widget, not a separate imitation.");
        Assert(fixture.WidgetMode == "detail" && fixture.Widget.Height >= 300,
            "Selecting Medium detail must change the actual widget immediately before Save.");
        Rect workArea = SystemParameters.WorkArea;
        Assert(Math.Abs(widget.Left - (workArea.Right - widget.Width - 12)) < 1 &&
               Math.Abs(widget.Top - (workArea.Bottom - widget.Height - 12)) < 1,
            "The live actual widget must immediately follow the selected bottom-right placement.");
        Assert(JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == savedSettings,
            "Live appearance must not mutate saved settings or scheduler inputs.");
        Console.WriteLine("WIDGET_LIVE_APPEARANCE PASS: actual window, mode, placement, saved-state isolation.");
    }

    private static void VerifyCancelRestoresSavedAppearance(Fixture fixture)
    {
        fixture.Select("ThemeBox", "dark");
        fixture.InvokeMain("CancelSettings_Click", fixture.MainWindow, new RoutedEventArgs());
        Pump();
        Assert(fixture.WidgetMode == "essential" && fixture.Widget.Height == 40 && fixture.Widget.IsVisible,
            $"Cancel must restore the existing widget's saved appearance and visibility: mode={fixture.WidgetMode}, height={fixture.Widget.Height}, visible={fixture.Widget.IsVisible}.");
        Assert(fixture.SelectedTag("ThemeBox") == "light" && fixture.CurrentThemeId == "light",
            "Cancel must restore the saved theme instead of leaking the unsaved resource palette.");
        Assert(fixture.SelectedTag("WidgetModeBox") == "essential" && fixture.SelectedTag("WidgetPositionBox") == "tc",
            "Cancel must restore the saved form values as well as the actual window.");
        Console.WriteLine("WIDGET_CANCEL PASS: appearance, theme, form, visibility restored.");
    }

    private static void VerifyCloseToTrayRestoresSavedAppearance(Fixture fixture)
    {
        fixture.Select("WidgetModeBox", "detail");
        fixture.Select("ThemeBox", "dark");
        fixture.MainWindow.Close();
        Pump();
        Assert(!fixture.MainWindow.IsVisible && fixture.Widget.IsVisible && fixture.WidgetMode == "essential",
            "Closing the main window to the tray must discard unsaved widget appearance.");
        Assert(fixture.CurrentThemeId == "light", "Tray hide must restore the saved theme.");
        fixture.InvokeApp("ShowMainWindow");
        Pump();
        Assert(fixture.MainWindow.IsVisible && fixture.SelectedTag("WidgetModeBox") == "essential" &&
               fixture.SelectedTag("ThemeBox") == "light",
            "Reopening from the tray must show saved form values, not reapply discarded changes.");
        Console.WriteLine("WIDGET_TRAY_RESTORE PASS: Close/hide/reopen discard unsaved appearance.");
    }

    private static void VerifyTemporaryVisibilityRestoresPriorVisibility(Fixture fixture)
    {
        fixture.Widget.Hide();
        fixture.Select("WidgetModeBox", "minimalist");
        Assert(fixture.Widget.IsVisible && fixture.WidgetMode == "minimalist",
            "An enabled but manually hidden widget must become visible for deliberate appearance preview.");
        fixture.InvokeMain("CancelSettings_Click", fixture.MainWindow, new RoutedEventArgs());
        Assert(!fixture.Widget.IsVisible && fixture.WidgetMode == "essential",
            "Cancel must preserve the widget's previously hidden state.");
        fixture.Widget.Show();
        fixture.Field<CheckBox>("WidgetEnabledCheckBox").IsChecked = false;
        Pump();
        Assert(!fixture.Widget.IsVisible, "Unchecking widget visibility must hide the actual widget immediately.");
        fixture.InvokeMain("CancelSettings_Click", fixture.MainWindow, new RoutedEventArgs());
        Assert(fixture.Widget.IsVisible, "Cancel must restore previously visible widget after temporary disable.");
        Console.WriteLine("WIDGET_VISIBILITY PASS: enabled/disabled and manually hidden state rollback.");
    }

    private static void VerifyHelperButtonConsistency(Fixture fixture)
    {
        string[] themes = fixture.Field<ComboBox>("ThemeBox").Items.OfType<ComboBoxItem>()
            .Select(item => (string)item.Tag).ToArray();
        string[] datePrefixes = ["CalendarDateCorrection", "HijriDateCorrection"];
        string[] directions = ["DecreaseButton", "IncreaseButton"];
        int checks = 0;
        foreach (string theme in themes)
        {
            fixture.Select("ThemeBox", theme);
            foreach (bool enabled in new[] { true, false })
            {
                fixture.Field<TextBox>("CorrectionFajrBox").Text = "0";
                foreach (string prefix in datePrefixes) fixture.Field<TextBox>(prefix + "Box").Text = "0";
                foreach (string direction in directions)
                {
                    Button reference = fixture.Field<Button>("CorrectionFajr" + direction);
                    reference.IsEnabled = enabled;
                    foreach (string prefix in datePrefixes)
                    {
                        Button button = fixture.Field<Button>(prefix + direction);
                        button.IsEnabled = enabled;
                        button.BringIntoView();
                        Pump();
                        fixture.MainWindow.UpdateLayout();
                        Assert(Math.Abs(button.ActualWidth - reference.ActualWidth) < 0.1 &&
                               Math.Abs(button.ActualHeight - reference.ActualHeight) < 0.1,
                            $"Date correction helper must match prayer correction helper size ({theme}, enabled={enabled}). " +
                            $"Date={button.ActualWidth}x{button.ActualHeight}, prayer={reference.ActualWidth}x{reference.ActualHeight}.");
                        Assert(ReferenceEquals(button.Style, reference.Style) && ReferenceEquals(button.Template, reference.Template),
                            "All date and prayer correction helpers must share a single style and default control template.");
                        Assert(button.FontSize == reference.FontSize && button.FontWeight == reference.FontWeight &&
                               button.Padding == reference.Padding && button.Margin == reference.Margin &&
                               BrushColor(button.Background) == BrushColor(reference.Background) &&
                               BrushColor(button.Foreground) == BrushColor(reference.Foreground) &&
                               BrushColor(button.BorderBrush) == BrushColor(reference.BorderBrush),
                            $"Date/prayer correction helper chrome differs ({theme}, enabled={enabled}).");
                        checks++;
                    }
                }
            }
        }
        fixture.InvokeMain("CancelSettings_Click", fixture.MainWindow, new RoutedEventArgs());
        Console.WriteLine($"HELPER_CONSISTENCY PASS: themes={themes.Length} geometry/style/font/color checks={checks} normal+disabled.");
        static Color BrushColor(Brush brush) => ((SolidColorBrush)brush).Color;
    }

    private static void VerifySaveRetainsTabAndEstablishesBaseline(Fixture fixture)
    {
        fixture.Select("WidgetModeBox", "detail");
        fixture.Select("WidgetPositionBox", "br");
        fixture.Select("ThemeBox", "dark");
        fixture.Field<Button>("SaveSettingsButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Pump();
        Assert(fixture.Field<TextBlock>("SettingsStatusLabel").Text.StartsWith("Pengaturan tersimpan.", StringComparison.Ordinal),
            "The real Save button must validate and complete the Save handler using isolated boundaries.");
        Assert(fixture.Field<TabControl>("MainTabs").SelectedIndex == 2,
            "Save must keep the active Settings tab instead of returning to the first tab.");
        using (JsonDocument saved = JsonDocument.Parse(File.ReadAllText(fixture.SavePath!)))
            Assert(saved.RootElement.GetProperty("widget_mode").GetString() == "detail" &&
                   saved.RootElement.GetProperty("theme").GetString() == "dark",
                "Save must persist the selected appearance through the isolated persistence boundary.");
        Assert(fixture.StartupWrites == 1 && !fixture.StartupValue,
            "Save must pass the explicit startup choice to the isolated startup boundary.");
        fixture.Select("WidgetModeBox", "minimalist");
        fixture.Select("ThemeBox", "light");
        fixture.MainWindow.Close();
        Pump();
        Assert(fixture.WidgetMode == "detail" && fixture.CurrentThemeId == "dark",
            "After Save, tray hide must return to the newly saved appearance, not the original baseline.");
        fixture.InvokeApp("OpenSettings");
        Pump();
        Assert(fixture.SelectedTag("WidgetModeBox") == "detail" && fixture.Field<TabControl>("MainTabs").SelectedIndex == 2,
            "The explicit Settings tray command must restore saved values and select Settings.");
        fixture.InvokeApp("OpenCalendar");
        Pump();
        Assert(fixture.Field<TabControl>("MainTabs").SelectedIndex == 1,
            "The explicit Calendar tray command must select Calendar.");
        fixture.InvokeApp("ShowMainWindow");
        Pump();
        Assert(fixture.Field<TabControl>("MainTabs").SelectedIndex == 1,
            "Generic tray activation must not change the user's active tab.");
        Console.WriteLine("WIDGET_SAVE_AND_TAB PASS: real Save handler, isolated file readback, new baseline, explicit tray navigation.");
    }

    private static void Pump()
    {
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    internal sealed class Fixture : IDisposable
    {
        private readonly Application _application;
        private readonly Type _appType;
        private readonly Dictionary<string, object?> _previousFields;
        private readonly object _previousSettings;
        private readonly Window? _originalWidget;
        private readonly Window? _previousMainWindow;

        public Fixture(Application application, bool isolateSave = false, bool initiallyHasWidget = true,
            bool savedWidgetEnabled = true, bool preventActivation = false)
        {
            _application = application;
            Assembly assembly = typeof(ProductUi::PrayerAutomation.UI.App).Assembly;
            _appType = typeof(ProductUi::PrayerAutomation.UI.App);
            _appType.GetMethod("InitializeComponent", I)!.Invoke(application, null);
            SettingsType = assembly.GetType("PrayerAutomation.UI.AppSettings", true)!;
            Settings = Activator.CreateInstance(SettingsType, nonPublic: true)!;
            SetSetting("AutoLockEnabled", false);
            SetSetting("LockScreenEnabled", false);
            SetSetting("LockScreenOptIn", false);
            SetSetting("StartupEnabled", false);
            SetSetting("RemindersEnabled", false);
            SetSetting("WidgetEnabled", savedWidgetEnabled);
            SetSetting("WidgetMode", "essential");
            SetSetting("WidgetMotion", "static");
            SetSetting("ThemeId", "light");
            foreach (string name in new[] { "RemindersEnabled", "AutoLockEnabled", "LockScreenEnabled", "LockScreenOptIn", "StartupEnabled" })
                Assert(SettingsType.GetProperty(name, I)!.GetValue(Settings) is false,
                    $"Synthetic interaction settings must disable {name} before constructing any WPF window.");
            _previousSettings = _appType.GetProperty("Settings", I)!.GetValue(application)!;
            _previousMainWindow = application.MainWindow;
            _previousFields = new Dictionary<string, object?>();
            foreach (string name in new[] { "_mainWindow", "_widget", "_widgetSettingEnabled",
                "_widgetAppearancePreviewActive", "_widgetVisibleBeforePreview", "_widgetExistedBeforePreview", "_isShuttingDown" })
                _previousFields[name] = _appType.GetField(name, I)!.GetValue(application);
            foreach (string name in new[] { "_widgetAppearancePreviewActive", "_widgetVisibleBeforePreview", "_widgetExistedBeforePreview", "_isShuttingDown" })
                _appType.GetField(name, I)!.SetValue(application, false);
            _appType.GetProperty("Settings", I)!.SetValue(application, Settings);
            InvokeTheme("light");

            Type windowType = assembly.GetType("PrayerAutomation.UI.MainWindow", true)!;
            if (isolateSave)
            {
                Type saverType = typeof(Action<>).MakeGenericType(SettingsType);
                ConstructorInfo? constructor = windowType.GetConstructor(I, null,
                    [SettingsType, saverType, typeof(Action<bool, bool>), typeof(Func<bool>)], null);
                Assert(constructor is not null,
                    "The original Save handler must support isolated persistence/startup operations for safe UI regression tests.");
                string directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PrayerAutomationSmoke", "scratch", "widget-save-fixture-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(directory);
                SavePath = Path.Combine(directory, "config.json");
                ParameterExpression candidate = Expression.Parameter(SettingsType, "candidate");
                Delegate saver = Expression.Lambda(saverType,
                    Expression.Call(Expression.Constant(this), GetType().GetMethod(nameof(CaptureSave), I)!,
                        Expression.Convert(candidate, typeof(object))), candidate).Compile();
                Action<bool, bool> setStartup = (enabled, hidden) => { StartupValue = enabled; StartupWrites++; };
                Func<bool> getStartup = () => StartupValue;
                MainWindow = (Window)constructor!.Invoke([Settings, saver, setStartup, getStartup]);
            }
            else
                MainWindow = (Window)windowType.GetConstructor(I, null, [SettingsType], null)!.Invoke([Settings]);
            Prepare(MainWindow);
            if (preventActivation) MainWindow.IsEnabled = false;
            if (initiallyHasWidget)
            {
                Type widgetType = assembly.GetType("PrayerAutomation.UI.ScheduleWidgetWindow", true)!;
                _originalWidget = (Window)widgetType.GetConstructor(I, null, [SettingsType], null)!.Invoke([Settings]);
                Prepare(_originalWidget);
            }
            _appType.GetField("_mainWindow", I)!.SetValue(application, MainWindow);
            _appType.GetField("_widget", I)!.SetValue(application, _originalWidget);
            _appType.GetField("_widgetSettingEnabled", I)!.SetValue(application, savedWidgetEnabled);
            application.MainWindow = MainWindow;
            MainWindow.Show();
            _originalWidget?.Show();
            Field<TabControl>("MainTabs").SelectedIndex = 2;
            Pump();
        }

        public Type SettingsType { get; }
        public object Settings { get; }
        public string? SavePath { get; }
        public int StartupWrites { get; private set; }
        public bool StartupValue { get; private set; }
        public Window MainWindow { get; }
        public Window? WidgetOrNull => _appType.GetField("_widget", I)!.GetValue(_application) as Window;
        public Window Widget => WidgetOrNull ?? throw new InvalidOperationException("The fixture's actual widget is absent.");
        public string WidgetMode => Widget.GetType().GetProperty("Mode", I)!.GetValue(Widget)!.ToString()!;
        public string WidgetMotion => Widget.GetType().GetProperty("Motion", I)!.GetValue(Widget)!.ToString()!;
        public object? AppField(string name) => _appType.GetField(name, I)!.GetValue(_application);
        public void SetAppField(string name, object? value) => _appType.GetField(name, I)!.SetValue(_application, value);
        public string CurrentThemeId
        {
            get
            {
                object palette = typeof(ProductUi::PrayerAutomation.UI.App).Assembly
                    .GetType("PrayerAutomation.UI.ThemeManager", true)!
                    .GetProperty("Current", I | BindingFlags.Static)!.GetValue(null)!;
                return palette.GetType().GetProperty("Id")!.GetValue(palette)!.ToString()!;
            }
        }
        public string? SelectedTag(string name) => (Field<ComboBox>(name).SelectedItem as ComboBoxItem)?.Tag as string;
        public object? InvokeMain(string name, params object?[] arguments) =>
            MainWindow.GetType().GetMethod(name, I)!.Invoke(MainWindow, arguments.Length == 0 ? null : arguments);
        public object? InvokeApp(string name, params object?[] arguments) =>
            _appType.GetMethod(name, I)!.Invoke(_application, arguments.Length == 0 ? null : arguments);
        public T Field<T>(string name) => (T)MainWindow.GetType().GetField(name, I)!.GetValue(MainWindow)!;
        public void SetSetting(string name, object value) => SettingsType.GetProperty(name, I)!.SetValue(Settings, value);
        public void Select(string name, string tag)
        {
            ComboBox box = Field<ComboBox>(name);
            box.SelectedItem = box.Items.OfType<ComboBoxItem>().Single(item => item.Tag as string == tag);
            Pump();
        }
        private void InvokeTheme(string theme) => _appType.GetMethod("ApplyThemePreview", I)!.Invoke(_application, [theme]);
        private void CaptureSave(object candidate) => File.WriteAllText(SavePath!, JsonSerializer.Serialize(candidate, SettingsType));
        private static void Prepare(Window window)
        {
            window.Opacity = 0;
            window.Topmost = false;
            window.ShowInTaskbar = false;
            window.ShowActivated = false;
            window.IsHitTestVisible = false;
        }

        public void Dispose()
        {
            MainWindow.GetType().GetMethod("AllowClose", I)!.Invoke(MainWindow, null);
            MainWindow.Close();
            Window? active = _appType.GetField("_widget", I)!.GetValue(_application) as Window;
            if (active is not null) active.Close();
            if (!ReferenceEquals(active, _originalWidget)) _originalWidget?.Close();
            _appType.GetProperty("Settings", I)!.SetValue(_application, _previousSettings);
            foreach ((string name, object? value) in _previousFields)
                _appType.GetField(name, I)!.SetValue(_application, value);
            _application.MainWindow = _previousMainWindow;
            InvokeTheme("light");
            Pump();
        }
    }
}
