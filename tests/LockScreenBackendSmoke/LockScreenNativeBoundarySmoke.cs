using System.IO;
using System.Security.Cryptography;
using PrayerAutomation.UI;

/// <summary>Verifies native apply/restore/maintenance boundaries preserve user-selected lock-screen sources and modes.</summary>
internal static class LockScreenNativeBoundarySmoke
{
    /// <summary>Verifies a user source change at the native apply-mode boundary preserves the user's mode and image without writes.</summary>
    /// <param name="root">Directory holding the synthetic fixture files.</param>
    internal static async Task VerifyApplyModeBoundaryPreservesUserImageAsync(string root)
    {
        string directory = Path.Combine(root, "apply-mode-boundary-spotlight");
        Directory.CreateDirectory(directory);
        string prayerImage = Path.Combine(directory, "prayer.png");
        string userImage = Path.Combine(directory, "user-new.png");
        File.WriteAllText(prayerImage, "synthetic-prayer-image");
        File.WriteAllText(userImage, "synthetic-user-new-picture");
        var native = new IsolatedLockScreenPlatform { ModeFlags = (0, 1) };
        native.BeforeModeWrite = () =>
        {
            native.CurrentImagePath = userImage;
            native.ModeFlags = (1, 0);
        };
        var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
        var store = new FileLockScreenRestoreStore(directory);
        var transaction = new LockScreenImageTransaction(backend, store);
        bool refused = false;
        try
        {
            await transaction.BeginOrExtendAsync(prayerImage,
                new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero), CancellationToken.None, TimeSpan.FromMinutes(7));
        }
        catch (InvalidOperationException) { refused = true; }
        native.BeforeModeWrite = null;
        Require(refused && native.ModeFlags == (1, 0) && native.CurrentImagePath == userImage &&
                native.ModeWriteCalls == 0 && native.ImageWriteCalls == 1 && store.Load() is null && File.Exists(userImage),
            $"A different user source at the native apply-mode boundary must preserve its selected mode without rollback writes: mode={native.ModeFlags}, new_source={native.CurrentImagePath == userImage}, refused={refused}, mode_writes={native.ModeWriteCalls}, image_writes={native.ImageWriteCalls}, journal_present={store.Load() is not null}.");
        Console.WriteLine("PASS: native apply-mode boundary preserves a new user source and Slideshow mode without rollback writes.");
    }

    /// <summary>Verifies a user source change at the native restore-mode boundary refuses the write and later recovery preserves it.</summary>
    /// <param name="root">Directory holding the synthetic fixture files.</param>
    internal static async Task VerifyRestoreModeBoundaryPreservesUserImageAsync(string root)
    {
        int cases = 0;
        foreach ((int slideshow, int spotlight) in new[] { (0, 1), (1, 0), (0, 0) })
        {
            string directory = Path.Combine(root, "restore-mode-boundary-" + slideshow + "-" + spotlight);
            Directory.CreateDirectory(directory);
            string originalImage = Path.Combine(directory, "original.png");
            string prayerImage = Path.Combine(directory, "prayer.png");
            string userImage = Path.Combine(directory, "user-new.png");
            File.WriteAllText(originalImage, "synthetic-original-picture");
            File.WriteAllText(prayerImage, "synthetic-prayer-image");
            File.WriteAllText(userImage, "synthetic-user-new-picture");
            var native = new IsolatedLockScreenPlatform { ModeFlags = (slideshow, spotlight), CurrentImagePath = originalImage };
            var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
            var store = new FileLockScreenRestoreStore(directory);
            var transaction = new LockScreenImageTransaction(backend, store);
            DateTimeOffset start = new(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);
            Require(await transaction.BeginOrExtendAsync(prayerImage, start, CancellationToken.None, TimeSpan.FromMinutes(7))
                    == LockScreenTransactionResult.Applied, "The isolated lease must apply before its restore boundary is tested.");
            int modeWrites = native.ModeWriteCalls;
            (int, int) userMode = (slideshow, spotlight) == (0, 0) ? (1, 0) : (0, 0);
            native.BeforeModeWrite = () =>
            {
                native.CurrentImagePath = userImage;
                native.ModeFlags = userMode;
            };
            bool refused = false;
            LockScreenTransactionResult? result = null;
            try { result = await transaction.RecoverAsync(true, start.AddMinutes(7), CancellationToken.None); }
            catch (InvalidOperationException) { refused = true; }
            native.BeforeModeWrite = null;
            Require(refused && native.ModeFlags == userMode && native.CurrentImagePath == userImage &&
                    native.ModeWriteCalls == modeWrites &&
                    store.Load() is { RestoreRequested: true } pending && pending.RestoreAtUtc == start.AddMinutes(7),
                $"A different user source at the native restore-mode boundary must refuse the write without a false successful restore: previous=({slideshow},{spotlight}), result={result}, mode={native.ModeFlags}, new_source={native.CurrentImagePath == userImage}, refused={refused}, mode_writes={native.ModeWriteCalls - modeWrites}, journal_present={store.Load() is not null}.");
            int imageWrites = native.ImageWriteCalls;
            Require(await transaction.RecoverAsync(true, start.AddMinutes(7).AddSeconds(1), CancellationToken.None)
                    == LockScreenTransactionResult.ExternalChangePreserved && store.Load() is null &&
                    native.ModeFlags == userMode && native.CurrentImagePath == userImage &&
                    native.ModeWriteCalls == modeWrites && native.ImageWriteCalls == imageWrites,
                "The next recovery must preserve the changed user image and mode, then clear only its obsolete journal without native writes.");
            cases++;
        }
        Console.WriteLine($"LOCKSCREEN_RESTORE_MODE_BOUNDARIES PASS: cases={cases}; previous_modes=Spotlight,Slideshow,Picture; user_source_and_mode_preserved=1; real_native_writes=0.");
    }

    /// <summary>Verifies Spotlight restore recovery succeeds without an original image URI, using only the saved mode.</summary>
    /// <param name="root">Directory holding the synthetic fixture files.</param>
    internal static async Task VerifySpotlightRecoveryWithoutOriginalImageAsync(string root)
    {
        string directory = Path.Combine(root, "spotlight-mode-only");
        Directory.CreateDirectory(directory);
        string prayerImage = Path.Combine(directory, "prayer.png");
        File.WriteAllText(prayerImage, "synthetic-prayer-image");
        var native = new IsolatedLockScreenPlatform
        {
            ModeFlags = (0, 1),
            HideImageInSpotlight = true
        };
        var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
        var store = new FileLockScreenRestoreStore(directory);
        var transaction = new LockScreenImageTransaction(backend, store);
        DateTimeOffset start = new(2026, 10, 22, 5, 0, 0, TimeSpan.Zero);

        Require(await transaction.BeginOrExtendAsync(prayerImage, start, CancellationToken.None)
            == LockScreenTransactionResult.Applied, "Spotlight must allow a temporary image without an original image URI.");
        LockScreenRestoreSnapshot captured = store.Load()!;
        Require(captured.PreviousMode == LockScreenBackgroundMode.Spotlight &&
                captured.PreviousImagePath is null && captured.PreviousImageSha256 is null,
            "A Spotlight journal must store the mode, not the original image URI, backup, or hash.");

        native.FailOnceAfterSpotlightModeWrite = true;
        bool interrupted = false;
        try { await transaction.RestoreAsync(CancellationToken.None); }
        catch (IOException) { interrupted = true; }
        Require(interrupted && store.Load() is { RestoreRequested: true },
            "An interrupted mode restoration must keep its persistent recovery journal.");
        Require(native.ModeFlags == (0, 1) && native.CurrentImagePath is null,
            "The synthetic interruption must leave Spotlight restored while its image URI is unavailable.");
        int writesBeforeRestart = native.ModeWriteCalls;
        var restarted = new LockScreenImageTransaction(backend, new FileLockScreenRestoreStore(directory));
        Require(await restarted.RecoverAsync(false, start.AddMinutes(61), CancellationToken.None)
                == LockScreenTransactionResult.Restored && store.Load() is null,
            "Confirmed Spotlight mode must complete restart recovery even when Windows exposes no image URI.");
        Require(native.ModeWriteCalls == writesBeforeRestart,
            "An already-restored Spotlight mode must not rewrite the user's mode or select a specific image.");
        Console.WriteLine("PASS: production Spotlight capture/restart recovery requires only the saved mode, not an original image URI.");
    }

    /// <summary>Verifies Picture mode capture stores a verified copy of the original image and restore returns it.</summary>
    /// <param name="root">Directory holding the synthetic fixture files.</param>
    internal static async Task VerifyPictureCaptureRestoreAsync(string root)
    {
        string directory = Path.Combine(root, "picture-backup");
        Directory.CreateDirectory(directory);
        string original = Path.Combine(directory, "user-original.png");
        string prayerImage = Path.Combine(directory, "prayer.png");
        File.WriteAllText(original, "synthetic-original-picture");
        File.WriteAllText(prayerImage, "synthetic-prayer-image");
        var native = new IsolatedLockScreenPlatform { ModeFlags = (0, 0), CurrentImagePath = original };
        var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
        var store = new FileLockScreenRestoreStore(directory);
        var transaction = new LockScreenImageTransaction(backend, store);
        DateTimeOffset start = new(2026, 10, 22, 5, 0, 0, TimeSpan.Zero);
        string originalHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(original)));
        Require(await transaction.BeginOrExtendAsync(prayerImage, start, CancellationToken.None)
            == LockScreenTransactionResult.Applied, "Picture mode must apply after a verified original-image backup.");
        LockScreenRestoreSnapshot captured = store.Load()!;
        Require(captured.PreviousImagePath is not null && captured.PreviousImagePath != original &&
                File.Exists(captured.PreviousImagePath) && captured.PreviousImageSha256 == originalHash,
            "Picture mode must persist a separately copied original image and its exact content hash.");
        Require(await transaction.RecoverAsync(false, start.AddMinutes(1), CancellationToken.None)
                == LockScreenTransactionResult.Restored && native.ModeFlags == (0, 0) &&
                native.CurrentImagePath == captured.PreviousImagePath &&
                Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(native.CurrentImagePath!))) == originalHash &&
                native.ImageWriteCalls == 2 && store.Load() is null,
            "Picture restoration must return its verified original image and mode, not only mode flags.");
        Require(File.Exists(native.CurrentImagePath!), "Cleanup must retain the restoration image while Windows still uses its path.");
        Console.WriteLine("PASS: production Picture capture/restore copies and verifies the original image; active restore files survive cleanup.");
    }

    /// <summary>Verifies the backend rechecks source ownership at the native setter and refuses a changed user picture.</summary>
    /// <param name="root">Directory holding the synthetic fixture files.</param>
    internal static async Task VerifyPreparationRecheckAsync(string root)
    {
        string directory = Path.Combine(root, "preparation-recheck");
        Directory.CreateDirectory(directory);
        string original = Path.Combine(directory, "user-original.png");
        string replacement = Path.Combine(directory, "user-new.png");
        string prayerImage = Path.Combine(directory, "prayer.png");
        File.WriteAllText(original, "synthetic-original-picture");
        File.WriteAllText(replacement, "synthetic-user-new-picture");
        File.WriteAllText(prayerImage, "synthetic-prayer-image");
        var native = new IsolatedLockScreenPlatform { ModeFlags = (0, 0), CurrentImagePath = original };
        native.BeforeImageWrite = () => native.CurrentImagePath = replacement;
        var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
        var store = new FileLockScreenRestoreStore(directory);
        var transaction = new LockScreenImageTransaction(backend, store);
        bool refused = false;
        try
        {
            await transaction.BeginOrExtendAsync(prayerImage,
                new DateTimeOffset(2026, 10, 22, 5, 0, 0, TimeSpan.Zero), CancellationToken.None);
        }
        catch (InvalidOperationException) { refused = true; }
        Require(refused && native.CurrentImagePath == replacement && native.ImageWriteCalls == 0 &&
                native.ModeWriteCalls == 0 && store.Load() is null,
            "A changed user picture while the native setter prepares its file must refuse the write and preserve that user picture.");
        Console.WriteLine("PASS: production backend rechecks ownership at the injected native setter boundary and preserves a changed user picture.");
    }

    /// <summary>Verifies a configured image lease survives mode-only Windows drift and restores at the deadline.</summary>
    /// <param name="root">Directory holding the synthetic fixture files.</param>
    internal static async Task VerifyConfiguredLeaseSurvivesModeDriftAsync(string root)
    {
        int checks = 0;
        DateTimeOffset start = new(2026, 10, 23, 8, 0, 0, TimeSpan.Zero);
        foreach ((int slideshow, int spotlight) in new[] { (0, 1), (1, 0) })
        {
            string directory = Path.Combine(root, "mode-drift-" + slideshow + "-" + spotlight);
            Directory.CreateDirectory(directory);
            string prayerImage = Path.Combine(directory, "prayer.png");
            string userImage = Path.Combine(directory, "user-new.png");
            File.WriteAllText(prayerImage, "synthetic-prayer-image");
            File.WriteAllText(userImage, "synthetic-user-new-picture");
            var native = new IsolatedLockScreenPlatform { ModeFlags = (slideshow, spotlight) };
            var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
            var store = new FileLockScreenRestoreStore(directory);
            var transaction = new LockScreenImageTransaction(backend, store);
            Require(await transaction.BeginOrExtendAsync(prayerImage, start, CancellationToken.None, TimeSpan.FromMinutes(7))
                    == LockScreenTransactionResult.Applied, "A configured image lease must initially apply.");
            LockScreenRestoreSnapshot captured = store.Load()!;
            checks++;
            // Model mode-only Windows drift while the exact managed source and its bytes remain unchanged.
            native.ModeFlags = (slideshow, spotlight);
            Require(await transaction.RecoverAsync(true, start.AddSeconds(3), CancellationToken.None)
                    == LockScreenTransactionResult.Active && native.ModeFlags == (0, 0) &&
                    native.CurrentImagePath == captured.TemporaryImagePath &&
                    store.Load()?.RestoreAtUtc == start.AddMinutes(7),
                "Mode-only Spotlight/slideshow drift after seconds must not delete the selected seven-minute lease.");
            checks++;
            int imageWrites = native.ImageWriteCalls;
            int modeWrites = native.ModeWriteCalls;
            await transaction.RecoverAsync(true, start.AddSeconds(30), CancellationToken.None);
            await transaction.RecoverAsync(true, start.AddSeconds(60), CancellationToken.None);
            Require(native.ImageWriteCalls == imageWrites && native.ModeWriteCalls == modeWrites,
                "Healthy early ticks must only verify the image, never repeatedly write personalization.");
            checks++;
            Require(await transaction.RecoverAsync(true, start.AddMinutes(7), CancellationToken.None)
                    == LockScreenTransactionResult.Restored && native.ModeFlags == (slideshow, spotlight) && store.Load() is null,
                "Lease expiry must restore the original dynamic mode, without choosing an old dynamic image.");
            checks++;
            // A dynamic mode can initially retain the managed URI; model its later content refresh before a new lease.
            native.CurrentImagePath = userImage;
            Require(await transaction.BeginOrExtendAsync(prayerImage, start.AddMinutes(10), CancellationToken.None, TimeSpan.FromMinutes(7))
                    == LockScreenTransactionResult.Applied, "A new isolated lease must initially apply.");
            native.ModeFlags = (slideshow, spotlight);
            native.CurrentImagePath = userImage;
            imageWrites = native.ImageWriteCalls;
            modeWrites = native.ModeWriteCalls;
            await transaction.RecoverAsync(true, start.AddMinutes(10).AddSeconds(3), CancellationToken.None);
            Require(native.CurrentImagePath == userImage && native.ModeFlags == (slideshow, spotlight) &&
                    native.ImageWriteCalls == imageWrites && native.ModeWriteCalls == modeWrites && store.Load() is null,
                "A different image is an external selection, not a mode-only lease drift; preserve it without writes.");
            checks++;
        }
        Console.WriteLine("LOCKSCREEN_MODE_DRIFT_HOLD PASS: checks=" + checks + "; modes=2; selected_minutes=7; early_drift_seconds=3; different_user_source_preserved=1; real_native_writes=0.");
    }

    /// <summary>Verifies the polling scheduler repairs confirmed mode drift promptly and restores at expiry.</summary>
    /// <param name="root">Directory holding the synthetic fixture files.</param>
    internal static async Task VerifySchedulerMaintainsModeDriftAsync(string root)
    {
        DateTimeOffset start = new(2026, 10, 23, 8, 0, 0, TimeSpan.Zero);
        int checks = 0;
        foreach ((int slideshow, int spotlight) in new[] { (0, 1), (1, 0) })
        {
            string directory = Path.Combine(root, "scheduler-drift-" + slideshow + "-" + spotlight);
            Directory.CreateDirectory(directory);
            string prayerImage = Path.Combine(directory, "prayer.png");
            File.WriteAllText(prayerImage, "synthetic-prayer-image");
            var native = new IsolatedLockScreenPlatform { ModeFlags = (slideshow, spotlight) };
            var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
            var store = new FileLockScreenRestoreStore(directory);
            var transaction = new LockScreenImageTransaction(backend, store);
            using var scheduler = new LockScreenImageScheduler(() => true,
                _ => [new("Asr", start.TimeOfDay)], () => TimeZoneInfo.Utc, prayerImage,
                transaction, durationMinutes: () => 7);
            Require((await scheduler.ProcessTickAsync(start)).Single() == LockScreenTransactionResult.Applied,
                "The isolated production scheduler must initially apply a seven-minute prayer image.");
            native.ModeFlags = (slideshow, spotlight);
            await scheduler.ProcessTickAsync(start.AddSeconds(3));
            Require(native.ModeFlags == (0, 0) && store.Load()?.RestoreAtUtc == start.AddMinutes(7),
                "The one-second scheduler must repair confirmed source-owned mode drift promptly, not wait thirty seconds.");
            checks++;
            int imageWrites = native.ImageWriteCalls;
            int modeWrites = native.ModeWriteCalls;
            await scheduler.ProcessTickAsync(start.AddSeconds(4));
            Require(native.ImageWriteCalls == imageWrites && native.ModeWriteCalls == modeWrites,
                "A healthy scheduler tick must be read-only.");
            checks++;
            native.ModeFlags = (slideshow, spotlight);
            await scheduler.ProcessTickAsync(start.AddSeconds(5));
            Require(native.ModeFlags == (0, 0) && store.Load()?.RestoreAtUtc == start.AddMinutes(7),
                "Repeated confirmed mode drift must retain the same absolute deadline.");
            checks++;
            await scheduler.ProcessTickAsync(start.AddMinutes(7));
            Require(native.ModeFlags == (slideshow, spotlight) && store.Load() is null,
                "Active polling must restore at expiry without an extra thirty-second healthy-state delay.");
            checks++;
        }
        Console.WriteLine("LOCKSCREEN_SCHEDULER_MODE_HOLD PASS: checks=" + checks + "; modes=2; polling_seconds=1; saved_minutes=7; native_calls=isolated-only.");
    }

    /// <summary>Verifies maintenance rechecks source ownership at the image and mode write boundaries and refuses changes.</summary>
    /// <param name="root">Directory holding the synthetic fixture files.</param>
    internal static async Task VerifyMaintenanceBoundaryPreservesUserImageAsync(string root)
    {
        DateTimeOffset start = new(2026, 10, 23, 8, 0, 0, TimeSpan.Zero);
        int checks = 0;
        foreach (string boundary in new[] { "image", "mode" })
        {
            string directory = Path.Combine(root, "maintenance-boundary-" + boundary);
            Directory.CreateDirectory(directory);
            string prayerImage = Path.Combine(directory, "prayer.png");
            string userImage = Path.Combine(directory, "user-new.png");
            File.WriteAllText(prayerImage, "synthetic-prayer-image");
            File.WriteAllText(userImage, "synthetic-user-new-image");
            var native = new IsolatedLockScreenPlatform { ModeFlags = (0, 1) };
            var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
            var store = new FileLockScreenRestoreStore(directory);
            var transaction = new LockScreenImageTransaction(backend, store);
            await transaction.BeginOrExtendAsync(prayerImage, start, CancellationToken.None, TimeSpan.FromMinutes(7));
            native.ModeFlags = (0, 1);
            Action changeImage = () => native.CurrentImagePath = userImage;
            if (boundary == "image") native.BeforeImageWrite = changeImage;
            else native.BeforeModeWrite = changeImage;
            int imageWrites = native.ImageWriteCalls;
            int modeWrites = native.ModeWriteCalls;
            bool refused = false;
            try { await transaction.RecoverAsync(true, start.AddSeconds(3), CancellationToken.None); }
            catch (InvalidOperationException) { refused = true; }
            Require(refused && native.CurrentImagePath == userImage && native.ModeFlags == (0, 1) &&
                    native.ModeWriteCalls == modeWrites && store.Load() is not null &&
                    (boundary != "image" || native.ImageWriteCalls == imageWrites),
                "Maintenance must recheck source ownership at the " + boundary + " write boundary and refuse a changed user image.");
            checks++;
            await transaction.RecoverAsync(true, start.AddSeconds(4), CancellationToken.None);
            Require(native.CurrentImagePath == userImage && native.ModeWriteCalls == modeWrites && store.Load() is null,
                "After a refused maintenance write, recovery must preserve the external selection and release the journal.");
            checks++;
        }
        Console.WriteLine("LOCKSCREEN_MAINTENANCE_BOUNDARIES PASS: checks=" + checks + "; boundaries=image,mode; external_source_preserved=1; real_native_writes=0.");
    }

    /// <summary>Verifies failed or unknown active recovery backs off for thirty seconds and resumes one-second polling after success.</summary>
    /// <param name="root">Directory holding the synthetic fixture files.</param>
    internal static async Task VerifyActiveRecoveryBackoffAsync(string root)
    {
        int checks = 0;
        DateTimeOffset start = new(2026, 10, 23, 8, 0, 0, TimeSpan.Zero);
        foreach (bool failMaintenance in new[] { false, true })
        {
            string directory = Path.Combine(root, failMaintenance ? "active-maintenance-failure" : "active-readback-unavailable");
            Directory.CreateDirectory(directory);
            string prayerImage = Path.Combine(directory, "prayer.png");
            File.WriteAllText(prayerImage, "synthetic-prayer-image");
            var native = new IsolatedLockScreenPlatform { ModeFlags = (0, 1) };
            var store = new FileLockScreenRestoreStore(directory);
            var transaction = new LockScreenImageTransaction(new WindowsLockScreenImageBackend(directory: directory, native: native), store);
            var errors = new List<Exception>();
            using var scheduler = new LockScreenImageScheduler(() => true,
                _ => new[] { new LockScreenPrayerTrigger("Asr", start.TimeOfDay) },
                () => TimeZoneInfo.Utc, prayerImage, transaction,
                log: (_, error) => errors.Add(error), durationMinutes: () => 7);
            Require((await scheduler.ProcessTickAsync(start)).SequenceEqual(new[] { LockScreenTransactionResult.Applied }),
                "The active backoff fixture must first apply its configured lease.");
            LockScreenRestoreSnapshot captured = store.Load()!;
            checks++;
            if (failMaintenance)
            {
                native.ModeFlags = (0, 1);
                native.BeforeImageWrite = () => throw new IOException("Synthetic maintenance preparation failure.");
            }
            else native.CurrentImagePath = null;
            try { await scheduler.ProcessTickAsync(start.AddSeconds(3)); }
            catch (IOException error) when (failMaintenance) { errors.Add(error); }
            Require(store.Load() is { IsApplied: true, RestoreRequested: false } pending &&
                    pending.RestoreAtUtc == start.AddMinutes(7) && errors.Count == (failMaintenance ? 1 : 0),
                "A maintenance failure or unknown readback must retain the bounded recovery journal, without requesting early restore.");
            checks++;
            int imageReads = native.ImageReadCalls;
            int imageWrites = native.ImageWriteCalls;
            int modeWrites = native.ModeWriteCalls;
            await scheduler.ProcessTickAsync(start.AddSeconds(4));
            await scheduler.ProcessTickAsync(start.AddSeconds(32));
            Require(native.ImageReadCalls == imageReads && native.ImageWriteCalls == imageWrites &&
                    native.ModeWriteCalls == modeWrites && errors.Count == (failMaintenance ? 1 : 0),
                "Failed or unknown active recovery must keep the thirty-second backoff instead of retrying every second.");
            checks++;
            native.BeforeImageWrite = null;
            native.CurrentImagePath = captured.TemporaryImagePath;
            await scheduler.ProcessTickAsync(start.AddSeconds(33));
            Require(native.ModeFlags == (0, 0) && store.Load()?.RestoreAtUtc == captured.RestoreAtUtc &&
                    native.ImageReadCalls > imageReads,
                "The due recovery must resume verified ownership without extending the original deadline.");
            checks++;
            modeWrites = native.ModeWriteCalls;
            native.ModeFlags = (0, 1);
            await scheduler.ProcessTickAsync(start.AddSeconds(34));
            Require(native.ModeFlags == (0, 0) && native.ModeWriteCalls == modeWrites + 1 &&
                    store.Load()?.RestoreAtUtc == captured.RestoreAtUtc,
                "After recovery succeeds, source-owned mode drift must again be checked on the one-second interval.");
            checks++;
        }
        Console.WriteLine($"LOCKSCREEN_ACTIVE_BACKOFF PASS: checks={checks}; cases=2; failed_retry_seconds=30; resumed_poll_seconds=1; real_native_writes=0.");
    }

    /// <summary>Requires the condition and throws otherwise.</summary>
    /// <param name="condition">Condition that must hold.</param>
    /// <param name="message">Failure message used when the condition is false.</param>
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

