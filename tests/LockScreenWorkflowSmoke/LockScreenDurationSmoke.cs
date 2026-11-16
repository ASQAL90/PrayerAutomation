using System.Reflection;
using PrayerAutomation.UI;

internal static class LockScreenDurationSmoke
{
    internal static async Task VerifyConfiguredHoldAsync(DateTimeOffset start)
    {
        MethodInfo method = typeof(LockScreenImageTransaction).GetMethod("BeginOrExtendAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
        ParameterInfo[] parameters = method.GetParameters();
        Require(parameters.Length == 4 && parameters[3].ParameterType == typeof(TimeSpan?),
            "The real transaction must accept the user's image-hold duration instead of always using 60 minutes.");
        int checks = 0;
        foreach (LockScreenBackgroundMode mode in new[] { LockScreenBackgroundMode.Spotlight,
                     LockScreenBackgroundMode.Slideshow, LockScreenBackgroundMode.Picture })
        {
            var backend = new FakeLockScreenBackend(mode);
            var store = new MemoryLockScreenRestoreStore();
            var transaction = new LockScreenImageTransaction(backend, store);
            var applied = (Task<LockScreenTransactionResult>)method.Invoke(transaction,
                ["prayer.png", start, CancellationToken.None, TimeSpan.FromMinutes(7)])!;
            Require(await applied == LockScreenTransactionResult.Applied &&
                    store.Load()?.RestoreAtUtc == start.AddMinutes(7),
                mode + ": persist exactly the selected seven-minute hold deadline.");
            checks++;
            var restarted = new LockScreenImageTransaction(backend, store);
            foreach (double seconds in new[] { 3d, 30d, 60d, 419d })
            {
                Require(await restarted.RecoverAsync(true, start.AddSeconds(seconds), CancellationToken.None)
                        == LockScreenTransactionResult.Active && backend.RestoreCalls == 0 &&
                        backend.CurrentMode == LockScreenBackgroundMode.Picture && store.Load() is { IsApplied: true },
                    mode + ": do not restore a confirmed prayer image before its selected deadline (seconds=" + seconds + ").");
                checks++;
            }
            Require(await restarted.RecoverAsync(true, start.AddMinutes(7), CancellationToken.None)
                    == LockScreenTransactionResult.Restored && backend.CurrentMode == mode && store.Load() is null,
                mode + ": restore the original mode only when the selected hold expires.");
            checks++;
        }
        Console.WriteLine("LOCKSCREEN_CONFIGURED_HOLD PASS: checks=" + checks + "; modes=3; selected_minutes=7; early_ticks=3,30,60,419; native_calls=0.");
        await VerifySchedulerUsesSavedDurationAsync(start);
    }

    private static async Task VerifySchedulerUsesSavedDurationAsync(DateTimeOffset start)
    {
        var constructor = typeof(LockScreenImageScheduler).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single();
        Require(constructor.GetParameters().Length == 7 &&
                constructor.GetParameters()[6].ParameterType == typeof(Func<int>),
            "The production scheduler must consume the saved image-hold duration instead of its sixty-minute default.");
        var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Spotlight);
        var store = new MemoryLockScreenRestoreStore();
        var transaction = new LockScreenImageTransaction(backend, store);
        int savedMinutes = 7;
        Func<bool> enabled = () => true;
        Func<DateTime, IReadOnlyList<LockScreenPrayerTrigger>> schedule = _ =>
            [new("Asr", start.TimeOfDay), new("Maghrib", start.AddMinutes(6).TimeOfDay)];
        Func<TimeZoneInfo> timezone = () => TimeZoneInfo.CreateCustomTimeZone("Synthetic-UTC+7", TimeSpan.FromHours(7), "Synthetic", "Synthetic");
        Func<int> duration = () => savedMinutes;
        using var scheduler = (LockScreenImageScheduler)constructor.Invoke([enabled, schedule, timezone, "prayer.png", transaction, null, duration]);
        var statuses = new List<string>();
        scheduler.StatusChanged += statuses.Add;
        Require((await scheduler.ProcessTickAsync(start)).Single() == LockScreenTransactionResult.Applied &&
                store.Load()?.RestoreAtUtc == start.AddMinutes(7),
            "The real prayer-event path must journal exactly the saved seven-minute duration.");
        savedMinutes = 1;
        await scheduler.ProcessTickAsync(start.AddSeconds(3));
        await scheduler.ProcessTickAsync(start.AddSeconds(30));
        await scheduler.ProcessTickAsync(start.AddSeconds(60));
        Require(backend.RestoreCalls == 0 && store.Load()?.RestoreAtUtc == start.AddMinutes(7),
            "Changing a setting or an early recovery tick must not shorten a currently active image deadline.");
        Require((await scheduler.ProcessTickAsync(start.AddMinutes(6))).Single() == LockScreenTransactionResult.Extended &&
                store.Load()?.RestoreAtUtc == start.AddMinutes(7) && backend.ApplyCalls == 1,
            "An overlapping prayer uses the new one-minute setting without shortening the existing deadline or rewriting the image.");
        string expectedTime = start.AddMinutes(7).ToLocalTime().ToString("HH:mm");
        Require(statuses.Last().Contains(expectedTime, StringComparison.Ordinal),
            "The displayed recovery time must use the persistent deadline, not a fixed sixty-minute estimate.");
        await scheduler.ProcessTickAsync(start.AddMinutes(7));
        Require(store.Load() is null && backend.RestoreCalls == 1 && backend.CurrentMode == LockScreenBackgroundMode.Spotlight,
            "The scheduler must restore mode-only Spotlight at the selected persisted deadline.");
        Console.WriteLine("LOCKSCREEN_SCHEDULER_DURATION PASS: checks=5; saved_minutes=7; early_ticks=3,30,60; overlap_minutes=1; native_calls=0.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
