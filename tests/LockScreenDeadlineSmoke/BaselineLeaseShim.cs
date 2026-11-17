namespace PrayerAutomation.UI;
// Only compiled on the prepatch baseline; runtime assertion still exercises the real scheduler.
/// <summary>Execution-request lease contract used by the baseline shim (prepatch only).</summary>
internal interface ILockScreenExecutionRequest : IDisposable
{
    /// <summary>Requests an execution lease until the given UTC deadline.</summary>
    /// <param name="restoreAtUtc">Deadline the lease must cover.</param>
    void RequestUntil(DateTimeOffset restoreAtUtc);
    /// <summary>Clears the execution lease request.</summary>
    void Clear();
}
