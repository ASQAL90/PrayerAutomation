using System.IO;
using System.Reflection;
using PrayerAutomation.UI;

internal static class Program
{
    internal static int Assertions;
    internal static readonly DateTimeOffset Epoch = new(2030, 1, 2, 12, 0, 0, TimeSpan.Zero);
    internal static void Check(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException("FAIL: " + name);
        Assertions++;
        Console.WriteLine("PASS: " + name);
    }
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
internal sealed class RecordingLease : ILockScreenExecutionRequest
{
    public bool Requested, Disposed, ThrowRequest; public int Acquires, Updates, Clears; public DateTimeOffset? Deadline;
    public void RequestUntil(DateTimeOffset deadline)
    {
        if (ThrowRequest) throw new IOException("synthetic-private-path");
        if (Disposed) throw new ObjectDisposedException("fake lease");
        if (!Requested) Acquires++;
        Requested = true; Deadline = deadline; Updates++;
    }
    public void Clear() { if (Requested) Clears++; Requested = false; Deadline = null; }
    public void Dispose() { Clear(); Disposed = true; }
}
internal sealed class MemoryStore : ILockScreenRestoreStore
{
    public LockScreenRestoreSnapshot? Snapshot;
    public bool Unreadable;
    public LockScreenRestoreSnapshot? Load() => Unreadable ? throw new IOException("synthetic-private-path") : Snapshot;
    public void Save(LockScreenRestoreSnapshot snapshot) => Snapshot = snapshot;
    public void Delete() => Snapshot = null;
}
internal sealed class FakeBackend : ILockScreenImageBackend
{
    public bool Applied, Restored, FailApply, FailRestore; public int ApplyCalls, RestoreCalls, MaintainCalls;
    public LockScreenImageOwnership Ownership = LockScreenImageOwnership.TemporaryImage;
    public Queue<LockScreenImageOwnership> OwnershipChecks = new();
    public Action? BeforeApply; public TaskCompletionSource? ApplyBlock;
    public LockScreenRestoreSnapshot CaptureSnapshot(string imagePath, DateTimeOffset deadline) =>
        new(1, LockScreenBackgroundMode.Spotlight, 0, 1, null, null, "private-temporary-path", new string('A',64), deadline, false);
    public async Task ApplyTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken token)
    {
        ApplyCalls++; BeforeApply?.Invoke(); if (ApplyBlock is not null) await ApplyBlock.Task;
        if (FailApply) throw new IOException("synthetic-private-path");
        Applied = true; Restored = false;
    }
    public Task MaintainTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken token)
    { MaintainCalls++; Ownership = LockScreenImageOwnership.TemporaryImage; return Task.CompletedTask; }
    public LockScreenImageOwnership CheckTemporaryImage(LockScreenRestoreSnapshot snapshot) =>
        OwnershipChecks.Count > 0 ? OwnershipChecks.Dequeue() : Ownership;
    public Task RestoreAsync(LockScreenRestoreSnapshot snapshot, CancellationToken token)
    { RestoreCalls++; if (FailRestore) throw new IOException("synthetic-private-path"); Restored = true; Applied = false; return Task.CompletedTask; }
    public bool IsRestored(LockScreenRestoreSnapshot snapshot) => Restored;
    public void Cleanup(LockScreenRestoreSnapshot snapshot) { }
}
