// Smoke test verifying the lock-screen mode value writer: valid transitions, guard refusal boundaries, and mode-only activation recovery, all through isolated native fakes.
using System.IO;
using System.Security.Cryptography;
using PrayerAutomation.UI;

/// <summary>Verifies lock-screen mode value writes, ownership guards, and activation boundaries against isolated native fakes.</summary>
internal static class LockScreenModeValueWriteSmoke
{
    /// <summary>Verifies that all valid Slideshow/Spotlight mode transitions resolve, apply with two guards and two writes, and reject invalid flags or refused guards fail-closed.</summary>
    internal static void VerifyValidTransitionsAndGuards()
    {
        (int Slideshow, int Spotlight)[] modes = [(0, 0), (1, 0), (0, 1)];
        int cases = 0;
        foreach (var current in modes)
        foreach (var requested in modes)
        {
            int slideshow = current.Slideshow;
            int spotlight = current.Spotlight;
            int guards = 0, writes = 0, slideshowWrites = 0, spotlightWrites = 0;
            /// <summary>Records a value write and requires the intermediate flag state never resolves to Unknown.</summary>
            void ValueWritten()
            {
                writes++;
                Require(LockScreenModeLogic.Resolve(slideshow, spotlight) != LockScreenBackgroundMode.Unknown,
                    $"A mode value write must not create conflicting flags: current={current}, requested={requested}, intermediate=({slideshow},{spotlight}).");
            }
            WindowsLockScreenNativePlatform.WriteModeFlagValues(requested.Slideshow, requested.Spotlight,
                value =>
                {
                    Require(guards == writes + 1, "Each slideshow value write must have its own preceding guard.");
                    slideshow = value;
                    slideshowWrites++;
                    ValueWritten();
                },
                value =>
                {
                    Require(guards == writes + 1, "Each Spotlight value write must have its own preceding guard.");
                    spotlight = value;
                    spotlightWrites++;
                    ValueWritten();
                },
                () => slideshow, () => spotlight,
                () =>
                {
                    guards++;
                    Require(writes == guards - 1 && guards <= 2,
                        "The ownership callback must run immediately before each of the two independently written values.");
                    return LockScreenModeLogic.Resolve(slideshow, spotlight) != LockScreenBackgroundMode.Unknown;
                });
            Require(guards == 2 && writes == 2 && slideshowWrites == 1 && spotlightWrites == 1 &&
                    (slideshow, spotlight) == requested,
                "Every valid current/requested mode pair must finish at the requested flags with two guards and two value writes.");
            cases++;
        }

        int invalidCases = 0;
        foreach (var invalid in new[] { (1, 1), (-1, 0), (0, -1), (2, 0), (0, 2) })
        {
            int writes = 0, guards = 0;
            bool refused = false;
            try
            {
                WindowsLockScreenNativePlatform.WriteModeFlagValues(invalid.Item1, invalid.Item2,
                    _ => writes++, _ => writes++, () => 0, () => 0, () => { guards++; return true; });
            }
            catch (ArgumentOutOfRangeException error) when (error.ParamName == "slideshowEnabled") { refused = true; }
            Require(refused && writes == 0 && guards == 0,
                "Invalid requested flags must be rejected before any value write or ownership callback.");
            invalidCases++;
        }

        foreach (int refusedGuard in new[] { 1, 2 })
        {
            int guards = 0, writes = 0;
            (int Slideshow, int Spotlight) flags = (0, 1);
            string? failure = null;
            try
            {
                WindowsLockScreenNativePlatform.WriteModeFlagValues(1, 0,
                    value => { flags = (value, flags.Spotlight); writes++; },
                    value => { flags = (flags.Slideshow, value); writes++; },
                    () => flags.Slideshow, () => flags.Spotlight,
                    () => ++guards != refusedGuard);
            }
            catch (InvalidOperationException error) { failure = error.Message; }
            string expectedFailure = refusedGuard == 1
                ? "The lock-screen source changed before the mode write; the write was refused."
                : "The lock-screen source changed during the mode write; further writes were refused.";
            Require(guards == refusedGuard && writes == refusedGuard - 1 && failure == expectedFailure &&
                    flags == (refusedGuard == 1 ? (0, 1) : (0, 0)),
                "A refused ownership guard must stop that value and all remaining writes without rollback or exception replacement.");
        }
        Console.WriteLine($"LOCKSCREEN_VALUE_WRITER PASS: transitions={cases}; guards_per_transition=2; values_per_transition=2; intermediate_conflicts=0; invalid_requests={invalidCases}; refused_guard_cases=2; compiled_production_helper=1; real_native_writes=0.");
    }

