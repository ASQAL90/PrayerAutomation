using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace PrayerAutomation.UI;

internal sealed class AppSettings
{
    public const string DefaultOnlineGeocodingEndpoint = "https://nominatim.openstreetmap.org";
    public const string DefaultReminderMessage = "Sebentar lagi waktu sholat {prayer} ({minutes_before} menit lagi).";
    public static readonly string[] CorrectionKeys = ["Fajr", "Sunrise", "Dhuha", "Dhuhr", "Asr", "Maghrib", "Isha"];

    [JsonPropertyName("latitude")] public double Latitude { get; set; } = -6.4336368;
    [JsonPropertyName("longitude")] public double Longitude { get; set; } = 107.1934549;
    [JsonPropertyName("timezone")] public string TimeZoneId { get; set; } = "Asia/Jakarta";
    [JsonPropertyName("location_name")] public string LocationName { get; set; } = "Bojongmangu, Bekasi";
    [JsonPropertyName("method")] public int MethodId { get; set; } = 20;
    [JsonPropertyName("country")] public string CalculationStandard { get; set; } = "Indonesia — Kementerian Agama";
    [JsonPropertyName("corrections")] public Dictionary<string, int> Corrections { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonPropertyName("enabled")] public bool RemindersEnabled { get; set; } = true;
    [JsonPropertyName("lock_enabled")] public bool LockScreenEnabled { get; set; }
    [JsonPropertyName("lock_screen_opt_in")] public bool LockScreenOptIn { get; set; }
    [JsonPropertyName("lock_screen_duration_minutes")] public int LockScreenDurationMinutes { get; set; } = 60;
    [JsonPropertyName("auto_lock_enabled")] public bool AutoLockEnabled { get; set; }
    [JsonPropertyName("startup")] public bool StartupEnabled { get; set; } = true;
    [JsonPropertyName("start_hidden")] public bool StartHidden { get; set; }
    [JsonPropertyName("online_geocoding_endpoint")] public string OnlineGeocodingEndpoint { get; set; } = DefaultOnlineGeocodingEndpoint;
    [JsonPropertyName("widget")] public bool WidgetEnabled { get; set; }
    [JsonPropertyName("taskbar_widget")] public bool TaskbarWidgetEnabled { get; set; }
    [JsonPropertyName("taskbar_widget_mode")] public string TaskbarWidgetMode { get; set; } = "popup";
    [JsonPropertyName("widget_mode")] public string WidgetMode { get; set; } = "essential";
    [JsonPropertyName("widget_motion")] public string WidgetMotion { get; set; } = "";
    [JsonPropertyName("widget_position")] public string WidgetPosition { get; set; } = "tc";
    [JsonPropertyName("theme")] public string ThemeId { get; set; } = "light";
    [JsonPropertyName("reminder_minutes")] public int ReminderMinutes { get; set; } = 3;
    [JsonPropertyName("reminder_message")] public string ReminderMessage { get; set; } = DefaultReminderMessage;
    [JsonPropertyName("dhuha_after_sunrise_minutes")] public int DhuhaAfterSunriseMinutes { get; set; } = 15;
    [JsonPropertyName("calendar_date_correction_days")] public int CalendarDateCorrectionDays { get; set; }
    [JsonPropertyName("hijri_date_correction_days")] public int HijriDateCorrectionDays { get; set; }
    [JsonExtensionData] public Dictionary<string, JsonElement>? OtherSettings { get; set; }

    public void Normalize()
    {
        if (!double.IsFinite(Latitude) || Latitude is < -90 or > 90) Latitude = -6.4336368;
        if (!double.IsFinite(Longitude) || Longitude is < -180 or > 180) Longitude = 107.1934549;
        TimeZoneId = string.IsNullOrWhiteSpace(TimeZoneId) ? "Asia/Jakarta" : TimeZoneId.Trim();
        LocationName = string.IsNullOrWhiteSpace(LocationName) ? "Lokasi belum diatur" : LocationName.Trim();
        OnlineGeocodingEndpoint = string.IsNullOrWhiteSpace(OnlineGeocodingEndpoint)
            ? DefaultOnlineGeocodingEndpoint
            : OnlineGeocodingEndpoint.Trim();
        CalculationStandard ??= "Indonesia — Kementerian Agama";
        ReminderMessage ??= DefaultReminderMessage;
        ReminderMinutes = Math.Clamp(ReminderMinutes, 0, 120);
        LockScreenDurationMinutes = Math.Clamp(LockScreenDurationMinutes, 1, 120);
        DhuhaAfterSunriseMinutes = Math.Clamp(DhuhaAfterSunriseMinutes, 0, 180);
        CalendarDateCorrectionDays = Math.Clamp(CalendarDateCorrectionDays, -7, 7);
        HijriDateCorrectionDays = Math.Clamp(HijriDateCorrectionDays, -7, 7);
        string savedWidgetMode = WidgetMode ?? "";
        bool legacyMinimalMode = string.Equals(savedWidgetMode, "minimal", StringComparison.OrdinalIgnoreCase);
        WidgetMode = WidgetSettingsLogic.NormalizeMode(savedWidgetMode);
        WidgetMotion = WidgetSettingsLogic.NormalizeMotion(WidgetMotion, legacyMinimalMode);
        WidgetPosition = WidgetSettingsLogic.NormalizePosition(WidgetPosition);
        // Only the popup renderer is supported; the native WinUI backend was removed.
        TaskbarWidgetMode = "popup";
        ThemeId = ThemeCatalog.NormalizeId(ThemeId);
        if (!LockScreenOptIn) LockScreenEnabled = false;

        Corrections ??= new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var normalized = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (string name in CorrectionKeys)
            normalized[name] = Corrections.TryGetValue(name, out int minutes) ? Math.Clamp(minutes, -30, 30) : 0;
        Corrections = normalized;

        // Legacy Python IDs differ from the calculation-method IDs in Rust.
        string standard = CalculationStandard.ToLowerInvariant();
        if (MethodId == 3 && (standard.Contains("muis") || standard.Contains("singapura")))
            MethodId = 11;
        else if (MethodId == 1)
            MethodId = 3;
    }
}

internal static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public static string ConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PrayerAutomation", "config.json");
    public static string? LoadWarning { get; private set; }

    public static AppSettings Load()
    {
        var settings = new AppSettings();
        try
        {
            if (File.Exists(ConfigPath))
                settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(ConfigPath), JsonOptions) ?? settings;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            LoadWarning = $"Pengaturan lama tidak dapat dibaca; file tidak diubah. {ex.Message}";
        }
        settings.Normalize();
        return settings;
    }

    public static void Save(AppSettings settings)
    {
        settings.Normalize();
        string directory = Path.GetDirectoryName(ConfigPath)!;
        Directory.CreateDirectory(directory);
        string tempPath = ConfigPath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(tempPath, ConfigPath, overwrite: true);
        LoadWarning = null;
    }
}
