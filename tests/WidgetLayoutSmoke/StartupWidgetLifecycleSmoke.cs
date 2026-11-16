extern alias ProductUi;

using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Fixture = WidgetSettingsInteractionSmoke.Fixture;

internal static class StartupWidgetLifecycleSmoke
{
    private const BindingFlags I = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static bool _isolationInstalled;

    public static void InstallWidgetIsolation()
    {
        if (_isolationInstalled) return;
        Type widgetType = typeof(ProductUi::PrayerAutomation.UI.App).Assembly
            .GetType("PrayerAutomation.UI.ScheduleWidgetWindow", true)!;
        Window.ShowActivatedProperty.OverrideMetadata(widgetType,
            new FrameworkPropertyMetadata(false, null, (_, _) => false));
        Window.TopmostProperty.OverrideMetadata(widgetType,
            new FrameworkPropertyMetadata(false, null, (_, _) => false));
        UIElement.OpacityProperty.OverrideMetadata(widgetType,
            new UIPropertyMetadata(0d, null, (_, _) => 0d));
        UIElement.IsHitTestVisibleProperty.OverrideMetadata(widgetType,
            new UIPropertyMetadata(false, null, (_, _) => false));
        UIElement.IsEnabledProperty.OverrideMetadata(widgetType,
            new UIPropertyMetadata(false, null, (_, _) => false));
        _isolationInstalled = true;
    }

