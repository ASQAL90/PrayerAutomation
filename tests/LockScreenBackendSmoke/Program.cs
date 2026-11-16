using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using PrayerAutomation.UI;

string root = Path.Combine(Environment.GetEnvironmentVariable("SMOKE_SCRATCH") ?? Path.GetTempPath(),
    "lockscreen-backend-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    string temporary = Path.Combine(root, LockScreenStatePaths.TemporaryImagePrefix + "transaction.png");
    string previous = Path.Combine(root, LockScreenStatePaths.RestoreImagePrefix + "transaction.png");
    string duplicate = Path.Combine(root, "user-chosen-identical.png");
    File.WriteAllText(temporary, "synthetic-prayer-image");
    File.WriteAllText(previous, "synthetic-original-image");
    File.Copy(temporary, duplicate);
    string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    var snapshot = new LockScreenRestoreSnapshot(1, LockScreenBackgroundMode.Picture, 0, 0,
        previous, Hash(previous), temporary, Hash(temporary), DateTimeOffset.UtcNow.AddHours(1), true);
    MethodInfo? classify = typeof(WindowsLockScreenImageBackend).GetMethod("ClassifyImageOwnership",
        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
    Assert(classify is not null,
        "The production backend must classify image ownership from source identity and hash, not hash alone.");
    LockScreenImageOwnership Check(LockScreenRestoreSnapshot state, string? path, LockScreenBackgroundMode mode) =>
        (LockScreenImageOwnership)classify!.Invoke(null, [state, path, mode])!;
    Assert(Check(snapshot, duplicate, LockScreenBackgroundMode.Picture) == LockScreenImageOwnership.DifferentImage,
        "A user-chosen distinct file with identical bytes must not be owned by the transaction.");
    Assert(Check(snapshot, temporary, LockScreenBackgroundMode.Picture) == LockScreenImageOwnership.TemporaryImage,
        "The exact temporary source identity and matching hash must be owned.");
    Assert(Check(snapshot, null, LockScreenBackgroundMode.Spotlight) == LockScreenImageOwnership.Unknown,
        "An unreadable source in a dynamic mode must remain Unknown, not a confirmed external change.");
    Assert(Check(snapshot with { RestoreRequested = true }, previous, LockScreenBackgroundMode.Slideshow)
        == LockScreenImageOwnership.RestorationImage,
        "Interrupted restoration must recognize the exact application-owned restoration source.");
    Assert(Check(snapshot, temporary, LockScreenBackgroundMode.Spotlight).ToString() == "TemporaryImageModeDrift",
        "An unchanged application-owned source with a mode-only override must retain its bounded image lease.");
    Assert(Check(snapshot with { IsApplied = false }, temporary, LockScreenBackgroundMode.Spotlight)
        == LockScreenImageOwnership.TemporaryImage,
        "A prepared transaction with a partial apply must remain recoverable.");
    File.WriteAllText(temporary, "tampered");
    Assert(Check(snapshot, temporary, LockScreenBackgroundMode.Picture) == LockScreenImageOwnership.Unknown,
        "A tampered managed image must stop restoration, not authorize an overwrite.");
    Assert(typeof(WindowsLockScreenImageBackend).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        .Any(ctor => ctor.GetParameters().Any(parameter => parameter.ParameterType.Name == "ILockScreenNativePlatform")),
        "The real backend must accept an isolated native platform so its Windows-operation boundaries can be tested safely.");
    await LockScreenModeValueWriteSmoke.VerifyOwnedSlideshowExpiryFromSpotlightAsync(root);
    LockScreenModeValueWriteSmoke.VerifyValidTransitionsAndGuards();
    await LockScreenModeValueWriteSmoke.VerifyRestoreSecondValueBoundaryPreservesUserImageAsync(root);
    await LockScreenNativeBoundarySmoke.VerifyConfiguredLeaseSurvivesModeDriftAsync(root);
    await LockScreenNativeBoundarySmoke.VerifySchedulerMaintainsModeDriftAsync(root);
    await LockScreenNativeBoundarySmoke.VerifyActiveRecoveryBackoffAsync(root);
    await LockScreenNativeBoundarySmoke.VerifyMaintenanceBoundaryPreservesUserImageAsync(root);
    await LockScreenNativeBoundarySmoke.VerifySpotlightRecoveryWithoutOriginalImageAsync(root);
    await LockScreenNativeBoundarySmoke.VerifyPictureCaptureRestoreAsync(root);
    await LockScreenNativeBoundarySmoke.VerifyPreparationRecheckAsync(root);
    await LockScreenNativeBoundarySmoke.VerifyRestoreModeBoundaryPreservesUserImageAsync(root);
    await LockScreenNativeBoundarySmoke.VerifyApplyModeBoundaryPreservesUserImageAsync(root);
    await LockScreenModeValueWriteSmoke.VerifyModeOnlyActivationBoundariesAsync(root);
    Console.WriteLine("PASS: production ownership classifier checks path+hash, unreadable sources, partial restore and mode overrides without native calls.");
}
finally
{
    Directory.Delete(root, recursive: true);
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
