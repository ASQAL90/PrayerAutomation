// Pure helpers for validating and formatting device-reported location data.
using System.Globalization;

namespace PrayerAutomation.UI;

/// <summary>Source reported by Windows for a device location fix.</summary>
internal enum DeviceLocationSource
{
    Satellite,
    WiFi,
    Cellular,
    IPAddress,
    Default,
    Unknown
}

/// <summary>Outcome status of a device location request.</summary>
internal enum DeviceLocationStatus
{
    Success,
    PermissionDenied,
    PermissionUnspecified,
    TimedOut,
    InvalidPosition,
    Unavailable
}

/// <summary>Device location fix result, including status, coordinates, accuracy, and source.</summary>
/// <param name="Status">Outcome of the location request.</param>
/// <param name="Latitude">Latitude in degrees, when available.</param>
/// <param name="Longitude">Longitude in degrees, when available.</param>
/// <param name="AccuracyMeters">Reported accuracy in meters, when available.</param>
/// <param name="Source">Source reported for the fix.</param>
internal sealed record DeviceLocationResult(
    DeviceLocationStatus Status,
    double? Latitude = null,
    double? Longitude = null,
    double? AccuracyMeters = null,
    DeviceLocationSource Source = DeviceLocationSource.Unknown)
{
    /// <summary>True when the request succeeded and both coordinates are present.</summary>
    public bool IsSuccess => Status == DeviceLocationStatus.Success &&
        Latitude.HasValue && Longitude.HasValue;
}

/// <summary>Pure validation and formatting helpers for device location data.</summary>
internal static class DeviceLocationLogic
{
    /// <summary>Returns true when latitude and longitude are finite and within valid ranges.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <returns><c>true</c> if the coordinates are valid.</returns>
    public static bool HasValidCoordinates(double latitude, double longitude) =>
        double.IsFinite(latitude) && latitude is >= -90 and <= 90 &&
        double.IsFinite(longitude) && longitude is >= -180 and <= 180;

    /// <summary>Returns true when the reported accuracy is finite and non-negative.</summary>
    /// <param name="accuracyMeters">Reported accuracy in meters.</param>
    /// <returns><c>true</c> if the accuracy value is valid.</returns>
    public static bool HasValidAccuracy(double accuracyMeters) =>
        double.IsFinite(accuracyMeters) && accuracyMeters >= 0;

    /// <summary>Returns a short human-readable label describing the location source.</summary>
    /// <param name="source">The reported location source.</param>
    /// <returns>A display label for the source.</returns>
    public static string SourceLabel(DeviceLocationSource source) => source switch
    {
        DeviceLocationSource.Satellite => "GPS/GNSS (satelit)",
        DeviceLocationSource.WiFi => "Wi-Fi (bukan GPS)",
        DeviceLocationSource.Cellular => "jaringan seluler (bukan GPS)",
        DeviceLocationSource.IPAddress => "alamat IP (perkiraan; bukan GPS)",
        DeviceLocationSource.Default => "lokasi default Windows (sumber tidak dipastikan)",
        _ => "sumber tidak diketahui (bukan GPS terverifikasi)"
    };

    /// <summary>Returns a short human-readable label describing the reported accuracy.</summary>
    /// <param name="accuracyMeters">Reported accuracy in meters, or null when not reported.</param>
    /// <returns>A display label for the accuracy.</returns>
    public static string AccuracyLabel(double? accuracyMeters) =>
        accuracyMeters is double value && HasValidAccuracy(value)
            ? $"akurasi dilaporkan Windows ±{value.ToString("0", CultureInfo.InvariantCulture)} m"
            : "akurasi tidak dilaporkan Windows";

    /// <summary>Formats a local location name combining the source label and coordinates.</summary>
    /// <param name="source">The reported location source.</param>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <returns>A display string for the detected location.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the coordinates are invalid.</exception>
    public static string FormatLocalLocationName(DeviceLocationSource source, double latitude, double longitude)
    {
        if (!HasValidCoordinates(latitude, longitude))
            throw new ArgumentOutOfRangeException(nameof(latitude), "Koordinat lokasi perangkat tidak valid.");

        string lat = latitude.ToString("0.####", CultureInfo.InvariantCulture);
        string lon = longitude.ToString("0.####", CultureInfo.InvariantCulture);
        return $"Lokasi perangkat ({SourceLabel(source)}) — {lat}, {lon}";
    }
}
