using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PrayerAutomation.UI;

/// <summary>Detects approximate network location by querying two IP geolocation providers and cross-checking them.</summary>
internal sealed class LocationDetectionService
{
    private static readonly Uri IpApiEndpoint = new("https://ipapi.co/json/");
    private static readonly Uri IpWhoEndpoint = new("https://ipwho.is/");
    private const double MaximumAgreementDistanceKm = 100.0;
    private readonly HttpClient _httpClient;

    /// <summary>Initializes the service, optionally with a caller-provided HTTP client.</summary>
    /// <param name="httpClient">Client to use; a default 8-second-timeout client is created when null.</param>
    public LocationDetectionService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };
        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("PrayerAutomation", "1.0"));
        }
    }

    /// <summary>Queries both providers concurrently and compares their answers.</summary>
    /// <param name="cancellationToken">Token to cancel the lookups.</param>
    /// <returns>Both provider results, their distance apart, and whether they agree.</returns>
    /// <exception cref="LocationLookupException">A provider is unreachable, times out, or returns invalid data.</exception>
    public async Task<NetworkLocationResult> DetectNetworkLocationAsync(
        CancellationToken cancellationToken = default)
    {
        Task<string> ipApiTask = GetJsonAsync(IpApiEndpoint, cancellationToken);
        Task<string> ipWhoTask = GetJsonAsync(IpWhoEndpoint, cancellationToken);
        await Task.WhenAll(ipApiTask, ipWhoTask).ConfigureAwait(false);

        ProviderLocation ipApi = ParseIpApi(await ipApiTask.ConfigureAwait(false));
        ProviderLocation ipWho = ParseIpWho(await ipWhoTask.ConfigureAwait(false));
        double distanceKm = DistanceKm(ipApi.Latitude, ipApi.Longitude, ipWho.Latitude, ipWho.Longitude);
        bool providersAgree =
            !string.IsNullOrWhiteSpace(ipApi.CountryCode) &&
            string.Equals(ipApi.CountryCode, ipWho.CountryCode, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(ipApi.TimeZoneId) &&
            string.Equals(ipApi.TimeZoneId, ipWho.TimeZoneId, StringComparison.OrdinalIgnoreCase) &&
            distanceKm <= MaximumAgreementDistanceKm &&
            !ipApi.IsLikelyProxyOrVpn &&
            !ipWho.IsLikelyProxyOrVpn;

        return new NetworkLocationResult(ipApi, ipWho, distanceKm, providersAgree);
    }

    /// <summary>Fetches raw JSON from an endpoint, mapping failures to <see cref="LocationLookupException"/>.</summary>
    /// <param name="endpoint">The provider URL.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>The response body.</returns>
    /// <exception cref="LocationLookupException">The request timed out or failed.</exception>
    private async Task<string> GetJsonAsync(Uri endpoint, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(
                endpoint,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new LocationLookupException($"Server lokasi {endpoint.Host} tidak merespons. Coba lagi nanti.");
        }
        catch (HttpRequestException ex)
        {
            throw new LocationLookupException($"Server lokasi {endpoint.Host} tidak bisa dihubungi. Coba lagi nanti.", ex);
        }
    }

    /// <summary>Parses an ipapi.co response.</summary>
    /// <param name="json">Response body.</param>
    /// <returns>The parsed provider location.</returns>
    /// <exception cref="LocationLookupException">The response is malformed or missing coordinates.</exception>
    private static ProviderLocation ParseIpApi(string json)
    {
        using JsonDocument document = ParseDocument(json, "ipapi.co");
        JsonElement root = document.RootElement;
        return new ProviderLocation(
            ProviderName: "ipapi.co",
            City: ReadString(root, "city"),
            Region: ReadString(root, "region"),
            Country: ReadString(root, "country_name") ?? ReadString(root, "country"),
            CountryCode: ReadString(root, "country_code"),
            Latitude: ReadCoordinate(root, "latitude", "ipapi.co"),
            Longitude: ReadCoordinate(root, "longitude", "ipapi.co"),
            TimeZoneId: ReadString(root, "timezone"),
            IsLikelyProxyOrVpn: ReadBoolean(root, "is_proxy") || ReadBoolean(root, "is_vpn"));
    }

    /// <summary>Parses an ipwho.is response.</summary>
    /// <param name="json">Response body.</param>
    /// <returns>The parsed provider location.</returns>
    /// <exception cref="LocationLookupException">The provider refused the request or returned malformed data.</exception>
    private static ProviderLocation ParseIpWho(string json)
    {
        using JsonDocument document = ParseDocument(json, "ipwho.is");
        JsonElement root = document.RootElement;
        if (!root.TryGetProperty("success", out JsonElement success) ||
            success.ValueKind != JsonValueKind.True)
        {
            throw new LocationLookupException("Server lokasi menolak permintaan. Coba lagi nanti.");
        }

        string? timeZone = null;
        if (root.TryGetProperty("timezone", out JsonElement timeZoneElement))
        {
            timeZone = timeZoneElement.ValueKind == JsonValueKind.Object
                ? ReadString(timeZoneElement, "id")
                : ReadString(timeZoneElement);
        }

        JsonElement security = default;
        bool hasSecurity = root.TryGetProperty("security", out security) && security.ValueKind == JsonValueKind.Object;
        return new ProviderLocation(
            ProviderName: "ipwho.is",
            City: ReadString(root, "city"),
            Region: ReadString(root, "region"),
            Country: ReadString(root, "country"),
            CountryCode: ReadString(root, "country_code"),
            Latitude: ReadCoordinate(root, "latitude", "ipwho.is"),
            Longitude: ReadCoordinate(root, "longitude", "ipwho.is"),
            TimeZoneId: timeZone,
            IsLikelyProxyOrVpn: hasSecurity &&
                (ReadBoolean(security, "proxy") || ReadBoolean(security, "vpn") ||
                 ReadBoolean(security, "tor") || ReadBoolean(security, "hosting")));
    }

    /// <summary>Parses JSON, wrapping syntax errors in <see cref="LocationLookupException"/>.</summary>
    /// <param name="json">Response body.</param>
    /// <param name="provider">Provider name used in error messages.</param>
    /// <returns>The parsed document.</returns>
    /// <exception cref="LocationLookupException">The JSON is invalid.</exception>
    private static JsonDocument ParseDocument(string json, string provider)
    {
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new LocationLookupException($"Server lokasi mengirim jawaban yang tidak valid.", ex);
        }
    }

    /// <summary>Reads a string-like property from an object element, or null if absent.</summary>
    /// <param name="element">The parent element.</param>
    /// <param name="propertyName">Property to read.</param>
    /// <returns>The property value as text, or null.</returns>
    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out JsonElement value)
            ? ReadString(value)
            : null;
    }

    /// <summary>Reads a scalar element as text; objects and arrays yield null.</summary>
    /// <param name="value">The element to read.</param>
    /// <returns>The value as text, or null.</returns>
    private static string? ReadString(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }
        if (value.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
        {
            return value.ToString();
        }
        return null;
    }

    /// <summary>Reads and range-checks a latitude or longitude property.</summary>
    /// <param name="element">The parent element.</param>
    /// <param name="propertyName">"latitude" or "longitude".</param>
    /// <param name="provider">Provider name used in error messages.</param>
    /// <returns>The coordinate in decimal degrees.</returns>
    /// <exception cref="LocationLookupException">The coordinate is missing, unparsable, or out of range.</exception>
    private static double ReadCoordinate(JsonElement element, string propertyName, string provider)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement value))
        {
            throw new LocationLookupException($"Server lokasi tidak mengirim koordinat yang valid.");
        }

        double coordinate = 0;
        bool parsed = value.ValueKind == JsonValueKind.Number
            ? value.TryGetDouble(out coordinate)
            : value.ValueKind == JsonValueKind.String &&
              double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out coordinate);
        if (!parsed || !double.IsFinite(coordinate) ||
            (propertyName == "latitude" && (coordinate < -90 || coordinate > 90)) ||
            (propertyName == "longitude" && (coordinate < -180 || coordinate > 180)))
        {
            throw new LocationLookupException($"Respons dari {provider} berisi koordinat di luar rentang yang valid.");
        }
        return coordinate;
    }

    /// <summary>Returns true only when the property exists and is JSON true.</summary>
    /// <param name="element">The parent element.</param>
    /// <param name="propertyName">Property to check.</param>
    /// <returns>True when the property is JSON true.</returns>
    private static bool ReadBoolean(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object &&
               element.TryGetProperty(propertyName, out JsonElement value) &&
               value.ValueKind == JsonValueKind.True;
    }

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

    /// <summary>Computes the geographic midpoint between two provider locations.</summary>
    /// <param name="first">First provider location.</param>
    /// <param name="second">Second provider location.</param>
    /// <returns>The midpoint as (latitude, longitude) in degrees.</returns>
    /// <exception cref="ArgumentException">The coordinates do not define a unique midpoint (antipodal).</exception>
    internal static (double Latitude, double Longitude) Midpoint(ProviderLocation first, ProviderLocation second)
    {
        double latitude1 = DegreesToRadians(first.Latitude);
        double longitude1 = DegreesToRadians(first.Longitude);
        double latitude2 = DegreesToRadians(second.Latitude);
        double longitude2 = DegreesToRadians(second.Longitude);
        double x = Math.Cos(latitude1) * Math.Cos(longitude1) + Math.Cos(latitude2) * Math.Cos(longitude2);
        double y = Math.Cos(latitude1) * Math.Sin(longitude1) + Math.Cos(latitude2) * Math.Sin(longitude2);
        double z = Math.Sin(latitude1) + Math.Sin(latitude2);
        double horizontalLength = Math.Sqrt(x * x + y * y);
        if (horizontalLength < 1e-12 && Math.Abs(z) < 1e-12)
        {
            throw new ArgumentException("Provider coordinates do not define a unique midpoint.");
        }
        return (RadiansToDegrees(Math.Atan2(z, horizontalLength)),
            RadiansToDegrees(Math.Atan2(y, x)));
    }

    /// <summary>Converts degrees to radians.</summary>
    /// <param name="degrees">Value in degrees.</param>
    /// <returns>Value in radians.</returns>
    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);
    /// <summary>Converts radians to degrees.</summary>
    /// <param name="radians">Value in radians.</param>
    /// <returns>Value in degrees.</returns>
    private static double RadiansToDegrees(double radians) => radians * (180.0 / Math.PI);
}

/// <summary>Location data reported by one network geolocation provider.</summary>
internal sealed record ProviderLocation(
    string ProviderName,
    string? City,
    string? Region,
    string? Country,
    string? CountryCode,
    double Latitude,
    double Longitude,
    string? TimeZoneId,
    bool IsLikelyProxyOrVpn);

/// <summary>Combined result of both network location providers, including agreement analysis.</summary>
internal sealed record NetworkLocationResult(
    ProviderLocation IpApi,
    ProviderLocation IpWho,
    double DistanceKm,
    bool ProvidersAgree);

/// <summary>Raised when a network location lookup fails or returns unusable data.</summary>
internal sealed class LocationLookupException : Exception
{
    /// <summary>Initializes the exception with a message.</summary>
    /// <param name="message">Error description.</param>
    public LocationLookupException(string message) : base(message)
    {
    }

    /// <summary>Initializes the exception with a message and inner exception.</summary>
    /// <param name="message">Error description.</param>
    /// <param name="innerException">The underlying error.</param>
    public LocationLookupException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
