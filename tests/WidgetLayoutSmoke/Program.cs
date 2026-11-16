extern alias ProductUi;

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PrayerAutomation.UI;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            var application = WpfApplicationIsolation.Create();
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
            StartupWidgetLifecycleSmoke.InstallWidgetIsolation();
            Exception? callbackException = null;
            dispatcher.UnhandledException += CaptureDispatcherException;

            try
            {
                dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                WpfApplicationIsolation.Verify(application);
                Assert(args.Length == 0 || args.SequenceEqual(new[] { "--marquee-render-only" }) ||
                       args.SequenceEqual(new[] { "--startup-widget-only" }) ||
                       args.SequenceEqual(new[] { "--minimalist-only" }) ||
                       args.SequenceEqual(new[] { "--widget-settings-only" }) ||
                       args.SequenceEqual(new[] { "--ui-minor-only" }) ||
                       args.SequenceEqual(new[] { "--ui-minor-top-only" }) ||
                       args.SequenceEqual(new[] { "--ui-minor-selection-only" }) ||
                       args.SequenceEqual(new[] { "--ui-minor-size-only" }) ||
                       args.SequenceEqual(new[] { "--lockscreen-duration-only" }) ||
                       args.SequenceEqual(new[] { "--copy-safety-only" }) ||
                       args.SequenceEqual(new[] { "--copy-gps-only" }) ||
                       args.SequenceEqual(new[] { "--online-advanced-only" }),
                    "Only a known isolated WPF suite may be selected.");
                if (args.SequenceEqual(new[] { "--online-advanced-only" }))
                    CopyHumanizeSmoke.RunOnlineAdvanced(application);
                else if (args.SequenceEqual(new[] { "--copy-safety-only" }))
                    CopyHumanizeSmoke.RunSafety(application);
                else if (args.SequenceEqual(new[] { "--copy-gps-only" }))
                    CopyHumanizeSmoke.RunGps(application);
                else if (args.SequenceEqual(new[] { "--ui-minor-only" }))
                    UiMinorSmoke.Run(application);
                else if (args.SequenceEqual(new[] { "--ui-minor-size-only" }))
                    UiMinorSmoke.RunSize(application);
                else if (args.SequenceEqual(new[] { "--ui-minor-selection-only" }))
                    UiMinorSmoke.RunSelection(application);
                else if (args.SequenceEqual(new[] { "--ui-minor-top-only" }))
                    UiMinorSmoke.RunTop(application);
                else if (args.SequenceEqual(new[] { "--marquee-render-only" }))
                    MarqueeRenderSmoke.Run();
                else if (args.SequenceEqual(new[] { "--startup-widget-only" }))
                    StartupWidgetLifecycleSmoke.Run(application);
                else if (args.SequenceEqual(new[] { "--minimalist-only" }))
                    VerifyMinimalistLayout(application);
                else if (args.SequenceEqual(new[] { "--lockscreen-duration-only" }))
                    LockScreenDurationSettingsSmoke.Run(application);
                else if (args.SequenceEqual(new[] { "--widget-settings-only" }))
                    WidgetSettingsInteractionSmoke.Run(application);
                else
                {
                    VerifyAutoLockScheduling();
                    VerifyCorrectionInputBehavior();
                    VerifyEssentialStaticLayout();
                    MarqueeRenderSmoke.Run();
                    VerifyEssentialMarqueeLayout();
                    VerifyMinimalistLayout(application);
                    VerifyMediumDetailLayout();
                    CopyHumanizeSmoke.RunOnlineAdvanced(application);
                    UiMinorSmoke.Run(application);
                    WidgetSettingsInteractionSmoke.Run(application);
                    StartupWidgetLifecycleSmoke.Run(application);
                    VisualAcceptanceSmoke.Run(application);
                }
                dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                WpfApplicationIsolation.Verify(application);
                Assert(callbackException is null, $"A WPF dispatcher callback failed: {callbackException}");
            }
            finally
            {
                dispatcher.UnhandledException -= CaptureDispatcherException;
                WpfApplicationIsolation.Verify(application);
                application.Shutdown();
                WpfApplicationIsolation.Verify(application);
            }

            Console.WriteLine(args.Length == 0
                ? "PASS: actual WPF layout distributes Essential static/marquee and aligns Minimalist content."
                : "PASS: selected isolated actual-WPF suite completed without dispatcher errors.");
            return 0;

            void CaptureDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs e)
            {
                callbackException = e.Exception;
                e.Handled = true;
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static void VerifyCorrectionInputBehavior()
    {
        Type? behaviorType = typeof(ScheduleWidgetWindow).Assembly.GetType("PrayerAutomation.UI.CorrectionInputBehavior");
        Assert(behaviorType is not null, "Prayer correction editors must have a reusable WPF input behavior.");
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        MethodInfo Require(string name) => behaviorType!.GetMethod(name, flags)
            ?? throw new InvalidOperationException($"CorrectionInputBehavior.{name} is missing.");

        var isReplacementAllowed = Require("IsReplacementAllowed");
        bool Allows(string current, int start, int length, string replacement) =>
            (bool)isReplacementAllowed.Invoke(null, [current, start, length, replacement])!;
        Assert(Allows("29", 0, 2, "-30") && Allows("30", 0, 2, "+30") && Allows("-", 1, 0, ""),
            "A signed correction within ±30, including an editable sign, must be accepted.");
        Assert(!Allows("30", 2, 0, "1") && !Allows("0", 1, 0, "x") &&
               !Allows("0", 0, 1, " ") && !Allows("30", 0, 2, "-31"),
            "Out-of-range, non-integer, and malformed correction edits must be rejected.");

        MethodInfo tryParse = Require("TryParseCorrection");
        object?[] blankArgs = ["", 0];
        object?[] positiveArgs = ["+30", 0];
        object?[] invalidArgs = ["-31", 0];
        Assert((bool)tryParse.Invoke(null, blankArgs)! && (int)blankArgs[1]! == 0 &&
               (bool)tryParse.Invoke(null, positiveArgs)! && (int)positiveArgs[1]! == 30 &&
               !(bool)tryParse.Invoke(null, invalidArgs)!,
            "Blank correction fields must save as zero and signed values must respect the ±30-minute bound.");

        MethodInfo attach = Require("Attach");
        var editor = new TextBox { Text = "30", SelectionStart = 2 };
        var decrease = new Button();
        var increase = new Button();
        attach.Invoke(null, [editor, decrease, increase]);
        var composition = new TextComposition(InputManager.Current, editor, "1");
        var input = new TextCompositionEventArgs(Keyboard.PrimaryDevice, composition)
        {
            RoutedEvent = TextCompositionManager.PreviewTextInputEvent
        };
        editor.RaiseEvent(input);
        Assert(input.Handled, "The real WPF PreviewTextInput event must block an invalid correction edit.");

        editor.Text = "0";
        decrease.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Assert(editor.Text == "-1", "The WPF decrease button must change the correction by one minute.");
        editor.Text = "30";
        increase.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Assert(editor.Text == "30", "The WPF increase button must clamp at the +30-minute limit.");

        MethodInfo isDateReplacementAllowed = Require("IsDateReplacementAllowed");
        bool AllowsDate(string current, int start, int length, string replacement) =>
            (bool)isDateReplacementAllowed.Invoke(null, [current, start, length, replacement])!;
        Assert(AllowsDate("+6", 1, 1, "7") && AllowsDate("-7", 0, 2, "") &&
               !AllowsDate("7", 1, 0, "8") && !AllowsDate("0", 0, 1, "-8"),
            "Calendar date corrections must accept signed values only within the ±7-day range.");
        MethodInfo tryParseDate = Require("TryParseDateCorrection");
        object?[] blankDateArgs = ["", 0];
        object?[] positiveDateArgs = ["+7", 0];
        object?[] invalidDateArgs = ["-8", 0];
        Assert((bool)tryParseDate.Invoke(null, blankDateArgs)! && (int)blankDateArgs[1]! == 0 &&
               (bool)tryParseDate.Invoke(null, positiveDateArgs)! && (int)positiveDateArgs[1]! == 7 &&
               !(bool)tryParseDate.Invoke(null, invalidDateArgs)!,
            "Blank calendar corrections must save as zero and enforce the ±7-day boundary.");
        MethodInfo attachDate = Require("AttachDateCorrection");
        var dateEditor = new TextBox { Text = "7", SelectionStart = 1 };
        var decreaseDate = new Button();
        var increaseDate = new Button();
        attachDate.Invoke(null, [dateEditor, decreaseDate, increaseDate]);
        var invalidDateComposition = new TextComposition(InputManager.Current, dateEditor, "8");
        var invalidDateInput = new TextCompositionEventArgs(Keyboard.PrimaryDevice, invalidDateComposition)
        {
            RoutedEvent = TextCompositionManager.PreviewTextInputEvent
        };
        dateEditor.RaiseEvent(invalidDateInput);
        Assert(invalidDateInput.Handled && !increaseDate.IsEnabled && decreaseDate.IsEnabled,
            "The real date-correction editor must reject an out-of-range keystroke and disable the +7 helper.");
        dateEditor.Text = "0";
        decreaseDate.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        increaseDate.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Assert(dateEditor.Text == "0", "Date helper buttons must decrement/increment by one day without changing the range.");
    }

    private static void VerifyAutoLockScheduling()
    {
        const BindingFlags instanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        PropertyInfo? autoLockProperty = typeof(AppSettings).GetProperty("AutoLockEnabled", instanceFlags);
        Assert(autoLockProperty is not null, "Auto-lock must be a separately persisted setting.");
        Assert((bool)autoLockProperty!.GetValue(new AppSettings())! == false,
            "Auto-lock must be disabled by default.");

        Type? schedulerType = typeof(ScheduleWidgetWindow).Assembly.GetType("PrayerAutomation.UI.AutoLockScheduler");
        Assert(schedulerType is not null, "Auto-lock must use a distinct, testable scheduler.");
        Type providerType = typeof(Func<DateTime, IReadOnlyList<PrayerEntry>>);
        ConstructorInfo? constructor = schedulerType!.GetConstructor(instanceFlags, null,
            [typeof(AppSettings), typeof(Func<bool>), providerType], null);
        Assert(constructor is not null, "The auto-lock scheduler must allow safe injected schedule/lock dependencies for tests.");
        MethodInfo? processTick = schedulerType.GetMethod("ProcessTick", instanceFlags);
        Assert(processTick is not null, "The auto-lock scheduler must expose its deterministic tick seam to tests.");

        PrayerEntry[] schedule =
        [
            new("Fajr", "Subuh", TimeSpan.FromHours(4)),
            new("Sunrise", "Terbit", TimeSpan.FromHours(5)),
            new("Dhuha", "Duha", TimeSpan.FromHours(6)),
            new("Dhuhr", "Dzuhur", TimeSpan.FromHours(12)),
            new("Asr", "Ashar", TimeSpan.FromHours(15)),
            new("Maghrib", "Maghrib", TimeSpan.FromHours(18)),
            new("Isha", "Isya", TimeSpan.FromHours(19))
        ];
        int lockCalls = 0;
        var settings = new AppSettings { RemindersEnabled = false };
        autoLockProperty!.SetValue(settings, true);
        object scheduler = constructor!.Invoke([
            settings,
            new Func<bool>(() => { lockCalls++; return true; }),
            new Func<DateTime, IReadOnlyList<PrayerEntry>>(_ => schedule)
        ])!;
        try
        {
            bool Process(DateTime now) => (bool)processTick!.Invoke(scheduler, [now])!;
            Assert(Process(new DateTime(2026, 5, 1, 4, 0, 1)) && lockCalls == 1,
                "Opted-in auto-lock must work even when text reminders are disabled.");
            Assert(!Process(new DateTime(2026, 5, 1, 4, 0, 1)) && lockCalls == 1,
                "One prayer event must not lock the workstation more than once per scheduler run.");
            Assert(!Process(new DateTime(2026, 5, 1, 5, 0, 0)) &&
                   !Process(new DateTime(2026, 5, 1, 6, 0, 0)) && lockCalls == 1,
                "Sunrise and Dhuha must not trigger the five-prayer auto-lock.");
            Assert(!Process(new DateTime(2026, 5, 1, 12, 0, 6)) && lockCalls == 1,
                "A missed prayer-time window must not lock the workstation late.");
            Assert(Process(new DateTime(2026, 5, 1, 12, 0, 0)) && lockCalls == 2 &&
                   Process(new DateTime(2026, 5, 1, 15, 0, 0)) && lockCalls == 3 &&
                   Process(new DateTime(2026, 5, 1, 18, 0, 0)) && lockCalls == 4 &&
                   Process(new DateTime(2026, 5, 1, 19, 0, 0)) && lockCalls == 5,
                "Subuh, Dzuhur, Ashar, Maghrib, and Isya must each trigger one opt-in lock event.");
        }
        finally
        {
            if (scheduler is IDisposable disposable) disposable.Dispose();
        }

        var disabledSettings = new AppSettings();
        autoLockProperty!.SetValue(disabledSettings, false);
        object disabledScheduler = constructor!.Invoke([
            disabledSettings,
            new Func<bool>(() => { lockCalls++; return true; }),
            new Func<DateTime, IReadOnlyList<PrayerEntry>>(_ => schedule)
        ])!;
        try
        {
            Assert(!(bool)processTick!.Invoke(disabledScheduler, [new DateTime(2026, 5, 1, 12, 0, 0)])! && lockCalls == 5,
                "With auto-lock disabled, no workstation-lock request may be dispatched.");
        }
        finally
        {
            if (disabledScheduler is IDisposable disposable) disposable.Dispose();
        }

        int failedScheduleCalls = 0;
        var failedSettings = new AppSettings();
        autoLockProperty!.SetValue(failedSettings, true);
        object failedScheduler = constructor!.Invoke([
            failedSettings,
            new Func<bool>(() => true),
            new Func<DateTime, IReadOnlyList<PrayerEntry>>(_ =>
            {
                failedScheduleCalls++;
                throw new InvalidOperationException("fixture schedule failure");
            })
        ])!;
        MethodInfo? refresh = schedulerType!.GetMethod("Refresh", instanceFlags);
        Assert(refresh is not null, "Saving settings must invalidate a cached auto-lock schedule failure.");
        try
        {
            DateTime failedDate = new(2026, 5, 2, 12, 0, 0);
            Assert(!(bool)processTick!.Invoke(failedScheduler, [failedDate])! &&
                   !(bool)processTick.Invoke(failedScheduler, [failedDate.AddSeconds(1)])! &&
                   failedScheduleCalls == 1,
                "A failed prayer schedule must be cached for the day rather than recalculated on every one-second tick.");
            refresh!.Invoke(failedScheduler, null);
            _ = processTick.Invoke(failedScheduler, [failedDate]);
            Assert(failedScheduleCalls == 2,
                "Refreshing settings must allow a failed auto-lock schedule to be retried.");
        }
        finally
        {
            if (failedScheduler is IDisposable disposable) disposable.Dispose();
        }
    }

    private static void VerifyEssentialStaticLayout()
    {
        using var widget = new WidgetFixture("essential", "static");
        (FrameworkElement body, _) = widget.Layout();

        string[] prayerNames = ["Subuh", "Dzuhur", "Ashar", "Maghrib", "Isya"];
        TextBlock[] items = Descendants(body).OfType<TextBlock>()
            .Where(item => prayerNames.Any(name => item.Text.StartsWith(name + "  ", StringComparison.Ordinal)))
            .OrderBy(item => RelativeBounds(item, body).X)
            .ToArray();
        Assert(items.Length == prayerNames.Length, $"Expected five static essential prayer labels, found {items.Length}.");
        Color expectedForeground = (Color)ColorConverter.ConvertFromString(ThemeManager.Current.WidgetText);
        Assert(items.All(item => item.Foreground is SolidColorBrush brush && brush.Color == expectedForeground),
            "All five static prayer labels must use the high-contrast widget text token, not low-contrast prayer accent colors.");

        double firstCenter = CenterX(items[0], body);
        double lastCenter = CenterX(items[^1], body);
        Assert(firstCenter <= body.ActualWidth * 0.22,
            $"Essential static first prayer is not aligned near the left edge: center={firstCenter:F1}, body={body.ActualWidth:F1}.");
        Assert(lastCenter >= body.ActualWidth * 0.78,
            $"Essential static last prayer is not distributed across the widget: center={lastCenter:F1}, body={body.ActualWidth:F1}.");
    }

    private static void VerifyEssentialMarqueeLayout()
    {
        using var widget = new WidgetFixture("essential", "marquee");
        (FrameworkElement body, _) = widget.Layout();

        Grid? viewport = Descendants(body).OfType<Grid>().SingleOrDefault(grid => grid.ClipToBounds);
        Assert(viewport is not null, "Essential marquee must keep a clipping viewport.");
        Grid marqueeViewport = viewport ?? throw new InvalidOperationException("Essential marquee viewport is missing.");
        Assert(marqueeViewport.ActualWidth >= body.ActualWidth * 0.90,
            $"Essential marquee viewport does not fill the widget: viewport={marqueeViewport.ActualWidth:F1}, body={body.ActualWidth:F1}.");
        TextBlock? line = Descendants(marqueeViewport).OfType<TextBlock>().SingleOrDefault(text =>
            System.Windows.Automation.AutomationProperties.GetAutomationId(text) == "EssentialMarqueeText");
        Assert(line is not null, "Essential marquee must expose one identifiable text line.");
        TextBlock marqueeLine = line ?? throw new InvalidOperationException("Essential marquee text is missing.");
        string[] prayerNames = ["Subuh", "Dzuhur", "Ashar", "Maghrib", "Isya"];
        Assert(prayerNames.All(name => marqueeLine.Text.Contains(name, StringComparison.Ordinal)) &&
               marqueeLine.TextTrimming == TextTrimming.None,
            "The marquee must retain the complete five-prayer schedule without character-count truncation.");

        Assert(marqueeLine.RenderTransform is TranslateTransform,
            "Essential marquee must move a full-width text line through its clipping viewport.");
        MethodInfo advance = typeof(ScheduleWidgetWindow).GetMethod("AdvanceMarquee", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ScheduleWidgetWindow.AdvanceMarquee is missing.");
        double fullWidthOffset = ((TranslateTransform)marqueeLine.RenderTransform).X;
        advance.Invoke(widget.Window, null);
        Assert(((TranslateTransform)marqueeLine.RenderTransform).X < fullWidthOffset,
            "Selecting marquee must move text across the normal full-width bar, even when the original five-prayer line fits.");
        Assert(marqueeLine.Text.Contains("Masehi:", StringComparison.Ordinal) &&
               marqueeLine.Text.Contains("Hijriyah:", StringComparison.Ordinal) &&
               marqueeLine.Text.Contains("Jam:", StringComparison.Ordinal) &&
               marqueeLine.Text.Contains("Berikutnya:", StringComparison.Ordinal),
            "The moving one-line widget must include both dates, current local time and the next-prayer countdown.");

        double fullViewportWidth = marqueeViewport.ActualWidth;
        marqueeViewport.Width = Math.Max(120, body.ActualWidth * 0.45);
        widget.Window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(widget.Window.UpdateLayout));
        widget.Window.UpdateLayout();
        Assert(marqueeViewport.ActualWidth < fullViewportWidth * 0.70 && marqueeLine.ActualWidth > marqueeViewport.ActualWidth,
            "A narrow Essential viewport must clip the complete schedule line by measured pixel width.");
        double previousOffset = ((TranslateTransform)marqueeLine.RenderTransform).X;
        advance.Invoke(widget.Window, null);
        Assert(((TranslateTransform)marqueeLine.RenderTransform).X < previousOffset,
            "Marquee movement must advance smoothly in measured pixels rather than dropping whole characters.");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var startTimer = (DispatcherTimer)typeof(ScheduleWidgetWindow).GetField("_marqueeStartTimer", flags)!.GetValue(widget.Window)!;
        var motionTimer = (DispatcherTimer)typeof(ScheduleWidgetWindow).GetField("_marqueeTimer", flags)!.GetValue(widget.Window)!;
        string previousText = marqueeLine.Text;
        double liveOffset = ((TranslateTransform)marqueeLine.RenderTransform).X;
        var pumpFrame = new DispatcherFrame();
        var stopPump = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(2700) };
        stopPump.Tick += (_, _) => { stopPump.Stop(); pumpFrame.Continue = false; };
        stopPump.Start();
        Dispatcher.PushFrame(pumpFrame);
        Assert(!startTimer.IsEnabled && motionTimer.IsEnabled &&
               ((TranslateTransform)marqueeLine.RenderTransform).X < liveOffset - 5,
            "The real WPF Dispatcher timers must animate marquee after its startup pause without invoking AdvanceMarquee manually.");
        Assert(marqueeLine.Text != previousText,
            "The marquee clock/countdown must refresh while its real movement timer is active.");

        var update = typeof(ScheduleWidgetWindow).GetMethod("UpdateMarqueeContent", flags)!;
        double beforeRefresh = ((TranslateTransform)marqueeLine.RenderTransform).X;
        update.Invoke(widget.Window, [new DateTime(2026, 5, 1, 10, 30, 5)]);
        widget.Window.UpdateLayout();
        Assert(Math.Abs(((TranslateTransform)marqueeLine.RenderTransform).X - beforeRefresh) < 0.001,
            "Updating the marquee clock/countdown must preserve the current scrolling offset.");
        Assert(marqueeLine.Text.Contains("Libur nasional Indonesia", StringComparison.Ordinal) &&
               marqueeLine.Text.Contains("Hari Buruh", StringComparison.Ordinal) &&
               marqueeLine.Text.Contains("Jam: 10:30:05", StringComparison.Ordinal),
            "The marquee must include the existing calendar's fixed-date holiday and the injected local clock.");
        Assert(marqueeLine.Text.Contains(WidgetMarqueeContent.ReligiousMessageAt(0), StringComparison.Ordinal),
            "Clock refreshes must retain the first attributed offline hadith until its full marquee cycle finishes.");

        for (int cycle = 1; cycle <= 5; cycle++)
        {
            double cycleWidth = (double)typeof(ScheduleWidgetWindow).GetField("_marqueeCycleWidth", flags)!.GetValue(widget.Window)!;
            long cyclesBefore = (long)typeof(ScheduleWidgetWindow).GetField("_marqueeCompletedCycles", flags)!.GetValue(widget.Window)!;
            typeof(ScheduleWidgetWindow).GetField("_marqueeOffset", flags)!.SetValue(widget.Window, cycleWidth - 0.1);
            ((TranslateTransform)marqueeLine.RenderTransform).X = -(cycleWidth - 0.1);
            var cycleFrame = new DispatcherFrame();
            var stopCycle = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(320) };
            stopCycle.Tick += (_, _) => { stopCycle.Stop(); cycleFrame.Continue = false; };
            stopCycle.Start();
            Dispatcher.PushFrame(cycleFrame);
            string expectedMessage = WidgetMarqueeContent.ReligiousMessageAt(cycle);
            long cyclesAfter = (long)typeof(ScheduleWidgetWindow).GetField("_marqueeCompletedCycles", flags)!.GetValue(widget.Window)!;
            double widthAfter = (double)typeof(ScheduleWidgetWindow).GetField("_marqueeCycleWidth", flags)!.GetValue(widget.Window)!;
            double offsetAfter = (double)typeof(ScheduleWidgetWindow).GetField("_marqueeOffset", flags)!.GetValue(widget.Window)!;
            Console.WriteLine($"MARQUEE_CYCLE_DIAGNOSTIC requested={cycle} before={cyclesBefore} after={cyclesAfter} width-before={cycleWidth:0.#} width-after={widthAfter:0.#} offset-after={offsetAfter:0.#} motion-enabled={motionTimer.IsEnabled}");
            Assert(marqueeLine.Text.Contains(expectedMessage, StringComparison.Ordinal) &&
                   !marqueeLine.Text.Contains(WidgetMarqueeContent.ReligiousMessageAt(cycle - 1), StringComparison.Ordinal),
                "The real marquee timer must rotate to the next attributed offline hadith only after the previous full cycle, then repeat after five sources.");
            Assert(System.Windows.Automation.AutomationProperties.GetName(marqueeLine).Contains(expectedMessage, StringComparison.Ordinal),
                "The marquee accessibility text must follow the same source-backed message as the visible text.");
        }

        typeof(ScheduleWidgetWindow).GetField("_marqueeContent", flags)!.SetValue(widget.Window, "Teks singkat");
        typeof(ScheduleWidgetWindow).GetField("_marqueeConfiguredContent", flags)!.SetValue(widget.Window, string.Empty);
        typeof(ScheduleWidgetWindow).GetField("_marqueeOffset", flags)!.SetValue(widget.Window, 0d);
        advance.Invoke(widget.Window, null);
        Assert(((TranslateTransform)marqueeLine.RenderTransform).X < 0 && marqueeLine.Width > marqueeViewport.ActualWidth,
            "Even a genuinely short marquee must repeat and keep moving rather than reverting to centered static text.");
        Console.WriteLine("MARQUEE_RUNTIME PASS: full-width motion, live timers, updated clock/countdown, offset preservation, holiday content and short-text repeat.");
    }

    private static void VerifyMinimalistLayout(Application application)
    {
        using var essential = new WidgetFixture("essential", "static");
        essential.Layout();
        using var widget = new WidgetFixture("minimalist", "static");
        (FrameworkElement body, Grid host) = widget.Layout();
        var frame = (Border)widget.Window.Content;
        var essentialFrame = (Border)essential.Window.Content;
        Console.WriteLine($"WIDGET_MINIMALIST_SLIM height={widget.Window.ActualHeight:0.#} essential={essential.Window.ActualHeight:0.#} " +
            $"paddingTop={frame.Padding.Top:0.#} paddingBottom={frame.Padding.Bottom:0.#} content={host.ActualHeight:0.#}");
        Assert(widget.Window.ActualHeight == essential.Window.ActualHeight && widget.Window.ActualHeight == 40 &&
               frame.Padding == essentialFrame.Padding && frame.Padding.Top <= 5 && frame.Padding.Bottom <= 5,
            "Minimalist must match the 40-DIP one-line widget height and compact vertical padding.");

        TextBlock[] labels = Descendants(body).OfType<TextBlock>().ToArray();
        TextBlock nextPrayer = labels.SingleOrDefault(text => text.Text.StartsWith("Berikutnya:", StringComparison.Ordinal))
            ?? throw new InvalidOperationException("Minimalist must show the next prayer label and time.");
        TextBlock countdown = labels.SingleOrDefault(text => text.Text.Length == 8 && text.Text[2] == ':' && text.Text[5] == ':')
            ?? throw new InvalidOperationException("Minimalist must show a separate countdown value.");
        Assert(RelativeBounds(nextPrayer!, body).X <= 4,
            $"Minimalist next-prayer text must begin at the left content edge: x={RelativeBounds(nextPrayer!, body).X:F1}.");
        Rect countdownBounds = RelativeBounds(countdown!, body);
        Assert(countdownBounds.Right >= body.ActualWidth * 0.90,
            $"Minimalist countdown must align near the right edge: right={countdownBounds.Right:F1}, body={body.ActualWidth:F1}.");
        Assert(nextPrayer!.FontSize == 15 && countdown!.FontSize == 14 &&
               nextPrayer.TextWrapping == TextWrapping.NoWrap && countdown.TextWrapping == TextWrapping.NoWrap,
            "Slimming must preserve both existing font sizes and the single-row layout.");

        int checks = 0;
        foreach (string prayer in new[] { "Subuh", "Dzuhur", "Ashar", "Maghrib", "Isya" })
        {
            nextPrayer.Text = $"Berikutnya: {prayer} · 23:59";
            countdown.Text = "23:59:59";
            widget.Window.UpdateLayout();
            foreach (TextBlock text in labels)
            {
                var formatted = new FormattedText(text.Text, System.Globalization.CultureInfo.InvariantCulture,
                    text.FlowDirection, new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch),
                    text.FontSize, text.Foreground, VisualTreeHelper.GetDpi(text).PixelsPerDip);
                Rect bounds = RelativeBounds(text, host);
                Assert(text.ActualHeight >= formatted.Height - 0.5 &&
                       text.ActualWidth >= formatted.WidthIncludingTrailingWhitespace - 0.5 &&
                       bounds.Top >= -0.5 && bounds.Bottom <= host.ActualHeight + 0.5 &&
                       bounds.Left >= -0.5 && bounds.Right <= host.ActualWidth + 0.5,
                    "Every next-prayer label and countdown must fit fully inside the slim widget without clipping.");
                checks++;
            }
            Assert(RelativeBounds(nextPrayer, body).Right + 13 <= RelativeBounds(countdown, body).Left,
                "Slimming must preserve the gap between the next prayer and its countdown.");
        }
        Button hide = Descendants(host).OfType<Button>().Single();
        Rect hideBounds = RelativeBounds(hide, host);
        Assert(hide.ActualHeight >= 28 && hide.ActualWidth >= 28 && hideBounds.Top >= -0.5 &&
               hideBounds.Bottom <= host.ActualHeight + 0.5 && hideBounds.Right <= host.ActualWidth + 0.5,
            "The hide button must retain its 28-DIP target and stay fully inside the compact frame.");
        hide.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Assert(!widget.Window.IsVisible, "The real Minimalist hide button must still hide the widget.");
        widget.Layout();
        Assert(widget.Window.IsVisible && widget.Window.ActualHeight == 40,
            "A hidden Minimalist widget must reopen with the same compact height.");

        using var fixture = new WidgetSettingsInteractionSmoke.Fixture(application, isolateSave: true, preventActivation: true);
        Window actual = fixture.Widget;
        string baseline = System.Text.Json.JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType);
        foreach (string theme in fixture.Field<ComboBox>("ThemeBox").Items.OfType<ComboBoxItem>().Select(item => (string)item.Tag))
        {
            fixture.Select("ThemeBox", theme);
            fixture.Select("WidgetModeBox", "minimalist");
            actual.UpdateLayout();
            var actualFrame = (Border)actual.Content;
            Assert(ReferenceEquals(actual, fixture.Widget) && actual.ActualHeight == 40 &&
                   actualFrame.Padding.Top == 5 && actualFrame.Padding.Bottom == 5 &&
                   Descendants(actualFrame).OfType<TextBlock>().Count(text =>
                       System.Windows.Automation.AutomationProperties.GetAutomationId(text) is "MinimalistNextPrayer" or "MinimalistCountdown") == 2 &&
                   System.Text.Json.JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType) == baseline,
                "The compiled production widget must preview the same compact Minimalist layout in every theme without changing saved settings.");
            fixture.InvokeMain("CancelSettings_Click", fixture.MainWindow, new RoutedEventArgs());
            Assert(fixture.WidgetMode == "essential" && actual.Height == 40 && fixture.CurrentThemeId == "light",
                "Cancel must restore saved one-line appearance and theme on the same compiled widget.");
            checks++;
        }
        Console.WriteLine($"WIDGET_MINIMALIST_SLIM PASS: checks={checks} height=40 padding=5 text-unclipped hide/reopen actual-preview/Cancel preserved.");
    }

    private static void VerifyMediumDetailLayout()
    {
        using var widget = new WidgetFixture("detail", "static");
        (FrameworkElement body, Grid host) = widget.Layout();
        var frame = (Border)widget.Window.Content;
        Rect bounds = RelativeBounds(body, host);
        double topGap = bounds.Top;
        double bottomGap = host.ActualHeight - bounds.Bottom;

        Console.WriteLine($"VISUAL MediumDetail height={widget.Window.ActualHeight:0.#} body={body.ActualHeight:0.#}/{host.ActualHeight:0.#} topGap={topGap:0.#} bottomGap={bottomGap:0.#} rightPadding={frame.Padding.Right:0.#}");
        Assert(widget.Window.ActualHeight <= 340,
            $"Medium Detail window remains too tall: {widget.Window.ActualHeight:F1} DIP.");
        Assert(body.ActualHeight >= host.ActualHeight * 0.75 && topGap <= 42 && bottomGap <= 42,
            $"Medium Detail has excessive vertical whitespace: body={body.ActualHeight:F1}/{host.ActualHeight:F1}, top={topGap:F1}, bottom={bottomGap:F1}.");
        Assert(frame.Padding.Right <= 10,
            $"Medium Detail right padding remains too wide: {frame.Padding.Right:F1} DIP.");

        Button? closeButton = Descendants(host).OfType<Button>()
            .FirstOrDefault(button => string.Equals(button.Content?.ToString(), "×", StringComparison.Ordinal));
        Assert(closeButton is not null && closeButton.Margin.Left <= 4,
            "Medium Detail close button still leaves an excessive right-side gap.");

        int scheduleRows = Descendants(body).OfType<TextBlock>()
            .Count(text => text.Text.Contains(':', StringComparison.Ordinal));
        Assert(scheduleRows >= 7,
            $"Medium Detail must retain all schedule rows after tightening layout; found {scheduleRows}.");
    }

    private static Rect RelativeBounds(FrameworkElement element, FrameworkElement ancestor)
    {
        Point origin = element.TransformToAncestor(ancestor).Transform(new Point(0, 0));
        return new Rect(origin, new Size(element.ActualWidth, element.ActualHeight));
    }

    private static double CenterX(FrameworkElement element, FrameworkElement ancestor)
    {
        Rect bounds = RelativeBounds(element, ancestor);
        return bounds.Left + bounds.Width / 2;
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            yield return child;
            foreach (DependencyObject descendant in Descendants(child))
                yield return descendant;
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class WidgetFixture : IDisposable
    {
        private readonly ScheduleWidgetWindow _window;

        public WidgetFixture(string mode, string motion)
        {
            var settings = new AppSettings
            {
                WidgetMode = mode,
                WidgetMotion = motion,
                WidgetPosition = "tc",
                RemindersEnabled = false,
                AutoLockEnabled = false,
                LockScreenEnabled = false,
                LockScreenOptIn = false,
                StartupEnabled = false
            };
            Assert(!settings.RemindersEnabled && !settings.AutoLockEnabled && !settings.LockScreenEnabled &&
                   !settings.LockScreenOptIn && !settings.StartupEnabled,
                "Fail closed: every synthetic widget fixture must disable reminders, session lock, image changes, consent and startup before constructing its window.");
            _window = new ScheduleWidgetWindow(settings)
            {
                Topmost = false,
                ShowActivated = false,
                Opacity = 0,
                IsHitTestVisible = false
            };
        }

        public ScheduleWidgetWindow Window => _window;

        public (FrameworkElement Body, Grid Host) Layout()
        {
            _window.Show();
            _window.UpdateLayout();
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Loaded, new Action(_window.UpdateLayout));
            _window.UpdateLayout();

            var frame = (Border)_window.Content;
            var host = (Grid)frame.Child;
            var body = (FrameworkElement)host.Children[0];
            Assert(_window.ActualWidth > 0 && _window.ActualHeight > 0,
                "WPF must realize non-zero widget geometry.");
            Assert(body.ActualWidth > 0 && body.ActualHeight > 0,
                "WPF must realize non-zero content geometry.");
            return (body, host);
        }

        public void Dispose() => _window.Close();
    }
}

namespace PrayerAutomation.UI
{
    internal static class AppLogger
    {
        public static Exception? LastException { get; private set; }

        public static void Write(string context, Exception exception) => LastException = exception;
    }
}
