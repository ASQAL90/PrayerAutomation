using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PrayerAutomation.UI;

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

    internal static OfflineReverseGeocoder Bundled => BundledData.Value;

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
            if (fields.Length != 6 ||
                string.IsNullOrWhiteSpace(fields[0]) ||
                !double.TryParse(fields[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude) ||
                !double.TryParse(fields[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude) ||
                !ValidCoordinates(latitude, longitude))
            {
                throw new InvalidDataException("Dataset geonames lokal memiliki baris yang tidak valid.");
            }

            cities.Add(new OfflineCity(
                fields[0],
                fields[1],
                fields[2],
                latitude,
                longitude,
                fields[5]));
        }

        if (cities.Count == 0)
            throw new InvalidDataException("Dataset geonames lokal tidak berisi permukiman.");

        return new OfflineReverseGeocoder(cities);
    }

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
            nearestDistanceKm);
    }

    // Settlement names only, not exhaustive street/village coverage. All query words
    // must prefix a word in city/region/country/code; city matches rank ahead of context.
    // Require two letters/digits, at most 128 UTF-16 units after trimming, and cap at 50.
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
                city.Longitude))
            .ToArray();
    }

    private static bool MatchesWordPrefix(string text, string word) =>
        text.StartsWith(word, StringComparison.Ordinal) ||
        text.Contains(" " + word, StringComparison.Ordinal);

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

    private static OfflineReverseGeocoder LoadBundledData()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "geonames", DataFileName);
        using var file = File.OpenRead(path);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return Load(reader);
    }

    private static bool ValidCoordinates(double latitude, double longitude) =>
        double.IsFinite(latitude) && latitude is >= -90 and <= 90 &&
        double.IsFinite(longitude) && longitude is >= -180 and <= 180;

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

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);

    private sealed record OfflineCity(
        string CityName,
        string RegionName,
        string CountryName,
        double Latitude,
        double Longitude,
        string CountryCode)
    {
        internal string SearchName { get; } = NormalizeSearchText(CityName);
        internal string SearchText { get; } = NormalizeSearchText(
            $"{CityName} {RegionName} {CountryName} {CountryCode}");
    }
}

internal sealed record OfflineLocationSuggestion(
    string CityName,
    string RegionName,
    string CountryName,
    string DisplayName,
    double Latitude,
    double Longitude);

internal sealed record OfflineLocationResult(
    string CityName,
    string RegionName,
    string CountryName,
    string CountryCode,
    string DisplayName,
    double DistanceKm);