/// <summary>Isolated <see cref="ILockScreenNativePlatform"/> with injectable mode flags, failures, and write-boundary callbacks.</summary>
internal sealed class IsolatedLockScreenPlatform : ILockScreenNativePlatform
{
    /// <summary>Slideshow and spotlight mode flags currently reported by the fake.</summary>
    internal (int SlideshowEnabled, int SpotlightEnabled) ModeFlags { get; set; }
    /// <summary>Current image path reported by the fake, or null when unavailable.</summary>
    internal string? CurrentImagePath { get; set; }
    /// <summary>When true, Spotlight mode hides the image URI after mode writes.</summary>
    internal bool HideImageInSpotlight { get; set; }
    /// <summary>When true, the first Spotlight mode write throws once after restoring Spotlight.</summary>
    internal bool FailOnceAfterSpotlightModeWrite { get; set; }
    /// <summary>Number of confirmed mode writes.</summary>
    internal int ModeWriteCalls { get; private set; }
    /// <summary>Number of individual mode-flag value writes.</summary>
    internal int ModeValueWriteCalls { get; private set; }
    /// <summary>Number of mode-flag value writes performed by the most recent <see cref="WriteModeFlags"/> call.</summary>
    internal int LastModeValueWriteCalls { get; private set; }
    /// <summary>Number of ownership-guard invocations.</summary>
    internal int ModeGuardCalls { get; private set; }
    /// <summary>Number of compatibility reads for mode flag values.</summary>
    internal int ModeCompatibilityReadCalls { get; private set; }
    /// <summary>Number of image writes applied.</summary>
    internal int ImageWriteCalls { get; private set; }
    /// <summary>Number of current-image read attempts.</summary>
    internal int ImageReadCalls { get; private set; }
    /// <summary>Callback invoked before an image write; used to inject user changes or failures.</summary>
    internal Action? BeforeImageWrite { get; set; }
    /// <summary>Callback invoked after an image write.</summary>
    internal Action? AfterImageWrite { get; set; }
    /// <summary>Callback invoked before a mode write; used to inject user changes.</summary>
    internal Action? BeforeModeWrite { get; set; }
    /// <summary>Callback invoked after the first mode-flag value write.</summary>
    internal Action? AfterFirstModeValueWrite { get; set; }
    /// <summary>Callback invoked after the ownership guard runs.</summary>
    internal Action? AfterModeOwnershipGuard { get; set; }
    /// <summary>Optional provider for an incompatible mode flag value.</summary>
    internal Func<int>? ReadIncompatibleModeFlag { get; set; }

