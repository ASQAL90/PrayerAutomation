namespace PrayerAutomation.UI;
// Only compiled on the prepatch baseline; runtime assertion still exercises the real scheduler.
internal interface ILockScreenExecutionRequest : IDisposable
{
    void RequestUntil(DateTimeOffset restoreAtUtc);
    void Clear();
}
