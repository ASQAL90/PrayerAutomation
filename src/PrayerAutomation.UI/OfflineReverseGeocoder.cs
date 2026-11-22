using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace PrayerAutomation.UI;

/// <summary>Reverse-geocodes coordinates against bundled or downloaded offline GeoNames datasets.</summary>
internal sealed class OfflineReverseGeocoder
{
    private const double DefaultMaximumDistanceKm = 75.0;
    private const double IndonesiaMaximumDistanceKm = 15.0;
    // Coastal polygons are simplified; a short tolerance keeps coastal cities on the
    // Indonesian layer instead of falling back to the sparse global list.
    private const double IndonesiaBoundaryToleranceKm = 2.0;
    private const int MaximumSearchResults = 50;
    private const int MaximumSearchQueryLength = 128;
    private const string GlobalDataFileName = "cities500.tsv.gz";
    private const string LegacyGlobalDataFileName = "cities1000.tsv.gz";
    private const string DownloadedGlobalDataFileName = "global.tsv.gz";
    private const string IndonesiaDataFileName = "indonesia.tsv.gz";
    private const string IndonesiaBoundaryFileName = "indonesia.boundary.bin";
    // Explicitly low-population localities (hamlets, sections) lose to real
    // settlements unless they are much closer. Legacy rows without population are
    // treated as normal settlements so existing datasets keep their behaviour.
    private const double LocalityDistancePenalty = 12.0;
    private const double PopulationBoostRadiusKm = 1.0;

    private static readonly Lazy<OfflineReverseGeocoder> BundledData = new(
        LoadBundledData,
        LazyThreadSafetyMode.ExecutionAndPublication);
    private static OfflineReverseGeocoder? _downloaded;

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

    /// <summary>The active instance: a verified download when installed, otherwise bundled data.</summary>
    internal static OfflineReverseGeocoder Current => Volatile.Read(ref _downloaded) ?? Bundled;

    /// <summary>Installs a verified downloaded dataset as the active geocoder.</summary>
    /// <param name="geocoder">The fully loaded downloaded dataset.</param>
    internal static void UseDownloaded(OfflineReverseGeocoder geocoder)
    {
        ArgumentNullException.ThrowIfNull(geocoder);
        Volatile.Write(ref _downloaded, geocoder);
    }

    /// <summary>Removes the downloaded dataset after a failed or obsolete cache check.</summary>
    internal static void ClearDownloaded() => Volatile.Write(ref _downloaded, null);