    /// <summary>Reports the platform as supported.</summary>
    /// <returns>Always true.</returns>
    public bool IsSupported() => true;
    /// <summary>Returns the fake mode flags.</summary>
    /// <returns>The slideshow and spotlight mode flags.</returns>
    public (int SlideshowEnabled, int SpotlightEnabled) ReadModeFlags() => ModeFlags;
    /// <summary>Returns the fake current image path, counting the read.</summary>
    /// <returns>The current image path.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the synthetic image URI is unavailable.</exception>
    public string GetCurrentLocalImagePath()
    {
        ImageReadCalls++;
        return CurrentImagePath ?? throw new InvalidOperationException("Synthetic unavailable lock-screen image URI.");
    }

    /// <summary>Simulates applying an image after the ownership guard allows it; honors injected callbacks and failures.</summary>
    /// <param name="imagePath">Image path to apply.</param>
    /// <param name="canWrite">Ownership guard; when it returns false the write is refused.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task when the write is allowed.</returns>
    public Task SetImageAsync(string imagePath, Func<bool> canWrite, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        BeforeImageWrite?.Invoke();
        if (!canWrite()) throw new InvalidOperationException("Synthetic source changed before native image write.");
        ImageWriteCalls++;
        CurrentImagePath = imagePath;
        AfterImageWrite?.Invoke();
        return Task.CompletedTask;
    }

