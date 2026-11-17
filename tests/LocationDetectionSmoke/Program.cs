// Smoke test verifying network location detection: provider agreement, response parsing, proxy/VPN signals, and IP minimization.
using System.Net;
using System.Reflection;
using System.Text;
using System.Collections.Concurrent;
using PrayerAutomation.UI;

/// <summary>Throws <see cref="InvalidOperationException"/> when the condition does not hold.</summary>
/// <param name="condition">Condition that must be true.</param>
/// <param name="message">Failure message used when the condition is false.</param>
static void Require(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

var matchingHandler = new JsonReplyHandler(new Dictionary<string, string>
{
    ["ipapi.co"] = """{"ip":"198.51.100.17","city":"Sample City","region":"Sample Region","country_name":"Sample Country","country_code":"ZZ","latitude":"10.0","longitude":20.0,"timezone":"Etc/UTC","org":"Example ISP"}""",
    ["ipwho.is"] = """{"success":true,"ip":"198.51.100.17","city":"Sample City","region":"Sample Region","country":"Sample Country","country_code":"ZZ","latitude":10.1,"longitude":"20.1","timezone":{"id":"Etc/UTC"},"connection":{"org":"Example ISP"},"security":{"proxy":false,"vpn":false}}"""
});
using (var client = new HttpClient(matchingHandler))
{
    NetworkLocationResult result = await new LocationDetectionService(client).DetectNetworkLocationAsync();
    Require(result.ProvidersAgree, "matching provider estimates should be considered consistent");
    Require(result.IpApi.CountryCode == "ZZ" && result.IpWho.CountryCode == "ZZ", "country codes should be parsed");
    Require(result.IpApi.TimeZoneId == "Etc/UTC" && result.IpWho.TimeZoneId == "Etc/UTC", "both timezone response shapes should be parsed");
    Require(result.DistanceKm > 0 && result.DistanceKm < 25, "distance should be computed from coordinates");
    Require(matchingHandler.RequestUris.Count == 2, "both providers should be queried once");
    Require(matchingHandler.RequestUris.Any(uri => uri.Host == "ipapi.co") && matchingHandler.RequestUris.Any(uri => uri.Host == "ipwho.is"), "requests should target both configured providers");
    Require(!result.ToString()!.Contains("198.51.100.17", StringComparison.Ordinal), "the public IP address must not be retained in the result");
    Require(typeof(ProviderLocation).GetProperty("IpAddress", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) is null, "provider result must not expose an IP address property");
}

var disagreeingHandler = new JsonReplyHandler(new Dictionary<string, string>
{
    ["ipapi.co"] = """{"city":"Sample City","region":"Sample Region","country_name":"Sample Country","country_code":"ZZ","latitude":10.0,"longitude":20.0,"timezone":"Etc/UTC"}""",
    ["ipwho.is"] = """{"success":true,"city":"Other City","region":"Other Region","country":"Other Country","country_code":"YY","latitude":10.1,"longitude":20.1,"timezone":{"id":"Etc/UTC"}}"""
});
using (var client = new HttpClient(disagreeingHandler))
{
    NetworkLocationResult result = await new LocationDetectionService(client).DetectNetworkLocationAsync();
    Require(!result.ProvidersAgree, "different provider countries must not be silently accepted");
}

var failedProviderHandler = new JsonReplyHandler(new Dictionary<string, string>
{
    ["ipapi.co"] = """{"city":"Sample City","country_code":"ZZ","latitude":10.0,"longitude":20.0,"timezone":"Etc/UTC"}""",
    ["ipwho.is"] = """{"success":false,"message":"reserved test response"}"""
});
using (var client = new HttpClient(failedProviderHandler))
{
    bool failedClosed = false;
    try
    {
        _ = await new LocationDetectionService(client).DetectNetworkLocationAsync();
    }
    catch (LocationLookupException)
    {
        failedClosed = true;
    }
    Require(failedClosed, "a provider failure must not return a partial estimate for automatic application");
}

var proxyHandler = new JsonReplyHandler(new Dictionary<string, string>
{
    ["ipapi.co"] = """{"city":"Sample City","region":"Sample Region","country_name":"Sample Country","country_code":"ZZ","latitude":10.0,"longitude":20.0,"timezone":"Etc/UTC","is_proxy":true}""",
    ["ipwho.is"] = """{"success":true,"city":"Sample City","region":"Sample Region","country":"Sample Country","country_code":"ZZ","latitude":10.1,"longitude":20.1,"timezone":{"id":"Etc/UTC"},"security":{"vpn":true}}"""
});
using (var client = new HttpClient(proxyHandler))
{
    NetworkLocationResult result = await new LocationDetectionService(client).DetectNetworkLocationAsync();
    Require(!result.ProvidersAgree, "proxy/VPN signals must block automatic application even when coordinates are close");
    Require(result.IpApi.IsLikelyProxyOrVpn && result.IpWho.IsLikelyProxyOrVpn, "proxy/VPN metadata should be normalized from both providers");
}

var invalidCoordinateHandler = new JsonReplyHandler(new Dictionary<string, string>
{
    ["ipapi.co"] = """{"city":"Sample City","country_code":"ZZ","latitude":91,"longitude":20,"timezone":"Etc/UTC"}""",
    ["ipwho.is"] = """{"success":true,"city":"Sample City","country_code":"ZZ","latitude":10,"longitude":20,"timezone":{"id":"Etc/UTC"}}"""
});
using (var client = new HttpClient(invalidCoordinateHandler))
{
    bool rejectedInvalidCoordinate = false;
    try
    {
        _ = await new LocationDetectionService(client).DetectNetworkLocationAsync();
    }
    catch (LocationLookupException)
    {
        rejectedInvalidCoordinate = true;
    }
    Require(rejectedInvalidCoordinate, "out-of-range latitude/longitude must be rejected");
}

var datelineMidpoint = LocationDetectionService.Midpoint(
    new ProviderLocation("ipapi.co", null, null, null, "ZZ", 0, 179.9, "Etc/UTC", false),
    new ProviderLocation("ipwho.is", null, null, null, "ZZ", 0, -179.9, "Etc/UTC", false));
Require(Math.Abs(datelineMidpoint.Latitude) < 0.01 &&
        Math.Abs(Math.Abs(datelineMidpoint.Longitude) - 180) < 0.01,
    "spherical midpoint must stay near the international date line instead of averaging through longitude zero");

if (args.Contains("--live", StringComparer.Ordinal))
{
    try
    {
        NetworkLocationResult live = await new LocationDetectionService().DetectNetworkLocationAsync();
        Require(live.IpApi.ProviderName == "ipapi.co" && live.IpWho.ProviderName == "ipwho.is",
            "the live lookup must validate responses from both named providers");
        string distanceBand = live.DistanceKm <= 100 ? "within-100-km" : "over-100-km";
        Console.WriteLine($"LIVE_PROVIDER_CHECK=PASS; providers=ipapi.co+ipwho.is; agreement={live.ProvidersAgree}; distance_band={distanceBand}; raw_IP_and_coordinates=not_printed.");
    }
    catch (LocationLookupException)
    {
        Console.WriteLine("LIVE_PROVIDER_CHECK=BLOCKED; response details and personal network data were not printed.");
        Environment.ExitCode = 2;
    }
}

Console.WriteLine("PASS: both network providers, coordinate/time-zone normalization, disagreement handling, and IP minimization.");

/// <summary>Fake <see cref="HttpMessageHandler"/> that replies with canned JSON per provider host and records requested URIs.</summary>
sealed class JsonReplyHandler : HttpMessageHandler
{
    private readonly IReadOnlyDictionary<string, string> _responses;
    /// <summary>Gets the URIs requested so far, in order.</summary>
    public ConcurrentQueue<Uri> RequestUris { get; } = new();

    /// <summary>Creates the handler with per-host JSON responses.</summary>
    /// <param name="responses">Map of host name to canned JSON body.</param>
    public JsonReplyHandler(IReadOnlyDictionary<string, string> responses)
    {
        _responses = responses;
    }

    /// <summary>Records the request URI and returns the canned JSON for the host, or throws for unexpected hosts.</summary>
    /// <param name="request">Outgoing HTTP request.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A canned HTTP 200 JSON response.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Uri uri = request.RequestUri ?? throw new InvalidOperationException("request URI missing");
        RequestUris.Enqueue(uri);
        if (!_responses.TryGetValue(uri.Host, out string? json))
        {
            throw new InvalidOperationException("unexpected provider host");
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }
}
