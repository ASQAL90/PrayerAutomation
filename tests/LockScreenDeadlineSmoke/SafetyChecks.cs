using System.IO;
using System.Reflection;
using PrayerAutomation.UI;
using static Program;

internal static class SafetyChecks
{
    private static object? Field(object scheduler, string name) => scheduler.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(scheduler);
    private static async Task ExpectIo(Func<Task> action, string name)
    {
        bool failed = false; try { await action(); } catch (IOException) { failed = true; }
        Check(failed, name);
    }
    internal static async Task RunAsync()
    {
        var lease = new RecordingLease(); var backend = new FakeBackend(); var store = new MemoryStore();
        var traces = new List<string>();
        using (var scheduler = Scheduler(backend, store, lease, triggers: [new("Dhuhr", Epoch.TimeOfDay), new("Asr", Epoch.AddMinutes(1).TimeOfDay)],
            trace: (context, text) => traces.Add(context + " " + text)))
        {
            await scheduler.ProcessTickAsync(Epoch);
            for (int second = 1; second < 55; second++) await scheduler.ProcessTickAsync(Epoch.AddSeconds(second));
            Check(lease.Acquires == 1 && lease.Updates == 1, "healthy active ticks do not renew lease");
            Check(backend.ApplyCalls == 1 && backend.MaintainCalls == 0, "healthy ticks never reapply image");
            Check(traces.Count < 10, "timeline does not spam each second");
            var extension = await scheduler.ProcessTickAsync(Epoch.AddMinutes(1));
            Check(extension.Single() == LockScreenTransactionResult.Extended, "second essential trigger extends real transaction");
            Check(lease.Acquires == 1 && lease.Updates == 2 && lease.Deadline == Epoch.AddMinutes(4), "extension keeps one lease and uses journal deadline");
            await scheduler.ProcessTickAsync(Epoch.AddMinutes(5)); // Simulated resumed timer, not SessionUnlock.
            Check(backend.Restored && store.Snapshot is null, "overdue resumed UTC tick restores WITHOUT SessionUnlock");
            Check(!lease.Requested && lease.Clears == 1, "restored transaction releases request");
            Check(traces.Any(t => t.Contains("tick-gap")) && traces.Any(t => t.Contains("restore-start") && t.Contains("due=True")), "delayed tick and due restore logged");
            Check(traces.All(t => !t.Contains("private") && !t.Contains(new string('A',64))) && traces.Any(t => t.Contains("visual=unverified")), "timeline excludes paths and hashes, distinguishes pixels");
        }
        var pendingPower = new FakePower(); var pendingClock = new ManualClock(Epoch);
        backend = new(); store = new();
        using (var scheduler = Scheduler(backend, store,
            new WindowsLockScreenExecutionRequest(native: pendingPower, clock: pendingClock),
            triggers: [new("Dhuhr", Epoch.TimeOfDay), new("Asr", Epoch.AddMinutes(1).TimeOfDay)]))
        {
            await scheduler.ProcessTickAsync(Epoch);
            pendingClock.Advance(TimeSpan.FromMinutes(1));
            backend.OwnershipChecks.Enqueue(LockScreenImageOwnership.TemporaryImage); // Recover remains Active.
            backend.OwnershipChecks.Enqueue(LockScreenImageOwnership.Unknown); // Begin cannot verify extension.
            var pendingExtension = await scheduler.ProcessTickAsync(Epoch.AddMinutes(1));
            Check(pendingExtension.Single() == LockScreenTransactionResult.PendingVerification &&
                store.Snapshot!.RestoreAtUtc == Epoch.AddMinutes(3), "F1 pending extension keeps original journal deadline");
            Console.WriteLine($"F1 journal={store.Snapshot!.RestoreAtUtc:O} watchdogDueSeconds={pendingClock.Timer!.DueAt.TotalSeconds} creates={pendingPower.Creates} sets={pendingPower.Sets}");
            Check(pendingPower.Creates == 1 && pendingPower.Sets == 1 &&
                pendingClock.Timer!.DueAt == TimeSpan.FromSeconds(210), "F1 pending extension retains original lease/watchdog deadline");
            pendingClock.Advance(TimeSpan.FromSeconds(150)); pendingClock.Fire();
            Check(pendingPower.Clears == 1 && pendingPower.Closes == 1 && store.Snapshot is not null,
                "F1 original deadline plus grace releases fake power without deleting pending journal");
        }
        bool enabled = true; lease = new(); backend = new(); store = new();
        using (var scheduler = Scheduler(backend, store, lease, () => enabled))
        {
            await scheduler.ProcessTickAsync(Epoch); enabled = false;
            await scheduler.RefreshAsync(Epoch.AddSeconds(10));
            Check(!lease.Requested && backend.Restored, "disable clears lease and restores");
        }
        lease = new(); backend = new(); store = new();
        using (var scheduler = Scheduler(backend, store, lease))
        {
            await scheduler.ProcessTickAsync(Epoch);
            var stopped = await scheduler.StopAndRestoreAsync();
            Check(stopped == LockScreenTransactionResult.Restored && !lease.Requested && Field(scheduler,"_timer") is null, "normal stop restores and stops owned timer/request");
        }
        lease = new(); backend = new(); store = new();
        using (var scheduler = Scheduler(backend, store, lease))
        {
            await scheduler.ProcessTickAsync(Epoch); backend.Ownership = LockScreenImageOwnership.DifferentImage;
            await scheduler.ProcessTickAsync(Epoch.AddSeconds(1));
            Check(!lease.Requested && backend.RestoreCalls == 0 && store.Snapshot is null, "external change preserved without restoration write");
        }
        lease = new(); backend = new() { FailApply = true }; store = new();
        using (var scheduler = Scheduler(backend, store, lease))
        {
            await ExpectIo(() => scheduler.ProcessTickAsync(Epoch), "failed apply propagates original failure");
            Check(!lease.Requested, "failed apply releases request");
            backend.FailApply = false;
            await scheduler.ProcessTickAsync(Epoch);
            Check(backend.ApplyCalls == 2, "failed trigger retains original retry/dedupe behavior");
        }
        lease = new(); backend = new(); store = new();
        using (var scheduler = Scheduler(backend, store, lease))
        {
            await scheduler.ProcessTickAsync(Epoch); backend.FailRestore = true;
            await ExpectIo(() => scheduler.ProcessTickAsync(Epoch.AddMinutes(3)), "restore failure retained");
            Check(store.Snapshot is { RestoreRequested: true } && lease.Updates == 1, "throwing restore keeps journal without lease renewal");
            await scheduler.ProcessTickAsync(Epoch.AddMinutes(3).AddSeconds(1));
            Check(backend.RestoreCalls == 1, "original 30-second recovery backoff preserved");
            backend.FailRestore = false;
            await scheduler.ProcessTickAsync(Epoch.AddMinutes(3).AddSeconds(30));
            Check(backend.Restored && !lease.Requested, "backoff retry restores and clears");
        }
        lease = new(); backend = new(); store = new();
        using (var scheduler = Scheduler(backend, store, lease))
        {
            await scheduler.ProcessTickAsync(Epoch); backend.Ownership = LockScreenImageOwnership.Unknown;
            await scheduler.ProcessTickAsync(Epoch.AddMinutes(3));
            await scheduler.ProcessTickAsync(Epoch.AddMinutes(4));
            Check(store.Snapshot is not null && lease.Updates == 1, "pending verification never renews request or deletes journal");
            store.Unreadable = true;
            await ExpectIo(() => scheduler.ProcessTickAsync(Epoch.AddMinutes(5)), "unreadable journal remains fail-closed");
            Check(lease.Updates == 1, "unreadable journal cannot extend lease");
        }
        lease = new(); backend = new() { ApplyBlock = new(TaskCreationOptions.RunContinuationsAsynchronously) }; store = new();
        var concurrent = Scheduler(backend, store, lease);
        Task tick = concurrent.RefreshAsync(Epoch);
        Check(lease.Requested && !tick.IsCompleted, "inflight async apply acquired request");
        await Task.Run(concurrent.Dispose);
        backend.ApplyBlock.SetResult(); await tick;
        Check(lease.Disposed && !lease.Requested && lease.Acquires == 1 && Field(concurrent,"_timer") is null, "Dispose during apply prevents late lease/timer resurrection");
        lease = new(); backend = new() { ApplyBlock = new(TaskCreationOptions.RunContinuationsAsynchronously) }; store = new();
        using (var scheduler = Scheduler(backend, store, lease))
        {
            Task inflight = scheduler.ProcessTickAsync(Epoch);
            Task<LockScreenTransactionResult> stop = scheduler.StopAndRestoreAsync();
            Check(!stop.IsCompleted && !lease.Requested, "shutdown releases immediately but still waits for inflight processing");
            backend.ApplyBlock.SetResult(); await inflight; await stop;
            Check(backend.Restored && lease.Acquires == 1, "late apply does not reacquire during shutdown restoration");
        }
        lease = new() { ThrowRequest = true }; backend = new(); store = new();
        using (var scheduler = Scheduler(backend, store, lease, trace: (_,_) => throw new IOException("private")))
        {
            await scheduler.ProcessTickAsync(Epoch); await scheduler.ProcessTickAsync(Epoch.AddMinutes(3));
            Check(backend.Restored, "throwing power seam and diagnostic callback never block ordinary apply/restore");
        }
        backend = new(); store = new();
        using (var scheduler = new LockScreenImageScheduler(() => true, _ => [new("Dhuhr",Epoch.TimeOfDay)], () => TimeZoneInfo.Utc,
            "synthetic-private",new LockScreenImageTransaction(backend,store), durationMinutes: () => 3))
        {
            await scheduler.ProcessTickAsync(Epoch); await scheduler.ProcessTickAsync(Epoch.AddMinutes(3));
            Check(backend.Restored, "default lease/trace are inert; old isolated callers need no native support");
        }
        foreach (string failure in new[] { "pending", "throw", "unreadable" })
        {
            var power = new FakePower(); var clock = new ManualClock(Epoch);
            var request = new WindowsLockScreenExecutionRequest(native:power,clock:clock);
            backend = new(); store = new();
            using var scheduler = Scheduler(backend,store,request);
            await scheduler.ProcessTickAsync(Epoch);
            if (failure == "pending") backend.Ownership = LockScreenImageOwnership.Unknown;
            else if (failure == "throw") backend.FailRestore = true;
            else store.Unreadable = true;
            try { await scheduler.ProcessTickAsync(Epoch.AddMinutes(3)); } catch (IOException) { }
            clock.Now = Epoch.AddMinutes(3).AddSeconds(30); clock.Fire();
            Check(power.Clears == 1 && power.Closes == 1 && store.Snapshot is not null, failure + " recovery expires real lease without deleting journal");
            try { await scheduler.ProcessTickAsync(Epoch.AddMinutes(4)); } catch (IOException) { }
            Check(power.Creates == 1 && power.Sets == 1, failure + " resumed recovery cannot reacquire expired scenario");
        }
    }
}
