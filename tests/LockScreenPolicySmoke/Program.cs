// Smoke test: an injected organisation policy must refuse a new lock-screen image
// before any snapshot or write, while an already active transaction stays restorable.
using PrayerAutomation.UI;

DateTimeOffset start = DateTimeOffset.UtcNow;

// Value data matters: a default/0 flag is not an enforced policy.
Assert(LockScreenPolicyValueRules.IsEnforced("NoChangingLockScreen", 1),
    "NoChangingLockScreen=1 must be recognised as an enforced policy.");
Assert(!LockScreenPolicyValueRules.IsEnforced("NoChangingLockScreen", 0) &&
       !LockScreenPolicyValueRules.IsEnforced("RotatingLockScreenEnabled", 0) &&
       !LockScreenPolicyValueRules.IsEnforced("SlideshowEnabled", 1),
    "A zero value or an ordinary feature flag must not be treated as a managed device.");
Assert(LockScreenPolicyValueRules.IsEnforced("LockScreenImageUrl", "file:///C:/wallpaper.jpg") &&
       !LockScreenPolicyValueRules.IsEnforced("LockScreenImageUrl", string.Empty),
    "Only a pinned image with a non-empty path is an enforced policy.");

// A fresh name per transaction forces Windows to allocate a clean derived-image
// cache slot; a reused path can render black or an old picture after a prayer.
string firstTemporary = LockScreenStatePaths.NewTemporaryImageName(".png");
string secondTemporary = LockScreenStatePaths.NewTemporaryImageName(".png");
Assert(firstTemporary != secondTemporary &&
       firstTemporary.StartsWith(LockScreenStatePaths.TemporaryImagePrefix, StringComparison.Ordinal),
    "Each temporary image must get a distinct managed name.");
string restoreName = LockScreenStatePaths.NewRestoreImageName(".jpg");
Assert(LockScreenStatePaths.IsManagedFile(
        Path.Combine(LockScreenStatePaths.DefaultDirectory, firstTemporary),
        LockScreenStatePaths.TemporaryImagePrefix, LockScreenStatePaths.DefaultDirectory) &&
       LockScreenStatePaths.IsManagedFile(
        Path.Combine(LockScreenStatePaths.DefaultDirectory, restoreName),
        LockScreenStatePaths.RestoreImagePrefix, LockScreenStatePaths.DefaultDirectory),
    "Fresh image names must remain covered by the managed-file ownership checks.");

{
    var backend = new ControlledBackend();
    var store = new ControlledStore();
    bool managed = true;
    var transaction = new LockScreenImageTransaction(backend, store, () => managed);

    LockScreenTransactionResult result = await transaction.BeginOrExtendAsync(
        "prayer.png", start, CancellationToken.None, TimeSpan.FromMinutes(1));

    Assert(result == LockScreenTransactionResult.ManagedByPolicy,
        "A managed device must return ManagedByPolicy instead of failing silently.");
    Assert(backend.SnapshotCalls == 0 && backend.ApplyCalls == 0 && store.Load() is null,
        "A managed device must be refused before any snapshot, apply, or journal write.");
    Assert(!transaction.HasActiveImage, "A refused transaction must not look active to session logging.");

    managed = false;
    result = await transaction.BeginOrExtendAsync(
        "prayer.png", start, CancellationToken.None, TimeSpan.FromMinutes(1));
    Assert(result == LockScreenTransactionResult.Applied && backend.ApplyCalls == 1 && store.Load() is not null,
        "Clearing the policy must allow a normal transaction on the next attempt.");
    Assert(transaction.HasActiveImage, "An applied transaction must be visible to session logging.");
}

