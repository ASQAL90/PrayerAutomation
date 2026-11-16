using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PrayerAutomation.UI;

internal sealed class LocationDetectionService
{
    private static readonly Uri IpApiEndpoint = new("https://ipapi.co/json/");
    private static readonly Uri IpWhoEndpoint = new("https://ipwho.is/");
    private const double MaximumAgreementDistanceKm = 100.0;
    private readonly HttpClient _httpClient;

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
            throw new LocationLookupException($"Penyedia lokasi {endpoint.Host} tidak merespons dalam batas waktu.");
        }
        catch (HttpRequestException ex)
        {
            throw new LocationLookupException($"Penyedia lokasi {endpoint.Host} tidak dapat dihubungi.", ex);
        }
    }

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

    private static ProviderLocation ParseIpWho(string json)
    {
        using JsonDocument document = ParseDocument(json, "ipwho.is");
        JsonElement root = document.RootElement;
        if (!root.TryGetProperty("success", out JsonElement success) ||
            success.ValueKind != JsonValueKind.True)
        {
            throw new LocationLookupException("Penyedia lokasi ipwho.is menolak atau tidak dapat memenuhi permintaan.");
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

    private static JsonDocument ParseDocument(string json, string provider)
    {
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new LocationLookupException($"Penyedia lokasi {provider} mengirim respons yang tidak valid.", ex);
        }
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out JsonElement value)
            ? ReadString(value)
            : null;
    }

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

    private static double ReadCoordinate(JsonElement element, string propertyName, string provider)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement value))
        {
            throw new LocationLookupException($"Respons dari {provider} tidak berisi koordinat yang valid.");
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

    private static bool ReadBoolean(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object &&
               element.TryGetProperty(propertyName, out JsonElement value) &&
               value.ValueKind == JsonValueKind.True;
    }

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

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);
    private static double RadiansToDegrees(double radians) => radians * (180.0 / Math.PI);
}

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

internal sealed record NetworkLocationResult(
    ProviderLocation IpApi,
    ProviderLocation IpWho,
    double DistanceKm,
    bool ProvidersAgree);

internal sealed class LocationLookupException : Exception
{
    public LocationLookupException(string message) : base(message)
    {
    }

    public LocationLookupException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
