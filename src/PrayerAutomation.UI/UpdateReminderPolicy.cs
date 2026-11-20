using System;

namespace PrayerAutomation.UI;

/// <summary>Pure timing rules for the deferred-update reminder.</summary>
internal static class UpdateReminderPolicy
{
    /// <summary>Reminder delay used when no lock-screen recovery is active.</summary>
    internal static readonly TimeSpan FallbackDelay = TimeSpan.FromMinutes(15);

    /// <summary>Short delay used when the journal exists but its deadline is unavailable or already passed.</summary>
    internal static readonly TimeSpan RecoveryPollDelay = TimeSpan.FromSeconds(5);

    /// <summary>Chooses the reminder time from the lock-screen recovery state.</summary>
    /// <param name="now">Current UTC time.</param>
    /// <param name="hasActiveImage">True while a temporary image is still inside its hold window.</param>
    /// <param name="hasPendingRecovery">True while the restore journal still exists.</param>
    /// <param name="deadline">Journal restore deadline, when readable.</param>
    /// <returns>UTC time when the reminder may be shown.</returns>
    internal static DateTimeOffset ResolveDue(
        DateTimeOffset now,
        bool hasActiveImage,
        bool hasPendingRecovery,
        DateTimeOffset? deadline)
    {
        if (hasPendingRecovery) return now + RecoveryPollDelay;
        if (hasActiveImage)
            return deadline is { } value && value > now ? value : now + RecoveryPollDelay;
        return now + FallbackDelay;
    }

    /// <summary>Formats a short countdown for the tray tooltip.</summary>
    /// <param name="remaining">Time left.</param>
    /// <returns>Text such as 14:05 or 1j 02m.</returns>
    internal static string FormatRemaining(TimeSpan remaining)
    {
        if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
        return remaining.TotalHours >= 1
            ? $"{(int)remaining.TotalHours}j {remaining.Minutes:00}m"
            : $"{remaining.Minutes:00}:{remaining.Seconds:00}";
    }
}