    /// <summary>Verifies that an owned Slideshow lease expiring under Spotlight drift restores with the two-value writer, persisting restore intent and retaining the original deadline.</summary>
    /// <param name="root">Scratch directory for the isolated fixture.</param>
    internal static async Task VerifyOwnedSlideshowExpiryFromSpotlightAsync(string root)
    {
        string directory = Path.Combine(root, "partial-mode-owned-slideshow-expiry");
        Directory.CreateDirectory(directory);
        string prayerImage = Path.Combine(directory, "prayer.png");
        File.WriteAllText(prayerImage, "synthetic-prayer-image");
        var native = new IsolatedLockScreenPlatform { ModeFlags = (1, 0) };
        var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
        var store = new FileLockScreenRestoreStore(directory);
        var transaction = new LockScreenImageTransaction(backend, store);
        DateTimeOffset start = new(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);
        DateTimeOffset deadline = start.AddMinutes(7);
        Require(await transaction.BeginOrExtendAsync(prayerImage, start, CancellationToken.None, TimeSpan.FromMinutes(7))
                == LockScreenTransactionResult.Applied, "The owned-source expiry fixture must first apply its seven-minute lease.");
        LockScreenRestoreSnapshot captured = store.Load()!;
        int modeWrites = native.ModeWriteCalls;
        int valueWrites = native.ModeValueWriteCalls;
        int guards = native.ModeGuardCalls;
        int imageWrites = native.ImageWriteCalls;
        native.ModeFlags = (0, 1);
        bool persistedIntentBeforeWrite = false, persistedIntentAfterFirstWrite = false;
        (int, int)? firstFlags = null;
        native.BeforeModeWrite = () => persistedIntentBeforeWrite = store.Load() == (captured with { RestoreRequested = true });
        native.AfterFirstModeValueWrite = () =>
        {
            firstFlags = native.ModeFlags;
            persistedIntentAfterFirstWrite = store.Load() == (captured with { RestoreRequested = true });
        };
        LockScreenTransactionResult? result = null;
        Exception? failure = null;
        try { result = await transaction.RecoverAsync(true, deadline, CancellationToken.None); }
        catch (InvalidOperationException error) { failure = error; }
        native.BeforeModeWrite = null;
        native.AfterFirstModeValueWrite = null;
        LockScreenRestoreSnapshot? pending = store.Load();
        bool ownedSourceUnchanged = native.CurrentImagePath == captured.TemporaryImagePath &&
            File.Exists(captured.TemporaryImagePath) && Hash(captured.TemporaryImagePath) == captured.TemporaryImageSha256;
        Console.WriteLine($"LOCKSCREEN_PARTIAL_MODE_EXPIRY result={result}; error_type={failure?.GetType().Name}; mode={native.ModeFlags}; first_flags={firstFlags}; last_value_writes={native.LastModeValueWriteCalls}; guards={native.ModeGuardCalls - guards}; journal_present={pending is not null}; restore_requested={pending?.RestoreRequested}; deadline_preserved={pending?.RestoreAtUtc == deadline}; source_path_and_hash_unchanged={ownedSourceUnchanged}; real_native_writes=0.");
        Require(result == LockScreenTransactionResult.Restored && failure is null && native.ModeFlags == (1, 0) &&
                firstFlags == (0, 0) && persistedIntentBeforeWrite && persistedIntentAfterFirstWrite &&
                captured.RestoreAtUtc == deadline && pending is null && transaction.RestoreAtUtc is null &&
                native.ModeWriteCalls == modeWrites + 1 && native.ModeValueWriteCalls == valueWrites + 2 &&
                native.LastModeValueWriteCalls == 2 && native.ModeGuardCalls == guards + 2 &&
                native.ImageWriteCalls == imageWrites && ownedSourceUnchanged,
            "Owned Slideshow expiry from Spotlight drift must restore with the real two-value writer, without creating a conflict, extending the deadline, or retaining its journal.");
        Console.WriteLine("LOCKSCREEN_PARTIAL_MODE_EXPIRY PASS: restored=1; persisted_restore_intent=1; deadline_extended=0; own_journal_cleared=1; compiled_production_helper=1.");
    }