    /// <summary>Simulates writing mode flags through the compatibility path, honoring injected callbacks and one-shot interruption.</summary>
    /// <param name="slideshowEnabled">Slideshow mode flag to write.</param>
    /// <param name="spotlightEnabled">Spotlight mode flag to write.</param>
    /// <param name="canWrite">Optional ownership guard forwarded to the native writer.</param>
    public void WriteModeFlags(int slideshowEnabled, int spotlightEnabled, Func<bool>? canWrite = null)
    {
        BeforeModeWrite?.Invoke();
        LastModeValueWriteCalls = 0;
        /// <summary>Records one mode-flag value write and confirms the first one.</summary>
        void ValueWritten()
        {
            ModeValueWriteCalls++;
            LastModeValueWriteCalls++;
            if (LastModeValueWriteCalls == 1)
            {
                ModeWriteCalls++;
                AfterFirstModeValueWrite?.Invoke();
            }
        }
        /// <summary>Reads a mode flag through the compatibility path, honoring an injected incompatible-value provider.</summary>
        /// <param name="slideshow">True to read the slideshow flag; false for spotlight.</param>
        /// <returns>The flag value.</returns>
        int ReadModeValue(bool slideshow)
        {
            ModeCompatibilityReadCalls++;
            return ReadIncompatibleModeFlag is not null ? ReadIncompatibleModeFlag() :
                slideshow ? ModeFlags.SlideshowEnabled : ModeFlags.SpotlightEnabled;
        }
        WindowsLockScreenNativePlatform.WriteModeFlagValues(slideshowEnabled, spotlightEnabled,
            value =>
            {
                ModeFlags = (value, ModeFlags.SpotlightEnabled);
                ValueWritten();
            },
            value =>
            {
                ModeFlags = (ModeFlags.SlideshowEnabled, value);
                ValueWritten();
            },
            () => ReadModeValue(slideshow: true), () => ReadModeValue(slideshow: false),
            canWrite is null ? null : () =>
            {
                ModeGuardCalls++;
                bool owned = canWrite();
                AfterModeOwnershipGuard?.Invoke();
                return owned;
            });
        if (ModeFlags == (0, 1) && HideImageInSpotlight) CurrentImagePath = null;
        if (ModeFlags == (0, 1) && FailOnceAfterSpotlightModeWrite)
        {
            FailOnceAfterSpotlightModeWrite = false;
            throw new IOException("Synthetic interruption after Spotlight mode was restored.");
        }
    }
}
