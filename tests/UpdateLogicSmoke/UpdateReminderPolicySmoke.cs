// Smoke test for the deferred-update reminder timing shown in the tray.
using PrayerAutomation.UI;

namespace UpdateLogicSmoke;

/// <summary>Verifies that the reminder follows lock-screen recovery and falls back safely.</summary>
internal static class UpdateReminderPolicySmoke
{
    /// <summary>Runs every reminder timing case.</summary>
    internal static void Verify()
    {
        DateTimeOffset now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);
        DateTimeOffset deadline = now.AddMinutes(20);

        Assert(UpdateReminderPolicy.ResolveDue(now, false, false, null) ==
               now + UpdateReminderPolicy.FallbackDelay,
            "Without recovery, the reminder must use the documented fallback delay.");
        Assert(UpdateReminderPolicy.ResolveDue(now, true, false, deadline) == deadline,
            "An active image must count down to its journal deadline.");
        Assert(UpdateReminderPolicy.ResolveDue(now, false, true, deadline) ==
               now + UpdateReminderPolicy.RecoveryPollDelay &&
               UpdateReminderPolicy.ResolveDue(now, true, true, deadline) ==
               now + UpdateReminderPolicy.RecoveryPollDelay,
            "A pending restore must be polled, not delayed to a deadline that has already been requested.");
        Assert(UpdateReminderPolicy.ResolveDue(now, true, false, now.AddMinutes(-1)) ==
               now + UpdateReminderPolicy.RecoveryPollDelay &&
               UpdateReminderPolicy.ResolveDue(now, false, true, null) ==
               now + UpdateReminderPolicy.RecoveryPollDelay,
            "A passed or unavailable deadline must poll briefly, not pop the reminder early.");
        Assert(UpdateReminderPolicy.FormatRemaining(TimeSpan.FromSeconds(65)) == "01:05" &&
               UpdateReminderPolicy.FormatRemaining(TimeSpan.FromMinutes(62)) == "1j 02m" &&
               UpdateReminderPolicy.FormatRemaining(TimeSpan.FromSeconds(-1)) == "00:00",
            "The tray countdown must stay short, readable, and non-negative.");

        Console.WriteLine("PASS: deferred-update reminder follows the lock-screen deadline, polls when it is unavailable, and formats a safe tray countdown.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("GAGAL: " + message);
    }
}
