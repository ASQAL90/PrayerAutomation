using System.Net;
using PrayerAutomation.UI;

/// <summary>Requires the condition and throws otherwise.</summary>
/// <param name="condition">Condition that must hold.</param>
/// <param name="message">Failure message used when the condition is false.</param>
static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

string cities = string.Join(Environment.NewLine, new[]
{
    string.Join('\t', new[] { "Near Town", "Sample State", "Sample Country", "10.01", "20.01", "ZZ" }),
    string.Join('\t', new[] { "Far City", "Other State", "Sample Country", "10.35", "20.35", "ZZ" })
});
OfflineReverseGeocoder geocoder = OfflineReverseGeocoder.Load(new StringReader(cities));
OfflineLocationResult? result = geocoder.FindNearest(10.0, 20.0);
Require(result is not null, "a nearby city should be returned for valid coordinates");
Require(result!.CityName == "Near Town", "the nearest city should win, not a farther record");
Require(result.DisplayName == "Near Town, Sample State, Sample Country", "region and country should form the local display name");
Require(result.DistanceKm > 0 && result.DistanceKm < 5, "nearest-city distance should be measured in kilometers");
Console.WriteLine("PASS: offline nearest-city selection and locality formatting.");
OfflineLocationResult? jakarta = OfflineReverseGeocoder.Bundled.FindNearest(-6.21462, 106.84513);
Require(jakarta is not null && jakarta.CityName == "Jakarta", "the bundled GeoNames data should resolve Jakarta offline");
Require(jakarta!.DisplayName.Contains("Indonesia", StringComparison.Ordinal), "the bundled dataset should supply a country label");
Console.WriteLine("PASS: bundled GeoNames data resolves Jakarta without network access.");

var handler = new RecordingHandler("""
    {"licence":"Data © OpenStreetMap contributors, ODbL 1.0. https://www.openstreetmap.org/copyright","address":{"city":"Sample City","state":"Sample State","country":"Sample Country","road":"Private Road","house_number":"17"},"display_name":"17, Private Road, Sample City, Sample State, Sample Country"}
    """);
using var httpClient = new HttpClient(handler);
var onlineGeocoder = new NominatimReverseGeocodingService(httpClient);
DeviceLocationResult syntheticDeviceLocation = new(
    DeviceLocationStatus.Success,
    10.12345,
    20.54321,
    AccuracyMeters: 15,
    Source: DeviceLocationSource.Satellite);
bool locationReadWithoutConsent = false;
bool rejectedWithoutConsent = false;
try
{
    await onlineGeocoder.ReverseGeocodeCurrentAsync(
        _ =>
        {
            locationReadWithoutConsent = true;
            return Task.FromResult(syntheticDeviceLocation);
        },
        "https://test.geocoding.example/nominatim",
        userConsentGranted: false);
}
catch (InvalidOperationException)
{
    rejectedWithoutConsent = true;
}
Require(rejectedWithoutConsent && !locationReadWithoutConsent && handler.RequestCount == 0,
    "without explicit consent neither location nor network may be requested");

OnlineLocationResult onlineResult = await onlineGeocoder.ReverseGeocodeCurrentAsync(
    _ => Task.FromResult(syntheticDeviceLocation),
    "https://test.geocoding.example/nominatim",
    userConsentGranted: true);
Require(onlineResult.DisplayName == "Sample City, Sample State, Sample Country",
    "the online response should be reduced to a locality label, not a street address");
Require(onlineResult.Attribution.Contains("OpenStreetMap contributors", StringComparison.Ordinal) &&
        onlineResult.Attribution.Contains("ODbL 1.0", StringComparison.Ordinal),
    "the response licence must be retained so the UI can show required data attribution.");
Require(handler.LastRequestUri?.Host == "test.geocoding.example" &&
        handler.LastRequestUri.AbsolutePath == "/nominatim/reverse" &&
        handler.LastRequestUri.Query.Contains("format=jsonv2", StringComparison.Ordinal) &&
        handler.LastRequestUri.Query.Contains("lat=10.12345", StringComparison.Ordinal) &&
        handler.LastRequestUri.Query.Contains("lon=20.54321", StringComparison.Ordinal) &&
        handler.LastRequestUri.Query.Contains("accept-language=id", StringComparison.Ordinal),
    "the consented request should use the configured HTTPS service with synthetic coordinates");
Require(handler.LastUserAgent.Contains("PrayerAutomation", StringComparison.Ordinal),
    "the HTTP request must identify this application for service policy compliance");
Require(handler.RequestCount == 1, "one consented lookup should issue exactly one request");

OnlineLocationResult cachedResult = await onlineGeocoder.ReverseGeocodeCurrentAsync(
    _ => Task.FromResult(syntheticDeviceLocation),
    "https://test.geocoding.example/nominatim",
    userConsentGranted: true);
Require(cachedResult.DisplayName == onlineResult.DisplayName && handler.RequestCount == 1,
    "a repeated identical location should be served from the in-memory cache");

bool ipBasedLocationRejected = false;
try
{
    await onlineGeocoder.ReverseGeocodeCurrentAsync(
        _ => Task.FromResult(syntheticDeviceLocation with { Source = DeviceLocationSource.IPAddress }),
        "https://test.geocoding.example/nominatim",
        userConsentGranted: true);
}
catch (InvalidOperationException)
{
    ipBasedLocationRejected = true;
}
Require(ipBasedLocationRejected && handler.RequestCount == 1,
    "IP-derived and saved/manual coordinates must not be sent to the online endpoint");

bool invalidEndpointRejectedBeforeLocation = false;
try
{
    await onlineGeocoder.ReverseGeocodeCurrentAsync(
        _ => throw new InvalidOperationException("location must not be read"),
        "http://not-secure.example",
        userConsentGranted: true);
}
catch (ArgumentException)
{
    invalidEndpointRejectedBeforeLocation = true;
}
Require(invalidEndpointRejectedBeforeLocation && handler.RequestCount == 1,
    "only HTTPS provider base URLs may be used, and validation must precede location access");

using var revokedConsent = new CancellationTokenSource();
bool cancellationPreventedSend = false;
try
{
    await onlineGeocoder.ReverseGeocodeCurrentAsync(
        _ =>
        {
            revokedConsent.Cancel();
            return Task.FromResult(syntheticDeviceLocation with { Latitude = 10.12346 });
        },
        "https://test.geocoding.example/nominatim",
        userConsentGranted: true,
        cancellationToken: revokedConsent.Token);
}
catch (OperationCanceledException)
{
    cancellationPreventedSend = true;
}
Require(cancellationPreventedSend && handler.RequestCount == 1,
    "revoking consent during location acquisition must prevent the HTTP request");
Console.WriteLine("PASS: online geocoding is consent-gated, HTTPS-only, source-restricted, cached, and HTTP-mocked.");

/// <summary>HTTP handler that records requests and returns a canned Nominatim response.</summary>
/// <param name="json">Canned JSON body returned for every request.</param>
sealed class RecordingHandler(string json) : HttpMessageHandler
{
    /// <summary>Number of requests received.</summary>
    public int RequestCount { get; private set; }
    /// <summary>URI of the most recent request, if any.</summary>
    public Uri? LastRequestUri { get; private set; }
    /// <summary>User-Agent header of the most recent request.</summary>
    public string LastUserAgent { get; private set; } = "";

    /// <summary>Records the request and returns the canned response.</summary>
    /// <param name="request">Incoming HTTP request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The canned 200 OK response.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestCount++;
        LastRequestUri = request.RequestUri;
        LastUserAgent = request.Headers.UserAgent.ToString();
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
    }
}
