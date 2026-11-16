namespace PrayerAutomation.UI;

internal sealed record TaskbarWidgetAvailability(bool CanEnable, int OsBuild, string Message);

internal static class TaskbarWidgetSupport
{
    public static TaskbarWidgetAvailability Inspect() => Evaluate(Environment.OSVersion.Version);

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

