using System;
using System.Threading;
using System.Threading.Tasks;

namespace PrayerAutomation.UI;

internal enum LockScreenBackgroundMode
{
    Unknown,
    Picture,
    Slideshow,
    Spotlight
}

internal static class LockScreenModeLogic
{
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

internal enum LockScreenImageOwnership
{
    TemporaryImage,
    TemporaryImageModeDrift,
    RestorationImage,
    DifferentImage,
    Unknown
}

internal enum LockScreenTransactionResult
{
    Applied,
    Extended,
    Active,
    Restored,
    ExternalChangePreserved,
    NoActiveTransaction,
    PendingVerification
}

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

internal interface ILockScreenImageBackend
{
    LockScreenRestoreSnapshot CaptureSnapshot(string imagePath, DateTimeOffset restoreAtUtc);
    Task ApplyTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken);
    Task MaintainTemporaryImageAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken);
    LockScreenImageOwnership CheckTemporaryImage(LockScreenRestoreSnapshot snapshot);
    Task RestoreAsync(LockScreenRestoreSnapshot snapshot, CancellationToken cancellationToken);
    bool IsRestored(LockScreenRestoreSnapshot snapshot);
    void Cleanup(LockScreenRestoreSnapshot snapshot);
}

internal interface ILockScreenRestoreStore
{
    LockScreenRestoreSnapshot? Load();
    void Save(LockScreenRestoreSnapshot snapshot);
    void Delete();
}

internal sealed class LockScreenImageTransaction
{
    internal static readonly TimeSpan TemporaryDuration = TimeSpan.FromMinutes(60);

    private readonly ILockScreenImageBackend _backend;
    private readonly ILockScreenRestoreStore _store;
    private readonly SemaphoreSlim _gate = new(1, 1);

    internal LockScreenImageTransaction(ILockScreenImageBackend backend, ILockScreenRestoreStore store)
    {
        _backend = backend;
        _store = store;
    }

    internal DateTimeOffset? RestoreAtUtc => _store.Load()?.RestoreAtUtc;

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

    private void ClearPending(LockScreenRestoreSnapshot snapshot)
    {
        _store.Delete();
        _backend.Cleanup(snapshot);
    }
}
