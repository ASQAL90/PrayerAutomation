using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PrayerAutomation.UI;

/// <summary>Reverse-geocodes coordinates against bundled offline GeoNames datasets.</summary>
internal sealed class OfflineReverseGeocoder
{
    private const double DefaultMaximumDistanceKm = 75.0;
    private const double IndonesiaMaximumDistanceKm = 15.0;
    // Coastal polygons are simplified; a short tolerance keeps coastal cities on the
    // Indonesian layer instead of falling back to the sparse global list.
    private const double IndonesiaBoundaryToleranceKm = 2.0;
    private const int MaximumSearchResults = 50;
    private const int MaximumSearchQueryLength = 128;
    private const string GlobalDataFileName = "cities1000.tsv.gz";
    private const string IndonesiaDataFileName = "indonesia.tsv.gz";
    private const string IndonesiaBoundaryFileName = "indonesia.boundary.bin";
    private static readonly Lazy<OfflineReverseGeocoder> BundledData = new(
        LoadBundledData,
        LazyThreadSafetyMode.ExecutionAndPublication);
    private readonly IReadOnlyList<OfflineCity> _cities;
    private readonly IndonesiaPlace[] _indonesiaPlaces;
    private readonly string[] _regencies;
    private readonly string[] _provinces;
    private readonly IndonesiaBoundary? _indonesiaBoundary;

    private OfflineReverseGeocoder(
        IReadOnlyList<OfflineCity> cities,
        IndonesiaPlace[]? indonesiaPlaces = null,
        string[]? regencies = null,
        string[]? provinces = null,
        IndonesiaBoundary? indonesiaBoundary = null)
    {
        _cities = cities;
        _indonesiaPlaces = indonesiaPlaces ?? Array.Empty<IndonesiaPlace>();
        _regencies = regencies ?? Array.Empty<string>();
        _provinces = provinces ?? Array.Empty<string>();
        _indonesiaBoundary = indonesiaBoundary;
    }

    /// <summary>The lazily loaded shared instance backed by the bundled datasets.</summary>
    internal static OfflineReverseGeocoder Bundled => BundledData.Value;

    /// <summary>Loads an offline dataset from tab-separated text.</summary>
    /// <param name="reader">Reader for the TSV dataset (name, region, country, lat, lon, code, optional DEM metres).</param>
    /// <param name="indonesiaReader">Optional Indonesian layer reader (name, regency, province, lat, lon, DEM metres).</param>
    /// <param name="indonesiaBoundary">Optional Indonesia boundary used to select the Indonesian layer.</param>
    /// <returns>The loaded geocoder.</returns>
    /// <exception cref="InvalidDataException">A row is invalid or the dataset contains no settlements.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="reader"/> is null.</exception>
    internal static OfflineReverseGeocoder Load(
        TextReader reader,
        TextReader? indonesiaReader = null,
        IndonesiaBoundary? indonesiaBoundary = null)
    {
        IReadOnlyList<OfflineCity> cities = ParseCities(reader);
        var places = Array.Empty<IndonesiaPlace>();
        var regencies = Array.Empty<string>();
        var provinces = Array.Empty<string>();
        if (indonesiaReader is not null)
        {
            (places, regencies, provinces) = ParseIndonesia(indonesiaReader);
        }
        return new OfflineReverseGeocoder(cities, places, regencies, provinces, indonesiaBoundary);
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

        if (_indonesiaBoundary is not null &&
            _indonesiaBoundary.Contains(latitude, longitude, IndonesiaBoundaryToleranceKm) &&
            TryFindNearestIndonesia(latitude, longitude, Math.Min(maximumDistanceKm, IndonesiaMaximumDistanceKm),
                out IndonesiaPlace place, out double placeDistanceKm))
        {
            string regency = _regencies[place.RegencyId];
            string province = _provinces[place.ProvinceId];
            string displayName = string.Join(", ", new[]
                {
                    place.Name,
                    regency,
                    province,
                    "Indonesia"
                }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase));
            return new OfflineLocationResult(
                place.Name,
                province,
                "Indonesia",
                "ID",
                displayName,
                placeDistanceKm,
                place.ElevationMeters == IndonesiaPlace.UnknownElevation ? null : place.ElevationMeters,
                regency,
                IsIndonesiaLayer: true);
        }

