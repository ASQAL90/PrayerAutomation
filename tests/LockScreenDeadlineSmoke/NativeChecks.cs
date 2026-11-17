// Smoke test verifying execution-lease ABI layout, bounded watchdog behavior, and failure memoization through fake native power APIs.
using System.Runtime.InteropServices;
using PrayerAutomation.UI;
using static Program;

/// <summary>Verifies execution-request power lease ABI marshaling, acquire/extend/clear lifecycle, and failure handling.</summary>
internal static class NativeChecks
{
    /// <summary>Runs the native lease ABI and lifecycle checks against the fake clock and power API.</summary>
    internal static void Run()
    {
        Check(Marshal.SizeOf<PowerRequestReasonContext>() == (IntPtr.Size == 8 ? 32 : 24), "full REASON_CONTEXT size matches host ABI");
        Check(Marshal.OffsetOf<PowerRequestReasonContext>("Reason").ToInt32() == 8, "reason union/simple string offset is 8");
        Check(Marshal.SizeOf<PowerRequestDetailedReason>() == (IntPtr.Size == 8 ? 24 : 16), "detailed union includes module, two ULONGs and string-array pointer");
        Check(Marshal.SizeOf<ReferenceReason32>() == 24 && Marshal.OffsetOf<ReferenceReason32>("Module").ToInt32() == 8, "documented x86 full-union reference layout is 24 bytes, offset 8 (modeled)");
        Check(Marshal.SizeOf<ReferenceReason64>() == 32 && Marshal.OffsetOf<ReferenceReason64>("Module").ToInt32() == 8, "documented x64 full-union reference layout is 32 bytes, offset 8 (modeled)");
        var clock = new ManualClock(Epoch); var native = new FakePower(); var traces = new List<string>();
        using (var request = new WindowsLockScreenExecutionRequest((context,text) => traces.Add(context+" "+text), native,clock))
        {
            Check(native.Creates == 0, "constructing Windows lease performs NO native calls");
            request.RequestUntil(Epoch.AddMinutes(3));
            Check(native.Creates == 1 && native.Sets == 1 && native.RequestType == 3, "only ExecutionRequired=3 is set once");
            Check(native.Version == 0 && native.Flags == 1 && native.ReasonValid, "native seam sees valid marshaled localized simple reason");
            for (int i = 0; i < 30; i++) request.RequestUntil(Epoch.AddMinutes(3));
            Check(native.Sets == 1, "healthy requests never increment native count");
            var oldCallback = clock.Timer!.Callback;
            request.RequestUntil(Epoch.AddMinutes(4));
            clock.Now = Epoch.AddMinutes(3).AddSeconds(30); oldCallback(null);
            Check(native.Clears == 0, "queued OLD watchdog callback cannot clear extended lease early");
            clock.Now = Epoch.AddMinutes(4).AddSeconds(29); clock.Fire();
            Check(native.Clears == 0, "lease expires only after actual deadline plus 30-second grace");
            clock.Now = Epoch.AddMinutes(4).AddSeconds(30); clock.Fire();
            Check(native.Clears == 1 && native.Closes == 1 && native.ClearType == 3, "bounded watchdog clears exact request and closes handle");
            request.RequestUntil(Epoch.AddMinutes(4)); clock.Fire();
            Check(native.Creates == 1 && native.Sets == 1 && native.Clears == 1, "same expired scenario cannot renew or reacquire");
            Check(traces.Any(t => t.Contains("requested=ExecutionRequired")) && traces.Any(t => t.Contains("expiry")) && traces.Any(t => t.Contains("clear")), "acquire/extend/expiry/clear telemetry uses requested wording");
        }
        foreach (string mode in new[] {"create", "set", "clear", "throw-create", "throw-set", "throw-clear"})
        {
            native = new FakePower { Failure = mode }; clock = new ManualClock(Epoch); traces = new();
            using (var request = new WindowsLockScreenExecutionRequest((context,text) => traces.Add(context+" "+text),native,clock))
            {
                request.RequestUntil(Epoch.AddMinutes(3)); request.RequestUntil(Epoch.AddMinutes(3)); request.RequestUntil(Epoch.AddMinutes(4));
                if (mode.Contains("create")) Check(native.Creates == 1 && native.Sets == 0, mode + " failure memoized; no tick retry");
                else if (mode.Contains("set")) Check(native.Creates == 1 && native.Sets == 1 && native.Closes == 1 && native.Clears == 0, mode + " failure closes unset handle without clear/renew");
                else
                {
                    request.Clear();
                    Check(native.Clears == 1 && native.Closes == 1, mode + " failure still closes handle");
                    Check(traces.Any(t => t.Contains("clear") && t.Contains("failure")), mode + " clear failure is diagnosed without raw exception");
                }
                Check(traces.Count <= 4, mode + " diagnostic failures bounded");
            }
        }
        native = new(); clock = new(Epoch);
        var disposed = new WindowsLockScreenExecutionRequest((_,_) => throw new InvalidOperationException("private"),native,clock);
        disposed.RequestUntil(Epoch.AddMinutes(3)); disposed.Dispose(); clock.Now = Epoch.AddMinutes(5); clock.Fire();
        disposed.RequestUntil(Epoch.AddMinutes(10)); disposed.Dispose();
        Check(native.Creates == 1 && native.Sets == 1 && native.Clears == 1 && native.Closes == 1, "Dispose/queued expiry idempotent; throwing telemetry nonfatal; no resurrection");
        native = new(); clock = new(Epoch);
        using (var request = new WindowsLockScreenExecutionRequest(native:native,clock:clock))
        {
            request.RequestUntil(Epoch.AddMinutes(3));
            clock.JumpUtc(Epoch.AddDays(-1));
            clock.Advance(TimeSpan.FromSeconds(210)); clock.Fire();
            Check(native.Clears == 1 && native.Closes == 1, "watchdog remains bounded after backward wall-clock adjustment");
            request.RequestUntil(Epoch.AddMinutes(4));
            Console.WriteLine($"F2 afterExpiryWithoutClear creates={native.Creates} sets={native.Sets} clears={native.Clears} closes={native.Closes}");
            Check(native.Creates == 1 && native.Sets == 1, "F2 higher deadline cannot reacquire expired scenario without Clear");
            request.Clear();
            request.RequestUntil(Epoch.AddMinutes(5));
            Check(native.Creates == 2 && native.Sets == 2, "F2 explicit Clear permits a future request in a new scenario");
            request.Dispose();
            Check(native.Clears == 2 && native.Closes == 2, "F2 new scenario remains bounded by ordinary Dispose");
        }
    }
}
/// <summary>Modeled x86 layout of the documented full REASON_CONTEXT union reference.</summary>
[StructLayout(LayoutKind.Sequential, Pack=4)]
internal struct ReferenceReason32 { internal uint Version, Flags, Module, Id, Count, Strings; }
/// <summary>Modeled x64 layout of the documented full REASON_CONTEXT union reference.</summary>
[StructLayout(LayoutKind.Sequential, Pack=8)]
internal struct ReferenceReason64 { internal uint Version, Flags; internal ulong Module; internal uint Id, Count; internal ulong Strings; }
/// <summary>Fake <see cref="ILockScreenPowerNative"/> recording lease operations and optionally injecting failures.</summary>
internal sealed class FakePower : ILockScreenPowerNative
{
    internal int Creates, Sets, Clears, Closes, RequestType, ClearType;
    internal uint Version, Flags; internal bool ReasonValid; internal string Failure = "";
    /// <summary>Simulates CreatePowerRequest, honoring the configured failure mode.</summary>
    /// <param name="reason">Reason context to marshal and record.</param>
    /// <param name="error">Win32-style error code output.</param>
    /// <returns>A fake handle, or -1 when the configured failure is "create".</returns>
    public nint Create(ref PowerRequestReasonContext reason,out int error)
    {
        Creates++;Version=reason.Version;Flags=reason.Flags;
        ReasonValid = Marshal.PtrToStringUni(reason.Reason.SimpleReasonString)?.Contains("Pemulihan") == true;
        error = Failure == "create" ? 5 : 0;
        if(Failure == "throw-create") throw new EntryPointNotFoundException("private");
        return Failure == "create" ? -1 : 123;
    }
    /// <summary>Simulates SetThreadExecutionState-style lease acquisition, honoring the configured failure mode.</summary>
    /// <param name="handle">Handle returned by <see cref="Create"/>.</param>
    /// <param name="type">Request type (3 = ExecutionRequired).</param>
    /// <param name="error">Win32-style error code output.</param>
    /// <returns>True on success; false when the configured failure is "set".</returns>
    public bool Set(nint handle,int type,out int error)
    { Sets++;RequestType=type;error=Failure=="set"?50:0;if(Failure=="throw-set")throw new EntryPointNotFoundException("private");return Failure!="set"; }
    /// <summary>Simulates ClearThreadExecutionState-style lease release, honoring the configured failure mode.</summary>
    /// <param name="handle">Handle to clear.</param>
    /// <param name="type">Request type to clear.</param>
    /// <param name="error">Win32-style error code output.</param>
    /// <returns>True on success; false when the configured failure is "clear".</returns>
    public bool Clear(nint handle,int type,out int error)
    { Clears++;ClearType=type;error=Failure=="clear"?5:0;if(Failure=="throw-clear")throw new EntryPointNotFoundException("private");return Failure!="clear"; }
    /// <summary>Simulates closing the power request handle.</summary>
    /// <param name="handle">Handle to close.</param>
    /// <returns>Always true.</returns>
    public bool Close(nint handle) { Closes++; return true; }
}
/// <summary>Manual <see cref="TimeProvider"/> for deterministic lease watchdog testing.</summary>
/// <param name="now">Initial time.</param>
internal sealed class ManualClock(DateTimeOffset now) : TimeProvider
{
    private DateTimeOffset _now = now;
    internal TimeSpan Elapsed;
    /// <summary>Gets or sets the current time; forward movement accumulates <see cref="Elapsed"/>.</summary>
    internal DateTimeOffset Now { get => _now; set { if (value > _now) Elapsed += value - _now; _now = value; } }
    /// <summary>Jumps the clock to an absolute time without accumulating elapsed time.</summary>
    /// <param name="value">New absolute time.</param>
    internal void JumpUtc(DateTimeOffset value) => _now = value;
    /// <summary>Advances the clock by a span, accumulating elapsed time.</summary>
    /// <param name="span">Amount to advance.</param>
    internal void Advance(TimeSpan span) { Elapsed += span; _now += span; }
    internal ManualTimer? Timer;
    /// <summary>Returns the current manual time.</summary>
    /// <returns>The current manual time.</returns>
    public override DateTimeOffset GetUtcNow() => Now;
    /// <summary>Creates a manual timer bound to this clock.</summary>
    /// <param name="callback">Timer callback.</param>
    /// <param name="state">State passed to the callback.</param>
    /// <param name="dueTime">Due time for the first tick.</param>
    /// <param name="period">Period between ticks.</param>
    /// <returns>The created manual timer.</returns>
    public override ITimer CreateTimer(TimerCallback callback,object? state,TimeSpan dueTime,TimeSpan period)
    { Timer = new(this,callback,state); Timer.Change(dueTime,period); return Timer; }
    /// <summary>Fires the pending timer when its due time has been reached.</summary>
    internal void Fire() { if (Timer is { } timer && Elapsed >= timer.DueAt) timer.Callback(timer.State); }
    /// <summary>Manual <see cref="ITimer"/> driven by <see cref="ManualClock.Elapsed"/>.</summary>
    /// <param name="clock">Owning clock.</param>
    /// <param name="callback">Timer callback.</param>
    /// <param name="state">State passed to the callback.</param>
    internal sealed class ManualTimer(ManualClock clock,TimerCallback callback,object? state) : ITimer
    {
        internal readonly TimerCallback Callback=callback;internal readonly object? State=state;
        internal TimeSpan DueAt;
        /// <summary>Schedules the timer relative to the owning clock's elapsed time.</summary>
        /// <param name="dueTime">Due time, or <see cref="Timeout.InfiniteTimeSpan"/> to disable.</param>
        /// <param name="period">Period between ticks.</param>
        /// <returns>Always true.</returns>
        public bool Change(TimeSpan dueTime,TimeSpan period)
        { DueAt = dueTime == Timeout.InfiniteTimeSpan ? TimeSpan.MaxValue : clock.Elapsed + dueTime; return true; }
        /// <summary>No-op disposal for the manual timer.</summary>
        public void Dispose() { }
        /// <summary>Asynchronously disposes the timer; always completes.</summary>
        /// <returns>A completed value task.</returns>
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