    /// <summary>Verifies that when the user picture changes after the first mode value write, the second value is refused, the user image is preserved, and restart recovery keeps the new choice.</summary>
    /// <param name="root">Scratch directory for the isolated fixture.</param>
    internal static async Task VerifyRestoreSecondValueBoundaryPreservesUserImageAsync(string root)
    {
        int cases = 0;
        foreach ((int slideshow, int spotlight) in new[] { (0, 1), (1, 0), (0, 0) })
        {
            string directory = Path.Combine(root, "restore-second-value-" + slideshow + "-" + spotlight);
            Directory.CreateDirectory(directory);
            string originalImage = Path.Combine(directory, "original.png");
            string prayerImage = Path.Combine(directory, "prayer.png");
            string userImage = Path.Combine(directory, "user-new.png");
            File.WriteAllText(originalImage, "synthetic-original-picture");
            File.WriteAllText(prayerImage, "synthetic-prayer-image");
            File.WriteAllText(userImage, "synthetic-user-new-picture");
            string userHash = Hash(userImage);
            var native = new IsolatedLockScreenPlatform { ModeFlags = (slideshow, spotlight), CurrentImagePath = originalImage };
            var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
            var store = new FileLockScreenRestoreStore(directory);
            var transaction = new LockScreenImageTransaction(backend, store);
            DateTimeOffset start = new(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);
            DateTimeOffset deadline = start.AddMinutes(7);
            Require(await transaction.BeginOrExtendAsync(prayerImage, start, CancellationToken.None, TimeSpan.FromMinutes(7))
                    == LockScreenTransactionResult.Applied, "Each second-value boundary fixture must first apply its seven-minute lease.");
            LockScreenRestoreSnapshot captured = store.Load()!;
            native.ModeFlags = (slideshow, spotlight) == (0, 1) ? (1, 0) : (0, 1);
            int modeWrites = native.ModeWriteCalls;
            int valueWrites = native.ModeValueWriteCalls;
            int guards = native.ModeGuardCalls;
            int imageWrites = native.ImageWriteCalls;
            int hookCalls = 0;
            bool intentAtBoundary = false, ownedAfterFirstWrite = false;
            native.AfterFirstModeValueWrite = () =>
            {
                hookCalls++;
                LockScreenRestoreSnapshot? intent = store.Load();
                intentAtBoundary = intent == (captured with { RestoreRequested = true }) &&
                    native.LastModeValueWriteCalls == 1 && native.ModeGuardCalls == guards + 1;
                LockScreenImageOwnership expectedOwnership = captured.PreviousMode == LockScreenBackgroundMode.Picture
                    ? LockScreenImageOwnership.RestorationImage : LockScreenImageOwnership.TemporaryImage;
                ownedAfterFirstWrite = intent is not null && backend.CheckTemporaryImage(intent) == expectedOwnership;
                native.CurrentImagePath = userImage;
                native.ModeFlags = (0, 0);
            };
            string? failure = null;
            LockScreenTransactionResult? result = null;
            try { result = await transaction.RecoverAsync(true, deadline, CancellationToken.None); }
            catch (InvalidOperationException error) { failure = error.Message; }
            native.AfterFirstModeValueWrite = null;
            Require(result is null && failure == "The lock-screen source changed during the mode write; further writes were refused." &&
                    hookCalls == 1 && intentAtBoundary && ownedAfterFirstWrite && native.ModeFlags == (0, 0) &&
                    native.CurrentImagePath == userImage && Hash(userImage) == userHash &&
                    native.ModeWriteCalls == modeWrites + 1 && native.ModeValueWriteCalls == valueWrites + 1 &&
                    native.LastModeValueWriteCalls == 1 && native.ModeGuardCalls == guards + 2 &&
                    native.ImageWriteCalls == imageWrites + (captured.PreviousMode == LockScreenBackgroundMode.Picture ? 1 : 0) &&
                    store.Load() == (captured with { RestoreRequested = true }) && transaction.RestoreAtUtc == deadline,
                $"A new user picture after the first independent mode value must refuse the second value, retain the original deadline/journal/failure, and avoid rollback: previous=({slideshow},{spotlight}), values={native.LastModeValueWriteCalls}, guards={native.ModeGuardCalls - guards}, failure={failure}.");
            int recoveryImageWrites = native.ImageWriteCalls;
            int recoveryModeWrites = native.ModeWriteCalls;
            int recoveryValueWrites = native.ModeValueWriteCalls;
            int recoveryGuards = native.ModeGuardCalls;
            var restarted = new LockScreenImageTransaction(backend, new FileLockScreenRestoreStore(directory));
            Require(await restarted.RecoverAsync(true, deadline.AddSeconds(1), CancellationToken.None)
                    == LockScreenTransactionResult.ExternalChangePreserved && store.Load() is null && restarted.RestoreAtUtc is null &&
                    native.ModeFlags == (0, 0) && native.CurrentImagePath == userImage && Hash(userImage) == userHash &&
                    native.ImageWriteCalls == recoveryImageWrites && native.ModeWriteCalls == recoveryModeWrites &&
                    native.ModeValueWriteCalls == recoveryValueWrites && native.ModeGuardCalls == recoveryGuards &&
                    File.Exists(originalImage) && File.Exists(prayerImage),
                "Restart recovery must preserve the new user picture and its selected mode, perform no extra image/mode writes, and clear only obsolete application state.");
            cases++;
        }
        Console.WriteLine($"LOCKSCREEN_RESTORE_SECOND_VALUE_BOUNDARIES PASS: cases={cases}; previous_modes=Spotlight,Slideshow,Picture; guards_per_attempt=2; values_before_refusal=1; pending_deadline_preserved=1; rollback_writes=0; next_recovery_writes=0; obsolete_journal_cleared=1; real_native_writes=0.");
    }

