using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace PrayerAutomation.UI;

/// <summary>Requests Windows execution-required power state for the temporary lock-screen period.</summary>
internal interface ILockScreenExecutionRequest : IDisposable
{
    /// <summary>Requests execution-required power state until the given UTC deadline.</summary>
    /// <param name="restoreAtUtc">UTC time after which the request is no longer needed.</param>
    void RequestUntil(DateTimeOffset restoreAtUtc);
    /// <summary>Releases any active execution-required request and resets the scenario.</summary>
    void Clear();
}

// A request, not a guarantee of execution or a displayed-image refresh. Windows policy,
// DC Modern Standby limits and user-initiated sleep still apply. Never request DisplayRequired.
/// <summary>Windows implementation of <see cref="ILockScreenExecutionRequest"/> using power requests; never requests DisplayRequired.</summary>
internal sealed class WindowsLockScreenExecutionRequest : ILockScreenExecutionRequest
{
    /// <summary>Extra lease beyond the restore deadline before the watchdog expires the request.</summary>
    internal static readonly TimeSpan RetryGrace = TimeSpan.FromSeconds(30);
    /// <summary>PowerRequestType value for ExecutionRequired.</summary>
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

    /// <summary>Initializes the request helper; no native calls are made until the first request.</summary>
    /// <param name="trace">Optional trace sink for timeline events.</param>
    /// <param name="native">Optional native interop override (for testing).</param>
    /// <param name="clock">Optional time provider (for testing).</param>
    internal WindowsLockScreenExecutionRequest(Action<string, string>? trace = null,
        ILockScreenPowerNative? native = null, TimeProvider? clock = null)
    {
        _trace = trace;
        _native = native ?? new WindowsLockScreenPowerNative(); // No native calls until RequestUntil.
        _clock = clock ?? TimeProvider.System;
    }

    /// <summary>Requests or renews the execution-required power request until the given deadline.</summary>
    /// <param name="restoreAtUtc">UTC restore deadline; a watchdog releases the request after RetryGrace past it.</param>
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

    /// <summary>Releases the power request and resets failure/expiry state.</summary>
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

    /// <summary>Releases the power request and removes the watchdog timer.</summary>
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

    /// <summary>Watchdog callback; expires the scenario if the generation still matches.</summary>
    /// <param name="generation">Watchdog generation captured when the timer was armed.</param>
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

    /// <summary>Disposes the watchdog timer and power handle; caller must hold the sync lock.</summary>
    private void ReleaseUnderLock()
    {
        _watchdogGeneration++;
        _watchdog?.Dispose();
        _watchdog = null;
        _handle?.Dispose();
        _handle = null;
    }

    /// <summary>Emits a privacy-safe trace entry; failures are swallowed.</summary>
    /// <param name="action">Trace action name.</param>
    /// <param name="detail">Trace detail payload.</param>
    private void Trace(string action, string detail)
    {
        try { _trace?.Invoke("lock-screen.power." + action, $"utc={_clock.GetUtcNow():O} {detail}"); }
        catch (Exception) { }
    }

    /// <summary>Safe handle wrapper for a Windows power request; clears the request on release.</summary>
    private sealed class PowerRequestHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private readonly ILockScreenPowerNative _native;
        private readonly Action<string,string>? _trace;
        /// <summary>True once the power request has been set; used to clear it on release.</summary>
        internal bool Requested;
        /// <summary>Initializes the wrapper around a native power-request handle.</summary>
        /// <param name="value">Native handle value.</param>
        /// <param name="native">Native interop used to clear/close the request.</param>
        /// <param name="trace">Optional trace sink.</param>
        internal PowerRequestHandle(nint value, ILockScreenPowerNative native, Action<string,string>? trace) : base(true)
        { SetHandle(value); _native = native; _trace = trace; }
        /// <summary>Clears the power request if set, then closes the handle.</summary>
        /// <returns>Always true; release failures are traced and swallowed.</returns>
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
/// <summary>Full REASON_CONTEXT structure for PowerCreateRequest.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct PowerRequestReasonContext
{
    internal uint Version;
    internal uint Flags;
    internal PowerRequestReasonUnion Reason;
}
/// <summary>Union of simple-string and detailed power request reasons.</summary>
[StructLayout(LayoutKind.Explicit)]
internal struct PowerRequestReasonUnion
{
    [FieldOffset(0)] internal nint SimpleReasonString;
    [FieldOffset(0)] internal PowerRequestDetailedReason Detailed;
}
/// <summary>Detailed reason payload for the reason union (module, ID, strings).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct PowerRequestDetailedReason
{
    internal nint LocalizedReasonModule;
    internal uint LocalizedReasonId;
    internal uint ReasonStringCount;
    internal nint ReasonStrings;
}

/// <summary>Native interop abstraction for Windows power requests.</summary>
internal interface ILockScreenPowerNative
{
    /// <summary>Creates a power request from a reason context.</summary>
    /// <param name="reason">Reason context to pass to the API.</param>
    /// <param name="error">Last Win32 error when creation fails.</param>
    /// <returns>The native handle, or 0/-1 on failure.</returns>
    nint Create(ref PowerRequestReasonContext reason, out int error);
    /// <summary>Sets a power request type on a handle.</summary>
    /// <param name="handle">Power request handle.</param>
    /// <param name="requestType">PowerRequestType value.</param>
    /// <param name="error">Last Win32 error on failure.</param>
    /// <returns>True on success.</returns>
    bool Set(nint handle, int requestType, out int error);
    /// <summary>Clears a power request type from a handle.</summary>
    /// <param name="handle">Power request handle.</param>
    /// <param name="requestType">PowerRequestType value.</param>
    /// <param name="error">Last Win32 error on failure.</param>
    /// <returns>True on success.</returns>
    bool Clear(nint handle, int requestType, out int error);
    /// <summary>Closes a power request handle.</summary>
    /// <param name="handle">Power request handle.</param>
    /// <returns>True on success.</returns>
    bool Close(nint handle);
}

/// <summary>Default <see cref="ILockScreenPowerNative"/> implementation using kernel32 P/Invoke.</summary>
internal sealed class WindowsLockScreenPowerNative : ILockScreenPowerNative
{
    /// <inheritdoc/>
    public nint Create(ref PowerRequestReasonContext reason, out int error)
    { nint result = PowerCreateRequest(ref reason); error = result == -1 || result == 0 ? Marshal.GetLastWin32Error() : 0; return result; }
    /// <inheritdoc/>
    public bool Set(nint handle, int requestType, out int error)
    { bool result = PowerSetRequest(handle, requestType); error = result ? 0 : Marshal.GetLastWin32Error(); return result; }
    /// <inheritdoc/>
    public bool Clear(nint handle, int requestType, out int error)
    { bool result = PowerClearRequest(handle, requestType); error = result ? 0 : Marshal.GetLastWin32Error(); return result; }
    /// <inheritdoc/>
    public bool Close(nint handle) => CloseHandle(handle);

    /// <summary>Creates a power request (kernel32).</summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint PowerCreateRequest(ref PowerRequestReasonContext context);
    /// <summary>Sets a power request type (kernel32).</summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PowerSetRequest(nint handle, int requestType);
    /// <summary>Clears a power request type (kernel32).</summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PowerClearRequest(nint handle, int requestType);
    /// <summary>Closes a kernel handle.</summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);
}
