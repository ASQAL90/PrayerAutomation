using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Expression = System.Linq.Expressions.Expression;

internal static class Program
{
    const BindingFlags I = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    const BindingFlags S = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    static readonly string[] Flags = ["RemindersEnabled", "AutoLockEnabled", "LockScreenEnabled", "LockScreenOptIn", "StartupEnabled", "StartHidden", "WidgetEnabled", "TaskbarWidgetEnabled"];
    static Assembly Product = null!;
    static Application App = null!;
    static Type AppType = null!, SettingsType = null!;
    static Window Window = null!;
    static object Settings = null!;
    static DispatcherOperation? Startup;
    static int Aborted, Failures, SaveWrites, StartupWrites, Prompts, SafetyReads = 0;
    static int SaveAttempts, TotalPrompts, LoggedErrors, ClosingCalls;
    static bool LastDiscardOnly;
    static bool ConstructorLoggerInjected;
    static Action? DuringSave;
    static string ReportRoot = "", AssemblyPath = "", Suite = "", CurrentCase = "";
    static string? SetupError;
    static string SavePath = "", Choice = "Cancel";
    static bool FailSave;
    static Action? DuringPrompt;
    static readonly List<object> Checks = [];
    static readonly List<string> Unhandled = [];
    static FieldInfo Field(string name) => Window.GetType().GetField(name,I) ?? throw new InvalidOperationException("Missing field: " + name);
    static T Control<T>(string name) => (T)Field(name).GetValue(Window)!;
    static object? Invoke(string name, params object?[] args) => Window.GetType().GetMethods(I | BindingFlags.DeclaredOnly).Single(m => m.Name == name && m.GetParameters().Length == args.Length).Invoke(Window,args);
    static object? AppInvoke(string name, params object?[] args) => AppType.GetMethod(name,I)!.Invoke(App,args);
    static void Require(bool pass, string message) { if (!pass) throw new InvalidOperationException("SAFETY/SETUP: " + message); }
    static void Check(string id, bool pass) { Checks.Add(new { id, pass }); if (!pass) Failures++; WriteReport(); }
    static string Json() => JsonSerializer.Serialize(Settings,SettingsType);
    static void Verify()
    {
        Require(Aborted == 1 && Startup?.Status == DispatcherOperationStatus.Aborted,"implicit startup not aborted exactly once");
        foreach (string n in new[]{"_singleInstanceMutex","_tray","_reminders","_autoLock","_lockScreenScheduler","_widget","_taskbarWidget","_taskbarPanel"})
            Require(AppType.GetField(n,I)!.GetValue(App) == null,"consequential startup object created: " + n);
        foreach (string n in Flags) Require(SettingsType.GetProperty(n,I)!.GetValue(Settings) is false,"synthetic saved flag changed: " + n);
        Require(!((DispatcherTimer)Field("_clockTimer").GetValue(Window)!).IsEnabled,"clock timer restarted");
        Require(PresentationSource.FromVisual(Window) == null,"main window became shown/native-backed");
        Require(!Dispatcher.CurrentDispatcher.HasShutdownStarted,"fixture dispatcher shut down unexpectedly");
    }
    static void Pump()
    {
        ((DispatcherTimer)Field("_locationSearchTimer").GetValue(Window)!).Stop();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle,new Action(()=>{}));
        Verify();
    }
    static void Capture(object candidate)
    {
        foreach (string n in Flags) Require(SettingsType.GetProperty(n,I)!.GetValue(candidate) is false,"unsafe save candidate: " + n);
        SaveAttempts++;
        DuringSave?.Invoke();
        if (FailSave) throw new IOException("Synthetic persistence failure");
        SaveWrites++;
        File.WriteAllText(SavePath,JsonSerializer.Serialize(candidate,SettingsType));
    }
    static object Prompt(bool discardOnly)
    {
        Prompts++;
        TotalPrompts++;
        LastDiscardOnly = discardOnly;
        DuringPrompt?.Invoke();
        Type result = Product.GetType("PrayerAutomation.UI.UnsavedChangesChoice",true)!;
        return Enum.Parse(result,Choice);
    }
    static void SetPrompt()
    {
        // Baseline lacks the seam; still exercise its real closing behavior for RED.
        FieldInfo? field = Window.GetType().GetField("_promptUnsavedChanges",I);
        if (field == null) return;
        ParameterExpression discard = Expression.Parameter(typeof(bool),"discardOnly");
        var body = Expression.Convert(Expression.Call(typeof(Program).GetMethod(nameof(Prompt),S)!,discard),field.FieldType.GetGenericArguments()[1]);
        field.SetValue(Window,Expression.Lambda(field.FieldType,body,discard).Compile());
    }
    static void Create()
    {
        AppType = Product.GetType("PrayerAutomation.UI.App",true)!;
        FieldInfo callback = typeof(DispatcherOperation).GetField("_method",BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Unsupported WPF callback field");
        void Abort(object? sender, DispatcherHookEventArgs e)
        {
            if (callback.GetValue(e.Operation) is Delegate d && d.Method.DeclaringType == typeof(Application) && d.Method.Name == "<.ctor>b__1_0")
            {
                Require(e.Operation.Priority == DispatcherPriority.Send && e.Operation.Status == DispatcherOperationStatus.Pending && e.Operation.Abort(),"cannot abort startup prefix");
                Startup = e.Operation; Aborted++;
            }
        }
        Dispatcher.CurrentDispatcher.Hooks.OperationPosted += Abort;
        try { App = (Application)Activator.CreateInstance(AppType)!; }
        finally { Dispatcher.CurrentDispatcher.Hooks.OperationPosted -= Abort; }
        Require(Aborted == 1 && Startup?.Status == DispatcherOperationStatus.Aborted,"startup abort contract before any pump");
        foreach (string n in new[]{"_singleInstanceMutex","_tray","_reminders","_autoLock","_lockScreenScheduler","_widget","_taskbarWidget","_taskbarPanel"})
            Require(AppType.GetField(n,I)!.GetValue(App) == null,"startup entered before initialization");
        App.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        App.DispatcherUnhandledException += (_,e) => { Unhandled.Add(e.Exception.GetType().FullName!); e.Handled = true; };
        AppType.GetMethod("InitializeComponent",I)!.Invoke(App,null);
        SettingsType = Product.GetType("PrayerAutomation.UI.AppSettings",true)!;
        object seed = Activator.CreateInstance(SettingsType,true)!;
        foreach (string n in Flags) SettingsType.GetProperty(n,I)!.SetValue(seed,false);
        foreach (var pair in new Dictionary<string,object>{{"Latitude",-6.2d},{"Longitude",106.8d},{"TimeZoneId","Asia/Jakarta"},{"LocationName","Saved fixture location"},{"ThemeId","light"}})
            SettingsType.GetProperty(pair.Key,I)!.SetValue(seed,pair.Value);
        Settings = JsonSerializer.Deserialize(JsonSerializer.Serialize(seed,SettingsType),SettingsType)!;
        foreach (string n in Flags) Require(SettingsType.GetProperty(n,I)!.GetValue(Settings) is false,"seed serializer flags");
        AppType.GetProperty("Settings",I)!.SetValue(App,Settings);
        AppType.GetMethod("ApplyThemePreview",I)!.Invoke(App,["light"]);
        Type saverType = typeof(Action<>).MakeGenericType(SettingsType);
        ParameterExpression candidate = Expression.Parameter(SettingsType,"candidate");
        Delegate saver = Expression.Lambda(saverType,Expression.Call(typeof(Program).GetMethod(nameof(Capture),S)!,Expression.Convert(candidate,typeof(object))),candidate).Compile();
        Type safetyType = Product.GetType("PrayerAutomation.UI.LockScreenSafetyInfo",true)!;
        var safetyCtor = safetyType.GetConstructors(I).Single(c => c.GetParameters().Length == 6);
        object safety = safetyCtor.Invoke(safetyCtor.GetParameters().Select(p => p.ParameterType == typeof(bool) ? (object)false : p.ParameterType == typeof(string) ? "Inert fixture: no personalization probe" : Activator.CreateInstance(p.ParameterType)).ToArray());
        Type inspectType = typeof(Func<>).MakeGenericType(safetyType);
        Delegate inspect = Expression.Lambda(inspectType,Expression.Block(Expression.PostIncrementAssign(Expression.Field(null,typeof(Program).GetField(nameof(SafetyReads),S)!)),Expression.Constant(safety,safetyType))).Compile();
        Type windowType = Product.GetType("PrayerAutomation.UI.MainWindow",true)!;
        var ctor = windowType.GetConstructor(I,null,[SettingsType,saverType,typeof(Action<bool,bool>),typeof(Func<bool>),inspectType,typeof(Action<string,Exception>)],null);
        ConstructorLoggerInjected = ctor != null;
        ctor ??= windowType.GetConstructor(I,null,[SettingsType,saverType,typeof(Action<bool,bool>),typeof(Func<bool>),inspectType],null) ?? throw new InvalidOperationException("safe constructor missing");
        Action<bool,bool> startup = (a,b) => { Require(!a && !b,"startup mock unsafe flags"); StartupWrites++; };
        Action<string,Exception> loggerCounter = (context,error)=>
        {
            Require(context is "settings.save" or "settings.startup","unexpected settings error context");
            LoggedErrors++;
        };
        Window = (Window)ctor.Invoke(ConstructorLoggerInjected ? [Settings,saver,startup,(Func<bool>)(()=>false),inspect,loggerCounter] : [Settings,saver,startup,(Func<bool>)(()=>false),inspect]);
        ((DispatcherTimer)Field("_clockTimer").GetValue(Window)!).Stop();
        ((DispatcherTimer)Field("_locationSearchTimer").GetValue(Window)!).Stop();
        foreach (string n in new[]{"_refreshDependentViews","_restoreWidgetAppearancePreview","_restoreTaskbarAppearancePreview"}) Field(n).SetValue(Window,(Action)(()=>{}));
        Field("_applyThemePreview").SetValue(Window,(Action<string>)(id => AppType.GetMethod("ApplyThemePreview",I)!.Invoke(App,[id])));
        Field("_applyWidgetAppearancePreview").SetValue(Window,(Action<bool,string,string,string>)((a,b,c,d)=>{}));
        Field("_applyTaskbarAppearancePreview").SetValue(Window,(Action<bool,string>)((a,b)=>{}));
        FieldInfo? logger = Window.GetType().GetField("_logSettingsError",I);
        if (!ConstructorLoggerInjected) logger?.SetValue(Window,loggerCounter);
        foreach (string n in new[]{"_locationDetectionService","_deviceLocationService"}) Field(n).SetValue(Window,null);
        AppType.GetField("_mainWindow",I)!.SetValue(App,Window);
        Window.Topmost = false; Window.ShowActivated = false; Window.ShowInTaskbar = false;
        SetPrompt();
        Require(PresentationSource.FromVisual(Window) == null,"main window must remain unshown");
        Verify();
    }
    static void Closing()
    {
        var e = new System.ComponentModel.CancelEventArgs();
        Invoke("OnClosing",Window,e);
        Check("X_remains_cancelled_for_tray_" + ++ClosingCalls,e.Cancel);
    }
    static void CloseCancelCase()
    {
        string saved = Json();
        Control<TextBox>("ReminderMessageBox").Text = "Draft preserved on X";
        Choice = "Cancel";
        int before = SafetyReads;
        Closing();
        Check("X_cancel_preserves_unsaved_draft",Control<TextBox>("ReminderMessageBox").Text == "Draft preserved on X");
        Check("X_cancel_prompts_once",Prompts == 1);
        Check("X_cancel_does_not_validate_save_or_startup",SafetyReads == before && SaveWrites == 0 && StartupWrites == 0 && Json() == saved);
    }
    static void StateEvent() => typeof(Window).GetMethod("OnStateChanged",I)!.Invoke(Window,[EventArgs.Empty]);
    static void MinimizeCancelCase()
    {
        Invoke("RestoreSavedSettingsPreview");
        string saved = Json();
        Window.WindowState = WindowState.Maximized; StateEvent();
        Control<TextBox>("ReminderMessageBox").Text = "Invalid draft retained when minimized";
        Control<TextBox>("ReminderMinutesBox").Text = "not a number";
        Choice = "Cancel"; Prompts = 0;
        bool restoredBeforePrompt = false;
        DuringPrompt = () => restoredBeforePrompt = Window.WindowState == WindowState.Maximized;
        int before = SafetyReads;
        Window.WindowState = WindowState.Minimized; StateEvent();
        DuringPrompt = null;
        Check("minimize_prompt_after_restore_previous_maximized",Prompts == 1 && restoredBeforePrompt);
        Check("minimize_cancel_keeps_previous_state",Window.WindowState == WindowState.Maximized);
        Check("minimize_cancel_preserves_invalid_entire_draft",Control<TextBox>("ReminderMessageBox").Text == "Invalid draft retained when minimized" && Control<TextBox>("ReminderMinutesBox").Text == "not a number");
        Check("minimize_dirty_check_never_validates_or_saves",SafetyReads == before && SaveWrites == 0 && StartupWrites == 0 && Json() == saved);
    }
    static void DiscardCancelCase()
    {
        Invoke("RestoreSavedSettingsPreview");
        Control<TextBox>("ReminderMessageBox").Text = "Keep draft when cancelling discard";
        Choice = "Cancel"; Prompts = 0;
        int before = SafetyReads;
        Invoke("CancelSettings_Click",Window,new RoutedEventArgs());
        Check("Batal_requires_confirmation",Prompts == 1);
        Check("Batal_cancel_keeps_draft",Control<TextBox>("ReminderMessageBox").Text == "Keep draft when cancelling discard");
        Check("Batal_cancel_never_validates_or_persists",SafetyReads == before && SaveWrites == 0 && StartupWrites == 0);
    }
    static void ExitCancelCase()
    {
        Invoke("RestoreSavedSettingsPreview");
        Control<TextBox>("ReminderMessageBox").Text = "Draft blocks central tray exit";
        Choice = "Cancel"; Prompts = 0;
        bool beforeShutdownGuard = false;
        DuringPrompt = () => beforeShutdownGuard = AppType.GetField("_isShuttingDown",I)!.GetValue(App) is false;
        AppInvoke("ShutdownApplication");
        DuringPrompt = null;
        Check("central_exit_prompts_before_shutdown_guard",Prompts == 1 && beforeShutdownGuard);
        Check("central_exit_cancel_keeps_application_running",AppType.GetField("_isShuttingDown",I)!.GetValue(App) is false && Field("_allowClose").GetValue(Window) is false);
        Check("central_exit_cancel_preserves_draft",Control<TextBox>("ReminderMessageBox").Text == "Draft blocks central tray exit" && SaveWrites == 0 && StartupWrites == 0);
    }
    static bool Dirty() => (bool)Invoke("HasUnsavedSettingsChanges")!;
    static bool Confirm(bool discardOnly = false) => (bool)Invoke("ConfirmUnsavedSettingsChanges",discardOnly)!;
    static object?[] Draft() => (object?[])Invoke("CaptureSettingsFormDraft")!;
    static void Quiet(Action action)
    {
        Field("_loadingSettings").SetValue(Window,true);
        try { action(); }
        finally { Field("_loadingSettings").SetValue(Window,false); }
    }
    static bool Running() => AppType.GetField("_isShuttingDown",I)!.GetValue(App) is false && Field("_allowClose").GetValue(Window) is false;
    static void Reset()
    {
        DuringPrompt = null; DuringSave = null; FailSave = false; Choice = "Cancel"; Prompts = 0;
        Invoke("RestoreSavedSettingsPreview");
        Window.WindowState = WindowState.Normal; StateEvent();
        Verify();
    }
    static void RunCase(string name, Action action)
    {
        CurrentCase = name; Reset(); action(); Pump(); WriteReport();
    }
    static void DirtyCase()
    {
        string saved = Json(); int writes = SaveWrites, startups = StartupWrites, reads = SafetyReads;
        Check("clean_form_has_no_unsaved_changes",!Dirty());
        Check("clean_confirm_skips_popup",Confirm() && Prompts == 0);
        string message = Control<TextBox>("ReminderMessageBox").Text;
        Control<TextBox>("ReminderMessageBox").Text = message + " edited";
        Check("edit_is_dirty",Dirty());
        Control<TextBox>("ReminderMessageBox").Text = message;
        Check("edit_reverted_skips_popup",!Dirty() && Confirm() && Prompts == 0);
        Control<TabControl>("MainTabs").SelectedIndex = 1;
        Control<ComboBox>("CalendarYearBox").SelectedIndex++;
        Invoke("LoadCalendar_Click",Window,new RoutedEventArgs());
        Control<ComboBox>("HijriYearBox").SelectedIndex++;
        Invoke("LoadHijriCalendar_Click",Window,new RoutedEventArgs());
        Invoke("SelectGregorianDate",new DateTime(2026,12,7));
        Check("calendar_tabs_years_dates_do_not_dirty_settings",!Dirty() && Confirm() && Prompts == 0);
        Check("raw_snapshot_covers_30_values",Draft().Length == 30);
        foreach (string n in new[]{"LocationNameBox","LatitudeBox","LongitudeBox","CorrectionFajrBox","CorrectionSunriseBox","CorrectionDhuhaBox","CorrectionDhuhrBox","CorrectionAsrBox","CorrectionMaghribBox","CorrectionIshaBox","DhuhaAfterSunriseBox","CalendarDateCorrectionBox","HijriDateCorrectionBox","ReminderMinutesBox","ReminderMessageBox","LockScreenDurationBox"})
        {
            TextBox box = Control<TextBox>(n); string old = box.Text;
            Quiet(()=>box.Text = old + " invalid raw draft "); bool dirty = Dirty();
            Quiet(()=>box.Text = old);
            Check("raw_text_dirty_and_revert_" + n,dirty && !Dirty());
        }
        ComboBox zone = Control<ComboBox>("TimeZoneBox"); string oldZone = zone.Text;
        Quiet(()=>zone.Text = "invalid raw timezone "); bool zoneDirty = Dirty(); Quiet(()=>zone.Text = oldZone);
        Check("raw_timezone_dirty_and_revert",zoneDirty && !Dirty());
        ComboBox method = Control<ComboBox>("MethodBox"); int oldMethod = method.SelectedIndex;
        Quiet(()=>method.SelectedIndex = (oldMethod + 1) % method.Items.Count); bool methodDirty = Dirty(); Quiet(()=>method.SelectedIndex = oldMethod);
        Check("calculation_method_dirty_and_revert",methodDirty && !Dirty());
        foreach (string n in new[]{"ThemeBox","WidgetModeBox","WidgetMotionBox","WidgetPositionBox","TaskbarWidgetModeBox"})
        {
            ComboBox combo = Control<ComboBox>(n); object oldItem = combo.SelectedItem;
            Quiet(()=>combo.SelectedIndex = (combo.SelectedIndex + 1) % combo.Items.Count); bool changed = Dirty();
            Quiet(()=>combo.SelectedItem = oldItem);
            var selected = (ComboBoxItem)oldItem; object tag = selected.Tag;
            Quiet(()=>selected.Tag = "unsupported RAW tag"); bool raw = Dirty(); Quiet(()=>selected.Tag = tag);
            Check("selection_and_raw_tag_dirty_" + n,changed && raw && !Dirty());
        }
        foreach (string n in new[]{"ReminderEnabledCheckBox","AutoLockEnabledCheckBox","LockScreenCheckBox","StartupEnabledCheckBox","StartHiddenCheckBox","WidgetEnabledCheckBox","TaskbarWidgetEnabledCheckBox"})
        {
            CheckBox box = Control<CheckBox>(n); bool enabled = box.IsEnabled; bool? old = box.IsChecked;
            Quiet(()=>{ box.IsEnabled = false; box.IsChecked = true; }); bool disabledDirty = Dirty();
            Quiet(()=>{ box.IsChecked = old; box.IsEnabled = enabled; });
            Check("disabled_flag_detected_without_persistence_" + n,disabledDirty && !Dirty());
        }
        Check("dirty_detection_is_read_only_no_validation_or_callbacks",Json() == saved && SaveWrites == writes && StartupWrites == startups && SafetyReads == reads);
    }
    static void MakeDraft(bool invalid = false)
    {
        Quiet(()=>
        {
            Control<TextBox>("LocationNameBox").Text = "Synthetic full draft";
            Control<TextBox>("LatitudeBox").Text = "-6.21";
            Control<TextBox>("LongitudeBox").Text = "106.81";
            Control<TextBox>("ReminderMessageBox").Text = "Entire draft must survive failure";
            Control<TextBox>("ReminderMinutesBox").Text = invalid ? "not a number " : "17";
            Control<TextBox>("CorrectionFajrBox").Text = "+3";
            Control<TextBox>("CorrectionIshaBox").Text = "-4";
            Control<TextBox>("CalendarDateCorrectionBox").Text = "+2";
            Control<TextBox>("HijriDateCorrectionBox").Text = "-1";
            Control<TextBox>("LockScreenDurationBox").Text = "19";
            Control<TextBox>("DhuhaAfterSunriseBox").Text = "27";
            foreach (string n in new[]{"ThemeBox","WidgetModeBox","WidgetMotionBox","WidgetPositionBox","TaskbarWidgetModeBox"})
            {
                ComboBox c = Control<ComboBox>(n); c.SelectedIndex = (c.SelectedIndex + 1) % c.Items.Count;
            }
        });
        Require(Dirty(),"draft setup must be dirty");
    }
    static void DecisionsCase()
    {
        object?[] baseline = Draft(); string saved = Json(); int writes = SaveWrites, starts = StartupWrites;
        MakeDraft(true); object?[] draft = Draft(); Choice = "Cancel";
        Check("cancel_refuses_continue",!Confirm());
        Check("cancel_preserves_all_raw_values_and_selects_edit_tab",Draft().SequenceEqual(draft) && Control<TabControl>("MainTabs").SelectedIndex == 2);
        bool nested = true; int p = Prompts;
        DuringPrompt = ()=>nested = Confirm();
        Check("reentrant_confirm_is_refused_without_second_prompt",!Confirm() && !nested && Prompts == p + 1);
        DuringPrompt = null; Choice = "Discard";
        Check("discard_allows_continue_and_restores_full_baseline",Confirm() && Draft().SequenceEqual(baseline) && !Dirty());
        MakeDraft(true); Choice = "Discard"; int safety = SafetyReads; p = Prompts;
        Invoke("CancelSettings_Click",Window,new RoutedEventArgs());
        Check("Batal_discard_only_prompt_restores_once",Prompts == p + 1 && LastDiscardOnly && SafetyReads == safety + 1 && Draft().SequenceEqual(baseline) && !Dirty());
        p = Prompts;
        Invoke("CancelSettings_Click",Window,new RoutedEventArgs());
        Check("Batal_clean_does_not_prompt",Prompts == p);
        MakeDraft(); draft = Draft(); Choice = "Save";
        Invoke("CancelSettings_Click",Window,new RoutedEventArgs());
        Check("Batal_discard_only_rejects_save_choice",LastDiscardOnly && Draft().SequenceEqual(draft) && Dirty());
        Check("cancel_discard_never_persist_or_update_startup",Json() == saved && SaveWrites == writes && StartupWrites == starts && Running());
    }
    static void FailuresCase()
    {
        Require(Window.GetType().GetField("_logSettingsError",I) != null,"save-failure test requires inert logging seam");
        Check("inert_logger_injected_via_constructor",ConstructorLoggerInjected);
        string saved = Json(); int writes = SaveWrites, starts = StartupWrites, attempts = SaveAttempts, logs = LoggedErrors;
        MakeDraft(true); object?[] draft = Draft(); Choice = "Save";
        Quiet(()=>
        {
            foreach (string n in new[]{"ReminderEnabledCheckBox","AutoLockEnabledCheckBox","LockScreenCheckBox","StartupEnabledCheckBox","StartHiddenCheckBox","WidgetEnabledCheckBox","TaskbarWidgetEnabledCheckBox"})
                Control<CheckBox>(n).IsChecked = true;
        });
        draft = Draft();
        AppInvoke("ShutdownApplication");
        Check("invalid_save_blocks_central_exit_and_keeps_all_draft",Running() && Draft().SequenceEqual(draft) && Dirty() && Control<TabControl>("MainTabs").SelectedIndex == 2);
        Check("invalid_save_never_calls_persistence_startup_or_logger",Json() == saved && SaveAttempts == attempts && SaveWrites == writes && StartupWrites == starts && LoggedErrors == logs);
        Check("invalid_save_has_validation_feedback",Control<TextBlock>("SettingsStatusLabel").Text.Contains("Waktu pengingat"));
        Reset(); MakeDraft(); draft = Draft(); Choice = "Save"; FailSave = true;
        AppInvoke("ShutdownApplication");
        Check("persistence_failure_blocks_central_exit_and_keeps_all_draft",Running() && Draft().SequenceEqual(draft) && Dirty() && Control<TabControl>("MainTabs").SelectedIndex == 2);
        Check("failed_persistence_attempts_once_no_startup_active_mutation",SaveAttempts == attempts + 1 && SaveWrites == writes && StartupWrites == starts && Json() == saved && LoggedErrors == logs + 1);
        Check("save_failure_has_feedback_and_releases_guards",Control<TextBlock>("SettingsStatusLabel").Text.Contains("tidak berhasil disimpan") && Field("_savingSettings").GetValue(Window) is false && Field("_resolvingUnsavedChanges").GetValue(Window) is false);
        FailSave = false;
        Choice = "Cancel"; int p = Prompts; AppInvoke("ShutdownApplication");
        Check("failed_exit_can_be_cancelled_again",Running() && Prompts == p + 1 && Draft().SequenceEqual(draft));
        DuringPrompt = ()=>AppInvoke("ShutdownApplication");
        p = Prompts; AppInvoke("ShutdownApplication"); DuringPrompt = null;
        Check("central_exit_nested_confirmation_does_not_set_shutdown_guard",Running() && Prompts == p + 1 && Draft().SequenceEqual(draft));
        AppType.GetField("_isShuttingDown",I)!.SetValue(App,true);
        p = Prompts; AppInvoke("ShutdownApplication");
        Check("central_exit_existing_shutdown_guard_skips_prompt",Prompts == p);
        AppType.GetField("_isShuttingDown",I)!.SetValue(App,false);
        Reset(); MakeDraft(true); draft = Draft(); Choice = "Save";
        Closing();
        Check("X_failed_validation_retains_entire_draft",Draft().SequenceEqual(draft) && Dirty());
        Reset(); Window.WindowState = WindowState.Normal; StateEvent(); MakeDraft(true); draft = Draft(); Choice = "Save";
        Window.WindowState = WindowState.Minimized; StateEvent();
        Check("minimize_failed_validation_restores_normal_and_full_draft",Window.WindowState == WindowState.Normal && Draft().SequenceEqual(draft) && Dirty());
    }
    static void SaveCase()
    {
        int writes = SaveWrites, starts = StartupWrites, attempts = SaveAttempts;
        MakeDraft(); Choice = "Save"; Control<TabControl>("MainTabs").SelectedIndex = 1;
        bool nestedSave = true, nestedConfirm = true;
        DuringSave = ()=>{ nestedSave = (bool)Invoke("TrySaveSettings")!; nestedConfirm = Confirm(); };
        Check("save_choice_allows_continue_only_after_success",Confirm());
        DuringSave = null;
        Check("save_success_calls_persistence_and_startup_once",SaveWrites == writes + 1 && SaveAttempts == attempts + 1 && StartupWrites == starts + 1);
        Check("save_callback_reentrancy_is_blocked",!nestedSave && !nestedConfirm);
        Check("save_establishes_new_raw_baseline_and_preserves_tab",!Dirty() && Control<TabControl>("MainTabs").SelectedIndex == 1);
        using var persisted = JsonDocument.Parse(File.ReadAllText(SavePath));
        Check("save_corrections_and_date_values_roundtrip",persisted.RootElement.GetProperty("corrections").GetProperty("Fajr").GetInt32() == 3 && persisted.RootElement.GetProperty("corrections").GetProperty("Isha").GetInt32() == -4 && persisted.RootElement.GetProperty("calendar_date_correction_days").GetInt32() == 2 && persisted.RootElement.GetProperty("hijri_date_correction_days").GetInt32() == -1);
        Check("save_file_equals_active_synthetic_settings",Json() == File.ReadAllText(SavePath));
        int p = Prompts; Check("saved_clean_confirm_never_saves_again",Confirm() && Prompts == p && SaveWrites == writes + 1);
        Control<TextBox>("ReminderMessageBox").Text = "Direct Save button fixture";
        Invoke("SaveSettings_Click",Window,new RoutedEventArgs());
        Check("direct_save_button_uses_same_pipeline",SaveWrites == writes + 2 && SaveAttempts == attempts + 2 && StartupWrites == starts + 2 && !Dirty() && Control<TabControl>("MainTabs").SelectedIndex == 1);
        Reset(); object?[] baseline = Draft(); MakeDraft(true); Choice = "Discard";
        Closing();
        Check("X_discard_restores_entire_form_without_persisting",Draft().SequenceEqual(baseline) && !Dirty() && SaveWrites == writes + 2 && StartupWrites == starts + 2 && Running());
        Reset(); MakeDraft(); Choice = "Save";
        Closing();
        Check("X_save_runs_successful_pipeline_once_before_hide",!Dirty() && SaveWrites == writes + 3 && SaveAttempts == attempts + 3 && StartupWrites == starts + 3 && Running());
        Reset(); Window.WindowState = WindowState.Maximized; StateEvent(); MakeDraft(); Choice = "Save";
        Window.WindowState = WindowState.Minimized; StateEvent();
        Check("minimize_save_runs_successful_pipeline_once",!Dirty() && Window.WindowState == WindowState.Maximized && SaveWrites == writes + 4 && SaveAttempts == attempts + 4 && StartupWrites == starts + 4 && Running());
    }
    static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        foreach (object child in LogicalTreeHelper.GetChildren(root))
            if (child is DependencyObject d) foreach (var nested in Descendants(d)) yield return nested;
    }
    static void DialogCase()
    {
        Type type = Product.GetType("PrayerAutomation.UI.UnsavedChangesDialog",true)!;
        foreach (bool discardOnly in new[]{false,true})
        {
            Window dialog = (Window)Activator.CreateInstance(type,I,null,[discardOnly],null)!;
            Check("dialog_defaults_cancel_" + discardOnly,type.GetProperty("Choice",I)!.GetValue(dialog)!.ToString() == "Cancel");
            Button[] buttons = Descendants(dialog).OfType<Button>().ToArray();
            Check("dialog_cancel_is_default_and_escape_" + discardOnly,buttons.Single(b=>b.Content?.ToString() == "Kembali mengedit") is { IsDefault:true, IsCancel:true });
            Check("dialog_save_visibility_" + discardOnly,buttons.Single(b=>b.Content?.ToString() == "Simpan").Visibility == (discardOnly ? Visibility.Collapsed : Visibility.Visible));
            var content = (FrameworkElement)dialog.Content;
            content.Measure(new Size(590,double.PositiveInfinity));
            content.Arrange(new Rect(0,0,590,content.DesiredSize.Height)); content.UpdateLayout();
            Check("dialog_offscreen_content_measured_" + discardOnly,content.ActualWidth > 0 && content.ActualHeight > 0 && buttons.Where(b=>b.Visibility == Visibility.Visible).All(b=>b.ActualWidth > 0 && b.ActualHeight > 0));
            Check("dialog_message_explains_unsaved_or_discard_" + discardOnly,Descendants(dialog).OfType<TextBlock>().Any(t=>t.Text.Contains(discardOnly ? "akan hilang" : "belum disimpan")));
            dialog.Close();
        }
        foreach (var pair in new[]{("Save_Click","Save"),("Discard_Click","Discard"),("Cancel_Click","Cancel")})
        {
            Window dialog = (Window)Activator.CreateInstance(type,I,null,[false],null)!;
            type.GetMethod(pair.Item1,I)!.Invoke(dialog,[dialog,new RoutedEventArgs()]);
            Check("dialog_handler_choice_" + pair.Item2,type.GetProperty("Choice",I)!.GetValue(dialog)!.ToString() == pair.Item2);
        }
    }
    static void WriteReport()
    {
        if (ReportRoot.Length == 0) return;
        var report = new { suite = Suite, current_case = CurrentCase, checks = Checks, count = Checks.Count, failed = Failures, passed = Checks.Count - Failures, setup_error = SetupError,
            startup_callbacks_aborted = Aborted, actual_assembly_sha256 = AssemblyPath.Length > 0 ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AssemblyPath))).ToLowerInvariant() : null,
            synthetic_save_writes = SaveWrites, synthetic_save_attempts = SaveAttempts, startup_mock_calls = StartupWrites, prompt_calls = TotalPrompts, inert_error_log_calls = LoggedErrors,
            product_EXE_launched = false, main_window_shown = false, real_profile_read_or_written = false, network_GPS_personalization_actions = false, unhandled = Unhandled };
        File.WriteAllText(Path.Combine(ReportRoot,"RESULT.json"),JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
    }
    [STAThread] static int Main(string[] args)
    {
        try
        {
            Require(args.Length == 2 && args[0] is "close-cancel" or "minimize-cancel" or "discard-cancel" or "exit-cancel" or "dirty" or "decisions" or "failures" or "save" or "dialog" or "all","allowlisted suite required");
            string root = Path.GetFullPath(args[1]);
            string approvedScratch = Path.GetFullPath(Environment.GetEnvironmentVariable("SMOKE_SCRATCH") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrayerAutomationSmoke", "scratch"));
            Require(root.StartsWith(approvedScratch + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "output outside approved scratch");
            Directory.CreateDirectory(root); SavePath = Path.Combine(root,"synthetic-config.json"); Require(!File.Exists(SavePath),"fresh save destination required");
            ReportRoot = root; Suite = args[0];
            string assemblyPath = new[]{"AyoJamaah.dll","PrayerAutomation.UI.dll"}.Select(n => Path.Combine(AppContext.BaseDirectory,n)).First(File.Exists);
            AssemblyPath = assemblyPath;
            AssemblyLoadContext.Default.Resolving += (c,n) => { string p = Path.Combine(AppContext.BaseDirectory,n.Name + ".dll"); return File.Exists(p) ? c.LoadFromAssemblyPath(p) : null; };
            Product = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
            Create();
            foreach (var (name,action) in new (string,Action)[]{("close-cancel",CloseCancelCase),("minimize-cancel",MinimizeCancelCase),("discard-cancel",DiscardCancelCase),("exit-cancel",ExitCancelCase),("dirty",DirtyCase),("decisions",DecisionsCase),("failures",FailuresCase),("save",SaveCase),("dialog",DialogCase)})
                if (args[0] == name || args[0] == "all") RunCase(name,action);
            if (AppType.GetField("_isShuttingDown",I)!.GetValue(App) is false) Pump();
            Check("no_async_dispatcher_failures",Unhandled.Count == 0);
            Invoke("AllowClose"); Window.Close(); Verify();
            WriteReport();
            Console.WriteLine(JsonSerializer.Serialize(new { count = Checks.Count, failed = Failures, passed = Checks.Count - Failures }));
            return Failures == 0 ? 0 : 1;
        }
        catch (Exception e) { SetupError = e.ToString(); WriteReport(); Console.Error.WriteLine(e); return 2; }
    }
}
