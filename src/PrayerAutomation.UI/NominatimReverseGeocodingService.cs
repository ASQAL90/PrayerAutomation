using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PrayerAutomation.UI;

internal sealed class NominatimReverseGeocodingService
{
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

    internal NominatimReverseGeocodingService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? SharedHttpClient;
    }

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

    private static Uri BuildRequestUri(Uri baseUri, string latitude, string longitude)
    {
        string endpoint = baseUri.AbsoluteUri.TrimEnd('/') + "/reverse";
        string query = "format=jsonv2&addressdetails=1&zoom=10&accept-language=id&lat=" +
            Uri.EscapeDataString(latitude) + "&lon=" + Uri.EscapeDataString(longitude);
        return new UriBuilder(endpoint) { Query = query }.Uri;
    }

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

    private sealed record CacheEntry(OnlineLocationResult Result, DateTimeOffset ExpiresAtUtc);
}

internal sealed record OnlineLocationResult(
    string CityName,
    string RegionName,
    string CountryName,
    string DisplayName,
    string Attribution);