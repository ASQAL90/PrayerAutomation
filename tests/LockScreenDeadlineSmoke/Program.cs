// Smoke test verifying the lock-screen image scheduler end to end with fake backends, stores, leases, and native power APIs.
using System.IO;
using System.Reflection;
using PrayerAutomation.UI;

/// <summary>Entry point and shared helpers for the lock-screen scheduler smoke suite.</summary>
internal static class Program
{
    /// <summary>Assertion counter for summary reporting.</summary>
    internal static int Assertions;
    /// <summary>Fixed pivot time used by all scheduler scenarios.</summary>
    internal static readonly DateTimeOffset Epoch = new(2030, 1, 2, 12, 0, 0, TimeSpan.Zero);
    /// <summary>Requires the condition and records and prints the assertion; throws otherwise.</summary>
    /// <param name="condition">Condition that must hold.</param>
    /// <param name="name">Assertion label.</param>
    internal static void Check(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException("FAIL: " + name);
        Assertions++;
        Console.WriteLine("PASS: " + name);
    }
    /// <summary>Builds a scheduler with injected fakes via the non-public constructor.</summary>
    /// <param name="backend">Fake snapshot/image backend.</param>
    /// <param name="store">Fake restore store.</param>
    /// <param name="lease">Execution-request lease.</param>
    /// <param name="enabled">Optional enabled predicate.</param>
    /// <param name="triggers">Optional prayer trigger list.</param>
    /// <param name="trace">Optional trace sink.</param>
    /// <returns>The constructed scheduler.</returns>
    internal static LockScreenImageScheduler Scheduler(FakeBackend backend, MemoryStore store, ILockScreenExecutionRequest lease,
        Func<bool>? enabled = null, IReadOnlyList<LockScreenPrayerTrigger>? triggers = null, Action<string,string>? trace = null)
    {
        var ctor = typeof(LockScreenImageScheduler).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single();
        var values = new Dictionary<string, object?> {
            ["isEnabled"] = enabled ?? (() => true),
            ["scheduleForDate"] = (Func<DateTime,IReadOnlyList<LockScreenPrayerTrigger>>)(_ => triggers ?? [new("Dhuhr", Epoch.TimeOfDay)]),
            ["timeZone"] = (Func<TimeZoneInfo>)(() => TimeZoneInfo.Utc),
            ["imagePath"] = "synthetic-private-image.png",
            ["transaction"] = new LockScreenImageTransaction(backend, store),
            ["log"] = null,
            ["durationMinutes"] = (Func<int>)(() => 3),
            ["executionRequest"] = lease,
            ["trace"] = trace
        };
        return (LockScreenImageScheduler)ctor.Invoke(ctor.GetParameters().Select(p => values[p.Name!]).ToArray());
    }
    /// <summary>Runs all scheduler smoke scenarios; returns 0 on success, 1 on failure.</summary>
    /// <returns>Process exit code.</returns>
    private static async Task<int> Main()
    {
        try
        {
            var lease = new RecordingLease(); var backend = new FakeBackend(); var store = new MemoryStore();
            backend.BeforeApply = () => Check(lease.Requested, "execution requested BEFORE image apply");
            using var scheduler = Scheduler(backend, store, lease);
            var results = await scheduler.ProcessTickAsync(Epoch);
            Check(results.Single() == LockScreenTransactionResult.Applied, "actual transaction applied");
            Check(lease.Deadline == store.Snapshot!.RestoreAtUtc, "lease bound to actual journal deadline");
            var traces = new List<string>();
            using var diagnosticScheduler = Scheduler(new FakeBackend(), new MemoryStore(), new RecordingLease(),
                trace: (context, text) => traces.Add(context + " " + text));
            await diagnosticScheduler.ProcessTickAsync(Epoch);
            Check(traces.Any(t => t.Contains("trigger") && t.Contains("utc=")), "privacy-safe UTC trigger timeline is emitted");
            await SafetyChecks.RunAsync();
            NativeChecks.Run();
            await BackendChecks.RunAsync();
            Console.WriteLine($"RESULT assertions={Assertions} failed=0 native=FAKE app=NOT_CONSTRUCTED");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine($"RESULT assertions={Assertions} failed=1 native=FAKE app=NOT_CONSTRUCTED");
            return 1;
        }
    }
}
/// <summary>Fake execution-request lease recording acquires, updates, clears, and disposal.</summary>
internal sealed class RecordingLease : ILockScreenExecutionRequest
{
    public bool Requested, Disposed, ThrowRequest; public int Acquires, Updates, Clears; public DateTimeOffset? Deadline;
    /// <summary>Records the requested deadline; throws when configured to fail or when disposed.</summary>
    /// <param name="deadline">Requested lease deadline.</param>
    public void RequestUntil(DateTimeOffset deadline)
    {
        if (ThrowRequest) throw new IOException("synthetic-private-path");
        if (Disposed) throw new ObjectDisposedException("fake lease");
        if (!Requested) Acquires++;
        Requested = true; Deadline = deadline; Updates++;
    }
    /// <summary>Marks the lease cleared when it was requested.</summary>
    public void Clear() { if (Requested) Clears++; Requested = false; Deadline = null; }
    /// <summary>Clears and marks the lease disposed.</summary>
    public void Dispose() { Clear(); Disposed = true; }
}
/// <summary>In-memory restore store with an injectable unreadable mode.</summary>
internal sealed class MemoryStore : ILockScreenRestoreStore
{
    public LockScreenRestoreSnapshot? Snapshot;
    public bool Unreadable;
    /// <summary>Returns the current snapshot, or throws when unreadable mode is set.</summary>
    /// <returns>The stored snapshot, if any.</returns>
    public LockScreenRestoreSnapshot? Load() => Unreadable ? throw new IOException("synthetic-private-path") : Snapshot;
    /// <summary>Stores the snapshot.</summary>
    /// <param name="snapshot">Snapshot to store.</param>
    public void Save(LockScreenRestoreSnapshot snapshot) => Snapshot = snapshot;
    /// <summary>Clears the stored snapshot.</summary>
    public void Delete() => Snapshot = null;
}
/// <summary>Fake image backend recording operations and injecting failures or blocking.</summary>
internal sealed class FakeBackend : ILockScreenImageBackend
{
    public bool Applied, Restored, FailApply, FailRestore; public int ApplyCalls, RestoreCalls, MaintainCalls;
    public LockScreenImageOwnership Ownership = LockScreenImageOwnership.TemporaryImage;
    public Queue<LockScreenImageOwnership> OwnershipChecks = new();
    public Action? BeforeApply; public TaskCompletionSource? ApplyBlock;
    /// <summary>Returns a fixed synthetic snapshot for the given deadline.</summary>
    /// <param name="imagePath">Image path to capture (unused by the fake).</param>
    /// <param name="deadline">Restore deadline for the snapshot.</param>
    /// <returns>The synthetic snapshot.</returns>
    public LockScreenRestoreSnapshot CaptureSnapshot(string imagePath, DateTimeOffset deadline) =>
        new(1, LockScreenBackgroundMode.Spotlight, 0, 1, null, null, "private-temporary-path", new string('A',64), deadline, false);
    /// <summary>Simulates applying the temporary image, honoring configured blocking and failure.</summary>
    /// <param name="snapshot">Snapshot being applied.</param>
    /// <param name="token">Cancellation token.</param>
    public async Task ApplyTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken token)
    {
        ApplyCalls++; BeforeApply?.Invoke(); if (ApplyBlock is not null) await ApplyBlock.Task;
        if (FailApply) throw new IOException("synthetic-private-path");
        Applied = true; Restored = false;
    }
    /// <summary>Simulates maintaining the temporary image by resetting ownership to the temporary image.</summary>
    /// <param name="snapshot">Snapshot being maintained.</param>
    /// <param name="token">Cancellation token.</param>
    public Task MaintainTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken token)
    { MaintainCalls++; Ownership = LockScreenImageOwnership.TemporaryImage; return Task.CompletedTask; }
    /// <summary>Returns the queued ownership result when present, otherwise the current ownership.</summary>
    /// <param name="snapshot">Snapshot to classify (unused by the fake).</param>
    public LockScreenImageOwnership CheckTemporaryImage(LockScreenRestoreSnapshot snapshot) =>
        OwnershipChecks.Count > 0 ? OwnershipChecks.Dequeue() : Ownership;
    /// <summary>Simulates restoring the original image, honoring the configured failure.</summary>
    /// <param name="snapshot">Snapshot to restore.</param>
    /// <param name="token">Cancellation token.</param>
    public Task RestoreAsync(LockScreenRestoreSnapshot snapshot, CancellationToken token)
    { RestoreCalls++; if (FailRestore) throw new IOException("synthetic-private-path"); Restored = true; Applied = false; return Task.CompletedTask; }
    /// <summary>Returns whether the restore completed.</summary>
    /// <param name="snapshot">Snapshot to check (unused by the fake).</param>
    /// <returns>True when the restore completed.</returns>
    public bool IsRestored(LockScreenRestoreSnapshot snapshot) => Restored;
    /// <summary>No-op cleanup for the fake backend.</summary>
    /// <param name="snapshot">Snapshot to clean up (unused by the fake).</param>
    public void Cleanup(LockScreenRestoreSnapshot snapshot) { }
}
