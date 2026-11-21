using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace PrayerAutomation.UI;

/// <summary>Persisted user settings, serialized to the config file and normalized on load.</summary>
internal sealed class AppSettings
{
    /// <summary>Default Nominatim endpoint used for online reverse geocoding.</summary>
    public const string DefaultOnlineGeocodingEndpoint = "https://nominatim.openstreetmap.org";
    /// <summary>Calendar source value for the civil/tabular Hijri calendar.</summary>
    public const string CalendarSourceCivil = "civil";
    /// <summary>Calendar source value for the official KHGT month table.</summary>
    public const string CalendarSourceKhgt = "khgt";
    /// <summary>Label of the Muhammadiyah prayer-time method.</summary>
    public const string MuhammadiyahMethodLabel = LocationDefaults.MuhammadiyahLabel;
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
    /// <summary>Observer height above sea level in metres; 0 means sea level.</summary>
    [JsonPropertyName("elevation_meters")] public int ElevationMeters { get; set; }
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
    [JsonPropertyName("lock_screen_duration_minutes")] public int LockScreenDurationMinutes { get; set; } = 15;
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
    /// <summary>Whether the app checks GitHub for updates automatically.</summary>
    [JsonPropertyName("update_auto_check")] public bool UpdateAutoCheck { get; set; } = true;
    /// <summary>Update tag the user chose to skip, or null.</summary>
    [JsonPropertyName("ignored_update_version")] public string? IgnoredUpdateVersion { get; set; }
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
    /// <summary>Whether mouse clicks pass through the desktop widget to the window underneath.</summary>
    [JsonPropertyName("widget_click_through")] public bool WidgetClickThrough { get; set; }
    /// <summary>Whether the Essential/Minimalist widget hides while the cursor is near it.</summary>
    [JsonPropertyName("widget_auto_hide_on_hover")] public bool WidgetAutoHideOnHover { get; set; }
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
    /// <summary>Hijri calendar source: civil/tabular or the official KHGT table.</summary>
    [JsonPropertyName("calendar_source")] public string CalendarSource { get; set; } = CalendarSourceCivil;
    /// <summary>True after the one-time KHGT offer has been answered on an existing installation.</summary>
    [JsonPropertyName("khgt_offer_seen")] public bool KhgtOfferSeen { get; set; }
    /// <summary>True when the official KHGT table is the selected Hijri calendar source.</summary>
    [JsonIgnore] public bool UsesKhgtCalendar => string.Equals(CalendarSource, CalendarSourceKhgt, StringComparison.OrdinalIgnoreCase);
    /// <summary>Unrecognized settings preserved across round trips.</summary>
    [JsonExtensionData] public Dictionary<string, JsonElement>? OtherSettings { get; set; }

    /// <summary>Creates the settings a new installation starts with, including the official KHGT calendar.</summary>
    /// <returns>Normalized factory defaults.</returns>
    public static AppSettings CreateFreshInstall()
    {
        var settings = new AppSettings { CalendarSource = CalendarSourceKhgt };
        settings.Normalize();
        return settings;
    }

