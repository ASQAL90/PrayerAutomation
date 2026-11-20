using System;
using System.Threading;
using System.Threading.Tasks;

namespace PrayerAutomation.UI;

/// <summary>Outcome of launching a prepared update.</summary>
internal enum UpdateLaunchResult
{
    Started,
    Cancelled,
    LockScreenPending
}

/// <summary>Readiness of the lock-screen restore journal before an update may start.</summary>
internal enum LockScreenUpdateReadiness
{
    Ready,
    TimedOut
}

/// <summary>User-facing progress of a lock-screen restore wait.</summary>
/// <param name="RemainingSeconds">Whole seconds left before the wait is declared timed out.</param>
/// <param name="RestoreAtUtc">Journal deadline, shown only as information; never used as proof of restoration.</param>
internal sealed record LockScreenRecoveryProgress(int RemainingSeconds, DateTimeOffset? RestoreAtUtc);

/// <summary>Bounded hold/retry policy used before an update touches installed files.</summary>
internal static class UpdateRecoveryPolicy
{
    internal static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(5);
    internal static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(60);

    /// <summary>Results that prove no further restore attempt is needed.</summary>
    /// <param name="result">Result returned by a restore attempt.</param>
    /// <returns>True when the update may proceed.</returns>
    internal static bool IsReady(LockScreenTransactionResult result) => result is
        LockScreenTransactionResult.Restored or
        LockScreenTransactionResult.NoActiveTransaction or
        LockScreenTransactionResult.ExternalChangePreserved or
        LockScreenTransactionResult.ManagedByPolicy;

    /// <summary>Retries a restore until it is verified or the wait budget is exhausted.</summary>
    /// <param name="restore">Performs one restore attempt.</param>
    /// <param name="cancellationToken">Cancels the wait when the update window closes.</param>
    /// <param name="delay">Injectable delay for tests; defaults to <see cref="Task.Delay(TimeSpan, CancellationToken)"/>.</param>
    /// <param name="now">Injectable clock for tests; defaults to UTC now.</param>
    /// <param name="restoreDeadline">Optional journal deadline for the information line.</param>
    /// <param name="progress">Optional progress sink.</param>
    /// <param name="timeout">Optional wait budget; defaults to 60 seconds.</param>
    /// <returns>Ready when restoration was verified, otherwise TimedOut.</returns>
    internal static async Task<LockScreenUpdateReadiness> WaitForRecoveryAsync(
        Func<CancellationToken, Task<LockScreenTransactionResult>> restore,
        CancellationToken cancellationToken,
        Func<TimeSpan, CancellationToken, Task>? delay = null,
        Func<DateTimeOffset>? now = null,
        Func<DateTimeOffset?>? restoreDeadline = null,
        Action<LockScreenRecoveryProgress>? progress = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(restore);
        delay ??= Task.Delay;
        now ??= () => DateTimeOffset.UtcNow;
        TimeSpan limit = timeout ?? WaitTimeout;
        DateTimeOffset start = now();
        DateTimeOffset? deadline = null;
        if (restoreDeadline is not null)
        {
            try { deadline = restoreDeadline(); }
            catch (Exception)
            {
                // A diagnostic deadline never blocks a restore attempt.
                deadline = null;
            }
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            TimeSpan elapsed = now() - start;
            if (elapsed >= limit)
            {
                progress?.Invoke(new LockScreenRecoveryProgress(0, deadline));
                return LockScreenUpdateReadiness.TimedOut;
            }
            int remaining = (int)Math.Ceiling((limit - elapsed).TotalSeconds);
            progress?.Invoke(new LockScreenRecoveryProgress(remaining, deadline));

            LockScreenTransactionResult result;
            try
            {
                result = await restore(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                // A failed attempt is not readiness. The caller logs the reason;
                // the journal is left untouched so the next attempt can retry.
                result = LockScreenTransactionResult.PendingVerification;
            }

            if (IsReady(result)) return LockScreenUpdateReadiness.Ready;

            elapsed = now() - start;
            if (elapsed >= limit)
            {
                progress?.Invoke(new LockScreenRecoveryProgress(0, deadline));
                return LockScreenUpdateReadiness.TimedOut;
            }
            TimeSpan wait = RetryInterval;
            TimeSpan left = limit - elapsed;
            if (wait > left) wait = left;
            await delay(wait, cancellationToken).ConfigureAwait(false);
        }
    }
}
