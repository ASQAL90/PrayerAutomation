extern alias ProductUi;

using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

internal static class LockScreenDurationSettingsSmoke
{
    internal static void Run(Application application)
    {
        using var fixture = new WidgetSettingsInteractionSmoke.Fixture(application,
            isolateSave: true, initiallyHasWidget: false, savedWidgetEnabled: false);
        TextBox? editor = fixture.MainWindow.GetType().GetField("LockScreenDurationBox",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)?.GetValue(fixture.MainWindow) as TextBox;
        Require(editor is not null,
            "Settings must expose a separate image-hold duration field, not reuse the minutes-before-prayer reminder field.");
        Require(editor!.Text == "60", "Existing profiles must keep the previous sixty-minute default.");
        editor.BringIntoView();
        Pump();
        Require(editor.IsVisible && editor.ActualHeight >= 28,
            "The image-hold editor must be reachable and readable through the actual Settings scroll view.");
        int checks = 3;
        foreach (string invalid in new[] { "", "0", "-1", "121", "1.5", "abc" })
        {
            editor.Text = invalid;
            fixture.Field<Button>("SaveSettingsButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Pump();
            Require(!File.Exists(fixture.SavePath!) && fixture.StartupWrites == 0 &&
                    fixture.Field<TextBlock>("SettingsStatusLabel").Text.Contains("1–120", StringComparison.Ordinal),
                "Invalid hold duration must refuse Save before persistence or startup actions: " + invalid + ".");
            checks++;
        }
        editor.Text = "7";
        fixture.Field<TextBox>("ReminderMinutesBox").Text = "3";
        fixture.InvokeMain("CancelSettings_Click", fixture.MainWindow, new RoutedEventArgs());
        Pump();
        Require(editor.Text == "60" && !File.Exists(fixture.SavePath!),
            "Cancel must discard an unsaved image duration without persisting it.");
        checks++;
        editor.Text = "7";
        fixture.Field<Button>("SaveSettingsButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Pump();
        Require(File.Exists(fixture.SavePath!) && fixture.Field<TabControl>("MainTabs").SelectedIndex == 2,
            "The real Save handler must persist a valid image duration without leaving Settings.");
        using (JsonDocument saved = JsonDocument.Parse(File.ReadAllText(fixture.SavePath!)))
            Require(saved.RootElement.GetProperty("lock_screen_duration_minutes").GetInt32() == 7 &&
                    saved.RootElement.GetProperty("reminder_minutes").GetInt32() == 3 &&
                    !saved.RootElement.GetProperty("auto_lock_enabled").GetBoolean() &&
                    !saved.RootElement.GetProperty("lock_enabled").GetBoolean(),
                "Saved image-hold minutes must remain separate from reminder lead time and consequential opt-ins.");
        checks += 2;
        editor.Text = "12";
        fixture.MainWindow.Close();
        fixture.InvokeApp("OpenSettings");
        Pump();
        Require(editor.Text == "7", "Hide/reopen must restore the newly saved seven-minute duration.");
        Require(!fixture.Field<TextBlock>("LockScreenSafetyText").Text.Contains("60 menit", StringComparison.OrdinalIgnoreCase),
            "Safety status must not promise a hard-coded sixty minutes; unavailable-mode messages need not promise any duration.");
        checks += 2;
        Console.WriteLine("LOCKSCREEN_DURATION_SETTINGS PASS: checks=" + checks + "; real_WPF=1; saved_minutes=7; reminder_minutes=3; original_profile_writes=0; native_setters=0.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
}
