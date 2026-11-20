using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PrayerAutomation.UI;

/// <summary>Lock-screen background modes known to the app.</summary>
internal enum LockScreenBackgroundMode
{
    Unknown,
    Picture,
    Slideshow,
    Spotlight
}

/// <summary>Resolves lock-screen background mode from Windows registry flags.</summary>
internal static class LockScreenModeLogic
{
    /// <summary>Resolves the mode from raw SlideshowEnabled/RotatingLockScreenEnabled flags; conflicting or missing flags yield Unknown.</summary>
    /// <param name="slideshowEnabled">Raw slideshow flag (0/1), or null when unread.</param>
    /// <param name="spotlightEnabled">Raw spotlight flag (0/1), or null when unread.</param>
    /// <returns>The resolved mode.</returns>
    internal static LockScreenBackgroundMode Resolve(int? slideshowEnabled, int? spotlightEnabled)
    {
        if (slideshowEnabled is not (0 or 1) || spotlightEnabled is not (0 or 1))
            return LockScreenBackgroundMode.Unknown;
        if (slideshowEnabled == 1 && spotlightEnabled == 1)
            return LockScreenBackgroundMode.Unknown;
        if (slideshowEnabled == 1)
            return LockScreenBackgroundMode.Slideshow;
        if (spotlightEnabled == 1)
            return LockScreenBackgroundMode.Spotlight;
        return LockScreenBackgroundMode.Picture;
    }
}

/// <summary>Ownership classification of the current lock-screen image.</summary>
internal enum LockScreenImageOwnership
{
    TemporaryImage,
    TemporaryImageModeDrift,
    RestorationImage,
    DifferentImage,
    Unknown
}

/// <summary>Outcome of a lock-screen image transaction.</summary>
internal enum LockScreenTransactionResult
{
    Applied,
    Extended,
    Active,
    Restored,
    ExternalChangePreserved,
    NoActiveTransaction,
    PendingVerification,
    ManagedByPolicy,
    ForcedRestored
}

/// <summary>Journaled snapshot of the lock-screen state before a temporary image is applied.</summary>
internal sealed record LockScreenRestoreSnapshot(
    int Version,
    LockScreenBackgroundMode PreviousMode,
    int PreviousSlideshowEnabled,
    int PreviousSpotlightEnabled,
    string? PreviousImagePath,
    string? PreviousImageSha256,
    string TemporaryImagePath,
    string TemporaryImageSha256,
    DateTimeOffset RestoreAtUtc,
    bool IsApplied,
    bool RestoreRequested = false);

/// <summary>Backend abstraction for capturing, applying, and restoring the lock-screen image.</summary>
internal interface ILockScreenImageBackend
{
    /// <summary>Captures a snapshot of the current lock-screen state.</summary>
    /// <param name="imagePath">Path of the temporary image to apply.</param>
    /// <param name="restoreAtUtc">UTC time at which the image should be restored.</param>
    /// <returns>The captured snapshot.</returns>
    LockScreenRestoreSnapshot CaptureSnapshot(string imagePath, DateTimeOffset restoreAtUtc);
    /// <summary>Applies the temporary image from a snapshot.</summary>
    /// <param name="snapshot">The snapshot to apply.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous apply.</returns>
    Task ApplyTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken);
    /// <summary>Repairs a confirmed mode drift on the active application-owned image.</summary>
    /// <param name="snapshot">The snapshot to maintain.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous repair.</returns>
    Task MaintainTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken);
    /// <summary>Classifies ownership of the current lock-screen image against a snapshot.</summary>
    /// <param name="snapshot">The snapshot to check against.</param>
    /// <returns>The ownership classification; Unknown when verification fails.</returns>
    LockScreenImageOwnership CheckTemporaryImage(LockScreenRestoreSnapshot snapshot);
    /// <summary>Restores the previous lock-screen state from a snapshot.</summary>
    /// <param name="snapshot">The snapshot to restore.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous restore.</returns>
    Task RestoreAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken);
    /// <summary>Checks whether the current state matches the pre-apply snapshot.</summary>
    /// <param name="snapshot">The snapshot to verify against.</param>
    /// <returns>True when the previous mode and image are verified restored.</returns>
    bool IsRestored(LockScreenRestoreSnapshot snapshot);
    /// <summary>Deletes managed files associated with a snapshot.</summary>
    /// <param name="snapshot">The snapshot whose files are removed.</param>
    void Cleanup(LockScreenRestoreSnapshot snapshot);
    /// <summary>Best-effort restore used by the update preflight; verification failures are tolerated.</summary>
    /// <param name="snapshot">The snapshot to restore.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous force restore.</returns>
    Task ForceRestoreAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken) =>
        RestoreAsync(snapshot, cancellationToken);
}

