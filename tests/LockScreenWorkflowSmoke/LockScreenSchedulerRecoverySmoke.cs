// Smoke test verifying lock-screen scheduler recovery: startup retry, disable retry, in-flight stop draining, and Spotlight status wording.
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using PrayerAutomation.UI;

/// <summary>Verifies scheduler recovery behavior via STA dispatcher fixtures and fake backends.</summary>
internal static class LockScreenSchedulerRecoverySmoke
{
    /// <summary>Verifies a failed startup restore retains its journal, arms the recovery timer, honors backoff, and retries.</summary>
    /// <param name="start">Trigger time for the fixture transaction.</param>
    internal static Task VerifyStartupRetryAsync(DateTimeOffset start) => InStaAsync(async () =>
    {
        var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Picture);
        var store = new MemoryLockScreenRestoreStore();
        var transaction = new LockScreenImageTransaction(backend, store);
        await transaction.BeginOrExtendAsync("prayer.png", start, CancellationToken.None);
        backend.RestoreFailuresRemaining = 1;
        using var scheduler = CreateScheduler(() => true, transaction);
        bool failed = false;
        try { await scheduler.StartAsync(start.AddMinutes(61)); }
        catch (IOException) { failed = true; }
        Assert(failed && TimerEnabled(scheduler) && store.Load() is not null,
            "A failed startup restore must keep its journal and arm the recovery timer.");
        int calls = backend.RestoreCalls;
        await scheduler.ProcessTickAsync(start.AddMinutes(61).AddSeconds(1));
        Assert(backend.RestoreCalls == calls, "Startup restore failures must honor the 30-second backoff.");
        await scheduler.ProcessTickAsync(start.AddMinutes(61).AddSeconds(31));
        Assert(store.Load() is null && backend.CurrentMode == LockScreenBackgroundMode.Picture,
            "The startup recovery pump must retry after a transient failure without restarting the app.");
        Console.WriteLine("PASS: startup recovery failure retains the journal, arms a timer, honors backoff and retries.");
    });

    /// <summary>Verifies disabling the feature during a restore failure keeps a recovery timer, honors backoff, and restores without reapplying.</summary>
    /// <param name="start">Trigger time for the fixture transaction.</param>
    internal static Task VerifyDisableRetryAsync(DateTimeOffset start) => InStaAsync(async () =>
    {
        bool enabled = true;
        var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Picture);
        var store = new MemoryLockScreenRestoreStore();
        var transaction = new LockScreenImageTransaction(backend, store);
        await transaction.BeginOrExtendAsync("prayer.png", start, CancellationToken.None);
        using var scheduler = CreateScheduler(() => enabled, transaction);
        await scheduler.StartAsync(start.AddMinutes(1));
        enabled = false;
        backend.RestoreFailuresRemaining = 1;
        bool failed = false;
        try { await scheduler.RefreshAsync(start.AddMinutes(2)); }
        catch (IOException) { failed = true; }
        Assert(failed && TimerEnabled(scheduler) && store.Load() is not null,
            "Disabling during a restore failure must keep a recovery timer active.");
        int calls = backend.RestoreCalls;
        await scheduler.ProcessTickAsync(start.AddMinutes(2).AddSeconds(1));
        Assert(backend.RestoreCalls == calls, "Disable failures must honor the recovery backoff.");
        await scheduler.ProcessTickAsync(start.AddMinutes(2).AddSeconds(31));
        Assert(store.Load() is null && backend.ApplyCalls == 1,
            "Disable recovery must restore without applying another temporary image.");
        Console.WriteLine("PASS: disable recovery failure retains a timer, backs off and restores without a new apply.");
    });

    /// <summary>Verifies StopAndRestoreAsync drains an in-flight apply and leaves the scheduler recovery-only.</summary>
    /// <param name="start">Trigger time for the fixture transaction.</param>
    internal static Task VerifyStopInFlightAsync(DateTimeOffset start) => InStaAsync(async () =>
    {
        var resumeApply = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Picture)
        {
            WaitBeforeApplying = resumeApply.Task,
            ApplyStarted = started
        };
        var store = new MemoryLockScreenRestoreStore();
        var transaction = new LockScreenImageTransaction(backend, store);
        using var scheduler = new LockScreenImageScheduler(() => true,
            _ => [new("Dhuhr", TimeSpan.FromHours(12)), new("Asr", TimeSpan.FromHours(12))],
            () => TimeZoneInfo.Utc, "prayer.png", transaction);
        DateTimeOffset due = new(start.Date.AddHours(12), TimeSpan.Zero);
        Task tick = scheduler.ProcessTickAsync(due);
        await started.Task;
        Task<LockScreenTransactionResult> stop = scheduler.StopAndRestoreAsync();
        resumeApply.SetResult();
        await tick;
        await stop;
        Assert(backend.ApplyCalls == 1 && store.Load() is null && backend.CurrentImage == "previous-picture-hash",
            "Exit must drain the in-flight tick and prevent a second trigger from applying after restoration.");
        await scheduler.ProcessTickAsync(due.AddSeconds(1));
        Assert(backend.ApplyCalls == 1 && store.Load() is null,
            "An Exit-requested scheduler must remain recovery-only even while the saved feature is enabled.");
        Console.WriteLine("PASS: Exit drains in-flight apply, blocks remaining triggers and keeps recovery-only intent.");
    });

    /// <summary>Verifies a Spotlight recovery status promises only mode restoration, not Picture-only image restoration.</summary>
    /// <param name="start">Trigger time for the fixture transaction.</param>
    internal static Task VerifySpotlightStatusAsync(DateTimeOffset start) => InStaAsync(async () =>
    {
        var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Spotlight);
        var transaction = new LockScreenImageTransaction(backend, new MemoryLockScreenRestoreStore());
        await transaction.BeginOrExtendAsync("prayer.png", start, CancellationToken.None);
        using var scheduler = CreateScheduler(() => false, transaction);
        string? status = null;
        scheduler.StatusChanged += message => status = message;
        await scheduler.StartAsync(start.AddMinutes(61));
        Assert(status is not null && status.Contains("Picture", StringComparison.Ordinal) &&
               !status.Contains("Mode dan gambar lock screen sebelumnya", StringComparison.Ordinal),
            "A Spotlight recovery status must promise only the prior mode; original-image restoration belongs only to Picture.");
        Console.WriteLine("PASS: recovery status distinguishes mode restoration from Picture-only original-image restoration.");
    });

    /// <summary>Creates a scheduler with no prayer triggers and a UTC timezone.</summary>
    /// <param name="enabled">Enabled predicate for the feature.</param>
    /// <param name="transaction">Transaction owned by the scheduler.</param>
    /// <returns>The configured scheduler.</returns>
    private static LockScreenImageScheduler CreateScheduler(Func<bool> enabled, LockScreenImageTransaction transaction) => new(
        enabled, _ => Array.Empty<LockScreenPrayerTrigger>(), () => TimeZoneInfo.Utc, "prayer.png", transaction);

    /// <summary>Returns true when the scheduler's internal recovery timer is enabled.</summary>
    /// <param name="scheduler">Scheduler to inspect.</param>
    /// <returns>True when the recovery timer is enabled.</returns>
    private static bool TimerEnabled(LockScreenImageScheduler scheduler) =>
        typeof(LockScreenImageScheduler).GetField("_timer", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(scheduler)
            is DispatcherTimer { IsEnabled: true };

    /// <summary>Runs the given test body on a dedicated STA thread with a WPF dispatcher.</summary>
    /// <param name="test">Async test body to execute.</param>
    /// <returns>A task completing when the test body finished or failed.</returns>
    private static Task InStaAsync(Func<Task> test)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
            dispatcher.BeginInvoke(new Action(async () =>
            {
                try { await test(); completion.SetResult(); }
                catch (Exception ex) { completion.SetException(ex); }
                finally { dispatcher.BeginInvokeShutdown(DispatcherPriority.Send); }
            }));
            Dispatcher.Run();
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }

    /// <summary>Throws <see cref="InvalidOperationException"/> when the condition does not hold.</summary>
    /// <param name="condition">Condition that must be true.</param>
    /// <param name="message">Failure message used when the condition is false.</param>
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
