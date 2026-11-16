using System.Globalization;

namespace PrayerAutomation.UI;

internal enum DeviceLocationSource
{
    Satellite,
    WiFi,
    Cellular,
    IPAddress,
    Default,
    Unknown
}

internal enum DeviceLocationStatus
{
    Success,
    PermissionDenied,
    PermissionUnspecified,
    TimedOut,
    InvalidPosition,
    Unavailable
}

internal sealed record DeviceLocationResult(
    DeviceLocationStatus Status,
    double? Latitude = null,
    double? Longitude = null,
    double? AccuracyMeters = null,
    DeviceLocationSource Source = DeviceLocationSource.Unknown)
{
    public bool IsSuccess => Status == DeviceLocationStatus.Success &&
        Latitude.HasValue && Longitude.HasValue;
}

internal static class DeviceLocationLogic
{
    public static bool HasValidCoordinates(double latitude, double longitude) =>
        double.IsFinite(latitude) && latitude is >= -90 and <= 90 &&
        double.IsFinite(longitude) && longitude is >= -180 and <= 180;

    public static bool HasValidAccuracy(double accuracyMeters) =>
        double.IsFinite(accuracyMeters) && accuracyMeters >= 0;

    public static string SourceLabel(DeviceLocationSource source) => source switch
    {
        DeviceLocationSource.Satellite => "GPS/GNSS (satelit)",
        DeviceLocationSource.WiFi => "Wi-Fi (bukan GPS)",
        DeviceLocationSource.Cellular => "jaringan seluler (bukan GPS)",
        DeviceLocationSource.IPAddress => "alamat IP (perkiraan; bukan GPS)",
        DeviceLocationSource.Default => "lokasi default Windows (sumber tidak dipastikan)",
        _ => "sumber tidak diketahui (bukan GPS terverifikasi)"
    };

    public static string AccuracyLabel(double? accuracyMeters) =>
        accuracyMeters is double value && HasValidAccuracy(value)
            ? $"akurasi dilaporkan Windows ±{value.ToString("0", CultureInfo.InvariantCulture)} m"
            : "akurasi tidak dilaporkan Windows";

    public static string FormatLocalLocationName(DeviceLocationSource source, double latitude, double longitude)
    {
        if (!HasValidCoordinates(latitude, longitude))
            throw new ArgumentOutOfRangeException(nameof(latitude), "Koordinat lokasi perangkat tidak valid.");

        string lat = latitude.ToString("0.####", CultureInfo.InvariantCulture);
        string lon = longitude.ToString("0.####", CultureInfo.InvariantCulture);
        return $"Lokasi perangkat ({SourceLabel(source)}) — {lat}, {lon}";
    }
}