    /// <summary>Verifies that mode-only activation stops at the second-value boundary (after-first-value, inside-second-guard, unknown/malformed/unreadable flags) without rollback, then recovers once the injection is removed.</summary>
    /// <param name="root">Scratch directory for the isolated fixture.</param>
    internal static async Task VerifyModeOnlyActivationBoundariesAsync(string root)
    {
        (int Slideshow, int Spotlight)[] targets = [(1, 0), (0, 1)];
        var failures = new List<string>();
        int controls = 0, refusedCases = 0;
        foreach (var requested in targets)
        foreach (string boundary in new[] { "control", "after-first-value", "inside-second-guard", "unknown-flag", "malformed-flag", "unreadable-flag" })
        {
            string target = requested.Slideshow == 1 ? "Slideshow" : "Spotlight";
            bool changesMode = boundary is "after-first-value" or "inside-second-guard";
            string directory = Path.Combine(root, "mode-only-activation-" + target + "-" + boundary);
            Directory.CreateDirectory(directory);
            string prayerImage = Path.Combine(directory, "prayer.png");
            File.WriteAllText(prayerImage, "synthetic-prayer-image");
            var native = new IsolatedLockScreenPlatform { ModeFlags = (requested.Slideshow, requested.Spotlight) };
            var backend = new WindowsLockScreenImageBackend(directory: directory, native: native);
            var store = new FileLockScreenRestoreStore(directory);
            var transaction = new LockScreenImageTransaction(backend, store);
            DateTimeOffset start = new(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);
            DateTimeOffset deadline = start.AddMinutes(7);
            Require(await transaction.BeginOrExtendAsync(prayerImage, start, CancellationToken.None, TimeSpan.FromMinutes(7))
                    == LockScreenTransactionResult.Applied, "Each activation boundary fixture must first apply its seven-minute lease.");
            LockScreenRestoreSnapshot captured = store.Load()!;
            /// <summary>Reports whether the owned temporary image is still present with its recorded hash.</summary>
            bool SourceUnchanged() => native.CurrentImagePath == captured.TemporaryImagePath &&
                File.Exists(captured.TemporaryImagePath) && Hash(captured.TemporaryImagePath) == captured.TemporaryImageSha256;
            Require(native.ModeFlags == (0, 0) && captured.RestoreAtUtc == deadline && SourceUnchanged() &&
                    backend.CheckTemporaryImage(captured) == LockScreenImageOwnership.TemporaryImage,
                "Activation must initially restore from the exact owned temporary Picture source, not preexisting mode drift.");
            int modeWrites = native.ModeWriteCalls, valueWrites = native.ModeValueWriteCalls;
            int guards = native.ModeGuardCalls, imageWrites = native.ImageWriteCalls;
            int compatibilityReads = native.ModeCompatibilityReadCalls;
            int hookCalls = 0, injectedReads = 0;
            bool intentBeforeWrite = false, intentAtBoundary = false, readAtLatestBoundary = false;
            LockScreenImageOwnership? ownershipBeforeChange = null, ownershipAfterChange = null, ownershipAtRead = null;
            (int SlideshowEnabled, int SpotlightEnabled) externalMode = requested.Slideshow == 1 ? (0, 1) : (1, 0);
            native.BeforeModeWrite = () => intentBeforeWrite = store.Load() == (captured with { RestoreRequested = true }) &&
                native.ModeFlags == (0, 0) && SourceUnchanged();
            /// <summary>Injects a mode-only change at the stated boundary after the first value write.</summary>
            void ChangeOnlyMode()
            {
                hookCalls++;
                Require(native.LastModeValueWriteCalls == 1 &&
                        native.ModeGuardCalls == guards + (boundary == "inside-second-guard" ? 2 : 1),
                    "The mode-only injection must occur after value one at its stated ownership boundary.");
                LockScreenRestoreSnapshot intent = store.Load()!;
                intentAtBoundary = intent == (captured with { RestoreRequested = true }) && SourceUnchanged();
                ownershipBeforeChange = backend.CheckTemporaryImage(intent);
                native.ModeFlags = externalMode;
                ownershipAfterChange = backend.CheckTemporaryImage(intent);
            }
            if (boundary == "after-first-value") native.AfterFirstModeValueWrite = ChangeOnlyMode;
            if (boundary == "inside-second-guard") native.AfterModeOwnershipGuard = () =>
            {
                if (native.LastModeValueWriteCalls == 1) ChangeOnlyMode();
            };
            if (boundary != "control") native.ReadIncompatibleModeFlag = () =>
            {
                injectedReads++;
                readAtLatestBoundary = native.LastModeValueWriteCalls == 1 && native.ModeGuardCalls == guards + 2 &&
                    store.Load() == (captured with { RestoreRequested = true }) && SourceUnchanged();
                ownershipAtRead = backend.CheckTemporaryImage(store.Load()!);
                if (boundary == "unreadable-flag") throw new IOException("Synthetic unreadable incompatible flag.");
                if (boundary == "unknown-flag") return -1;
                if (boundary == "malformed-flag") return 2;
                return requested.Slideshow == 1 ? native.ModeFlags.SpotlightEnabled : native.ModeFlags.SlideshowEnabled;
            };
            LockScreenTransactionResult? result = null;
            Exception? failure = null;
            try { result = await transaction.RecoverAsync(true, deadline, CancellationToken.None); }
            catch (Exception error) when (error is InvalidOperationException or IOException) { failure = error; }
            LockScreenRestoreSnapshot? pending = store.Load();
            var firstMode = native.ModeFlags;
            int firstValues = native.ModeValueWriteCalls - valueWrites;
            int firstGuards = native.ModeGuardCalls - guards;
            int firstReads = native.ModeCompatibilityReadCalls - compatibilityReads;
            bool sourceUnchanged = SourceUnchanged();
            bool conflict = LockScreenModeLogic.Resolve(firstMode.SlideshowEnabled, firstMode.SpotlightEnabled)
                == LockScreenBackgroundMode.Unknown;
            bool firstPass = intentBeforeWrite && captured.RestoreAtUtc == deadline && sourceUnchanged && !conflict &&
                native.ModeWriteCalls == modeWrites + 1 && native.ImageWriteCalls == imageWrites && firstGuards == 2;
            if (boundary == "control")
            {
                controls++;
                firstPass &= result == LockScreenTransactionResult.Restored && failure is null && firstValues == 2 &&
                    firstMode == (requested.Slideshow, requested.Spotlight) && pending is null && transaction.RestoreAtUtc is null &&
                    backend.IsRestored(captured with { RestoreRequested = true });
            }
            else
            {
                refusedCases++;
                string expectedError = boundary == "unreadable-flag"
                    ? "Synthetic unreadable incompatible flag."
                    : "The incompatible lock-screen feature is not disabled; activation was refused.";
                firstPass &= result is null && failure?.Message == expectedError &&
                    failure.GetType() == (boundary == "unreadable-flag" ? typeof(IOException) : typeof(InvalidOperationException)) &&
                    firstValues == 1 && native.LastModeValueWriteCalls == 1 && firstReads == 1 && injectedReads == 1 &&
                    readAtLatestBoundary && ownershipAtRead == LockScreenImageOwnership.TemporaryImage &&
                    firstMode == (changesMode ? externalMode : (0, 0)) &&
                    pending == (captured with { RestoreRequested = true }) && transaction.RestoreAtUtc == deadline &&
                    (!changesMode || (hookCalls == 1 && intentAtBoundary &&
                        ownershipBeforeChange == LockScreenImageOwnership.TemporaryImage &&
                        ownershipAfterChange == LockScreenImageOwnership.TemporaryImage));
            }
            native.BeforeModeWrite = null;
            native.AfterFirstModeValueWrite = null;
            native.AfterModeOwnershipGuard = null;
            native.ReadIncompatibleModeFlag = null;
            LockScreenTransactionResult? nextResult = null;
            int nextValues = 0, nextGuards = 0;
            bool nextPass = true, recoveryIntentUnchanged = false;
            if (boundary != "control")
            {
                int nextModeWrites = native.ModeWriteCalls, nextValueWrites = native.ModeValueWriteCalls;
                int nextGuardCalls = native.ModeGuardCalls, nextReadCalls = native.ModeCompatibilityReadCalls;
                native.BeforeModeWrite = () => recoveryIntentUnchanged = store.Load() == (captured with { RestoreRequested = true });
                var restarted = new LockScreenImageTransaction(backend, new FileLockScreenRestoreStore(directory));
                nextResult = await restarted.RecoverAsync(true, deadline.AddSeconds(1), CancellationToken.None);
                native.BeforeModeWrite = null;
                nextValues = native.ModeValueWriteCalls - nextValueWrites;
                nextGuards = native.ModeGuardCalls - nextGuardCalls;
                nextPass = nextResult == LockScreenTransactionResult.Restored && recoveryIntentUnchanged &&
                    native.ModeFlags == (requested.Slideshow, requested.Spotlight) &&
                    backend.IsRestored(captured with { RestoreRequested = true }) && store.Load() is null &&
                    restarted.RestoreAtUtc is null && transaction.RestoreAtUtc is null && SourceUnchanged() &&
                    native.ModeWriteCalls == nextModeWrites + 1 && nextValues == 2 && nextGuards == 2 &&
                    native.ModeCompatibilityReadCalls == nextReadCalls + 1 && native.ImageWriteCalls == imageWrites;
            }
            bool strictPass = firstPass && nextPass;
            Console.WriteLine("LOCKSCREEN_MODE_ONLY_ACTIVATION " + System.Text.Json.JsonSerializer.Serialize(new
            {
                target, boundary, initial_restore_mode = "Picture", result = result?.ToString(),
                error_type = failure?.GetType().Name, error_message = failure?.Message,
                first_slideshow = firstMode.SlideshowEnabled, first_spotlight = firstMode.SpotlightEnabled,
                application_created_conflict = conflict, source_path_and_hash_unchanged = sourceUnchanged,
                ownership_before_external_mode = ownershipBeforeChange?.ToString(),
                ownership_after_external_mode = ownershipAfterChange?.ToString(),
                ownership_at_compatibility_read = ownershipAtRead?.ToString(),
                restore_intent_at_boundary = intentAtBoundary, restore_intent_retained = pending?.RestoreRequested,
                deadline_retained = pending?.RestoreAtUtc == deadline, journal_retained = pending is not null,
                first_recovery_values = firstValues, first_recovery_guards = firstGuards,
                first_compatibility_reads = firstReads, compatibility_read_after_guard_two = readAtLatestBoundary,
                subsequent_result = nextResult?.ToString(), subsequent_values = nextValues, subsequent_guards = nextGuards,
                recovery_original_intent_and_deadline = recoveryIntentUnchanged,
                own_journal_cleared = store.Load() is null, strict_pass = strictPass,
                compiled_production_helper = true, real_native_writes = 0
            }));
            if (!strictPass) failures.Add($"{target}/{boundary}: first={result}, mode={firstMode}, values={firstValues}, guards={firstGuards}, error={failure?.Message}; next={nextResult}.");
        }
        Require(failures.Count == 0,
            "Mode-only activation must refuse value two without rollback, retain its original restore intent/deadline/source, and recover once the injection is removed: " + string.Join(" | ", failures));
        Console.WriteLine($"LOCKSCREEN_MODE_ONLY_ACTIVATION_BOUNDARIES PASS: controls={controls}; refused_cases={refusedCases}; targets=Slideshow,Spotlight; mode_only_boundaries=after-first-value,inside-second-guard; unreadable_and_unknown_fail_closed=1; values_before_refusal=1; rollback_writes=0; recovery_restored_and_verified=1; deadline_extended=0; compiled_production_helper=1; real_native_writes=0.");
    }

    /// <summary>Computes the SHA-256 hex digest of a file.</summary>
    /// <param name="path">File to hash.</param>
    /// <returns>Uppercase hex SHA-256 digest.</returns>
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    /// <summary>Throws <see cref="InvalidOperationException"/> when the condition does not hold.</summary>
    /// <param name="condition">Condition that must be true.</param>
    /// <param name="message">Failure message used when the condition is false.</param>
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
