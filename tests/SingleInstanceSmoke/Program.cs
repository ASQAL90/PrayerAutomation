// Smoke test verifying the single-instance guard: a second launch exits while the first instance remains running.
using System.Diagnostics;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: SingleInstanceSmoke <path-to-app-exe>");
    return 2;
}

string executable = Path.GetFullPath(args[0]);
if (!File.Exists(executable))
{
    Console.Error.WriteLine("Application executable not found.");
    return 2;
}

Process? first = null;
Process? second = null;
try
{
    first = Start(executable);
    if (!WaitForWindow(first, TimeSpan.FromSeconds(30)))
    {
        Console.Error.WriteLine("FAIL: first instance did not open its main window.");
        return 1;
    }

    second = Start(executable);
    if (!second.WaitForExit(5000))
    {
        Console.Error.WriteLine("FAIL: second instance remained running; single-instance guard is missing.");
        return 1;
    }

    first.Refresh();
    if (first.HasExited)
    {
        Console.Error.WriteLine("FAIL: first instance exited when the second instance launched.");
        return 1;
    }

    Console.WriteLine("PASS: second launch exits while the first instance remains active.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FAIL: {ex.GetType().Name}: {ex.Message}");
    return 1;
}
finally
{
    Terminate(second);
    Terminate(first);
}

/// <summary>Starts the application executable with its own directory as working directory.</summary>
/// <param name="executable">Path to the application executable.</param>
/// <returns>The started process.</returns>
static Process Start(string executable) => Process.Start(new ProcessStartInfo(executable)
{
    WorkingDirectory = Path.GetDirectoryName(executable)!,
    UseShellExecute = true
}) ?? throw new InvalidOperationException("Process.Start returned null.");

/// <summary>Waits until the process shows a main window or the timeout elapses.</summary>
/// <param name="process">Process to poll.</param>
/// <param name="timeout">Maximum wait time.</param>
/// <returns>True when a main-window handle appeared before the deadline.</returns>
static bool WaitForWindow(Process process, TimeSpan timeout)
{
    DateTime deadline = DateTime.UtcNow + timeout;
    while (DateTime.UtcNow < deadline)
    {
        process.Refresh();
        if (process.HasExited) return false;
        if (process.MainWindowHandle != IntPtr.Zero) return true;
        Thread.Sleep(100);
    }
    return false;
}

/// <summary>Best-effort kill of the process tree; ignores processes that already exited.</summary>
/// <param name="process">Process to terminate, or null.</param>
static void Terminate(Process? process)
{
    if (process is null) return;
    try
    {
        process.Refresh();
        if (!process.HasExited) process.Kill(entireProcessTree: true);
        process.WaitForExit(3000);
    }
    catch (InvalidOperationException) { }
    catch (System.ComponentModel.Win32Exception) { }
}
