// Evaluates whether the taskbar widget is supported on the current Windows version.
namespace PrayerAutomation.UI;

/// <summary>Availability of the taskbar widget on the running OS.</summary>
/// <param name="CanEnable">True when the taskbar widget can be enabled.</param>
/// <param name="OsBuild">Build number of the OS.</param>
/// <param name="Message">User-facing explanation of the availability.</param>
internal sealed record TaskbarWidgetAvailability(bool CanEnable, int OsBuild, string Message);

/// <summary>Checks Windows version requirements for the taskbar widget.</summary>
internal static class TaskbarWidgetSupport
{
    /// <summary>Inspects the current OS to determine taskbar widget availability.</summary>
    /// <returns>The availability result for this machine.</returns>
    public static TaskbarWidgetAvailability Inspect() => Evaluate(Environment.OSVersion.Version);

    /// <summary>Evaluates taskbar widget availability for a specific OS version.</summary>
    /// <param name="version">OS version to evaluate.</param>
    /// <returns>The availability result for the given version.</returns>
    internal static TaskbarWidgetAvailability Evaluate(Version version)
    {
        bool knownWindows = version.Major == 10 && version.Minor == 0;
        bool supported = knownWindows && version.Build >= 19041;
        string os = knownWindows ? version.Build >= 22000 ? "Windows 11" : "Windows 10"
            : version.Major < 10 ? "Windows versi lama" : "Versi Windows belum dikenali";
        string note = supported
            ? "Panel mengikuti ruang kosong taskbar di semua sisi. Saat sempit, panel diringkas; saat penuh, disembunyikan. Klik membuka Detail. Centang lalu Simpan."
            : "Memerlukan Windows 10 build 19041 atau lebih baru. Gunakan widget desktop.";
        return new TaskbarWidgetAvailability(supported, version.Build,
            $"{os}. {note} Tanpa admin; pengaturan Windows tidak diubah.");
    }
}

