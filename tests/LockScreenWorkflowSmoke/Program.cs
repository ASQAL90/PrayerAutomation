using PrayerAutomation.UI;
using System.IO;
using System.Security.Cryptography;

Assert(LockScreenModeLogic.Resolve(0, 1) == LockScreenBackgroundMode.Spotlight,
    "Slideshow=0 and Spotlight=1 must resolve to Spotlight.");
Assert(LockScreenModeLogic.Resolve(1, 0) == LockScreenBackgroundMode.Slideshow,
    "Slideshow=1 and Spotlight=0 must resolve to slideshow.");
Assert(LockScreenModeLogic.Resolve(0, 0) == LockScreenBackgroundMode.Picture,
    "Both flags disabled must resolve to Picture.");
Assert(LockScreenModeLogic.Resolve(1, 1) == LockScreenBackgroundMode.Unknown &&
       LockScreenModeLogic.Resolve(2, 0) == LockScreenBackgroundMode.Unknown,
    "Conflicting or malformed mode flags must fail closed.");

var modes = new[]
{
    LockScreenBackgroundMode.Spotlight,
    LockScreenBackgroundMode.Slideshow,
    LockScreenBackgroundMode.Picture,
};

DateTimeOffset start = new(2026, 10, 22, 12, 0, 0, TimeSpan.FromHours(7));
await LockScreenDurationSmoke.VerifyConfiguredHoldAsync(start);
foreach (LockScreenBackgroundMode mode in modes)
{
    var backend = new FakeLockScreenBackend(mode);
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);

    LockScreenTransactionResult applied = await workflow.BeginOrExtendAsync("prayer.png", start, CancellationToken.None);
    Assert(applied == LockScreenTransactionResult.Applied, $"{mode}: temporary image was not applied.");
    LockScreenRestoreSnapshot active = store.Load() ?? throw new InvalidOperationException($"{mode}: restore journal missing.");
    Assert(active.RestoreAtUtc == start.AddMinutes(60), $"{mode}: restore deadline must be 60 minutes after the prayer trigger.");
    Assert(backend.CurrentMode == LockScreenBackgroundMode.Picture && backend.CurrentImage == "prayer-hash",
        $"{mode}: temporary image must be the only active lock-screen picture.");

    LockScreenTransactionResult restored = await workflow.RestoreAsync(CancellationToken.None);
    Assert(restored == LockScreenTransactionResult.Restored, $"{mode}: restore did not complete.");
    Assert(backend.CurrentMode == mode, $"{mode}: prior background mode was not restored.");
    if (mode == LockScreenBackgroundMode.Picture)
        Assert(backend.CurrentImage == "previous-picture-hash", "Picture mode must restore its previous image.");
    Assert(store.Load() is null, $"{mode}: successful restoration must clear the journal.");
}

{
    var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Spotlight);
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);
    await workflow.BeginOrExtendAsync("prayer.png", start, CancellationToken.None);
    int applyCalls = backend.ApplyCalls;

    LockScreenTransactionResult extended = await workflow.BeginOrExtendAsync("prayer.png", start.AddMinutes(20), CancellationToken.None);
    LockScreenRestoreSnapshot active = store.Load() ?? throw new InvalidOperationException("Extended transaction journal missing.");
    Assert(extended == LockScreenTransactionResult.Extended, "A second prayer while active must extend, not replace, the original transaction.");
    Assert(active.RestoreAtUtc == start.AddMinutes(80), "Extension must retain the original state and extend expiry from the latest prayer trigger.");
    Assert(backend.ApplyCalls == applyCalls, "An active prayer image must not be reapplied or re-snapshotted.");
}

{
    var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Unknown);
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);
    bool failedClosed = false;
    try { await workflow.BeginOrExtendAsync("prayer.png", start, CancellationToken.None); }
    catch (InvalidOperationException) { failedClosed = true; }
    Assert(failedClosed && backend.ApplyCalls == 0 && store.Load() is null,
        "Unknown prior mode must fail closed without changing the image or leaving a journal.");
}

{
    var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Picture) { PreviousPictureAvailable = false };
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);
    bool failedClosed = false;
    try { await workflow.BeginOrExtendAsync("prayer.png", start, CancellationToken.None); }
    catch (InvalidOperationException) { failedClosed = true; }
    Assert(failedClosed && backend.ApplyCalls == 0 && store.Load() is null,
        "Picture mode without a recoverable previous image must fail closed.");
}

