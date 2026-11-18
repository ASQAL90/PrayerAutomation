using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace PrayerAutomation.UI;

/// <summary>Persisted user settings, serialized to the config file and normalized on load.</summary>
internal sealed class AppSettings
{
    /// <summary>Default Nominatim endpoint used for online reverse geocoding.</summary>
    public const string DefaultOnlineGeocodingEndpoint = "https://nominatim.openstreetmap.org";
    /// <summary>Default reminder notification message template.</summary>
    public const string DefaultReminderMessage = "Sebentar lagi waktu sholat {prayer} ({minutes_before} menit lagi).";
    /// <summary>Maximum allowed length of a reminder message.</summary>
    public const int MaxReminderMessageLength = 200;
    /// <summary>Canonical prayer keys for which per-prayer minute corrections are stored.</summary>
    public static readonly string[] CorrectionKeys = ["Fajr", "Sunrise", "Dhuha", "Dhuhr", "Asr", "Maghrib", "Isha"];

    /// <summary>Latitude in decimal degrees (WGS84).</summary>
    [JsonPropertyName("latitude")] public double Latitude { get; set; } = -6.4336368;
    /// <summary>Longitude in decimal degrees (WGS84).</summary>
    [JsonPropertyName("longitude")] public double Longitude { get; set; } = 107.1934549;
    /// <summary>IANA time zone identifier used for schedules.</summary>
    [JsonPropertyName("timezone")] public string TimeZoneId { get; set; } = "Asia/Jakarta";
    /// <summary>Display name of the configured location.</summary>
    [JsonPropertyName("location_name")] public string LocationName { get; set; } = "Bojongmangu, Bekasi";
    /// <summary>Calculation method identifier (Rust/native method IDs).</summary>
    [JsonPropertyName("method")] public int MethodId { get; set; } = 20;
    /// <summary>Calculation standard label, e.g. Kementerian Agama.</summary>
    [JsonPropertyName("country")] public string CalculationStandard { get; set; } = "Indonesia — Kementerian Agama";
    /// <summary>Per-prayer minute corrections keyed by prayer name.</summary>
    [JsonPropertyName("corrections")] public Dictionary<string, int> Corrections { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>Whether prayer time reminders are enabled.</summary>
    [JsonPropertyName("enabled")] public bool RemindersEnabled { get; set; } = true;
    /// <summary>Whether the lock-screen image feature is enabled.</summary>
    [JsonPropertyName("lock_enabled")] public bool LockScreenEnabled { get; set; }
    /// <summary>Explicit opt-in required before the lock-screen image feature can run.</summary>
    [JsonPropertyName("lock_screen_opt_in")] public bool LockScreenOptIn { get; set; }
    /// <summary>Minutes the temporary lock-screen image stays applied.</summary>
    [JsonPropertyName("lock_screen_duration_minutes")] public int LockScreenDurationMinutes { get; set; } = 60;
    /// <summary>Whether the auto-lock scheduler is enabled.</summary>
    [JsonPropertyName("auto_lock_enabled")] public bool AutoLockEnabled { get; set; }
    /// <summary>Whether the app registers itself to run at Windows sign-in.</summary>
    [JsonPropertyName("startup")] public bool StartupEnabled { get; set; } = true;
    /// <summary>Whether the app starts hidden to the tray.</summary>
    [JsonPropertyName("start_hidden")] public bool StartHidden { get; set; }
    /// <summary>Base URL of the online reverse-geocoding service.</summary>
    [JsonPropertyName("online_geocoding_endpoint")] public string OnlineGeocodingEndpoint { get; set; } = DefaultOnlineGeocodingEndpoint;
    /// <summary>Whether the floating schedule widget is enabled.</summary>
    [JsonPropertyName("widget")] public bool WidgetEnabled { get; set; }
    /// <summary>Whether the taskbar countdown panel is enabled.</summary>
    [JsonPropertyName("taskbar_widget")] public bool TaskbarWidgetEnabled { get; set; }
    /// <summary>Taskbar widget renderer mode; only "popup" is supported.</summary>
    [JsonPropertyName("taskbar_widget_mode")] public string TaskbarWidgetMode { get; set; } = "popup";
    /// <summary>Schedule widget display mode.</summary>
    [JsonPropertyName("widget_mode")] public string WidgetMode { get; set; } = "essential";
    /// <summary>Schedule widget motion (e.g. marquee).</summary>
    [JsonPropertyName("widget_motion")] public string WidgetMotion { get; set; } = "";
    /// <summary>Schedule widget screen position code.</summary>
    [JsonPropertyName("widget_position")] public string WidgetPosition { get; set; } = "tc";
    /// <summary>Whether the Essential/Minimalist desktop widget stays above other windows.</summary>
    [JsonPropertyName("widget_always_on_top")] public bool WidgetAlwaysOnTop { get; set; } = true;
    /// <summary>Theme identifier applied at startup.</summary>
    [JsonPropertyName("theme")] public string ThemeId { get; set; } = "light";
    /// <summary>Minutes before a prayer time at which to remind.</summary>
    [JsonPropertyName("reminder_minutes")] public int ReminderMinutes { get; set; } = 3;
    /// <summary>Template for reminder notification text.</summary>
    [JsonPropertyName("reminder_message")] public string ReminderMessage { get; set; } = DefaultReminderMessage;
    /// <summary>Minutes after sunrise at which Dhuha time is set.</summary>
    [JsonPropertyName("dhuha_after_sunrise_minutes")] public int DhuhaAfterSunriseMinutes { get; set; } = 15;
    /// <summary>Offset in days applied to the computed Hijri date.</summary>
    [JsonPropertyName("hijri_date_correction_days")] public int HijriDateCorrectionDays { get; set; }
    /// <summary>Unrecognized settings preserved across round trips.</summary>
    [JsonExtensionData] public Dictionary<string, JsonElement>? OtherSettings { get; set; }

    /// <summary>Clamps values and repairs legacy settings in place; called after load and before save.</summary>
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
        if (ReminderMessage.Length > MaxReminderMessageLength)
            ReminderMessage = ReminderMessage[..MaxReminderMessageLength].TrimEnd();
        ReminderMinutes = Math.Clamp(ReminderMinutes, 0, 120);
        LockScreenDurationMinutes = Math.Clamp(LockScreenDurationMinutes, 1, 120);
        DhuhaAfterSunriseMinutes = Math.Clamp(DhuhaAfterSunriseMinutes, 0, 180);
        HijriDateCorrectionDays = Math.Clamp(HijriDateCorrectionDays, -7, 7);
        // The Gregorian date follows the computer clock; drop the retired correction key.
        OtherSettings?.Remove("calendar_date_correction_days");
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

/// <summary>Loads and saves <see cref="AppSettings"/> as JSON in the user config directory.</summary>
internal static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    /// <summary>Full path of the JSON config file.</summary>
    public static string ConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PrayerAutomation", "config.json");
    /// <summary>Non-fatal warning from the last <see cref="Load"/>, if any.</summary>
    public static string? LoadWarning { get; private set; }

    /// <summary>Loads settings from the config file, returning defaults on missing or unreadable files.</summary>
    /// <returns>The normalized settings.</returns>
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

    /// <summary>Normalizes and atomically writes settings to the config file.</summary>
    /// <param name="settings">Settings to persist.</param>
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