    public static void Run(Application application)
    {
        var failures = new List<string>();
        int cases = 0;
        foreach (bool tray in new[] { false, true })
        {
            Case("disabled-draft-" + (tray ? "tray" : "cancel"), true, false, (fixture, recovery) =>
            {
                string saved = Serialize(fixture);
                Assert(fixture.WidgetOrNull is null, "Startup race fixture must begin with no actual widget.");
                fixture.Field<CheckBox>("WidgetEnabledCheckBox").IsChecked = false;
                Pump();
                Assert(fixture.WidgetOrNull is null && Serialize(fixture) == saved,
                    "The actual Unchecked event must keep the disabled draft isolated while recovery is pending.");
                recovery.Complete();
                bool draftRespected = fixture.WidgetOrNull?.IsVisible != true;
                Discard(fixture, tray);
                bool baselineRestored = fixture.WidgetOrNull?.IsVisible == true &&
                    fixture.WidgetMode == "essential" && fixture.WidgetMotion == "static";
                Assert(Serialize(fixture) == saved, "Startup/Cancel/tray must not mutate saved scheduler inputs.");
                var errors = new List<string>();
                if (!draftRespected) errors.Add("Startup completion must respect an active disabled widget draft.");
                if (!baselineRestored) errors.Add(tray
                    ? "Tray hide after pending startup must restore the saved-enabled widget that was initially absent."
                    : "Cancel after pending startup must restore the saved-enabled widget that was initially absent.");
                Assert(errors.Count == 0, string.Join(Environment.NewLine, errors));
            });
            Case("motion-initial-null-" + (tray ? "tray" : "cancel"), true, false, (fixture, recovery) =>
            {
                string saved = Serialize(fixture);
                fixture.Select("WidgetMotionBox", "marquee");
                Window preview = fixture.Widget;
                Assert(preview.IsVisible && fixture.WidgetMotion == "marquee" &&
                    Find<TextBlock>(preview, "EssentialMarqueeText") is not null,
                    "The real WidgetMotionBox event must create an actual marquee preview from an initially absent widget.");
                recovery.Complete();
                Assert(ReferenceEquals(preview, fixture.WidgetOrNull) && fixture.WidgetMotion == "marquee",
                    "Startup completion must preserve the same actual window and its active motion draft.");
                Discard(fixture, tray);
                Assert(fixture.WidgetOrNull?.IsVisible == true && fixture.WidgetMotion == "static" &&
                    fixture.SelectedTag("WidgetMotionBox") == "static" && Serialize(fixture) == saved,
                    "Discard after startup must restore saved static motion and enabled visibility from an initial-null preview.");
            });
            Case("saved-disabled-preview-" + (tray ? "tray" : "cancel"), false, false, (fixture, recovery) =>
            {
                string saved = Serialize(fixture);
                fixture.Field<CheckBox>("WidgetEnabledCheckBox").IsChecked = true;
                fixture.Select("WidgetMotionBox", "marquee");
                Assert(fixture.Widget.IsVisible && fixture.WidgetMotion == "marquee",
                    "Checking the actual widget control must preview a saved-disabled widget without saving it.");
                recovery.Complete();
                Assert(fixture.Widget.IsVisible && fixture.WidgetMotion == "marquee",
                    "Startup must not suppress an enabled draft whose saved setting is disabled.");
                Discard(fixture, tray);
                Assert(fixture.WidgetOrNull is null && Serialize(fixture) == saved,
                    "Discard must close an initial-null temporary widget when the saved widget setting remains disabled.");
            });
            Case("discard-before-recovery-" + (tray ? "tray" : "cancel"), true, false, (fixture, recovery) =>
            {
                fixture.Select("WidgetMotionBox", "marquee");
                Discard(fixture, tray);
                Assert(fixture.WidgetOrNull is null,
                    "Discard before recovery completes must close the temporary initial-null preview.");
                recovery.Complete();
                Assert(fixture.Widget.IsVisible && fixture.WidgetMotion == "static",
                    "Startup after an earlier Cancel/tray hide must create the saved-enabled static widget.");
            });
        }
        Case("save-disabled-before-recovery", true, false, (fixture, recovery) =>
        {
            fixture.Field<CheckBox>("WidgetEnabledCheckBox").IsChecked = false;
            Save(fixture);
            Assert(fixture.SettingsType.GetProperty("WidgetEnabled", I)!.GetValue(fixture.Settings) is false,
                "The actual Save handler must establish the disabled draft as the saved baseline.");
            recovery.Complete();
            Assert(fixture.WidgetOrNull?.IsVisible != true,
                "Startup must not recreate a widget disabled by Save while recovery was pending.");
        });
        foreach (bool savedEnabled in new[] { false, true })
            Case("save-motion-manual-hide-before-recovery-" + savedEnabled, savedEnabled, false, (fixture, recovery) =>
            {
                fixture.Field<CheckBox>("WidgetEnabledCheckBox").IsChecked = true;
                fixture.Select("WidgetMotionBox", "marquee");
                Save(fixture);
                Window savedWidget = fixture.Widget;
                HideWidget(savedWidget);
                recovery.Complete();
                Assert(ReferenceEquals(savedWidget, fixture.WidgetOrNull) && !savedWidget.IsVisible &&
                    fixture.WidgetMotion == "marquee",
                    "Startup completion must preserve a newly saved widget's manual hidden state and saved motion.");
                Discard(fixture, false);
                Assert(!savedWidget.IsVisible && fixture.WidgetMotion == "marquee",
                    "Cancel after a completed Save must not roll back its saved motion or manual visibility.");
            });
        Case("preexisting-manual-hidden", true, true, (fixture, recovery) =>
        {
            Window widget = fixture.Widget;
            HideWidget(widget);
            recovery.Complete();
            Assert(ReferenceEquals(widget, fixture.WidgetOrNull) && !widget.IsVisible,
                "Startup completion must not unhide a preexisting manually hidden widget.");
        });
        Case("preexisting-hidden-motion-preview-cancel", true, true, (fixture, recovery) =>
        {
            Window widget = fixture.Widget;
            HideWidget(widget);
            fixture.Select("WidgetMotionBox", "marquee");
            Assert(widget.IsVisible && fixture.WidgetMotion == "marquee", "A deliberate motion preview must be visible.");
            recovery.Complete();
            Discard(fixture, false);
            Assert(ReferenceEquals(widget, fixture.WidgetOrNull) && !widget.IsVisible && fixture.WidgetMotion == "static",
                "Cancel must restore a preexisting widget's manually hidden state across pending startup.");
        });
        Case("shutdown-guard", true, false, (fixture, recovery) =>
        {
            fixture.SetAppField("_isShuttingDown", true);
            recovery.Complete();
            Assert(fixture.WidgetOrNull is null, "Startup completion must not create a widget during shutdown.");
        });
        foreach (bool savedEnabled in new[] { false, true })
            Case("normal-completion-" + savedEnabled, savedEnabled, false, (fixture, recovery) =>
            {
                recovery.Complete();
                Assert(savedEnabled ? fixture.WidgetOrNull?.IsVisible == true : fixture.WidgetOrNull is null,
                    "Normal isolated startup completion must follow the saved widget setting.");
            });
        Assert(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Console.WriteLine($"WIDGET_STARTUP_LIFECYCLE PASS: cases={cases} actual checkbox/motion/Cancel/Save/tray, initial-null, saved-disabled, manual-hidden, shutdown; production sole-await replay only.");

        void Case(string name, bool enabled, bool preexisting, Action<Fixture, StartupTailReplay> verify)
        {
            cases++;
            try
            {
                using var fixture = new Fixture(application, isolateSave: true, initiallyHasWidget: preexisting,
                    savedWidgetEnabled: enabled, preventActivation: true);
                using var recovery = new StartupTailReplay(application, fixture);
                verify(fixture, recovery);
                Console.WriteLine("STARTUP_CASE PASS: " + name);
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.StartsWith("Fail closed:", StringComparison.Ordinal)) throw;
                failures.Add(name + ": " + exception.Message);
                Console.WriteLine("STARTUP_CASE FAIL: " + name + " :: " + exception.Message);
            }
        }
    }

