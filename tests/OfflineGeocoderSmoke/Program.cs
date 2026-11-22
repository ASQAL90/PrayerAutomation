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

static OfflineLocationResult? Lookup(double latitude, double longitude) =>
    OfflineReverseGeocoder.Bundled.FindNearest(latitude, longitude);

string cities = string.Join(Environment.NewLine, new[]
{
    string.Join('\t', new[] { "Near Town", "Sample State", "Sample Country", "10.01", "20.01", "ZZ", "640" }),
    string.Join('\t', new[] { "Far City", "Other State", "Sample Country", "10.35", "20.35", "ZZ" })
});
OfflineReverseGeocoder geocoder = OfflineReverseGeocoder.Load(new StringReader(cities));
OfflineLocationResult? result = geocoder.FindNearest(10.0, 20.0);
Require(result is not null, "a nearby city should be returned for valid coordinates");
Require(result!.CityName == "Near Town", "the nearest city should win, not a farther record");
Require(result.DisplayName == "Near Town, Sample State, Sample Country", "region and country should form the local display name");
Require(result.DistanceKm > 0 && result.DistanceKm < 5, "nearest-city distance should be measured in kilometers");
Require(result.ElevationMeters == 640, "the optional DEM column should surface as the settlement height");
OfflineLocationSuggestion[] suggestions = geocoder.Search("Near").ToArray();
Require(suggestions.Length == 1 && suggestions[0].ElevationMeters == 640, "search suggestions should keep the height");
Console.WriteLine("PASS: offline nearest-city selection, locality formatting, and elevation.");
OfflineLocationResult? cicendo = Lookup(-6.91139, 107.60528);
Require(cicendo is not null && cicendo.IsIndonesiaLayer, "Cicendo should use the detailed Indonesian layer");
Require(cicendo!.DisplayName == "Cicendo, Kota Bandung, Jawa Barat, Indonesia",
    "the Indonesian layer should use Indonesian regency and province names");
Require(cicendo.ElevationMeters == 718 && cicendo.ElevationSource == "copernicus-glo-90",
    "the Indonesian layer should prefer the sampled Copernicus height");

OfflineLocationResult? jakarta = Lookup(-6.21462, 106.84513);
Require(jakarta is not null && jakarta.CityName == "Jakarta" && jakarta.IsIndonesiaLayer,
    "the bundled Indonesian layer should resolve Jakarta offline");
Require(jakarta!.DisplayName == "Jakarta, Daerah Khusus Ibukota Jakarta, Indonesia",
    "Jakarta's display name should use the Indonesian province label");
Require(jakarta.ElevationMeters == 17 && jakarta.ElevationSource == "copernicus-glo-90",
    "the bundled dataset should carry the sampled Jakarta height");
Console.WriteLine("PASS: bundled GeoNames data resolves Indonesian localities, provinces, and elevation offline.");

// Schema v2 keeps the sampled elevation and its provenance.
string v2Cities = string.Join(Environment.NewLine, new[]
{
    string.Join('\t', new[] { "V2 Town", "North", "Sample Country", "11.01", "21.01", "ZZ", "120", "145", "PPL", "5000", "aliasone|aliastwo", "copernicus-glo-90", "median-3x3" }),
    string.Join('\t', new[] { "V2 Village", "South", "Sample Country", "11.40", "21.40", "ZZ", "-9999", "", "PPL", "0", "", "", "" })
});
OfflineReverseGeocoder v2 = OfflineReverseGeocoder.Load(new StringReader(v2Cities));
OfflineLocationResult? v2Result = v2.FindNearest(11.0, 21.0);
Require(v2Result is not null && v2Result.ElevationMeters == 145 &&
        v2Result.ElevationSource == "copernicus-glo-90",
    "schema v2 must prefer sampled elevation and retain its source");
OfflineLocationResult? v2Fallback = v2.FindNearest(11.4, 21.4);
Require(v2Fallback is not null && v2Fallback.ElevationMeters is null,
    "an unknown DEM sentinel must not become a fabricated height");