{
    var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Slideshow);
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);
    await workflow.BeginOrExtendAsync("prayer.png", start, CancellationToken.None);
    backend.UserChangesLockScreen("user-selected-picture");

    LockScreenTransactionResult result = await workflow.RestoreAsync(CancellationToken.None);
    Assert(result == LockScreenTransactionResult.ExternalChangePreserved,
        "A user-selected lock-screen change must not be overwritten during automatic restoration.");
    Assert(backend.CurrentMode == LockScreenBackgroundMode.Picture && backend.CurrentImage == "user-selected-picture",
        "External lock-screen choice must remain untouched.");
    Assert(store.Load() is null, "An externally changed lock screen must release the old transaction journal.");
}

{
    var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Spotlight) { FailAfterApplyingTemporaryImage = true };
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);
    bool failed = false;
    try { await workflow.BeginOrExtendAsync("prayer.png", start, CancellationToken.None); }
    catch (IOException) { failed = true; }
    Assert(failed && backend.CurrentMode == LockScreenBackgroundMode.Spotlight && store.Load() is null,
        "A partial image-set failure must roll back to the previous mode.");
}

{
    var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Spotlight);
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);
    await workflow.BeginOrExtendAsync("prayer.png", start, CancellationToken.None);

    LockScreenTransactionResult beforeDeadline = await workflow.RecoverAsync(true, start.AddMinutes(59), CancellationToken.None);
    Assert(beforeDeadline == LockScreenTransactionResult.Active, "An enabled transaction must remain active before its deadline.");
    LockScreenTransactionResult expired = await workflow.RecoverAsync(true, start.AddMinutes(60), CancellationToken.None);
    Assert(expired == LockScreenTransactionResult.Restored && backend.CurrentMode == LockScreenBackgroundMode.Spotlight,
        "An expired transaction must restore the original mode.");
}

{
    bool enabled = true;
    var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Spotlight);
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);
    var schedule = new[]
    {
        new LockScreenPrayerTrigger("Sunrise", TimeSpan.FromHours(6)),
        new LockScreenPrayerTrigger("Dhuhr", TimeSpan.FromHours(12)),
    };
    using var scheduler = new LockScreenImageScheduler(
        () => enabled,
        _ => schedule,
        () => TimeZoneInfo.Utc,
        "prayer.png",
        workflow);

    var due = await scheduler.ProcessTickAsync(new DateTimeOffset(2026, 10, 22, 12, 0, 3, TimeSpan.Zero));
    Assert(due.Count == 1 && due[0] == LockScreenTransactionResult.Applied && backend.ApplyCalls == 1,
        "The scheduler must apply the image at an essential prayer, not sunrise.");
    var duplicate = await scheduler.ProcessTickAsync(new DateTimeOffset(2026, 10, 22, 12, 0, 4, TimeSpan.Zero));
    Assert(duplicate.Count == 0 && backend.ApplyCalls == 1,
        "A prayer trigger must be processed only once per local prayer day.");

    enabled = false;
    await scheduler.RefreshAsync(new DateTimeOffset(2026, 10, 22, 12, 0, 5, TimeSpan.Zero));
    Assert(backend.CurrentMode == LockScreenBackgroundMode.Spotlight && store.Load() is null,
        "Disabling the feature must stop scheduling and restore any active transaction.");
}

{
    var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Spotlight);
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);
    var sunriseOnly = new[] { new LockScreenPrayerTrigger("Sunrise", TimeSpan.FromHours(6)) };
    using var scheduler = new LockScreenImageScheduler(
        () => true,
        _ => sunriseOnly,
        () => TimeZoneInfo.Utc,
        "prayer.png",
        workflow);
    await scheduler.ProcessTickAsync(new DateTimeOffset(2026, 10, 22, 6, 0, 0, TimeSpan.Zero));
    Assert(backend.ApplyCalls == 0, "Sunrise is not a prayer trigger for this feature.");
}