    private static string Serialize(Fixture fixture) => JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType);
    private static void Save(Fixture fixture)
    {
        fixture.Field<Button>("SaveSettingsButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Pump();
        Assert(fixture.Field<TextBlock>("SettingsStatusLabel").Text.StartsWith("Pengaturan tersimpan.", StringComparison.Ordinal) &&
            fixture.StartupWrites == 1 && !fixture.StartupValue && fixture.Field<TabControl>("MainTabs").SelectedIndex == 2,
            "The real Save click must finish using synthetic persistence/startup boundaries and retain Settings tab.");
        using JsonDocument saved = JsonDocument.Parse(System.IO.File.ReadAllText(fixture.SavePath!));
        foreach (string name in new[] { "enabled", "auto_lock_enabled", "lock_enabled", "lock_screen_opt_in", "startup" })
            Assert(!saved.RootElement.GetProperty(name).GetBoolean(), "Synthetic Save readback must keep all consequential flags disabled.");
        Assert(saved.RootElement.GetProperty("widget").GetBoolean() ==
            (bool)fixture.SettingsType.GetProperty("WidgetEnabled", I)!.GetValue(fixture.Settings)! &&
            saved.RootElement.GetProperty("widget_motion").GetString() == fixture.SelectedTag("WidgetMotionBox"),
            "The isolated persistence file must read back the actual widget draft saved by the production handler.");
    }
    private static void Discard(Fixture fixture, bool tray)
    {
        if (tray) fixture.MainWindow.Close();
        else Descendants(fixture.MainWindow).OfType<Button>().Single(button =>
            button.Content as string == "Batalkan perubahan").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Pump();
        if (tray) Assert(!fixture.MainWindow.IsVisible, "The actual Closing event must hide the main window to the tray.");
    }
    private static void HideWidget(Window widget)
    {
        Descendants(widget).OfType<Button>().Single(button => button.Content as string == "×")
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Pump();
        Assert(!widget.IsVisible, "The actual widget hide button must hide the real window.");
    }
    private static T? Find<T>(DependencyObject root, string id) where T : DependencyObject =>
        Descendants(root).OfType<T>().SingleOrDefault(element =>
            System.Windows.Automation.AutomationProperties.GetAutomationId(element) == id);
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            yield return child;
            foreach (DependencyObject descendant in Descendants(child)) yield return descendant;
        }
    }
    private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    // Never invoke OnStartup: replay only its verified, already-suspended sole await.
    private sealed class StartupTailReplay : IDisposable
    {
        private readonly object _machine;
        private readonly MethodInfo _moveNext;
        private readonly FieldInfo _state;
        private readonly Fixture _fixture;
        private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool _finished;
        private Exception? _error;

        public StartupTailReplay(Application application, Fixture fixture)
        {
            _fixture = fixture;
            Assert(ReferenceEquals(SynchronizationContext.Current?.GetType(), typeof(DispatcherSynchronizationContext)),
                "Startup replay must be created on the installed WPF DispatcherSynchronizationContext.");
            Type appType = typeof(ProductUi::PrayerAutomation.UI.App);
            MethodInfo startup = appType.GetMethod("OnStartup", I)!;
            Type machineType = startup.GetCustomAttribute<AsyncStateMachineAttribute>()!.StateMachineType;
            FieldInfo[] fields = machineType.GetFields(I);
            _state = fields.Single(field => field.Name == "<>1__state" && field.FieldType == typeof(int));
            FieldInfo owner = fields.Single(field => field.FieldType == appType);
            FieldInfo builder = fields.Single(field => field.FieldType == typeof(AsyncVoidMethodBuilder));
            FieldInfo awaiter = fields.Single(field => field.FieldType == typeof(TaskAwaiter));
            Assert(fields.Count(field => field.Name.StartsWith("<>u__", StringComparison.Ordinal)) == 1,
                "Fail closed: startup replay requires exactly one production await.");
            _moveNext = machineType.GetMethod("MoveNext", I)!;
            int resumeOffset = VerifyResumeOnly(_moveNext, _state, owner, awaiter);
            VerifyNoStartupObjects();
            _machine = Activator.CreateInstance(machineType)!;
            _state.SetValue(_machine, 0);
            owner.SetValue(_machine, application);
            awaiter.SetValue(_machine, _completion.Task.GetAwaiter());
            builder.SetValue(_machine, AsyncVoidMethodBuilder.Create());
            _completion.Task.GetAwaiter().OnCompleted(() =>
            {
                try
                {
                    Assert(Dispatcher.CurrentDispatcher.CheckAccess() &&
                        SynchronizationContext.Current is DispatcherSynchronizationContext,
                        "Production startup continuation must run on the fixture's UI dispatcher.");
                    Assert((int)_state.GetValue(_machine)! == 0,
                        "Fail closed: never enter the initial startup state or prefix.");
                    _moveNext.Invoke(_machine, null);
                    Assert((int)_state.GetValue(_machine)! == -2, "Replayed production startup tail must finish exactly once.");
                    VerifyNoStartupObjects();
                }
                catch (Exception exception) { _error = exception; }
                finally { _finished = true; }
            });
            Console.WriteLine($"STARTUP_REPLAY_GUARD PASS: production=OnStartup sole-await=0 resume-offset={resumeOffset} prefix-calls=0 startup-objects=null.");
        }
        public void Complete()
        {
            Assert(!_finished, "Startup recovery must be completed only once.");
            _completion.SetResult();
            var frame = new DispatcherFrame();
            var timeout = new DispatcherTimer { Interval = TimeSpan.FromSeconds(8) };
            var poll = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            timeout.Tick += (_, _) => frame.Continue = false;
            poll.Tick += (_, _) => { if (_finished) frame.Continue = false; };
            timeout.Start();
            poll.Start();
            try { if (!_finished) Dispatcher.PushFrame(frame); }
            finally { timeout.Stop(); poll.Stop(); }
            Assert(_finished && _error is null, "Controlled production startup continuation failed: " + _error);
        }
        public void Dispose() { if (!_finished && !_completion.Task.IsCompleted) Complete(); }
        private void VerifyNoStartupObjects()
        {
            foreach (string name in new[] { "_singleInstanceMutex", "_tray", "_reminders", "_autoLock", "_lockScreenScheduler" })
                Assert(_fixture.AppField(name) is null,
                    "Fail closed: isolated startup tail must never create or run real startup objects (" + name + ").");
        }
    }

    private static int VerifyResumeOnly(MethodInfo method, FieldInfo state, FieldInfo owner, FieldInfo awaiter)
    {
        Instruction[] il = Decode(method).ToArray();
        int selector = Array.FindIndex(il, instruction => instruction.OpCode.FlowControl == FlowControl.Cond_Branch);
        Assert(selector > 0 && selector < 12 &&
            (il[selector].OpCode == OpCodes.Brfalse || il[selector].OpCode == OpCodes.Brfalse_S),
            "Fail closed: the compiled sole-await state must branch directly on zero before startup prefix.");
        int stateLoad = Array.FindIndex(il, instruction => instruction.OpCode == OpCodes.Ldfld && Equals(instruction.Operand, state));
        Assert(stateLoad >= 0 && stateLoad + 1 < selector, "Fail closed: missing entry state load.");
        int stateLocal = LocalIndex(il[stateLoad + 1].OpCode, il[stateLoad + 1].Operand, store: true);
        Assert(stateLocal >= 0 && LocalIndex(il[selector - 1].OpCode, il[selector - 1].Operand, store: false) == stateLocal,
            "Fail closed: the entry branch must test the production state local, not a different flag.");
        foreach (Instruction instruction in il.Take(selector))
            Assert(instruction.OpCode == OpCodes.Ldarg_0 || instruction.OpCode == OpCodes.Nop ||
                (instruction.OpCode == OpCodes.Ldfld && (Equals(instruction.Operand, state) || Equals(instruction.Operand, owner))) ||
                LocalIndex(instruction.OpCode, instruction.Operand, store: true) >= 0 ||
                LocalIndex(instruction.OpCode, instruction.Operand, store: false) >= 0,
                "Fail closed: no calls, writes, or allocations may precede the verified resume selector.");
        int target = (int)il[selector].Operand!;
        int resume = Array.FindIndex(il, instruction => instruction.Offset == target);
        for (int branch = 0; branch < 2 && il[resume].OpCode == OpCodes.Nop; branch++)
        {
            resume++;
            Assert(LocalIndex(il[resume].OpCode, il[resume].Operand, store: false) == stateLocal &&
                (il[resume + 1].OpCode == OpCodes.Brfalse || il[resume + 1].OpCode == OpCodes.Brfalse_S),
                "Fail closed: nested startup exception region must route state zero directly to the resumed awaiter.");
            target = (int)il[resume + 1].Operand!;
            resume = Array.FindIndex(il, instruction => instruction.Offset == target);
        }
        Assert(resume > selector && il[resume].OpCode == OpCodes.Ldarg_0 &&
            il[resume + 1].OpCode == OpCodes.Ldfld && Equals(il[resume + 1].Operand, awaiter),
            "Fail closed: zero-state target must begin by loading only the injected awaiter.");
        int getResult = Array.FindIndex(il, resume, instruction => instruction.OpCode.FlowControl == FlowControl.Call);
        Assert(getResult > resume && il[getResult].Operand is MethodInfo call &&
            call.DeclaringType == typeof(TaskAwaiter) && call.Name == nameof(TaskAwaiter.GetResult),
            "Fail closed: the first resumed call must be the injected awaiter's GetResult.");
        foreach (Instruction instruction in il.Skip(resume).Take(getResult - resume))
            Assert(instruction.OpCode.FlowControl is not (FlowControl.Branch or FlowControl.Cond_Branch or FlowControl.Call or FlowControl.Return or FlowControl.Throw) &&
                (instruction.Operand is not FieldInfo field || Equals(field, state) || Equals(field, awaiter)) &&
                (instruction.OpCode != OpCodes.Initobj || Equals(instruction.Operand, typeof(TaskAwaiter))),
                "Fail closed: resumed awaiter cleanup must not reach any startup prefix or unrelated field.");
        return target;
    }
    private static int LocalIndex(OpCode op, object? operand, bool store)
    {
        OpCode[] small = store ? [OpCodes.Stloc_0, OpCodes.Stloc_1, OpCodes.Stloc_2, OpCodes.Stloc_3]
            : [OpCodes.Ldloc_0, OpCodes.Ldloc_1, OpCodes.Ldloc_2, OpCodes.Ldloc_3];
        int index = Array.IndexOf(small, op);
        if (index >= 0) return index;
        if (op == (store ? OpCodes.Stloc : OpCodes.Ldloc) || op == (store ? OpCodes.Stloc_S : OpCodes.Ldloc_S))
            return Convert.ToInt32(operand);
        return -1;
    }
    private sealed record Instruction(int Offset, OpCode OpCode, object? Operand);
    private static IEnumerable<Instruction> Decode(MethodInfo method)
    {
        byte[] bytes = method.GetMethodBody()!.GetILAsByteArray()!;
        Dictionary<short, OpCode> codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!)
            .GroupBy(op => op.Value).ToDictionary(group => group.Key, group => group.First());
        int cursor = 0;
        while (cursor < bytes.Length)
        {
            int offset = cursor;
            short value = bytes[cursor++];
            if (value == 0xfe) value = unchecked((short)(0xfe00 | bytes[cursor++]));
            OpCode op = codes[value];
            object? operand = null;
            switch (op.OperandType)
            {
                case OperandType.InlineNone: break;
                case OperandType.ShortInlineI: operand = (sbyte)bytes[cursor++]; break;
                case OperandType.ShortInlineVar: operand = bytes[cursor++]; break;
                case OperandType.InlineVar: operand = BitConverter.ToUInt16(bytes, cursor); cursor += 2; break;
                case OperandType.InlineI: operand = ReadInt(); break;
                case OperandType.InlineI8: operand = BitConverter.ToInt64(bytes, cursor); cursor += 8; break;
                case OperandType.ShortInlineR: operand = BitConverter.ToSingle(bytes, cursor); cursor += 4; break;
                case OperandType.InlineR: operand = BitConverter.ToDouble(bytes, cursor); cursor += 8; break;
                case OperandType.ShortInlineBrTarget: int delta = (sbyte)bytes[cursor++]; operand = cursor + delta; break;
                case OperandType.InlineBrTarget: int distance = ReadInt(); operand = cursor + distance; break;
                case OperandType.InlineSwitch:
                    int count = ReadInt();
                    int end = cursor + count * 4;
                    var targets = new int[count];
                    for (int i = 0; i < count; i++) targets[i] = ReadInt() + end;
                    operand = targets;
                    break;
                case OperandType.InlineField: operand = method.Module.ResolveField(ReadInt()); break;
                case OperandType.InlineMethod: operand = method.Module.ResolveMethod(ReadInt()); break;
                case OperandType.InlineType: operand = method.Module.ResolveType(ReadInt()); break;
                case OperandType.InlineTok: operand = method.Module.ResolveMember(ReadInt()); break;
                case OperandType.InlineString:
                case OperandType.InlineSig: operand = ReadInt(); break;
                default: throw new InvalidOperationException("Fail closed: unsupported compiled startup IL operand.");
            }
            yield return new Instruction(offset, op, operand);
        }
        int ReadInt() { int value = BitConverter.ToInt32(bytes, cursor); cursor += 4; return value; }
    }
}

