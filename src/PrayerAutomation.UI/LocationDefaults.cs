namespace PrayerAutomation.UI;

/// <summary>A supported calculation method shown in the settings form.</summary>
/// <param name="Id">Method identifier understood by the native prayer core.</param>
/// <param name="Name">Indonesian label shown to the user.</param>
/// <param name="TimeZoneId">Representative IANA zone used only as a hint.</param>
/// <param name="FajrAngle">Fajr angle used by the method.</param>
/// <param name="IshaAngle">Isha angle used by the method.</param>
internal sealed record CalculationMethod(
    int Id,
    string Name,
    string TimeZoneId,
    double FajrAngle,
    double IshaAngle);

/// <summary>Country/time-zone defaults shared by every location input path.</summary>
internal static class LocationDefaults
{
    /// <summary>Label shared with settings and tests for the Muhammadiyah method.</summary>
    internal const string MuhammadiyahLabel = "Indonesia — Muhammadiyah";

    /// <summary>Methods supported by the native prayer core, in display order.</summary>
    internal static readonly IReadOnlyList<CalculationMethod> Methods =
    [
        new(20, "Indonesia — Kementerian Agama (Kemenag)", "Asia/Jakarta", 20, 18),
        new(99, MuhammadiyahLabel, "Asia/Jakarta", 18, 18),
        new(17, "Malaysia — JAKIM", "Asia/Kuala_Lumpur", 20, 18),
        new(11, "Singapura — MUIS", "Asia/Singapore", 20, 18),
        new(4, "Arab Saudi — Umm al-Qura", "Asia/Riyadh", 18.5, 18),
        new(13, "Turki — Diyanet", "Europe/Istanbul", 18, 17),
        new(3, "Muslim World League", "Asia/Jakarta", 18, 17),
        new(2, "Amerika Utara — ISNA", "America/New_York", 15, 15)
    ];

    private static readonly CalculationMethod IndonesiaKemenag = Methods[0];
    private static readonly CalculationMethod IndonesiaMuhammadiyah = Methods[1];
    private static readonly CalculationMethod MalaysiaJakim = Methods[2];
    private static readonly CalculationMethod SingaporeMuis = Methods[3];
    private static readonly CalculationMethod SaudiUmmAlQura = Methods[4];
    private static readonly CalculationMethod TurkeyDiyanet = Methods[5];
    private static readonly CalculationMethod MuslimWorldLeague = Methods[6];
    private static readonly CalculationMethod NorthAmericaIsna = Methods[7];

    /// <summary>Chooses the method implied by a country code and time zone.</summary>
    /// <param name="countryCode">ISO country code from an offline or network lookup, when known.</param>
    /// <param name="timeZoneId">IANA time zone already resolved for the location.</param>
    /// <param name="currentMethodId">Currently selected method; preserves an explicit Muhammadiyah choice.</param>
    /// <returns>The method to apply.</returns>
    internal static CalculationMethod ResolveAutoCalculationMethod(
        string? countryCode, string timeZoneId, int currentMethodId)
    {
        string country = (countryCode ?? string.Empty).Trim().ToUpperInvariant();
        bool indonesia = country == "ID" ||
            timeZoneId.StartsWith("Asia/Jakarta", StringComparison.OrdinalIgnoreCase) ||
            timeZoneId.StartsWith("Asia/Makassar", StringComparison.OrdinalIgnoreCase) ||
            timeZoneId.StartsWith("Asia/Jayapura", StringComparison.OrdinalIgnoreCase) ||
            timeZoneId.StartsWith("Asia/Pontianak", StringComparison.OrdinalIgnoreCase);
        if (indonesia && currentMethodId == 99) return IndonesiaMuhammadiyah;
        if (indonesia) return IndonesiaKemenag;
        return country switch
        {
            "MY" => MalaysiaJakim,
            "SG" => SingaporeMuis,
            "SA" => SaudiUmmAlQura,
            "TR" => TurkeyDiyanet,
            "US" or "CA" => NorthAmericaIsna,
            _ => MethodForTimeZone(timeZoneId)
        };
    }

    /// <summary>Falls back to a country-specific method when only the time zone is known.</summary>
    /// <param name="timeZoneId">IANA time zone identifier.</param>
    /// <returns>The matching method, or Muslim World League as the global default.</returns>
    internal static CalculationMethod MethodForTimeZone(string timeZoneId) =>
        timeZoneId.StartsWith("Asia/Kuala_Lumpur", StringComparison.OrdinalIgnoreCase) ? MalaysiaJakim :
        timeZoneId.StartsWith("Asia/Singapore", StringComparison.OrdinalIgnoreCase) ? SingaporeMuis :
        timeZoneId.StartsWith("Asia/Riyadh", StringComparison.OrdinalIgnoreCase) ? SaudiUmmAlQura :
        timeZoneId.StartsWith("Europe/Istanbul", StringComparison.OrdinalIgnoreCase) ? TurkeyDiyanet :
        timeZoneId.StartsWith("America/", StringComparison.OrdinalIgnoreCase) ? NorthAmericaIsna :
        MuslimWorldLeague;

    /// <summary>Resolves and validates the IANA time zone for WGS84 coordinates.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <returns>A usable IANA or Windows time zone identifier.</returns>
    /// <exception cref="ArgumentException">Coordinates are outside WGS84 ranges.</exception>
    /// <exception cref="TimeZoneNotFoundException">No time zone can be resolved.</exception>
    internal static string ResolveTimeZoneForCoordinates(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude is < -90 or > 90 ||
            !double.IsFinite(longitude) || longitude is < -180 or > 180)
            throw new ArgumentException("Koordinat harus berada dalam rentang latitude/longitude WGS84.");

        string timeZoneId = GeoTimeZone.TimeZoneLookup.GetTimeZone(latitude, longitude).Result;
        try { _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch (TimeZoneNotFoundException)
        {
            if (TimeZoneInfo.TryConvertIanaIdToWindowsId(timeZoneId, out string? windowsId))
                _ = TimeZoneInfo.FindSystemTimeZoneById(windowsId);
            else throw;
        }
        return timeZoneId;
    }
}
