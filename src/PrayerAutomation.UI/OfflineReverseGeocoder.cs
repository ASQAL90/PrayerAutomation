using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PrayerAutomation.UI;

/// <summary>Reverse-geocodes coordinates against a bundled offline geonames dataset (cities1000).</summary>
internal sealed class OfflineReverseGeocoder
{
    private const double DefaultMaximumDistanceKm = 75.0;
    private const int MaximumSearchResults = 50;
    private const int MaximumSearchQueryLength = 128;
    private const string DataFileName = "cities1000.tsv.gz";
    private static readonly Lazy<OfflineReverseGeocoder> BundledData = new(
        LoadBundledData,
        LazyThreadSafetyMode.ExecutionAndPublication);
    private readonly IReadOnlyList<OfflineCity> _cities;

    private OfflineReverseGeocoder(IReadOnlyList<OfflineCity> cities)
    {
        _cities = cities;
    }

    /// <summary>The lazily loaded shared instance backed by the bundled dataset.</summary>
    internal static OfflineReverseGeocoder Bundled => BundledData.Value;

    /// <summary>Loads an offline dataset from tab-separated text.</summary>
    /// <param name="reader">Reader for the TSV dataset (name, region, country, lat, lon, code, optional DEM metres).</param>
    /// <returns>The loaded geocoder.</returns>
    /// <exception cref="InvalidDataException">A row is invalid or the dataset contains no settlements.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> is null.</exception>
    internal static OfflineReverseGeocoder Load(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var cities = new List<OfflineCity>();
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] fields = line.Split('\t');
            if (fields.Length is not (6 or 7) ||
                string.IsNullOrWhiteSpace(fields[0]) ||
                !double.TryParse(fields[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude) ||
                !double.TryParse(fields[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude) ||
                !ValidCoordinates(latitude, longitude))
            {
                throw new InvalidDataException("Dataset geonames lokal memiliki baris yang tidak valid.");
            }

            int? elevationMeters = null;
            if (fields.Length == 7 && !string.IsNullOrWhiteSpace(fields[6]) &&
                int.TryParse(fields[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out int dem) &&
                dem is >= -500 and <= 9000)
            {
                elevationMeters = dem;
            }

            cities.Add(new OfflineCity(
                fields[0],
                fields[1],
                fields[2],
                latitude,
                longitude,
                fields[5],
                elevationMeters));
        }

        if (cities.Count == 0)
            throw new InvalidDataException("Dataset geonames lokal tidak berisi permukiman.");

        return new OfflineReverseGeocoder(cities);
    }

    /// <summary>Finds the nearest settlement to a coordinate within a maximum distance.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="maximumDistanceKm">Maximum distance in kilometers; negative or non-finite values throw.</param>
    /// <returns>The nearest settlement, or null when none is within range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Coordinates or distance are invalid.</exception>
    internal OfflineLocationResult? FindNearest(
        double latitude,
        double longitude,
        double maximumDistanceKm = DefaultMaximumDistanceKm)
    {
        if (!ValidCoordinates(latitude, longitude))
            throw new ArgumentOutOfRangeException(nameof(latitude), "Koordinat harus berada dalam rentang latitude/longitude WGS84.");
        if (!double.IsFinite(maximumDistanceKm) || maximumDistanceKm < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumDistanceKm));

        OfflineCity? nearest = null;
        double nearestDistanceKm = double.PositiveInfinity;
        foreach (OfflineCity city in _cities)
        {
            double distanceKm = DistanceKm(latitude, longitude, city.Latitude, city.Longitude);
            if (distanceKm < nearestDistanceKm)
            {
                nearest = city;
                nearestDistanceKm = distanceKm;
            }
        }

        if (nearest is null || nearestDistanceKm > maximumDistanceKm)
            return null;

        string displayName = string.Join(", ", new[]
            {
                nearest.CityName,
                nearest.RegionName,
                nearest.CountryName
            }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase));
        return new OfflineLocationResult(
            nearest.CityName,
            nearest.RegionName,
            nearest.CountryName,
            nearest.CountryCode,
            displayName,
            nearestDistanceKm,
            nearest.ElevationMeters);
    }

    // Settlement names only, not exhaustive street/village coverage. All query words
    // must prefix a word in city/region/country/code; city matches rank ahead of context.
    // Require two letters/digits, at most 128 UTF-16 units after trimming, and cap at 50.
    /// <summary>Searches settlements by name prefix words; city matches rank ahead of context.</summary>
    /// <param name="query">Search text; all words must prefix a word in city/region/country/code.</param>
    /// <param name="maxResults">Maximum results to return (capped at 50).</param>
    /// <returns>Matching suggestions, or an empty list for blank/short/oversized queries.</returns>
    internal IReadOnlyList<OfflineLocationSuggestion> Search(string query, int maxResults = 8)
    {
        if (maxResults <= 0 || string.IsNullOrWhiteSpace(query))
            return Array.Empty<OfflineLocationSuggestion>();
        query = query.Trim();
        if (query.Length > MaximumSearchQueryLength)
            return Array.Empty<OfflineLocationSuggestion>();
        string name = NormalizeSearchText(query);
        if (name.EnumerateRunes().Count(Rune.IsLetterOrDigit) < 2)
            return Array.Empty<OfflineLocationSuggestion>();
        string[] words = name.Split(' ');
        return _cities.Where(city => words.All(word => MatchesWordPrefix(city.SearchText, word)))
            .OrderBy(city => city.SearchName == name ||
                name.StartsWith(city.SearchName + " ", StringComparison.Ordinal) ? 0 :
                city.SearchName.StartsWith(name, StringComparison.Ordinal) ||
                city.SearchName.StartsWith(words[0], StringComparison.Ordinal) ? 1 :
                words.Any(word => MatchesWordPrefix(city.SearchName, word)) ? 2 : 3)
            .ThenByDescending(city => words.Count(word => MatchesWordPrefix(city.SearchName, word)))
            .ThenBy(city => city.SearchName, StringComparer.Ordinal)
            .ThenBy(city => city.RegionName, StringComparer.Ordinal)
            .ThenBy(city => city.CountryName, StringComparer.Ordinal)
            .ThenBy(city => city.CountryCode, StringComparer.Ordinal)
            .ThenBy(city => city.Latitude)
            .ThenBy(city => city.Longitude)
            .ThenBy(city => city.CityName, StringComparer.Ordinal)
            .Take(Math.Min(maxResults, MaximumSearchResults))
            .Select(city => new OfflineLocationSuggestion(
                city.CityName,
                city.RegionName,
                city.CountryName,
                string.Join(", ", new[] { city.CityName, city.RegionName, city.CountryName }
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)),
                city.Latitude,
                city.Longitude,
                city.ElevationMeters))
            .ToArray();
    }

    /// <summary>Returns true when text starts with the word or contains it after a space.</summary>
    /// <param name="text">Normalized search text.</param>
    /// <param name="word">Normalized word to match.</param>
    /// <returns>True on prefix match.</returns>
    private static bool MatchesWordPrefix(string text, string word) =>
        text.StartsWith(word, StringComparison.Ordinal) ||
        text.Contains(" " + word, StringComparison.Ordinal);

    /// <summary>Normalizes text for search: strips diacritics, upper-cases, collapses separators.</summary>
    /// <param name="value">Raw input text.</param>
    /// <returns>The normalized text; empty when normalization fails.</returns>
    private static string NormalizeSearchText(string value)
    {
        string decomposed;
        try
        {
            decomposed = value.Normalize(NormalizationForm.FormD);
        }
        catch (ArgumentException)
        {
            // Search must not introduce a new validation failure in the TSV loader.
            return string.Empty;
        }
        var normalized = new StringBuilder(value.Length);
        bool separator = false;
        foreach (Rune rune in decomposed.EnumerateRunes())
        {
            UnicodeCategory category = Rune.GetUnicodeCategory(rune);
            if (category is UnicodeCategory.NonSpacingMark or
                UnicodeCategory.SpacingCombiningMark or UnicodeCategory.EnclosingMark)
                continue;
            if (!Rune.IsLetterOrDigit(rune))
            {
                separator = normalized.Length > 0;
                continue;
            }
            if (separator)
                normalized.Append(' ');
            normalized.Append(Rune.ToUpperInvariant(rune).ToString());
            separator = false;
        }
        return normalized.ToString();
    }

    /// <summary>Loads the bundled geonames dataset from the application directory.</summary>
    /// <returns>The loaded geocoder.</returns>
    private static OfflineReverseGeocoder LoadBundledData()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "geonames", DataFileName);
        using var file = File.OpenRead(path);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return Load(reader);
    }

    /// <summary>Checks WGS84 latitude/longitude ranges.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <returns>True when both are finite and in range.</returns>
    private static bool ValidCoordinates(double latitude, double longitude) =>
        double.IsFinite(latitude) && latitude is >= -90 and <= 90 &&
        double.IsFinite(longitude) && longitude is >= -180 and <= 180;

    /// <summary>Computes the great-circle distance between two points using the haversine formula.</summary>
    /// <param name="latitude1">Latitude of the first point in degrees.</param>
    /// <param name="longitude1">Longitude of the first point in degrees.</param>
    /// <param name="latitude2">Latitude of the second point in degrees.</param>
    /// <param name="longitude2">Longitude of the second point in degrees.</param>
    /// <returns>Distance in kilometers.</returns>
    private static double DistanceKm(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double earthRadiusKm = 6371.0088;
        double lat1 = DegreesToRadians(latitude1);
        double lat2 = DegreesToRadians(latitude2);
        double deltaLatitude = lat2 - lat1;
        double deltaLongitude = DegreesToRadians(longitude2 - longitude1);
        double haversine = Math.Pow(Math.Sin(deltaLatitude / 2), 2) +
            Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLongitude / 2), 2);
        return earthRadiusKm * 2 * Math.Asin(Math.Sqrt(Math.Clamp(haversine, 0, 1)));
    }

    /// <summary>Converts degrees to radians.</summary>
    /// <param name="degrees">Value in degrees.</param>
    /// <returns>Value in radians.</returns>
    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);

    /// <summary>A city record in the offline dataset with precomputed search keys.</summary>
    private sealed record OfflineCity(
        string CityName,
        string RegionName,
        string CountryName,
        double Latitude,
        double Longitude,
        string CountryCode,
        int? ElevationMeters)
    {
        /// <summary>Normalized city name used for search ranking.</summary>
        internal string SearchName { get; } = NormalizeSearchText(CityName);
        /// <summary>Normalized "city region country code" text used for matching.</summary>
        internal string SearchText { get; } = NormalizeSearchText(
            $"{CityName} {RegionName} {CountryName} {CountryCode}");
    }
}

/// <summary>A settlement suggestion returned by search.</summary>
internal sealed record OfflineLocationSuggestion(
    string CityName,
    string RegionName,
    string CountryName,
    string DisplayName,
    double Latitude,
    double Longitude,
    int? ElevationMeters);

/// <summary>The result of a nearest-settlement lookup.</summary>
internal sealed record OfflineLocationResult(
    string CityName,
    string RegionName,
    string CountryName,
    string CountryCode,
    string DisplayName,
    double DistanceKm,
    int? ElevationMeters);