    /// <summary>Clamps values and repairs legacy settings in place; called after load and before save.</summary>
    public void Normalize()
    {
        if (!double.IsFinite(Latitude) || Latitude is < -90 or > 90) Latitude = -6.4336368;
        if (!double.IsFinite(Longitude) || Longitude is < -180 or > 180) Longitude = 107.1934549;
        ElevationMeters = Math.Clamp(ElevationMeters, 0, 9000);
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
        LockScreenDurationMinutes = Math.Clamp(LockScreenDurationMinutes, 1, 60);
        DhuhaAfterSunriseMinutes = Math.Clamp(DhuhaAfterSunriseMinutes, 0, 180);
        HijriDateCorrectionDays = Math.Clamp(HijriDateCorrectionDays, -7, 7);
        CalendarSource = string.Equals(CalendarSource, CalendarSourceKhgt, StringComparison.OrdinalIgnoreCase)
            ? CalendarSourceKhgt
            : CalendarSourceCivil;
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
        // The Muhammadiyah method only sets Fajr/Isha angles; drop the retired "hisab" suffix.
        if (MethodId == 99 && standard.Contains("muhammadiyah"))
            CalculationStandard = MuhammadiyahMethodLabel;
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
    /// <summary>Previous config kept before every save, used by the one-click restore.</summary>
    public static string BackupPath => ConfigPath + ".bak";
    /// <summary>Non-fatal warning from the last <see cref="Load"/>, if any.</summary>
    public static string? LoadWarning { get; private set; }
    /// <summary>Non-fatal warning from the last backup attempt, if any.</summary>
    public static string? BackupWarning { get; private set; }

    /// <summary>Loads settings from the config file, returning defaults on missing or unreadable files.</summary>
    /// <returns>The normalized settings.</returns>
    public static AppSettings Load()
    {
        var settings = AppSettings.CreateFreshInstall();
        try
        {
            if (File.Exists(ConfigPath))
                settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(ConfigPath), JsonOptions) ?? settings;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            LoadWarning = $"Pengaturan lama tidak dapat dibaca; file tidak diubah. {ex.Message}";
        }
        // Pengaturan lama tetap mempertahankan pilihannya dan mendapat tawaran sekali di jendela utama.
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
        BackupWarning = null;
        if (File.Exists(ConfigPath))
        {
            try
            {
                string backupTemp = BackupPath + ".tmp";
                File.Copy(ConfigPath, backupTemp, overwrite: true);
                File.Move(backupTemp, BackupPath, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // A backup failure must not block saving the user's current choices.
                BackupWarning = "Cadangan pengaturan gagal dibuat: " + ex.Message;
            }
        }
        string tempPath = ConfigPath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(tempPath, ConfigPath, overwrite: true);
        LoadWarning = null;
    }

    /// <summary>Restores the previous config file and returns the normalized settings.</summary>
    /// <returns>The settings read from <see cref="BackupPath"/>.</returns>
    /// <exception cref="FileNotFoundException">No backup exists yet.</exception>
    /// <exception cref="InvalidDataException">The backup cannot be deserialized.</exception>
    public static AppSettings RestoreFromBackup()
    {
        if (!File.Exists(BackupPath))
            throw new FileNotFoundException("Belum ada cadangan pengaturan untuk dipulihkan.", BackupPath);
        string json = File.ReadAllText(BackupPath);
        AppSettings restored = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions)
            ?? throw new InvalidDataException("Cadangan pengaturan tidak berisi data yang valid.");
        restored.Normalize();
        string directory = Path.GetDirectoryName(ConfigPath)!;
        Directory.CreateDirectory(directory);
        string tempPath = ConfigPath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(restored, JsonOptions));
        File.Move(tempPath, ConfigPath, overwrite: true);
        LoadWarning = null;
        BackupWarning = null;
        return restored;
    }

    /// <summary>
    /// Deletes the saved config and its backup, then returns fresh-install defaults.
    /// Log, update, and lock-screen recovery files are deliberately untouched.
    /// </summary>
    /// <returns>The normalized factory settings for the current session.</returns>
    /// <exception cref="IOException">One or more settings files could not be deleted.</exception>
    public static AppSettings Reset()
    {
        var errors = new List<string>();
        foreach (string path in new[] { ConfigPath, BackupPath, ConfigPath + ".tmp", BackupPath + ".tmp" })
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                errors.Add(Path.GetFileName(path) + ": " + ex.Message);
            }
        }
        if (errors.Count > 0)
            throw new IOException("Pengaturan tidak dapat dihapus seluruhnya: " + string.Join("; ", errors));
        LoadWarning = null;
        BackupWarning = null;
        return AppSettings.CreateFreshInstall();
    }
}
