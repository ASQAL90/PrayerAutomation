using System.Runtime.InteropServices;
using PrayerAutomation.UI;
using static Program;

internal static class NativeChecks
{
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
[StructLayout(LayoutKind.Sequential, Pack=4)]
internal struct ReferenceReason32 { internal uint Version, Flags, Module, Id, Count, Strings; }
[StructLayout(LayoutKind.Sequential, Pack=8)]
internal struct ReferenceReason64 { internal uint Version, Flags; internal ulong Module; internal uint Id, Count; internal ulong Strings; }
internal sealed class FakePower : ILockScreenPowerNative
{
    internal int Creates, Sets, Clears, Closes, RequestType, ClearType;
    internal uint Version, Flags; internal bool ReasonValid; internal string Failure = "";
    public nint Create(ref PowerRequestReasonContext reason,out int error)
    {
        Creates++;Version=reason.Version;Flags=reason.Flags;
        ReasonValid = Marshal.PtrToStringUni(reason.Reason.SimpleReasonString)?.Contains("Pemulihan") == true;
        error = Failure == "create" ? 5 : 0;
        if(Failure == "throw-create") throw new EntryPointNotFoundException("private");
        return Failure == "create" ? -1 : 123;
    }
    public bool Set(nint handle,int type,out int error)
    { Sets++;RequestType=type;error=Failure=="set"?50:0;if(Failure=="throw-set")throw new EntryPointNotFoundException("private");return Failure!="set"; }
    public bool Clear(nint handle,int type,out int error)
    { Clears++;ClearType=type;error=Failure=="clear"?5:0;if(Failure=="throw-clear")throw new EntryPointNotFoundException("private");return Failure!="clear"; }
    public bool Close(nint handle) { Closes++; return true; }
}
internal sealed class ManualClock(DateTimeOffset now) : TimeProvider
{
    private DateTimeOffset _now = now;
    internal TimeSpan Elapsed;
    internal DateTimeOffset Now { get => _now; set { if (value > _now) Elapsed += value - _now; _now = value; } }
    internal void JumpUtc(DateTimeOffset value) => _now = value;
    internal void Advance(TimeSpan span) { Elapsed += span; _now += span; }
    internal ManualTimer? Timer;
    public override DateTimeOffset GetUtcNow() => Now;
    public override ITimer CreateTimer(TimerCallback callback,object? state,TimeSpan dueTime,TimeSpan period)
    { Timer = new(this,callback,state); Timer.Change(dueTime,period); return Timer; }
    internal void Fire() { if (Timer is { } timer && Elapsed >= timer.DueAt) timer.Callback(timer.State); }
    internal sealed class ManualTimer(ManualClock clock,TimerCallback callback,object? state) : ITimer
    {
        internal readonly TimerCallback Callback=callback;internal readonly object? State=state;
        internal TimeSpan DueAt;
        public bool Change(TimeSpan dueTime,TimeSpan period)
        { DueAt = dueTime == Timeout.InfiniteTimeSpan ? TimeSpan.MaxValue : clock.Elapsed + dueTime; return true; }
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