    /// <summary>Loads an offline dataset from tab-separated text.</summary>
    /// <param name="reader">Reader for the TSV dataset (legacy 6/7 columns or schema v2).</param>
    /// <param name="indonesiaReader">Optional Indonesian layer reader.</param>
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
            (places, regencies, provinces) = ParseIndonesia(indonesiaReader);
        return new OfflineReverseGeocoder(cities, places, regencies, provinces, indonesiaBoundary);
    }

    /// <summary>Loads a verified download directory, falling back to bundled Indonesian data.</summary>
    /// <param name="directory">Directory containing the downloaded TSV and boundary files.</param>
    /// <returns>The loaded geocoder.</returns>
    /// <exception cref="InvalidDataException">The downloaded global dataset is invalid.</exception>
    internal static OfflineReverseGeocoder LoadFromDirectory(string directory)
    {
        IReadOnlyList<OfflineCity> cities = ReadGzip(
            Path.Combine(directory, DownloadedGlobalDataFileName),
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
            (places, regencies, provinces, boundary) = LoadBundledIndonesiaLayer();
        }

        return new OfflineReverseGeocoder(cities, places, regencies, provinces, boundary);
    }

    /// <summary>Finds the nearest meaningful settlement to a coordinate.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="maximumDistanceKm">Maximum distance in kilometers; negative or non-finite values throw.</param>
    /// <returns>The best settlement, or null when none is within range.</returns>
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
            int? elevation = place.ElevationMeters == IndonesiaPlace.UnknownElevation ? null : place.ElevationMeters;
            return new OfflineLocationResult(
                place.Name,
                province,
                "Indonesia",
                "ID",
                displayName,
                placeDistanceKm,
                elevation,
                regency,
                IsIndonesiaLayer: true,
                ElevationSource: place.ElevationSource);
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
            if (distanceKm > maximumDistanceKm) continue;
            if (nearest is null || IsBetter(distanceKm, city.Population, city.FeatureCode,
                    nearestDistanceKm, nearest.Population, nearest.FeatureCode))
            {
                nearest = city;
                nearestDistanceKm = distanceKm;
            }
        }

        if (nearest is null)
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
            nearest.ElevationMeters,
            ElevationSource: nearest.ElevationSource);
    }

    // Settlement names only, not exhaustive street/village coverage. All query words
    // must prefix a word in name/context/alias. Indonesian records are merged with the
    // global list; a locality is never rewritten into a different place by an alias.
    /// <summary>Searches settlements by name, alias, and administrative context.</summary>
    /// <param name="query">Search text; all words must prefix a word in the indexed text.</param>
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
        int take = Math.Min(maxResults, MaximumSearchResults);
        var candidates = new List<SearchCandidate>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (IndonesiaPlace place in _indonesiaPlaces)
        {
            if (!words.All(word => MatchesWordPrefix(place.SearchText, word))) continue;
            if (!seen.Add(PlaceKey(place.Name, place.Latitude, place.Longitude))) continue;
            string regency = _regencies[place.RegencyId];
            string province = _provinces[place.ProvinceId];
            string displayName = string.Join(", ", new[] { place.Name, regency, province, "Indonesia" }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase));
            int? elevation = place.ElevationMeters == IndonesiaPlace.UnknownElevation ? null : place.ElevationMeters;
            candidates.Add(new SearchCandidate(
                Rank(name, words, place.SearchName, place.AliasSearchText, place.SearchText),
                place.Population,
                displayName,
                new OfflineLocationSuggestion(
                    place.Name,
                    regency,
                    "Indonesia",
                    "ID",
                    displayName,
                    place.Latitude,
                    place.Longitude,
                    elevation,
                    place.ElevationSource)));
        }

        foreach (OfflineCity city in _cities)
        {
            if (!words.All(word => MatchesWordPrefix(city.SearchText, word))) continue;
            if (!seen.Add(PlaceKey(city.CityName, city.Latitude, city.Longitude))) continue;
            string countryName = TranslateCountry(city.CountryName, city.CountryCode);
            string displayName = string.Join(", ", new[] { city.CityName, city.RegionName, countryName }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase));
            candidates.Add(new SearchCandidate(
                Rank(name, words, city.SearchName, city.AliasSearchText, city.SearchText),
                city.Population,
                displayName,
                new OfflineLocationSuggestion(
                    city.CityName,
                    city.RegionName,
                    countryName,
                    city.CountryCode,
                    displayName,
                    city.Latitude,
                    city.Longitude,
                    city.ElevationMeters,
                    city.ElevationSource)));
        }

        return candidates
            .OrderBy(candidate => candidate.Rank)
            .ThenByDescending(candidate => candidate.Population)
            .ThenBy(candidate => candidate.DisplayName, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Suggestion.Latitude)
            .ThenBy(candidate => candidate.Suggestion.Longitude)
            .Take(take)
            .Select(candidate => candidate.Suggestion)
            .ToArray();
    }

    /// <summary>Parses the global layer, accepting legacy 6/7-column and schema-v2 rows.</summary>
    /// <param name="reader">TSV reader.</param>
    /// <returns>Parsed cities.</returns>
    private static IReadOnlyList<OfflineCity> ParseCities(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var cities = new List<OfflineCity>(200_000);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] fields = line.Split('\t');
            if (fields.Length is < 6 or > 13 ||
                string.IsNullOrWhiteSpace(fields[0]) ||
                !double.TryParse(fields[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude) ||
                !double.TryParse(fields[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude) ||
                !ValidCoordinates(latitude, longitude))
                throw new InvalidDataException("Dataset geonames lokal memiliki baris yang tidak valid.");

            bool legacy = fields.Length is 6 or 7;
            if (!legacy && fields.Length < 13)
                throw new InvalidDataException("Dataset geonames lokal memiliki baris yang tidak valid.");

            (int? elevation, string? source) = legacy
                ? ParseLegacyElevation(fields.Length == 7 ? fields[6] : string.Empty)
                : ParseElevation(fields[6], fields[7], fields.Length == 13 ? fields[11] : null);
            string featureCode = legacy ? string.Empty : Clean(fields[8]);
            long population = legacy ? 0 : ParsePopulation(fields[9]);
            string aliases = legacy ? string.Empty : fields[10];

            cities.Add(new OfflineCity(
                Clean(fields[0]),
                Clean(fields[1]),
                Clean(fields[2]),
                latitude,
                longitude,
                Clean(fields[5]),
                elevation,
                source,
                featureCode,
                population,
                BuildAliasSearchText(aliases)));
        }

        if (cities.Count == 0)
            throw new InvalidDataException("Dataset geonames lokal tidak berisi permukiman.");
        return cities;
    }

    /// <summary>Parses the Indonesian layer, accepting legacy and schema-v2 rows.</summary>
    /// <param name="reader">TSV reader.</param>
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
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] fields = line.Split('\t');
            if (fields.Length is < 6 or > 15 ||
                string.IsNullOrWhiteSpace(fields[0]) ||
                !double.TryParse(fields[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude) ||
                !double.TryParse(fields[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude) ||
                !ValidCoordinates(latitude, longitude))
                throw new InvalidDataException("Dataset lokasi Indonesia memiliki baris yang tidak valid.");

            bool legacy = fields.Length == 6;
            if (!legacy && fields.Length < 15)
                throw new InvalidDataException("Dataset lokasi Indonesia memiliki baris yang tidak valid.");

            (int? elevation, string? source) = legacy
                ? ParseLegacyElevation(fields[5])
                : ParseElevation(fields[5], fields[6], fields[13]);
            string featureCode = legacy ? string.Empty : Clean(fields[7]);
            long population = legacy ? 0 : ParsePopulation(fields[8]);
            string aliases = legacy ? string.Empty : fields[9];
            string district = legacy ? string.Empty : Clean(fields[10]);
            string regionCode = legacy ? string.Empty : Clean(fields[11]);
            bool officialMatch = legacy || fields[12] == "1";

            string regencyName = Clean(fields[1]);
            string provinceName = Clean(fields[2]);
            if (!regencyIds.TryGetValue(regencyName, out ushort regencyId))
            {
                if (regencies.Count > ushort.MaxValue)
                    throw new InvalidDataException("Terlalu banyak kabupaten/kota pada dataset Indonesia.");
                regencyId = (ushort)regencies.Count;
                regencyIds.Add(regencyName, regencyId);
                regencies.Add(regencyName);
            }
            if (!provinceIds.TryGetValue(provinceName, out byte provinceId))
            {
                if (provinces.Count > byte.MaxValue)
                    throw new InvalidDataException("Terlalu banyak provinsi pada dataset Indonesia.");
                provinceId = (byte)provinces.Count;
                provinceIds.Add(provinceName, provinceId);
                provinces.Add(provinceName);
            }

            string placeName = Clean(fields[0]);
            string aliasText = BuildAliasSearchText(aliases);
            places.Add(new IndonesiaPlace
            {
                Name = placeName,
                RegencyId = regencyId,
                ProvinceId = provinceId,
                Latitude = latitude,
                Longitude = longitude,
                ElevationMeters = elevation ?? IndonesiaPlace.UnknownElevation,
                ElevationSource = source,
                FeatureCode = featureCode,
                Population = population,
                District = district,
                RegionCode = regionCode,
                OfficialMatch = officialMatch,
                SearchName = NormalizeSearchText(placeName),
                AliasSearchText = aliasText,
                SearchText = NormalizeSearchText(
                    $"{placeName} {district} {regencyName} {provinceName} Indonesia {regionCode} {aliases.Replace('|', ' ')}")
            });
        }

        if (places.Count == 0)
            throw new InvalidDataException("Dataset lokasi Indonesia tidak berisi permukiman.");
        return (places.ToArray(), regencies.ToArray(), provinces.ToArray());
    }

    /// <summary>Finds the best Indonesian place, preferring real settlements over unnamed localities.</summary>
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
            if (candidateDistance > maximumDistanceKm) continue;
            if (double.IsPositiveInfinity(distanceKm) || IsBetter(candidateDistance, candidate.Population,
                    candidate.FeatureCode, distanceKm, place.Population, place.FeatureCode))
            {
                place = candidate;
                distanceKm = candidateDistance;
            }
        }
        return !double.IsPositiveInfinity(distanceKm);
    }

    /// <summary>Returns true when a candidate outranks the current best place.</summary>
    private static bool IsBetter(
        double candidateDistance, long candidatePopulation, string? candidateFeature,
        double bestDistance, long bestPopulation, string? bestFeature)
    {
        double candidateScore = candidateDistance * PlaceDistanceFactor(candidatePopulation, candidateFeature);
        double bestScore = bestDistance * PlaceDistanceFactor(bestPopulation, bestFeature);
        if (Math.Abs(candidateScore - bestScore) > 1e-9)
            return candidateScore < bestScore;
        if (candidatePopulation > 0 && bestPopulation > 0 &&
            candidateDistance <= bestDistance + PopulationBoostRadiusKm &&
            candidatePopulation >= bestPopulation * 10)
            return true;
        return candidateDistance < bestDistance;
    }

    /// <summary>Low-population localities need to be much closer to beat a settlement.</summary>
    private static double PlaceDistanceFactor(long population, string? featureCode)
    {
        if (population >= 1000) return 1.0;
        if (featureCode == "PPLC" || (featureCode?.StartsWith("PPLA", StringComparison.Ordinal) ?? false)) return 1.0;
        if (featureCode is "PPLL" or "PPLX" or "PPLQ" or "PPLH" or "PPLW") return LocalityDistancePenalty;
        // Legacy rows have no population or feature code; keep their old nearest behaviour.
        if (population == 0 && string.IsNullOrEmpty(featureCode)) return 1.0;
        return population > 0 ? 1.0 : LocalityDistancePenalty;
    }

    /// <summary>Computes a search rank: exact name, exact alias, prefix name, prefix alias, then context.</summary>
    private static int Rank(string query, string[] words, string searchName, string aliasText, string searchText)
    {
        if (searchName == query || query.StartsWith(searchName + " ", StringComparison.Ordinal)) return 0;
        if (!string.IsNullOrEmpty(aliasText) && aliasText == query) return 1;
        if (searchName.StartsWith(query, StringComparison.Ordinal) ||
            searchName.StartsWith(words[0], StringComparison.Ordinal)) return 2;
        if (!string.IsNullOrEmpty(aliasText) && words.All(word => MatchesWordPrefix(aliasText, word))) return 3;
        if (words.Any(word => MatchesWordPrefix(searchName, word))) return 4;
        return words.All(word => MatchesWordPrefix(searchText, word)) ? 5 : 6;
    }

    /// <summary>Builds a de-duplication key that keeps near-identical rows from one place.</summary>
    private static string PlaceKey(string name, double latitude, double longitude) =>
        string.Concat(
            NormalizeSearchText(name),
            "|",
            latitude.ToString("F3", CultureInfo.InvariantCulture),
            ",",
            longitude.ToString("F3", CultureInfo.InvariantCulture));

    /// <summary>Returns true when text starts with the word or contains it after a space.</summary>
    private static bool MatchesWordPrefix(string text, string word) =>
        text.StartsWith(word, StringComparison.Ordinal) ||
        text.Contains(" " + word, StringComparison.Ordinal);

    /// <summary>Normalizes text for search: strips diacritics, upper-cases, collapses separators.</summary>
    private static string NormalizeSearchText(string value)
    {
        string decomposed;
        try { decomposed = value.Normalize(NormalizationForm.FormD); }
        catch (ArgumentException) { return string.Empty; }
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
            if (separator) normalized.Append(' ');
            normalized.Append(Rune.ToUpperInvariant(rune).ToString());
            separator = false;
        }
        return normalized.ToString();
    }

    /// <summary>Parses the DEM and sampled elevation columns, preferring the sampled value.</summary>
    private static (int? Meters, string? Source) ParseElevation(string demField, string sampledField, string? sourceField)
    {
        if (TryParseElevation(sampledField, out int sampled))
        {
            string source = Clean(sourceField ?? string.Empty);
            return (sampled, source.Length == 0 ? "copernicus-glo-90" : source);
        }
        if (TryParseElevation(demField, out int dem))
            return (dem, "geonames-dem");
        return (null, null);
    }

    /// <summary>Parses the legacy single elevation column.</summary>
    private static (int? Meters, string? Source) ParseLegacyElevation(string field) =>
        TryParseElevation(field, out int value) ? (value, "geonames-dem") : (null, null);

    private static bool TryParseElevation(string field, out int value) =>
        int.TryParse(field, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value is >= -500 and <= 9000;

    private static long ParsePopulation(string field) =>
        long.TryParse(field, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value) && value is >= 0 and <= 100_000_000
            ? value : 0;

    /// <summary>Joins alternate names into a normalized search text; '|' is the TSV inner separator.</summary>
    private static string BuildAliasSearchText(string aliases)
    {
        if (string.IsNullOrWhiteSpace(aliases)) return string.Empty;
        return NormalizeSearchText(aliases.Replace('|', ' '));
    }

    private static string Clean(string value) => string.Join(" ", value.Split()).Trim();

    /// <summary>Loads the bundled datasets from the application directory.</summary>
    /// <returns>The loaded geocoder; the Indonesian layer is optional and never breaks the global layer.</returns>
    private static OfflineReverseGeocoder LoadBundledData()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "geonames");
        string globalPath = Path.Combine(directory, GlobalDataFileName);
        if (!File.Exists(globalPath)) globalPath = Path.Combine(directory, LegacyGlobalDataFileName);
        IReadOnlyList<OfflineCity> cities = ReadGzip(globalPath, ParseCities);
        var (places, regencies, provinces, boundary) = LoadBundledIndonesiaLayer();
        return new OfflineReverseGeocoder(cities, places, regencies, provinces, boundary);
    }

    /// <summary>Loads only the bundled Indonesian layer, used as a fallback for downloaded data.</summary>
    private static (IndonesiaPlace[] Places, string[] Regencies, string[] Provinces, IndonesiaBoundary? Boundary)
        LoadBundledIndonesiaLayer()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "geonames");
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
        }
        return (places, regencies, provinces, boundary);
    }

    /// <summary>Reads a gzip-compressed dataset with the given parser.</summary>
    private static T ReadGzip<T>(string path, Func<TextReader, T> parser)
    {
        using var file = File.OpenRead(path);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return parser(reader);
    }

    /// <summary>Checks WGS84 latitude/longitude ranges.</summary>
    private static bool ValidCoordinates(double latitude, double longitude) =>
        double.IsFinite(latitude) && latitude is >= -90 and <= 90 &&
        double.IsFinite(longitude) && longitude is >= -180 and <= 180;

    /// <summary>Computes the great-circle distance between two points using the haversine formula.</summary>
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

    /// <summary>Translates the most common country names to Indonesian.</summary>
    private static string TranslateCountry(string englishName, string countryCode)
    {
        if (!string.IsNullOrWhiteSpace(countryCode) &&
            CountryNames.TryGetValue(countryCode.ToUpperInvariant(), out string? name))
            return name;
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
        int? ElevationMeters,
        string? ElevationSource,
        string FeatureCode,
        long Population,
        string AliasSearchText)
    {
        internal string SearchName { get; } = NormalizeSearchText(CityName);
        internal string SearchText { get; } = NormalizeSearchText(
            $"{CityName} {RegionName} {CountryName} {TranslateCountry(CountryName, CountryCode)} {CountryCode} {AliasSearchText}");
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
        internal string? ElevationSource;
        internal string FeatureCode;
        internal long Population;
        internal string District;
        internal string RegionCode;
        internal bool OfficialMatch;
        internal string SearchName;
        internal string AliasSearchText;
        internal string SearchText;
    }

    private sealed record SearchCandidate(int Rank, long Population, string DisplayName, OfflineLocationSuggestion Suggestion);
}

/// <summary>A settlement suggestion returned by search.</summary>
internal sealed record OfflineLocationSuggestion(
    string CityName,
    string RegionName,
    string CountryName,
    string CountryCode,
    string DisplayName,
    double Latitude,
    double Longitude,
    int? ElevationMeters,
    string? ElevationSource = null);

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
    bool IsIndonesiaLayer = false,
    string? ElevationSource = null);
