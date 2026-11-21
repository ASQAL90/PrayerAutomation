using PrayerAutomation.UI;

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

// Every supported method is reachable from the shared catalog.
Require(LocationDefaults.Methods.Count == 8, "the shared catalog must expose all eight supported methods");
Require(LocationDefaults.Methods.Select(m => m.Id).Distinct().Count() == 8, "method IDs must be unique");

CalculationMethod For(string? country, string zone, int current = 20) =>
    LocationDefaults.ResolveAutoCalculationMethod(country, zone, current);

Require(For("ID", "Asia/Jakarta").Id == 20, "Indonesia defaults to Kemenag 20");
Require(For("ID", "Asia/Makassar").Id == 20, "eastern Indonesia still defaults to Kemenag 20");
Require(For("ID", "Asia/Jakarta", 99).Id == 99, "an explicit Muhammadiyah choice is preserved in Indonesia");
Require(For("MY", "Asia/Kuala_Lumpur").Id == 17, "Malaysia defaults to JAKIM 17");
Require(For("SG", "Asia/Singapore").Id == 11, "Singapore defaults to MUIS 11");
Require(For("SA", "Asia/Riyadh").Id == 4, "Saudi Arabia defaults to Umm al-Qura 4");
Require(For("TR", "Europe/Istanbul").Id == 13, "Turkey defaults to Diyanet 13");
Require(For("US", "America/New_York").Id == 2, "the United States defaults to ISNA 2");
Require(For("CA", "America/Toronto").Id == 2, "Canada defaults to ISNA 2");
Require(For(null, "Asia/Kuala_Lumpur").Id == 17, "the time-zone fallback still recognises Malaysia");
Require(For("ZZ", "Europe/London").Id == 3, "unknown regions use Muslim World League 3");

string jakarta = LocationDefaults.ResolveTimeZoneForCoordinates(-6.21462, 106.84513);
Require(jakarta.StartsWith("Asia/", StringComparison.Ordinal), "coordinates must resolve to a usable IANA zone");
bool rejected = false;
try { LocationDefaults.ResolveTimeZoneForCoordinates(91, 0); }
catch (ArgumentException) { rejected = true; }
Require(rejected, "out-of-range coordinates must be rejected before any network or zone lookup");

Console.WriteLine("PASS: LocationDefaults resolves every country/time-zone rule and validates coordinates.");