// WPF queues startup at Send priority in its constructor even without Run.
// Abort only that exact callback before any Dispatcher pump; retain the actual
// product App type so its compiled BAML resources still load without imitation.
internal static class WpfApplicationIsolation
{
    private static DispatcherOperation? _startup;
    private static int _abortedCallbacks;

    internal static Application Create()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo callbackField = typeof(DispatcherOperation).GetField("_method", flags)
            ?? throw new InvalidOperationException("Fail closed: cannot verify the WPF constructor's queued startup callback.");
        Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        dispatcher.Hooks.OperationPosted += AbortStartup;
        try
        {
            var application = (Application)Activator.CreateInstance(typeof(ProductUi::PrayerAutomation.UI.App))!;
            Verify(application);
            return application;
        }
        finally { dispatcher.Hooks.OperationPosted -= AbortStartup; }

        void AbortStartup(object? sender, DispatcherHookEventArgs e)
        {
            if (callbackField.GetValue(e.Operation) is not Delegate callback ||
                callback.Method.DeclaringType != typeof(Application) || callback.Method.Name != "<.ctor>b__1_0") return;
            if (e.Operation.Priority != DispatcherPriority.Send || e.Operation.Status != DispatcherOperationStatus.Pending ||
                !e.Operation.Abort())
                throw new InvalidOperationException("Fail closed: WPF startup callback must be aborted while still pending.");
            _startup = e.Operation;
            _abortedCallbacks++;
        }
    }

    internal static void Verify(Application application)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        if (_abortedCallbacks != 1 || _startup?.Status != DispatcherOperationStatus.Aborted)
            throw new InvalidOperationException("Fail closed: WPF's implicit startup callback must be suppressed exactly once.");
        foreach (string name in new[] { "_singleInstanceMutex", "_tray", "_reminders", "_autoLock", "_lockScreenScheduler" })
            if (typeof(ProductUi::PrayerAutomation.UI.App).GetField(name, flags)!.GetValue(application) is not null)
                throw new InvalidOperationException("Fail closed: the harness must not enter production startup (" + name + ").");
        Console.WriteLine("WPF_STARTUP_SUPPRESSED PASS: implicit-callbacks-aborted=1 production-prefix=0 app.Run=0 scheduler-objects=0.");
    }
}
