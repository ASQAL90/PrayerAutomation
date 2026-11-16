using PrayerAutomation.UI;

DateTime knownGregorian = new(2026, 1, 1);
NativeCalendarDate knownHijri = NativePrayerCore.HijriDate(knownGregorian);
Check(knownHijri.Year == 1447 && knownHijri.Month == 7 && knownHijri.Day == 12 && knownHijri.WeekdayMonday0 == 3,
    "2026-01-01 Gregorian/Hijri and Monday-based weekday mapping changed");

int[] expectedMonthLengths = [30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29];
int totalDays = 0;
for (int month = 1; month <= 12; month++)
{
    NativeHijriMonth loaded = NativePrayerCore.HijriMonth(1500, month);
    Check(loaded.HijriYear == 1500 && loaded.HijriMonth == month,
        $"Hijri year 1500 month {month}: month identity changed");
    Check(loaded.DaysInMonth == expectedMonthLengths[month - 1],
        $"Hijri year 1500 month {month}: civil month length changed");
    DateTime start = new(loaded.GregorianYear, loaded.GregorianMonth, loaded.GregorianDay);
    NativeCalendarDate firstDay = NativePrayerCore.HijriDate(start);
    Check(firstDay.Year == 1500 && firstDay.Month == month && firstDay.Day == 1,
        $"Hijri year 1500 month {month}: start date does not round-trip");
    totalDays += loaded.DaysInMonth;
}
Check(totalDays == 354, $"Hijri year 1500 should contain 354 civil days, got {totalDays}");

DateTime ramadanStart = GregorianDateForHijri(1448, 9, 1);
Check(HasObservation(ramadanStart, "Awal Ramadan", CalendarObservanceKind.ReligiousEvent),
    "estimated Ramadan start marker is missing");
DateTime nuzulDate = GregorianDateForHijri(1448, 9, 17);
Check(HasObservation(nuzulDate, "Nuzulul Qur'an", CalendarObservanceKind.ReligiousEvent),
    "Nuzulul Qur'an marker is missing");
DateTime tasuaDate = GregorianDateForHijri(1448, 1, 9);
Check(HasObservation(tasuaDate, "Tasu'a", CalendarObservanceKind.SunnahFast),
    "Tasu'a Sunnah fast marker is missing");
DateTime arafahDate = GregorianDateForHijri(1448, 12, 9);
Check(HasObservation(arafahDate, "Arafah", CalendarObservanceKind.SunnahFast),
    "Arafah Sunnah fast marker is missing");
DateTime eidDate = GregorianDateForHijri(1448, 10, 1);
Check(HasObservation(eidDate, "Idulfitri", CalendarObservanceKind.EidNoFast),
    "Idulfitri no-fasting marker is missing");
DateTime eidAdhaDate = GregorianDateForHijri(1448, 12, 10);
Check(HasObservation(eidAdhaDate, "Iduladha", CalendarObservanceKind.EidNoFast),
    "Iduladha no-fasting marker is missing");

DateTime tashriqDate = GregorianDateForHijri(1448, 12, 11);
IReadOnlyList<CalendarObservance> tashriq = NativePrayerCore.CalendarObservances(tashriqDate, 0);
Check(tashriq.Any(item => item.Kind == CalendarObservanceKind.NoFast && item.Name.Contains("Hari Tasyriq", StringComparison.Ordinal)),
    "11 Dhul Hijjah must be marked as a no-fasting Tashriq day");
DateTime day13DhulHijjah = GregorianDateForHijri(1448, 12, 13);
Check(!NativePrayerCore.CalendarObservances(day13DhulHijjah, 0).Any(item => item.Name.Contains("Ayyamul Bidh", StringComparison.Ordinal)),
    "13 Dhul Hijjah must not be labeled Ayyamul Bidh");
CalendarMarker tashriqMarker = PrayerUiLogic.CalendarMarkerFor(tashriqDate, tashriqDate, false, tashriq);
Check(tashriqMarker.IsToday && tashriqMarker.HasFastingProhibition && !tashriqMarker.HasSunnahFast,
    "today/no-fasting marker categories are inconsistent for Tashriq");

DateTime newIslamicYear = GregorianDateForHijri(1448, 1, 1);
IReadOnlyList<CalendarObservance> correctedNewYear = NativePrayerCore.CalendarObservances(newIslamicYear.AddDays(-1), 1);
Check(correctedNewYear.Any(item => item.Name.Contains("Tahun Baru Islam", StringComparison.Ordinal)),
    "calendar correction must shift the Islamic New Year marker by one day");

string xaml = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "MainWindow.xaml"));
string codeBehind = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "MainWindow.xaml.cs"));
Check(xaml.Contains("AutomationProperties.AutomationId=\"GregorianCalendarTab\"", StringComparison.Ordinal) &&
      xaml.Contains("AutomationProperties.AutomationId=\"HijriCalendarTab\"", StringComparison.Ordinal),
    "both Gregorian and Hijri calendar views must remain present");
Check(HasAccessibleEventNames(codeBehind, "private Button CreateGregorianDateButton", "private void RenderHijriYear") &&
      HasAccessibleEventNames(codeBehind, "private Button CreateHijriDateButton", "private void SelectGregorianDate"),
    "both calendar views must expose observance names in their accessible button descriptions");
Check(codeBehind.Contains("tanggal resmi dapat berbeda menurut rukyat", StringComparison.OrdinalIgnoreCase),
    "the Hijri calendar must state that civil dates can differ from local rukyat/authority");

Console.WriteLine("PASS: Gregorian/Hijri round-trip, 1500 H month lengths, observance/no-fast markers, correction offset, and accessible names in both source views; no UI input automation used.");

static DateTime GregorianDateForHijri(int year, int month, int day)
{
    NativeHijriMonth start = NativePrayerCore.HijriMonth(year, month);
    return new DateTime(start.GregorianYear, start.GregorianMonth, start.GregorianDay).AddDays(day - 1);
}

static bool HasObservation(DateTime date, string namePart, CalendarObservanceKind kind) =>
    NativePrayerCore.CalendarObservances(date, 0).Any(item =>
        item.Kind == kind && item.Name.Contains(namePart, StringComparison.Ordinal));

static bool HasAccessibleEventNames(string source, string methodSignature, string nextMethodSignature)
{
    int start = source.IndexOf(methodSignature, StringComparison.Ordinal);
    if (start < 0)
        return false;
    int end = source.IndexOf(nextMethodSignature, start + methodSignature.Length, StringComparison.Ordinal);
    if (end < 0)
        return false;
    string body = source[start..end];
    return body.Contains("AutomationProperties.SetName(button", StringComparison.Ordinal) &&
           body.Contains("eventDescription", StringComparison.Ordinal);
}

static void Check(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}
