using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace PrayerAutomation.UI;

internal interface ILockScreenExecutionRequest : IDisposable
{
    void RequestUntil(DateTimeOffset restoreAtUtc);
    void Clear();
}

// A request, not a guarantee of execution or a displayed-image refresh. Windows policy,
// DC Modern Standby limits and user-initiated sleep still apply. Never request DisplayRequired.
internal sealed class WindowsLockScreenExecutionRequest : ILockScreenExecutionRequest
{
    internal static readonly TimeSpan RetryGrace = TimeSpan.FromSeconds(30);
    internal const int ExecutionRequired = 3;
    private readonly object _sync = new();
    private readonly ILockScreenPowerNative _native;
    private readonly TimeProvider _clock;
    private readonly Action<string, string>? _trace;
    private PowerRequestHandle? _handle;
    private ITimer? _watchdog;
    private long _watchdogGeneration;
    private DateTimeOffset? _deadline;
    private bool _failed;
    private bool _expired;
    private bool _disposed;

    internal WindowsLockScreenExecutionRequest(Action<string, string>? trace = null,
        ILockScreenPowerNative? native = null, TimeProvider? clock = null)
    {
        _trace = trace;
        _native = native ?? new WindowsLockScreenPowerNative(); // No native calls until RequestUntil.
        _clock = clock ?? TimeProvider.System;
    }

    public void RequestUntil(DateTimeOffset restoreAtUtc)
    {
        lock (_sync)
        {
            if (_disposed || _failed || _expired || _deadline == restoreAtUtc) return;
            DateTimeOffset expiresAt = restoreAtUtc.Add(RetryGrace);
            if (expiresAt <= _clock.GetUtcNow()) return;
            try
            {
                if (_handle is null)
                {
                    // Do not renew an expired scenario on resumed/healthy ticks.
                    if (_deadline.HasValue && restoreAtUtc <= _deadline.Value) return;
                    nint text = Marshal.StringToHGlobalUni("Pemulihan gambar lock screen sementara pada batas waktu");
                    nint raw;
                    int error;
                    try
                    {
                        var reason = new PowerRequestReasonContext
                        {
                            Version = 0,
                            Flags = 1,
                            Reason = new PowerRequestReasonUnion { SimpleReasonString = text }
                        };
                        raw = _native.Create(ref reason, out error);
                    }
                    finally { Marshal.FreeHGlobal(text); }
                    if (raw == 0 || raw == -1)
                    {
                        _failed = true;
                        Trace("create-failure", $"win32={error}");
                        return;
                    }
                    _handle = new PowerRequestHandle(raw, _native, _trace);
                    if (!_native.Set(raw, ExecutionRequired, out error))
                    {
                        _failed = true;
                        Trace("set-failure", $"win32={error}");
                        _handle.Dispose();
                        _handle = null;
                        return;
                    }
                    _handle.Requested = true;
                    Trace("acquire", $"requested=ExecutionRequired deadlineUtc={restoreAtUtc:O} expiresUtc={expiresAt:O}");
                }
                else if (_deadline != restoreAtUtc)
                {
                    Trace("extend", $"requested=ExecutionRequired deadlineUtc={restoreAtUtc:O} expiresUtc={expiresAt:O}");
                }
                _deadline = restoreAtUtc;
                long generation = ++_watchdogGeneration;
                _watchdog?.Dispose();
                // One-shot relative timer bounds the lease even if the UTC clock moves backwards.
                _watchdog = _clock.CreateTimer(_ => Expire(generation), null,
                    expiresAt - _clock.GetUtcNow(), Timeout.InfiniteTimeSpan);
            }
            catch (Exception)
            {
                // Optional power support must never obstruct startup, image rollback or restoration.
                _failed = true;
                ReleaseUnderLock();
                Trace("request-failure", "category=unavailable");
            }
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            ReleaseUnderLock();
            _deadline = null;
            _failed = false;
            _expired = false;
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            ReleaseUnderLock();
            _watchdog?.Dispose();
            _watchdog = null;
        }
    }