{
    var backend = new ControlledBackend();
    var store = new ControlledStore();
    var transaction = new LockScreenImageTransaction(backend, store);
    await transaction.BeginOrExtendAsync("prayer.png", start, CancellationToken.None, TimeSpan.FromMinutes(60));

    // The policy appeared while the image was already active: keep the journal so
    // the normal restore path can still return the original lock screen.
    var managedTransaction = new LockScreenImageTransaction(backend, store, () => true);
    LockScreenTransactionResult result = await managedTransaction.BeginOrExtendAsync(
        "prayer.png", start.AddMinutes(20), CancellationToken.None, TimeSpan.FromMinutes(1));

    Assert(result == LockScreenTransactionResult.Extended && backend.ApplyCalls == 1 && store.Load() is not null,
        "A newly applied policy must not strand an active image without its restore journal.");
    LockScreenTransactionResult restored = await managedTransaction.RestoreAsync(CancellationToken.None);
    Assert(restored == LockScreenTransactionResult.Restored && store.Load() is null && backend.CurrentMode == LockScreenBackgroundMode.Spotlight,
        "The active transaction must still restore after the policy appears.");
}

{
    var transaction = new LockScreenImageTransaction(new ControlledBackend(), new QuarantinedStore());
    Assert(!transaction.HasActiveImage,
        "A quarantined or unreadable journal must fail closed for session logging instead of throwing.");
}

Console.WriteLine("PASS: enforced policy values are distinguished from defaults, refusal happens before any write, an existing transaction remains restorable, and each transaction uses a distinct managed image name.");

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

/// <summary>In-memory journal used only by this smoke.</summary>
internal sealed class ControlledStore : ILockScreenRestoreStore
{
    private LockScreenRestoreSnapshot? _snapshot;
    public LockScreenRestoreSnapshot? Load() => _snapshot;
    public void Save(LockScreenRestoreSnapshot snapshot) => _snapshot = snapshot;
    public void Delete() => _snapshot = null;
    public void ClearAll() => Delete();
}

/// <summary>Store that simulates a quarantined journal awaiting explicit review.</summary>
internal sealed class QuarantinedStore : ILockScreenRestoreStore
{
    public LockScreenRestoreSnapshot? Load() => throw new InvalidDataException("journal quarantined");
    public void Save(LockScreenRestoreSnapshot snapshot) => throw new InvalidOperationException("blocked");
    public void Delete() => throw new InvalidOperationException("blocked");
    public void ClearAll() => throw new InvalidOperationException("blocked");
}

/// <summary>Backend with call counters so a policy refusal can be proven write-free.</summary>
internal sealed class ControlledBackend : ILockScreenImageBackend
{
    public int SnapshotCalls { get; private set; }
    public int ApplyCalls { get; private set; }
    public LockScreenBackgroundMode CurrentMode { get; private set; } = LockScreenBackgroundMode.Spotlight;
    private string _image = "spotlight";

    public LockScreenRestoreSnapshot CaptureSnapshot(string imagePath, DateTimeOffset restoreAtUtc)
    {
        SnapshotCalls++;
        return new LockScreenRestoreSnapshot(1, CurrentMode, 0, 1, null, null,
            imagePath, "temporary-hash", restoreAtUtc, false);
    }

    public Task ApplyTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken)
    {
        ApplyCalls++;
        CurrentMode = LockScreenBackgroundMode.Picture;
        _image = snapshot.TemporaryImageSha256;
        return Task.CompletedTask;
    }

    public Task MaintainTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public LockScreenImageOwnership CheckTemporaryImage(LockScreenRestoreSnapshot snapshot) =>
        _image == snapshot.TemporaryImageSha256
            ? LockScreenImageOwnership.TemporaryImage
            : LockScreenImageOwnership.DifferentImage;

    public Task RestoreAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken)
    {
        CurrentMode = snapshot.PreviousMode;
        _image = snapshot.PreviousImageSha256 ?? "spotlight";
        return Task.CompletedTask;
    }

    public bool IsRestored(LockScreenRestoreSnapshot snapshot) =>
        CurrentMode == snapshot.PreviousMode;

    public void Cleanup(LockScreenRestoreSnapshot snapshot) { }
}
