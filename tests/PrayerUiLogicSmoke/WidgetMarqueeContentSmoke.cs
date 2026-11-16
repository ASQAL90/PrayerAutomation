using System.Reflection;
using PrayerAutomation.UI;

internal static class WidgetMarqueeContentSmoke
{
    internal static void VerifyOfflineHadithContent(IReadOnlyList<PrayerEntry> schedule)
    {
        var settings = new AppSettings { LocationName = "Synthetic offline fixture", TimeZoneId = "SE Asia Standard Time" };
        DateTime now = new(2026, 5, 1, 10, 30, 5);
        NativeCalendarDate hijri = NativePrayerCore.HijriDate(now.Date);
        string content = WidgetMarqueeContent.Build(now, settings, hijri, schedule, null, []);
        Require(content.Contains("Hadis (ringkasan makna Indonesia):", StringComparison.Ordinal) &&
                content.Contains("https://sunnah.com/", StringComparison.Ordinal),
            "The production marquee must include an offline Indonesian meaning summary with a verifiable hadith source by default.");

        MethodInfo select = typeof(WidgetMarqueeContent).GetMethod("ReligiousMessageAt", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The offline hadith catalog must expose deterministic selection by completed marquee cycle.");
        string Message(long cycle) => (string)select.Invoke(null, [cycle])!;
        (string Reference, string Url)[] sources =
        [
            ("Sahih al-Bukhari 1", "https://sunnah.com/bukhari:1"),
            ("Sahih al-Bukhari 6464", "https://sunnah.com/bukhari:6464"),
            ("Sahih al-Bukhari 69", "https://sunnah.com/bukhari:69"),
            ("Sahih Muslim 2593", "https://sunnah.com/muslim:2593"),
            ("Sahih Muslim 1005", "https://sunnah.com/muslim:1005")
        ];
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < sources.Length; index++)
        {
            string message = Message(index);
            Require(message.StartsWith("Hadis (ringkasan makna Indonesia):", StringComparison.Ordinal) &&
                    message.Contains(sources[index].Reference, StringComparison.Ordinal) &&
                    message.EndsWith(sources[index].Url, StringComparison.Ordinal) &&
                    !message.Contains('\n') && !message.Contains('\r'),
                "Every offline hadith must retain its own reference and URL, stay on one line, and be labeled as an Indonesian meaning summary.");
            Require(Message(index) == message && Message(index + sources.Length) == message,
                "Hadith selection must be deterministic and repeat after the complete five-source cycle.");
            seen.Add(message);
        }
        Require(seen.Count == sources.Length && Message(-1) == Message(sources.Length - 1),
            "The catalog must contain five distinct source-backed messages and normalize a negative cycle safely.");
        Require(content.Contains(Message(0), StringComparison.Ordinal), "The first marquee pass must start with the first offline hadith.");
        const string custom = "Synthetic custom message";
        string customContent = WidgetMarqueeContent.Build(now, settings, hijri, schedule, null, [], custom);
        Require(customContent.Contains(custom, StringComparison.Ordinal) && !customContent.Contains("Hadis (", StringComparison.Ordinal),
            "An explicit custom message must retain the existing override behavior.");
        string suppressed = WidgetMarqueeContent.Build(now, settings, hijri, schedule, null, [], string.Empty);
        Require(!suppressed.Contains("Hadis (", StringComparison.Ordinal), "An explicitly empty message must retain the existing no-message behavior.");
        Console.WriteLine("PASS: five offline hadith meaning summaries preserve attribution, deterministic rotation, single-line text and explicit message overrides.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