/// <summary>Persistence abstraction for the restore journal.</summary>
internal interface ILockScreenRestoreStore
{
    /// <summary>Loads the pending snapshot, or null when none exists.</summary>
    /// <returns>The pending snapshot.</returns>
    LockScreenRestoreSnapshot? Load();
    /// <summary>Persists a snapshot to the journal.</summary>
    /// <param name="snapshot">The snapshot to save.</param>
    void Save(LockScreenRestoreSnapshot snapshot);
    /// <summary>Deletes the journaled snapshot.</summary>
    void Delete();
    /// <summary>Deletes the journal and every quarantined variant; used for explicit user cancellation.</summary>
    void ClearAll();
}

/// <summary>Coordinates capture, apply, extend, recovery and restore of the temporary lock-screen image under a gate.</summary>
internal sealed class LockScreenImageTransaction
{
    /// <summary>Default temporary-image duration (60 minutes).</summary>
    internal static readonly TimeSpan TemporaryDuration = TimeSpan.FromMinutes(60);

    private readonly ILockScreenImageBackend _backend;
    private readonly ILockScreenRestoreStore _store;
    private readonly Func<bool>? _isManagedByPolicy;
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>Initializes the transaction coordinator.</summary>
    /// <param name="backend">The lock-screen image backend.</param>
    /// <param name="store">The restore journal store.</param>
    /// <param name="isManagedByPolicy">Optional policy probe consulted before a new snapshot.</param>
    internal LockScreenImageTransaction(
        ILockScreenImageBackend backend,
        ILockScreenRestoreStore store,
        Func<bool>? isManagedByPolicy = null)
    {
        _backend = backend;
        _store = store;
        _isManagedByPolicy = isManagedByPolicy;
    }

    /// <summary>The restore deadline of the pending transaction, if any.</summary>
    internal DateTimeOffset? RestoreAtUtc => _store.Load()?.RestoreAtUtc;

    /// <summary>True while an applied, not-yet-restored image is recorded in the journal.</summary>
    internal bool HasActiveImage
    {
        get
        {
            try
            {
                LockScreenRestoreSnapshot? pending = _store.Load();
                return pending is not null && pending.IsApplied && !pending.RestoreRequested &&
                    DateTimeOffset.UtcNow < pending.RestoreAtUtc;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                // A session event must never fail because the journal is unreadable or quarantined.
                return false;
            }
        }
    }