        OfflineCity? nearest = null;
        double nearestDistanceKm = double.PositiveInfinity;
        // A global Indonesian city can still be the geometrically nearest record for
        // a coordinate just across a land border. Do not label that point Indonesia
        // when the dedicated boundary says it is outside the country.
        bool outsideIndonesia = _indonesiaBoundary is not null &&
            !_indonesiaBoundary.Contains(latitude, longitude, IndonesiaBoundaryToleranceKm);
        foreach (OfflineCity city in _cities)
        {
            if (outsideIndonesia && string.Equals(city.CountryCode, "ID", StringComparison.OrdinalIgnoreCase))
                continue;
            double distanceKm = DistanceKm(latitude, longitude, city.Latitude, city.Longitude);
            if (distanceKm < nearestDistanceKm)
            {
                nearest = city;
                nearestDistanceKm = distanceKm;
            }
        }

        if (nearest is null || nearestDistanceKm > maximumDistanceKm)
            return null;

        string countryName = TranslateCountry(nearest.CountryName, nearest.CountryCode);
        string globalDisplayName = string.Join(", ", new[]
            {
                nearest.CityName,
                nearest.RegionName,
                countryName
            }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase));
        return new OfflineLocationResult(
            nearest.CityName,
            nearest.RegionName,
            countryName,
            nearest.CountryCode,
            globalDisplayName,
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
                TranslateCountry(city.CountryName, city.CountryCode),
                string.Join(", ", new[]
                    {
                        city.CityName,
                        city.RegionName,
                        TranslateCountry(city.CountryName, city.CountryCode)
                    }
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)),
                city.Latitude,
                city.Longitude,
                city.ElevationMeters))
            .ToArray();
    }

    /// <summary>Parses the global cities1000 layer.</summary>
    /// <param name="reader">TSV reader.</param>
    /// <returns>Parsed cities.</returns>
    private static IReadOnlyList<OfflineCity> ParseCities(TextReader reader)
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

        return cities;
    }

    /// <summary>Parses the Indonesian layer and interns its regency and province names.</summary>
    /// <param name="reader">TSV reader (name, regency, province, lat, lon, DEM metres).</param>
    /// <returns>The place array plus the regency and province string tables.</returns>
    private static (IndonesiaPlace[] Places, string[] Regencies, string[] Provinces) ParseIndonesia(TextReader reader)
    {
        var regencyIds = new Dictionary<string, ushort>(StringComparer.Ordinal);
        var regencies = new List<string> { string.Empty };
        var provinceIds = new Dictionary<string, byte>(StringComparer.Ordinal);
        var provinces = new List<string> { string.Empty };
        var places = new List<IndonesiaPlace>(270_000);

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
                throw new InvalidDataException("Dataset lokasi Indonesia memiliki baris yang tidak valid.");
            }

            if (!regencyIds.TryGetValue(fields[1], out ushort regencyId))
            {
                if (regencies.Count > ushort.MaxValue)
                    throw new InvalidDataException("Terlalu banyak kabupaten/kota pada dataset Indonesia.");
                regencyId = (ushort)regencies.Count;
                regencyIds.Add(fields[1], regencyId);
                regencies.Add(fields[1]);
            }

            if (!provinceIds.TryGetValue(fields[2], out byte provinceId))
            {
                if (provinces.Count > byte.MaxValue)
                    throw new InvalidDataException("Terlalu banyak provinsi pada dataset Indonesia.");
                provinceId = (byte)provinces.Count;
                provinceIds.Add(fields[2], provinceId);
                provinces.Add(fields[2]);
            }

            int elevation = IndonesiaPlace.UnknownElevation;
            if (!string.IsNullOrWhiteSpace(fields[5]) &&
                int.TryParse(fields[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out int dem) &&
                dem is >= -500 and <= 9000)
            {
                elevation = dem;
            }

            places.Add(new IndonesiaPlace
            {
                Name = fields[0],
                RegencyId = regencyId,
                ProvinceId = provinceId,
                Latitude = latitude,
                Longitude = longitude,
                ElevationMeters = elevation
            });
        }

        if (places.Count == 0)
            throw new InvalidDataException("Dataset lokasi Indonesia tidak berisi permukiman.");

        return (places.ToArray(), regencies.ToArray(), provinces.ToArray());
    }

    /// <summary>Finds the nearest Indonesian place within the Indonesian layer radius.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="maximumDistanceKm">Caller limit, already capped to the Indonesian layer radius.</param>
    /// <param name="place">The nearest place when found.</param>
    /// <param name="distanceKm">Its distance in kilometers.</param>
    /// <returns>True when a place is within range.</returns>
    private bool TryFindNearestIndonesia(
        double latitude,
        double longitude,
        double maximumDistanceKm,
        out IndonesiaPlace place,
        out double distanceKm)
    {
        place = default;
        distanceKm = double.PositiveInfinity;
        foreach (IndonesiaPlace candidate in _indonesiaPlaces)
        {
            double candidateDistance = DistanceKm(latitude, longitude, candidate.Latitude, candidate.Longitude);
            if (candidateDistance < distanceKm)
            {
                place = candidate;
                distanceKm = candidateDistance;
            }
        }

        return distanceKm <= maximumDistanceKm;
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

    /// <summary>Loads the bundled datasets from the application directory.</summary>
    /// <returns>The loaded geocoder; the Indonesian layer is optional and never breaks the global layer.</returns>
    private static OfflineReverseGeocoder LoadBundledData()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "geonames");
        IReadOnlyList<OfflineCity> cities = ReadGzip(
            Path.Combine(directory, GlobalDataFileName),
            ParseCities);

        var places = Array.Empty<IndonesiaPlace>();
        var regencies = Array.Empty<string>();
        var provinces = Array.Empty<string>();
        IndonesiaBoundary? boundary = null;
        try
        {
            (places, regencies, provinces) = ReadGzip(
                Path.Combine(directory, IndonesiaDataFileName),
                ParseIndonesia);
            using var boundaryStream = File.OpenRead(Path.Combine(directory, IndonesiaBoundaryFileName));
            boundary = IndonesiaBoundary.Load(boundaryStream);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or NotSupportedException)
        {
            // The global list still provides a usable label; keep the feature honest.
            places = Array.Empty<IndonesiaPlace>();
            regencies = Array.Empty<string>();
            provinces = Array.Empty<string>();
            boundary = null;
        }

        return new OfflineReverseGeocoder(cities, places, regencies, provinces, boundary);
    }

    /// <summary>Reads a gzip-compressed dataset with the given parser.</summary>
    /// <typeparam name="T">Parser result type.</typeparam>
    /// <param name="path">Dataset path.</param>
    /// <param name="parser">Parser to apply.</param>
    /// <returns>The parsed dataset.</returns>
    private static T ReadGzip<T>(string path, Func<TextReader, T> parser)
    {
        using var file = File.OpenRead(path);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return parser(reader);
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

    /// <summary>Translates the most common country names to Indonesian.</summary>
    /// <param name="englishName">GeoNames English country name.</param>
    /// <param name="countryCode">ISO 3166-1 alpha-2 code.</param>
    /// <returns>The Indonesian name when known, otherwise the original name.</returns>
    private static string TranslateCountry(string englishName, string countryCode)
    {
        if (!string.IsNullOrWhiteSpace(countryCode) &&
            CountryNames.TryGetValue(countryCode.ToUpperInvariant(), out string? name))
        {
            return name;
        }
        return englishName;
    }

    private static readonly Dictionary<string, string> CountryNames = new(StringComparer.Ordinal)
    {
        ["ID"] = "Indonesia", ["MY"] = "Malaysia", ["SG"] = "Singapura", ["TH"] = "Thailand",
        ["PH"] = "Filipina", ["VN"] = "Vietnam", ["BN"] = "Brunei", ["TL"] = "Timor-Leste",
        ["AU"] = "Australia", ["NZ"] = "Selandia Baru", ["CN"] = "Tiongkok", ["JP"] = "Jepang",
        ["KR"] = "Korea Selatan", ["KP"] = "Korea Utara", ["IN"] = "India", ["PK"] = "Pakistan",
        ["BD"] = "Bangladesh", ["LK"] = "Sri Lanka", ["NP"] = "Nepal", ["MM"] = "Myanmar",
        ["SA"] = "Arab Saudi", ["AE"] = "Uni Emirat Arab", ["QA"] = "Qatar", ["KW"] = "Kuwait",
        ["BH"] = "Bahrain", ["OM"] = "Oman", ["JO"] = "Yordania", ["LB"] = "Lebanon",
        ["SY"] = "Suriah", ["IQ"] = "Irak", ["IR"] = "Iran", ["YE"] = "Yaman",
        ["PS"] = "Palestina", ["EG"] = "Mesir", ["TR"] = "Turki", ["IL"] = "Israel",
        ["US"] = "Amerika Serikat", ["CA"] = "Kanada", ["MX"] = "Meksiko", ["BR"] = "Brasil",
        ["AR"] = "Argentina", ["CL"] = "Cile", ["PE"] = "Peru", ["CO"] = "Kolombia",
        ["GB"] = "Inggris", ["IE"] = "Irlandia", ["NL"] = "Belanda", ["BE"] = "Belgia",
        ["DE"] = "Jerman", ["FR"] = "Prancis", ["IT"] = "Italia", ["ES"] = "Spanyol",
        ["PT"] = "Portugal", ["CH"] = "Swiss", ["AT"] = "Austria", ["SE"] = "Swedia",
        ["NO"] = "Norwegia", ["DK"] = "Denmark", ["FI"] = "Finlandia", ["PL"] = "Polandia",
        ["CZ"] = "Ceko", ["GR"] = "Yunani", ["HU"] = "Hungaria", ["RO"] = "Rumania",
        ["BG"] = "Bulgaria", ["RU"] = "Rusia", ["UA"] = "Ukraina", ["ZA"] = "Afrika Selatan",
        ["NG"] = "Nigeria", ["KE"] = "Kenya", ["ET"] = "Etiopia", ["MA"] = "Maroko",
        ["DZ"] = "Aljazair", ["TN"] = "Tunisia", ["LY"] = "Libya", ["SD"] = "Sudan",
        ["KZ"] = "Kazakhstan", ["UZ"] = "Uzbekistan", ["AF"] = "Afganistan", ["HK"] = "Hong Kong",
        ["TW"] = "Taiwan", ["MO"] = "Makau"
    };

    /// <summary>A city record in the global offline dataset with precomputed search keys.</summary>
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
            $"{CityName} {RegionName} {CountryName} {TranslateCountry(CountryName, CountryCode)} {CountryCode}");
    }

    /// <summary>A compact Indonesian place record; regency and province are interned.</summary>
    private struct IndonesiaPlace
    {
        internal const int UnknownElevation = int.MinValue;
        internal string Name;
        internal ushort RegencyId;
        internal byte ProvinceId;
        internal double Latitude;
        internal double Longitude;
        internal int ElevationMeters;
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
    int? ElevationMeters,
    string? RegencyName = null,
    bool IsIndonesiaLayer = false);