    private void Expire(long generation)
    {
        lock (_sync)
        {
            if (_disposed || generation != _watchdogGeneration || !_deadline.HasValue || _handle is null) return;
            _expired = true; // Only Clear ends this scenario; a later deadline cannot revive it.
            Trace("expiry", $"deadlineUtc={_deadline:O} visual=unverified");
            ReleaseUnderLock(); // Keep the deadline: no tick-driven renewal after expiry.
        }
    }

    private void ReleaseUnderLock()
    {
        _watchdogGeneration++;
        _watchdog?.Dispose();
        _watchdog = null;
        _handle?.Dispose();
        _handle = null;
    }

    private void Trace(string action, string detail)
    {
        try { _trace?.Invoke("lock-screen.power." + action, $"utc={_clock.GetUtcNow():O} {detail}"); }
        catch (Exception) { }
    }

    private sealed class PowerRequestHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private readonly ILockScreenPowerNative _native;
        private readonly Action<string,string>? _trace;
        internal bool Requested;
        internal PowerRequestHandle(nint value, ILockScreenPowerNative native, Action<string,string>? trace) : base(true)
        { SetHandle(value); _native = native; _trace = trace; }
        protected override bool ReleaseHandle()
        {
            try
            {
                if (Requested)
                {
                    bool cleared = _native.Clear(handle, ExecutionRequired, out int error);
                    try { _trace?.Invoke("lock-screen.power.clear", $"utc={DateTimeOffset.UtcNow:O} result={(cleared ? "requested-cleared" : "failure")} win32={error}"); }
                    catch (Exception) { }
                }
            }
            catch (Exception)
            {
                try { _trace?.Invoke("lock-screen.power.clear", $"utc={DateTimeOffset.UtcNow:O} result=failure category=unavailable"); }
                catch (Exception) { }
            }
            finally
            {
                try { _native.Close(handle); }
                catch (Exception) { }
            }
            return true;
        }
    }
}

// Full REASON_CONTEXT union; a simple-string-only struct is undersized on both ABIs.
[StructLayout(LayoutKind.Sequential)]
internal struct PowerRequestReasonContext
{
    internal uint Version;
    internal uint Flags;
    internal PowerRequestReasonUnion Reason;
}
[StructLayout(LayoutKind.Explicit)]
internal struct PowerRequestReasonUnion
{
    [FieldOffset(0)] internal nint SimpleReasonString;
    [FieldOffset(0)] internal PowerRequestDetailedReason Detailed;
}
[StructLayout(LayoutKind.Sequential)]
internal struct PowerRequestDetailedReason
{
    internal nint LocalizedReasonModule;
    internal uint LocalizedReasonId;
    internal uint ReasonStringCount;
    internal nint ReasonStrings;
}

internal interface ILockScreenPowerNative
{
    nint Create(ref PowerRequestReasonContext reason, out int error);
    bool Set(nint handle, int requestType, out int error);
    bool Clear(nint handle, int requestType, out int error);
    bool Close(nint handle);
}

internal sealed class WindowsLockScreenPowerNative : ILockScreenPowerNative
{
    public nint Create(ref PowerRequestReasonContext reason, out int error)
    { nint result = PowerCreateRequest(ref reason); error = result == -1 || result == 0 ? Marshal.GetLastWin32Error() : 0; return result; }
    public bool Set(nint handle, int requestType, out int error)
    { bool result = PowerSetRequest(handle, requestType); error = result ? 0 : Marshal.GetLastWin32Error(); return result; }
    public bool Clear(nint handle, int requestType, out int error)
    { bool result = PowerClearRequest(handle, requestType); error = result ? 0 : Marshal.GetLastWin32Error(); return result; }
    public bool Close(nint handle) => CloseHandle(handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint PowerCreateRequest(ref PowerRequestReasonContext context);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PowerSetRequest(nint handle, int requestType);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PowerClearRequest(nint handle, int requestType);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);
}