{
    string root = Path.Combine(Environment.GetEnvironmentVariable("SMOKE_SCRATCH") ?? Path.GetTempPath(),
        "lockscreen-workflow-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    try
    {
        string temporaryPath = Path.Combine(root, LockScreenStatePaths.TemporaryImagePrefix + "test.png");
        string previousPath = Path.Combine(root, LockScreenStatePaths.RestoreImagePrefix + "test.jpg");
        File.WriteAllText(temporaryPath, "temporary-image-bytes");
        File.WriteAllText(previousPath, "previous-image-bytes");
        static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

        var snapshot = new LockScreenRestoreSnapshot(
            1, LockScreenBackgroundMode.Picture, 0, 0, previousPath, Hash(previousPath),
            temporaryPath, Hash(temporaryPath), start.AddMinutes(60), true);
        var fileStore = new FileLockScreenRestoreStore(root);
        fileStore.Save(snapshot);
        LockScreenRestoreSnapshot loaded = fileStore.Load() ?? throw new InvalidOperationException("Persistent restore journal was not read back.");
        Assert(loaded == snapshot, "Persistent restore journal must round-trip all restore data.");
        fileStore.Delete();
        Assert(fileStore.Load() is null, "Persistent restore journal must be removable after verified restoration.");

        bool rejectedUnmanagedPath = false;
        try { fileStore.Save(snapshot with { TemporaryImagePath = Path.Combine(root, "not-managed.png") }); }
        catch (InvalidDataException) { rejectedUnmanagedPath = true; }
        Assert(rejectedUnmanagedPath && fileStore.Load() is null,
            "The state store must reject paths outside its application-owned file prefixes.");

        string journalPath = Path.Combine(root, LockScreenStatePaths.JournalFileName);
        File.WriteAllText(journalPath, "{not valid JSON");
        bool rejectedCorruptState = false;
        try { fileStore.Load(); }
        catch (InvalidDataException) { rejectedCorruptState = true; }
        Assert(rejectedCorruptState && !File.Exists(journalPath) &&
               Directory.GetFiles(root, LockScreenStatePaths.JournalFileName + ".invalid-*").Length == 1,
            "A corrupt journal must be quarantined without being interpreted as permission to change the lock screen.");
        for (int attempt = 0; attempt < 2; attempt++)
        {
            bool loadBlocked = false;
            bool saveBlocked = false;
            var restartedStore = new FileLockScreenRestoreStore(root);
            try { restartedStore.Load(); }
            catch (InvalidDataException) { loadBlocked = true; }
            try { restartedStore.Save(snapshot); }
            catch (InvalidDataException) { saveBlocked = true; }
            Assert(loadBlocked && saveBlocked && !File.Exists(journalPath),
                "Quarantined state must remain a durable blocker on repeated load/save and across restart.");
        }
    }
    finally
    {
        try { Directory.Delete(root, recursive: true); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
}

{
    var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Picture) { FailOnceAfterRestoringPicture = true };
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);
    await workflow.BeginOrExtendAsync("prayer.png", start, CancellationToken.None);
    bool restoreFailed = false;
    try { await workflow.RestoreAsync(CancellationToken.None); }
    catch (IOException) { restoreFailed = true; }
    Assert(restoreFailed && store.Load() is not null, "Partial restore must preserve the recovery journal.");
    var restarted = new LockScreenImageTransaction(backend, store);
    LockScreenTransactionResult recovered = await restarted.RecoverAsync(false, start.AddMinutes(61), CancellationToken.None);
    Assert(recovered == LockScreenTransactionResult.Restored && backend.CurrentMode == LockScreenBackgroundMode.Picture && store.Load() is null,
        "Restart must resume an interrupted application-owned restore, not discard it as a user-selected image.");
}

{
    var backend = new FakeLockScreenBackend(LockScreenBackgroundMode.Picture);
    var store = new MemoryLockScreenRestoreStore();
    var workflow = new LockScreenImageTransaction(backend, store);
    await workflow.BeginOrExtendAsync("prayer.png", start, CancellationToken.None);
    backend.UserChangesLockScreen("user-selected-picture");
    LockScreenTransactionResult result = await workflow.RecoverAsync(true, start.AddMinutes(1), CancellationToken.None);
    Assert(result == LockScreenTransactionResult.ExternalChangePreserved && store.Load() is null && backend.RestoreCalls == 0,
        "Active recovery must verify ownership before reporting Active and preserve a changed user image immediately.");
}

await LockScreenSchedulerRecoverySmoke.VerifyStartupRetryAsync(start);
await LockScreenSchedulerRecoverySmoke.VerifySpotlightStatusAsync(start);
await LockScreenSchedulerRecoverySmoke.VerifyDisableRetryAsync(start);
await LockScreenSchedulerRecoverySmoke.VerifyStopInFlightAsync(start);

Console.WriteLine("PASS: Spotlight, slideshow and picture modes restore; 60-minute expiry, extension, fail-closed, user-override, prayer scheduling and persistent journal cases verified.");

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

internal sealed class MemoryLockScreenRestoreStore : ILockScreenRestoreStore
{
    private LockScreenRestoreSnapshot? _snapshot;
    public LockScreenRestoreSnapshot? Load() => _snapshot;
    public void Save(LockScreenRestoreSnapshot snapshot) => _snapshot = snapshot;
    public void Delete() => _snapshot = null;
}

internal sealed class FakeLockScreenBackend(LockScreenBackgroundMode initialMode) : ILockScreenImageBackend
{
    private LockScreenRestoreSnapshot? _snapshot;
    private readonly string _initialImage = initialMode == LockScreenBackgroundMode.Picture ? "previous-picture-hash" : "dynamic-background";

    public LockScreenBackgroundMode CurrentMode { get; private set; } = initialMode;
    public string CurrentImage { get; private set; } = initialMode == LockScreenBackgroundMode.Picture ? "previous-picture-hash" : "dynamic-background";
    public bool PreviousPictureAvailable { get; init; } = true;
    public bool FailAfterApplyingTemporaryImage { get; init; }
    public bool FailOnceAfterRestoringPicture { get; init; }
    private bool _restoreFailedOnce;
    public int ApplyCalls { get; private set; }
    public int RestoreCalls { get; private set; }
    public int RestoreFailuresRemaining { get; set; }
    public Task? WaitBeforeApplying { get; set; }
    public TaskCompletionSource? ApplyStarted { get; set; }

    public LockScreenRestoreSnapshot CaptureSnapshot(string imagePath, DateTimeOffset restoreAtUtc)
    {
        if (CurrentMode == LockScreenBackgroundMode.Unknown)
            throw new InvalidOperationException("Unknown mode.");
        if (CurrentMode == LockScreenBackgroundMode.Picture && !PreviousPictureAvailable)
            throw new InvalidOperationException("Previous picture unavailable.");
        _snapshot = new LockScreenRestoreSnapshot(
            Version: 1,
            PreviousMode: CurrentMode,
            PreviousSlideshowEnabled: CurrentMode == LockScreenBackgroundMode.Slideshow ? 1 : 0,
            PreviousSpotlightEnabled: CurrentMode == LockScreenBackgroundMode.Spotlight ? 1 : 0,
            PreviousImagePath: CurrentMode == LockScreenBackgroundMode.Picture ? "previous.jpg" : null,
            PreviousImageSha256: CurrentMode == LockScreenBackgroundMode.Picture ? _initialImage : null,
            TemporaryImagePath: "temporary.png",
            TemporaryImageSha256: "prayer-hash",
            RestoreAtUtc: restoreAtUtc,
            IsApplied: false);
        return _snapshot;
    }

    public async Task ApplyTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken)
    {
        ApplyCalls++;
        ApplyStarted?.TrySetResult();
        if (WaitBeforeApplying is not null) await WaitBeforeApplying;
        CurrentImage = snapshot.TemporaryImageSha256;
        CurrentMode = LockScreenBackgroundMode.Picture;
        if (FailAfterApplyingTemporaryImage) throw new IOException("Synthetic partial API failure.");
    }

    public LockScreenImageOwnership CheckTemporaryImage(LockScreenRestoreSnapshot snapshot) =>
        CurrentImage == snapshot.TemporaryImageSha256
            ? LockScreenImageOwnership.TemporaryImage
            : snapshot.RestoreRequested && CurrentImage == snapshot.PreviousImageSha256
                ? LockScreenImageOwnership.RestorationImage
            : CurrentImage == "unavailable"
                ? LockScreenImageOwnership.Unknown
                : LockScreenImageOwnership.DifferentImage;

    public Task MaintainTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (CurrentImage != snapshot.TemporaryImageSha256)
            throw new InvalidOperationException("Synthetic external image must not be overwritten during maintenance.");
        CurrentMode = LockScreenBackgroundMode.Picture;
        return Task.CompletedTask;
    }

    public Task RestoreAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken)
    {
        RestoreCalls++;
        if (RestoreFailuresRemaining > 0)
        {
            RestoreFailuresRemaining--;
            throw new IOException("Synthetic transient restore failure.");
        }
        if (snapshot.PreviousMode == LockScreenBackgroundMode.Picture)
        {
            CurrentImage = snapshot.PreviousImageSha256!;
            if (FailOnceAfterRestoringPicture && !_restoreFailedOnce)
            {
                _restoreFailedOnce = true;
                CurrentMode = LockScreenBackgroundMode.Slideshow;
                throw new IOException("Synthetic failure after restoring picture but before mode flags.");
            }
        }
        else
            CurrentImage = "dynamic-background";
        CurrentMode = snapshot.PreviousMode;
        return Task.CompletedTask;
    }

    public bool IsRestored(LockScreenRestoreSnapshot snapshot) =>
        CurrentMode == snapshot.PreviousMode &&
        (snapshot.PreviousMode != LockScreenBackgroundMode.Picture || CurrentImage == snapshot.PreviousImageSha256);

    public void Cleanup(LockScreenRestoreSnapshot snapshot) { }

    public void UserChangesLockScreen(string image)
    {
        CurrentImage = image;
        CurrentMode = LockScreenBackgroundMode.Picture;
    }
}
