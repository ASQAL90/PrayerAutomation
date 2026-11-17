using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PrayerAutomation.UI;

/// <summary>Reverse-geocodes GPS coordinates via an online Nominatim-compatible service with caching and rate limiting.</summary>
internal sealed class NominatimReverseGeocodingService
{
    /// <summary>Default Nominatim base URL.</summary>
    internal const string DefaultBaseUri = "https://nominatim.openstreetmap.org";
    private static readonly HttpClient SharedHttpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private static readonly SemaphoreSlim RequestGate = new(1, 1);
    private static readonly object CacheLock = new();
    private static readonly Dictionary<string, CacheEntry> MemoryCache = new(StringComparer.Ordinal);
    private static readonly TimeSpan MinimumRequestInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(1);
    private const int MaximumCacheEntries = 128;
    private const int MaximumResponseBytes = 65_536;
    private static DateTimeOffset _lastRequestStartedUtc = DateTimeOffset.MinValue;
    private readonly HttpClient _httpClient;

    /// <summary>Initializes the service, optionally with a caller-provided HTTP client.</summary>
    /// <param name="httpClient">Client to use; the shared client is used when null.</param>
    internal NominatimReverseGeocodingService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? SharedHttpClient;
    }

    /// <summary>Validates and normalizes a user-provided service base URL (HTTPS, no credentials/query/fragment, not an /reverse endpoint).</summary>
    /// <param name="value">Candidate base URL; falls back to the default when null/empty.</param>
    /// <param name="baseUri">The normalized base URI.</param>
    /// <param name="error">Validation error message when normalization fails.</param>
    /// <returns>True when the URL is usable.</returns>
    internal static bool TryNormalizeBaseUri(string? value, out Uri baseUri, out string error)
    {
        baseUri = new Uri(DefaultBaseUri, UriKind.Absolute);
        error = "";
        string candidateText = value?.Trim() ?? "";
        if (!Uri.TryCreate(candidateText, UriKind.Absolute, out Uri? candidate) || string.IsNullOrWhiteSpace(candidate.Host))
        {
            error = "Masukkan URL dasar layanan geocoding yang valid.";
            return false;
        }
        if (!string.Equals(candidate.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            error = "Layanan online hanya boleh memakai HTTPS.";
            return false;
        }
        if (!string.IsNullOrEmpty(candidate.UserInfo) || !string.IsNullOrEmpty(candidate.Query) || !string.IsNullOrEmpty(candidate.Fragment))
        {
            error = "URL dasar tidak boleh berisi kredensial, query, atau fragment.";
            return false;
        }
        if (candidate.AbsolutePath.TrimEnd('/').EndsWith("/reverse", StringComparison.OrdinalIgnoreCase) ||
            candidate.AbsolutePath.TrimEnd('/').EndsWith("/reverse.php", StringComparison.OrdinalIgnoreCase))
        {
            error = "Masukkan URL dasar server, bukan URL endpoint /reverse.";
            return false;
        }

        baseUri = new Uri(candidate.GetLeftPart(UriPartial.Path).TrimEnd('/'), UriKind.Absolute);
        return true;
    }

    /// <summary>Reverse-geocodes the current device location; requires explicit user consent and GPS/Wi-Fi/cellular source.</summary>
    /// <param name="acquireCurrentLocation">Acquires the current device location.</param>
    /// <param name="endpointBaseUri">Base URL of the geocoding service.</param>
    /// <param name="userConsentGranted">Must be true; online geocoding sends coordinates to the network.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The resolved location.</returns>
    /// <exception cref="InvalidOperationException">Consent missing, location unavailable, or source not GPS/Wi-Fi/cellular.</exception>
    /// <exception cref="ArgumentException"><paramref name="endpointBaseUri"/> is invalid.</exception>
    internal async Task<OnlineLocationResult> ReverseGeocodeCurrentAsync(
        Func<CancellationToken, Task<DeviceLocationResult>> acquireCurrentLocation,
        string endpointBaseUri,
        bool userConsentGranted,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acquireCurrentLocation);
        if (!userConsentGranted)
            throw new InvalidOperationException("Reverse geocoding online memerlukan persetujuan eksplisit.");
        if (!TryNormalizeBaseUri(endpointBaseUri, out Uri baseUri, out string endpointError))
            throw new ArgumentException(endpointError, nameof(endpointBaseUri));

        DeviceLocationResult position = await acquireCurrentLocation(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (!position.IsSuccess || position.Latitude is not double latitude || position.Longitude is not double longitude ||
            !DeviceLocationLogic.HasValidCoordinates(latitude, longitude))
        {
            throw new InvalidOperationException("Lokasi perangkat terbaru tidak tersedia atau tidak valid.");
        }
        if (position.Source is not (DeviceLocationSource.Satellite or DeviceLocationSource.WiFi or DeviceLocationSource.Cellular))
            throw new InvalidOperationException("Sumber lokasi bukan GPS, Wi-Fi, atau jaringan seluler; koordinat tidak dikirim.");

        string latitudeText = latitude.ToString("G", CultureInfo.InvariantCulture);
        string longitudeText = longitude.ToString("G", CultureInfo.InvariantCulture);
        string cacheKey = $"{baseUri.AbsoluteUri}|{latitudeText}|{longitudeText}";
        if (TryGetCached(cacheKey, out OnlineLocationResult? cached))
            return cached;

        await RequestGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (TryGetCached(cacheKey, out cached))
                return cached;

            TimeSpan elapsed = DateTimeOffset.UtcNow - _lastRequestStartedUtc;
            TimeSpan wait = MinimumRequestInterval - elapsed;
            if (wait > TimeSpan.Zero)
                await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            Uri requestUri = BuildRequestUri(baseUri, latitudeText, longitudeText);
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Version version = typeof(NominatimReverseGeocodingService).Assembly.GetName().Version ?? new Version(1, 0);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PrayerAutomation", version.ToString()));
            _lastRequestStartedUtc = DateTimeOffset.UtcNow;

            using HttpResponseMessage response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Layanan geocoding mengembalikan HTTP {(int)response.StatusCode}.", null, response.StatusCode);

            string json = await ReadLimitedResponseAsync(response.Content, cancellationToken).ConfigureAwait(false);
            OnlineLocationResult result = ParseResponse(json);
            AddToCache(cacheKey, result);
            return result;
        }
        finally
        {
            RequestGate.Release();
        }
    }

    /// <summary>Builds the /reverse request URI for the given coordinates.</summary>
    /// <param name="baseUri">Normalized service base URI.</param>
    /// <param name="latitude">Latitude text.</param>
    /// <param name="longitude">Longitude text.</param>
    /// <returns>The request URI.</returns>
    private static Uri BuildRequestUri(Uri baseUri, string latitude, string longitude)
    {
        string endpoint = baseUri.AbsoluteUri.TrimEnd('/') + "/reverse";
        string query = "format=jsonv2&addressdetails=1&zoom=10&accept-language=id&lat=" +
            Uri.EscapeDataString(latitude) + "&lon=" + Uri.EscapeDataString(longitude);
        return new UriBuilder(endpoint) { Query = query }.Uri;
    }

    /// <summary>Reads a response body with a hard size cap; throws when exceeded.</summary>
    /// <param name="content">The response content to read.</param>
    /// <param name="cancellationToken">Token to cancel the read.</param>
    /// <returns>The response body as text.</returns>
    /// <exception cref="InvalidDataException">The body exceeds the size cap.</exception>
    private static async Task<string> ReadLimitedResponseAsync(HttpContent content, CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength is long length && length > MaximumResponseBytes)
            throw new InvalidDataException("Respons geocoding melebihi batas ukuran yang diizinkan.");

        await using Stream responseStream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var buffer = new MemoryStream();
        byte[] chunk = new byte[8192];
        while (true)
        {
            int read = await responseStream.ReadAsync(chunk.AsMemory(0, chunk.Length), cancellationToken).ConfigureAwait(false);
            if (read == 0)
                break;
            if (buffer.Length + read > MaximumResponseBytes)
                throw new InvalidDataException("Respons geocoding melebihi batas ukuran yang diizinkan.");
            buffer.Write(chunk, 0, read);
        }
        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    /// <summary>Parses a Nominatim /reverse response into a location result.</summary>
    /// <param name="json">Response body.</param>
    /// <returns>The parsed location.</returns>
    /// <exception cref="InvalidDataException">Address details or license attribution are missing or unusable.</exception>
    private static OnlineLocationResult ParseResponse(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 16 });
        if (!document.RootElement.TryGetProperty("address", out JsonElement address) || address.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Respons geocoding tidak berisi rincian lokasi.");

        string city = FirstString(address, "city", "town", "village", "municipality", "hamlet", "locality", "suburb", "borough", "county");
        string region = FirstString(address, "state", "province", "region", "state_district");
        string country = FirstString(address, "country");
        string displayName = string.Join(", ", new[] { city, region, country }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new InvalidDataException("Layanan tidak mengembalikan nama permukiman atau wilayah yang dapat digunakan.");

        string attribution = FirstString(document.RootElement, "licence");
        if (string.IsNullOrWhiteSpace(attribution))
            throw new InvalidDataException("Respons geocoding tidak menyertakan atribusi lisensi.");
        return new OnlineLocationResult(city, region, country, displayName, attribution);
    }

    /// <summary>Returns the first non-empty trimmed string among the given properties.</summary>
    /// <param name="element">The parent element.</param>
    /// <param name="propertyNames">Property names to try in order.</param>
    /// <returns>The first non-empty value, or empty string.</returns>
    private static string FirstString(JsonElement element, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String)
            {
                string? text = value.GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }
        }
        return "";
    }

    /// <summary>Looks up a cache entry, dropping expired entries.</summary>
    /// <param name="key">Cache key.</param>
    /// <param name="result">The cached result on hit.</param>
    /// <returns>True on a fresh cache hit.</returns>
    private static bool TryGetCached(string key, out OnlineLocationResult result)
    {
        lock (CacheLock)
        {
            if (MemoryCache.TryGetValue(key, out CacheEntry? entry))
            {
                if (entry.ExpiresAtUtc > DateTimeOffset.UtcNow)
                {
                    result = entry.Result;
                    return true;
                }
                MemoryCache.Remove(key);
            }
        }
        result = null!;
        return false;
    }

    /// <summary>Adds a result to the memory cache, evicting expired and oldest entries as needed.</summary>
    /// <param name="key">Cache key.</param>
    /// <param name="result">The result to cache.</param>
    private static void AddToCache(string key, OnlineLocationResult result)
    {
        lock (CacheLock)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            foreach (string expiredKey in MemoryCache.Where(pair => pair.Value.ExpiresAtUtc <= now).Select(pair => pair.Key).ToArray())
                MemoryCache.Remove(expiredKey);
            if (MemoryCache.Count >= MaximumCacheEntries)
            {
                string oldestKey = MemoryCache.MinBy(pair => pair.Value.ExpiresAtUtc).Key;
                MemoryCache.Remove(oldestKey);
            }
            MemoryCache[key] = new CacheEntry(result, now.Add(CacheLifetime));
        }
    }

    /// <summary>Cache entry with its expiration time.</summary>
    private sealed record CacheEntry(OnlineLocationResult Result, DateTimeOffset ExpiresAtUtc);
}

/// <summary>Location resolved by the online reverse-geocoding service, including license attribution.</summary>
internal sealed record OnlineLocationResult(
    string CityName,
    string RegionName,
    string CountryName,
    string DisplayName,
    string Attribution);