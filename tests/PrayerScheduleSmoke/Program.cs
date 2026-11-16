using PrayerAutomation.UI;

var date = new DateTime(2026, 1, 1);
int[] methods = [20, 99, 17, 11, 4, 13, 3, 2];
foreach (int method in methods)
{
    AppSettings settings = CreateSettings(method);
    IReadOnlyList<PrayerEntry> schedule = PrayerScheduleService.ForDate(date, settings);
    Check(schedule.Count == 7, $"method {method}: expected seven displayed times");
    Check(schedule.Select(entry => entry.Key).SequenceEqual(
        ["Fajr", "Sunrise", "Dhuha", "Dhuhr", "Asr", "Maghrib", "Isha"]),
        $"method {method}: display order changed");
    Check(schedule.All(entry => entry.Time >= TimeSpan.Zero && entry.Time < TimeSpan.FromDays(1)),
        $"method {method}: a time is outside the day");
    Check(schedule.Zip(schedule.Skip(1), (left, right) => left.Time < right.Time).All(isOrdered => isOrdered),
        $"method {method}: times are not chronological");
    Check(schedule[2].Time - schedule[1].Time == TimeSpan.FromMinutes(15),
        $"method {method}: Dhuha offset from sunrise is not 15 minutes");
}

var ummAlQura = PrayerScheduleService.ForDate(date, CreateSettings(4));
Check(ummAlQura[^1].Time - ummAlQura[^2].Time == TimeSpan.FromMinutes(90),
    "Umm al-Qura: Isha interval is not 90 minutes after Maghrib");

AppSettings baselineSettings = CreateSettings(20);
IReadOnlyDictionary<string, TimeSpan> baseline = PrayerScheduleService.ForDate(date, baselineSettings)
    .ToDictionary(entry => entry.Key, entry => entry.Time);
AppSettings correctedSettings = CreateSettings(20);
correctedSettings.Corrections["Fajr"] = 7;
correctedSettings.Corrections["Sunrise"] = -2;
correctedSettings.Corrections["Dhuha"] = 3;
IReadOnlyDictionary<string, TimeSpan> corrected = PrayerScheduleService.ForDate(date, correctedSettings)
    .ToDictionary(entry => entry.Key, entry => entry.Time);
Check(corrected["Fajr"] - baseline["Fajr"] == TimeSpan.FromMinutes(7), "Fajr correction not applied");
Check(corrected["Sunrise"] - baseline["Sunrise"] == TimeSpan.FromMinutes(-2), "Sunrise correction not applied");
Check(corrected["Dhuha"] - baseline["Dhuha"] == TimeSpan.FromMinutes(3), "Dhuha correction not applied");
foreach (string key in new[] { "Dhuhr", "Asr", "Maghrib", "Isha" })
    Check(corrected[key] == baseline[key], $"unexpected correction leaked into {key}");

AppSettings legacyMwl = CreateSettings(1);
legacyMwl.CalculationStandard = "Muslim World League";
IReadOnlyDictionary<string, TimeSpan> migratedMwl = PrayerScheduleService.ForDate(date, legacyMwl)
    .ToDictionary(entry => entry.Key, entry => entry.Time);
IReadOnlyDictionary<string, TimeSpan> directMwl = PrayerScheduleService.ForDate(date, CreateSettings(3))
    .ToDictionary(entry => entry.Key, entry => entry.Time);
Check(migratedMwl.SequenceEqual(directMwl), "legacy MWL method ID did not retain its mapped schedule");
Check(PrayerScheduleService.ReminderEntries(PrayerScheduleService.ForDate(date, CreateSettings(20))).Count == 5,
    "reminder schedule should exclude sunrise and Dhuha");
Console.WriteLine("PASS: eight calculation methods, Dhuha/Umm al-Qura derived times, manual offsets, legacy MWL mapping, and reminder filtering.");

static AppSettings CreateSettings(int method) => new()
{
    Latitude = -6.4336368,
    Longitude = 107.1934549,
    TimeZoneId = "Asia/Jakarta",
    CalculationStandard = method == 1 || method == 3 ? "Muslim World League" : "Test method",
    MethodId = method,
    DhuhaAfterSunriseMinutes = 15,
};

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