    /// <summary>Applies the temporary image, or extends an existing application-owned transaction.</summary>
    /// <param name="imagePath">Path of the image to apply.</param>
    /// <param name="nowUtc">Current UTC time.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <param name="temporaryDuration">Optional duration override (1-120 minutes).</param>
    /// <returns>The transaction outcome.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="temporaryDuration"/> is outside 1-120 minutes.</exception>
    internal async Task<LockScreenTransactionResult> BeginOrExtendAsync(
        string imagePath,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken,
        TimeSpan? temporaryDuration = null)
    {
        TimeSpan duration = temporaryDuration ?? TemporaryDuration;
        if (duration < TimeSpan.FromMinutes(1) || duration > TimeSpan.FromMinutes(120))
            throw new ArgumentOutOfRangeException(nameof(temporaryDuration), "The lock-screen image duration must be between 1 and 120 minutes.");
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            LockScreenRestoreSnapshot? pending = _store.Load();
            if (pending is not null)
            {
                if (pending.RestoreRequested || !pending.IsApplied)
                {
                    LockScreenTransactionResult recovery = await RestoreUnderGateAsync(cancellationToken).ConfigureAwait(false);
                    if (recovery == LockScreenTransactionResult.PendingVerification) return recovery;
                    pending = _store.Load();
                }
            }
            if (pending is not null)
            {
                LockScreenImageOwnership ownership = _backend.CheckTemporaryImage(pending);
                if (ownership == LockScreenImageOwnership.TemporaryImageModeDrift)
                {
                    await _backend.MaintainTemporaryImageAsync(pending, cancellationToken).ConfigureAwait(false);
                    ownership = _backend.CheckTemporaryImage(pending);
                }
                if (ownership == LockScreenImageOwnership.TemporaryImage)
                {
                    DateTimeOffset requestedDeadline = nowUtc.Add(duration);
                    if (requestedDeadline > pending.RestoreAtUtc)
                    {
                        pending = pending with { RestoreAtUtc = requestedDeadline, IsApplied = true };
                        _store.Save(pending);
                    }
                    return LockScreenTransactionResult.Extended;
                }

                if (ownership == LockScreenImageOwnership.Unknown)
                    return LockScreenTransactionResult.PendingVerification;

                ClearPending(pending);
            }

            // A policy check happens before any snapshot or registry write so a managed
            // device is refused cleanly instead of failing at every prayer time.
            if (_isManagedByPolicy?.Invoke() == true)
                return LockScreenTransactionResult.ManagedByPolicy;

            LockScreenRestoreSnapshot snapshot = _backend.CaptureSnapshot(imagePath, nowUtc.Add(duration)) with
            {
                IsApplied = false
            };
            try
            {
                _store.Save(snapshot);
            }
            catch
            {
                _backend.Cleanup(snapshot);
                throw;
            }

            try
            {
                await _backend.ApplyTemporaryImageAsync(snapshot, cancellationToken).ConfigureAwait(false);
                if (_backend.CheckTemporaryImage(snapshot) != LockScreenImageOwnership.TemporaryImage)
                    throw new InvalidOperationException("Windows did not confirm the temporary lock-screen image.");

                snapshot = snapshot with { IsApplied = true };
                _store.Save(snapshot);
                return LockScreenTransactionResult.Applied;
            }
            catch
            {
                await RollBackFailedApplyAsync(snapshot, cancellationToken).ConfigureAwait(false);
                throw;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>True when a restore journal exists, including an unreadable or quarantined one.</summary>
    internal bool HasPending
    {
        get
        {
            try { return _store.Load() is not null; }
            catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
            {
                return true;
            }
        }
    }

    /// <summary>Clears the pending journal without restoring; only for explicit user cancellation.</summary>
    internal void ClearPending()
    {
        LockScreenRestoreSnapshot? pending = null;
        try { pending = _store.Load(); }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException) { }
        if (pending is not null) ClearPending(pending);
        _store.ClearAll();
    }

    /// <summary>Restores the previous lock-screen state, verifying the result.</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The transaction outcome.</returns>
    internal async Task<LockScreenTransactionResult> RestoreAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await RestoreUnderGateAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Forces the journal to a closed state so an update can proceed without a 60-second hold.</summary>
    /// <param name="cancellationToken">Token to cancel the gate wait.</param>
    /// <returns><see cref="LockScreenTransactionResult.ForcedRestored"/> after best-effort cleanup.</returns>
    internal async Task<LockScreenTransactionResult> ForceRestoreForUpdateAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            LockScreenRestoreSnapshot? pending;
            try { pending = _store.Load(); }
            catch (Exception) { pending = null; }

            if (pending is not null)
            {
                try { await _backend.ForceRestoreAsync(pending, cancellationToken).ConfigureAwait(false); }
                catch (Exception) { }
                try { ClearPending(pending); }
                catch (Exception) { }
            }
            try { _store.ClearAll(); }
            catch (Exception) { }
            return LockScreenTransactionResult.ForcedRestored;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Recovers from a crash or shutdown: restores an expired/disabled transaction, reports otherwise.</summary>
    /// <param name="isEnabled">Whether the feature is currently enabled.</param>
    /// <param name="nowUtc">Current UTC time.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The recovery outcome.</returns>
    internal async Task<LockScreenTransactionResult> RecoverAsync(
        bool isEnabled,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            LockScreenRestoreSnapshot? pending = _store.Load();
            if (pending is null)
                return LockScreenTransactionResult.NoActiveTransaction;

            if (!isEnabled || nowUtc >= pending.RestoreAtUtc || pending.RestoreRequested || !pending.IsApplied)
                return await RestoreUnderGateAsync(cancellationToken).ConfigureAwait(false);

            LockScreenImageOwnership ownership = _backend.CheckTemporaryImage(pending);
            if (ownership == LockScreenImageOwnership.TemporaryImageModeDrift)
            {
                await _backend.MaintainTemporaryImageAsync(pending, cancellationToken).ConfigureAwait(false);
                ownership = _backend.CheckTemporaryImage(pending);
            }
            if (ownership == LockScreenImageOwnership.TemporaryImage)
                return LockScreenTransactionResult.Active;
            if (ownership == LockScreenImageOwnership.Unknown)
                return LockScreenTransactionResult.PendingVerification;

            if (_backend.IsRestored(pending))
            {
                ClearPending(pending);
                return LockScreenTransactionResult.Restored;
            }

            ClearPending(pending);
            return LockScreenTransactionResult.ExternalChangePreserved;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Restore implementation assuming the gate is held; never runs concurrently.</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The transaction outcome.</returns>
    private async Task<LockScreenTransactionResult> RestoreUnderGateAsync(CancellationToken cancellationToken)
    {
        LockScreenRestoreSnapshot? pending = _store.Load();
        if (pending is null)
            return LockScreenTransactionResult.NoActiveTransaction;

        // Spotlight restoration is mode-only; its current image URI is not a recovery requirement.
        if (pending.PreviousMode == LockScreenBackgroundMode.Spotlight && _backend.IsRestored(pending))
        {
            ClearPending(pending);
            return LockScreenTransactionResult.Restored;
        }

        LockScreenImageOwnership ownership = _backend.CheckTemporaryImage(pending);
        if (ownership == LockScreenImageOwnership.Unknown)
            return LockScreenTransactionResult.PendingVerification;

        if (ownership == LockScreenImageOwnership.DifferentImage)
        {
            bool alreadyRestored = _backend.IsRestored(pending);
            ClearPending(pending);
            return alreadyRestored
                ? LockScreenTransactionResult.Restored
                : LockScreenTransactionResult.ExternalChangePreserved;
        }

        if (ownership == LockScreenImageOwnership.RestorationImage && !pending.RestoreRequested)
            return LockScreenTransactionResult.PendingVerification;
        pending = pending with { RestoreRequested = true };
        _store.Save(pending);
        await _backend.RestoreAsync(pending, cancellationToken).ConfigureAwait(false);
        if (!_backend.IsRestored(pending))
            throw new InvalidOperationException("Windows did not confirm restoration of the previous lock-screen state.");

        ClearPending(pending);
        return LockScreenTransactionResult.Restored;
    }

    /// <summary>Rolls back a failed apply, deleting the journal when restoration is confirmed.</summary>
    /// <param name="snapshot">The snapshot to roll back.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the rollback.</returns>
    private async Task RollBackFailedApplyAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken)
    {
        LockScreenImageOwnership ownership = _backend.CheckTemporaryImage(snapshot);
        if (ownership == LockScreenImageOwnership.Unknown)
            return;

        if (ownership == LockScreenImageOwnership.TemporaryImage)
        {
            snapshot = snapshot with { RestoreRequested = true };
            _store.Save(snapshot);
            await _backend.RestoreAsync(snapshot, CancellationToken.None).ConfigureAwait(false);
            if (!_backend.IsRestored(snapshot))
                return;
        }
        else if (!_backend.IsRestored(snapshot))
        {
            ClearPending(snapshot);
            return;
        }

        ClearPending(snapshot);
    }

    /// <summary>Deletes the journal and cleans up managed files; caller must hold the gate.</summary>
    /// <param name="snapshot">The snapshot to clear.</param>
    private void ClearPending(LockScreenRestoreSnapshot snapshot)
    {
        _store.Delete();
        _backend.Cleanup(snapshot);
    }
}
