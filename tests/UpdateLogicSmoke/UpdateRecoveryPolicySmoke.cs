// Smoke test for the bounded lock-screen restore wait used by update preflight.
using PrayerAutomation.UI;

namespace UpdateLogicSmoke;

/// <summary>Verifies hold/retry behavior without any WPF or Windows runtime dependency.</summary>
internal static class UpdateRecoveryPolicySmoke
{
    /// <summary>Runs every policy case.</summary>
    internal static async Task VerifyAllAsync()
    {
        DateTimeOffset start = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

        Assert(UpdateRecoveryPolicy.IsReady(LockScreenTransactionResult.Restored) &&
               UpdateRecoveryPolicy.IsReady(LockScreenTransactionResult.NoActiveTransaction) &&
               UpdateRecoveryPolicy.IsReady(LockScreenTransactionResult.ExternalChangePreserved) &&
               UpdateRecoveryPolicy.IsReady(LockScreenTransactionResult.ManagedByPolicy) &&
               !UpdateRecoveryPolicy.IsReady(LockScreenTransactionResult.PendingVerification) &&
               !UpdateRecoveryPolicy.IsReady(LockScreenTransactionResult.Applied) &&
               !UpdateRecoveryPolicy.IsReady(LockScreenTransactionResult.Active),
            "Only verified or already-safe results may release the update.");

        {
            var clock = new FakeClock(start);
            int attempts = 0;
            LockScreenUpdateReadiness readiness = await UpdateRecoveryPolicy.WaitForRecoveryAsync(
                _ =>
                {
                    attempts++;
                    return Task.FromResult(LockScreenTransactionResult.Restored);
                },
                CancellationToken.None,
                delay: clock.Delay,
                now: clock.Read);

            Assert(readiness == LockScreenUpdateReadiness.Ready && attempts == 1 && clock.Delays == 0,
                "A verified restore must continue immediately without waiting.");
        }

        {
            var clock = new FakeClock(start);
            var results = new Queue<LockScreenTransactionResult>(
            [
                LockScreenTransactionResult.PendingVerification,
                LockScreenTransactionResult.PendingVerification,
                LockScreenTransactionResult.Restored
            ]);
            int attempts = 0;
            LockScreenUpdateReadiness readiness = await UpdateRecoveryPolicy.WaitForRecoveryAsync(
                _ =>
                {
                    attempts++;
                    return Task.FromResult(results.Dequeue());
                },
                CancellationToken.None,
                delay: clock.Delay,
                now: clock.Read);

            Assert(readiness == LockScreenUpdateReadiness.Ready && attempts == 3 && clock.Delays == 2 &&
                   clock.Waits.All(wait => wait == UpdateRecoveryPolicy.RetryInterval),
                "Pending verification must retry every five seconds until a verified result appears.");
        }

        {
            var clock = new FakeClock(start);
            var progress = new List<LockScreenRecoveryProgress>();
            DateTimeOffset deadline = start.AddMinutes(15);
            int attempts = 0;
            LockScreenUpdateReadiness readiness = await UpdateRecoveryPolicy.WaitForRecoveryAsync(
                _ =>
                {
                    attempts++;
                    return Task.FromResult(LockScreenTransactionResult.PendingVerification);
                },
                CancellationToken.None,
                delay: clock.Delay,
                now: clock.Read,
                restoreDeadline: () => deadline,
                progress: progress.Add);

            Assert(readiness == LockScreenUpdateReadiness.TimedOut && attempts == 12 && clock.Delays == 12,
                "An unresolved journal must stop after twelve attempts and never be treated as ready.");
            Assert(progress.Count > 0 && progress[^1].RemainingSeconds == 0 &&
                   progress.All(item => item.RestoreAtUtc == deadline),
                "Progress must end at zero and carry the journal deadline as information only.");
        }

        {
            var clock = new FakeClock(start);
            int attempts = 0;
            LockScreenUpdateReadiness readiness = await UpdateRecoveryPolicy.WaitForRecoveryAsync(
                _ =>
                {
                    attempts++;
                    if (attempts == 1) throw new IOException("transient restore failure");
                    return Task.FromResult(LockScreenTransactionResult.Restored);
                },
                CancellationToken.None,
                delay: clock.Delay,
                now: clock.Read);

            Assert(readiness == LockScreenUpdateReadiness.Ready && attempts == 2 && clock.Delays == 1,
                "A transient restore exception must be retried instead of aborting the update immediately.");
        }

        {
            var clock = new FakeClock(start);
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            int attempts = 0;
            bool cancelled = false;
            try
            {
                await UpdateRecoveryPolicy.WaitForRecoveryAsync(
                    _ =>
                    {
                        attempts++;
                        return Task.FromResult(LockScreenTransactionResult.Restored);
                    },
                    cts.Token,
                    delay: clock.Delay,
                    now: clock.Read);
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            Assert(cancelled && attempts == 0, "Closing the window must stop the wait before another restore attempt.");
        }

        Console.WriteLine("PASS: update preflight waits at most 60 seconds, retries every five seconds, keeps the journal on timeout, and honors cancellation.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("GAGAL: " + message);
    }

    /// <summary>Deterministic clock and delay used by the policy cases.</summary>
    private sealed class FakeClock(DateTimeOffset start)
    {
        private DateTimeOffset _now = start;

        internal int Delays { get; private set; }

        internal List<TimeSpan> Waits { get; } = [];

        internal DateTimeOffset Read() => _now;

        internal Func<TimeSpan, CancellationToken, Task> Delay => (wait, token) =>
        {
            token.ThrowIfCancellationRequested();
            Delays++;
            Waits.Add(wait);
            _now += wait;
            return Task.CompletedTask;
        };
    }
}