string aliasQuery = v2.Search("aliasone").FirstOrDefault()?.CityName ?? string.Empty;
Require(aliasQuery == "V2 Town", "GeoNames alternate names must be searchable without rewriting place names");
Console.WriteLine("PASS: schema v2 elevation provenance, DEM fallback, and alternate-name search.");

// Cibarusa/Cibarusah and Cileungsi/Cileungsir stay distinct; coordinate ranking wins.
foreach (var (name, regency, latitude, longitude) in new[]
{
    ("Cibarusa", "Kabupaten Bekasi", -6.43028, 107.06306),
    ("Cibarusah", "Kabupaten Sukabumi", -7.2517, 106.6252),
    ("Cileungsi", "Kabupaten Bogor", -6.71, 106.87861),
    ("Cileungsir", "Kabupaten Bogor", -6.39472, 106.95917)
})
{
    OfflineLocationResult? place = Lookup(latitude, longitude);
    Require(place is not null && place.IsIndonesiaLayer && place.CityName == name && place.RegencyName == regency,
        $"{name} must resolve to its own place in {regency}");
}
OfflineLocationSuggestion[] cibarusaResults = OfflineReverseGeocoder.Current.Search("Cibarusa").ToArray();
Require(cibarusaResults.Length > 0 && cibarusaResults[0].CityName == "Cibarusa",
    "an exact place name must outrank rows that merely carry it as an alias");
Console.WriteLine("PASS: similarly named Indonesian places remain distinct and rank by exact name.");

foreach (var point in new[]
{
    (Latitude: -9.10611, Longitude: 124.8925, Province: "Nusa Tenggara Timur"),
    (Latitude: 3.9384, Longitude: 108.38401, Province: "Kepulauan Riau"),
    (Latitude: 4.09, Longitude: 117.81, Province: "Kalimantan Utara"),
    (Latitude: 3.61099, Longitude: 125.4858, Province: "Sulawesi Utara")
})
{
    OfflineLocationResult? border = Lookup(point.Latitude, point.Longitude);
    Require(border is not null && border.IsIndonesiaLayer && border.CountryName == "Indonesia" &&
            border.RegionName == point.Province,
        $"Indonesian border point {point.Latitude},{point.Longitude} should stay on the Indonesian layer");
}

OfflineLocationResult? makkah = Lookup(21.4225, 39.8262);
Require(makkah is not null && makkah.CountryName == "Arab Saudi" && !makkah.IsIndonesiaLayer,
    "global country names should be translated to Indonesian");
OfflineLocationResult? amsterdam = Lookup(52.374, 4.88969);
Require(amsterdam is not null && amsterdam.CountryName == "Belanda",
    "the Netherlands should be translated to Indonesian");
OfflineLocationResult? sarawak = Lookup(1.55, 110.35);
Require(sarawak is not null && sarawak.CountryName == "Malaysia" && !sarawak.IsIndonesiaLayer,
    "Sarawak must not be labelled Indonesia");
OfflineLocationResult? dili = Lookup(-8.56, 125.57);
Require(dili is not null && dili.CountryName == "Timor-Leste",
    "Dili must be labelled Timor-Leste");
OfflineLocationResult? singapore = Lookup(1.35, 103.85);
Require(singapore is not null && singapore.CountryName == "Singapura",
    "Singapore must use the Indonesian country label");
OfflineLocationResult? papuaNewGuinea = Lookup(-2.6, 141.0);
Require(papuaNewGuinea is not null && papuaNewGuinea.CountryCode != "ID" &&
        papuaNewGuinea.DisplayName.Contains("Papua New Guinea", StringComparison.Ordinal),
    "a point across the Papua border must not borrow the nearest Indonesian city name");
OfflineLocationResult? sebatikMalaysia = Lookup(4.25, 117.75);
Require(sebatikMalaysia is not null && sebatikMalaysia.CountryName == "Malaysia",
    "the Malaysian side of Sebatik must stay Malaysia");
Require(Lookup(-20.0, 80.0) is null, "the middle of the Indian Ocean should have no settlement label");
Console.WriteLine("PASS: border points, translated countries, and out-of-range coordinates are handled honestly.");

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
