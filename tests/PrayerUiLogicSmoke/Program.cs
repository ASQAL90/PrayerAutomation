using PrayerAutomation.UI;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

Assert(DeviceLocationLogic.SourceLabel(DeviceLocationSource.Satellite).Contains("GPS", StringComparison.OrdinalIgnoreCase),
    "Satellite-backed device location must be labeled as GPS/GNSS.");
Assert(DeviceLocationLogic.SourceLabel(DeviceLocationSource.WiFi).Contains("Wi-Fi", StringComparison.OrdinalIgnoreCase),
    "Wi-Fi-backed position must not be mislabeled as GPS.");
Assert(DeviceLocationLogic.SourceLabel(DeviceLocationSource.IPAddress).Contains("perkiraan", StringComparison.OrdinalIgnoreCase),
    "IP-based device location must be explicitly described as an estimate.");
Assert(!DeviceLocationLogic.HasValidCoordinates(91.0, 0.0) &&
       !DeviceLocationLogic.HasValidCoordinates(0.0, 181.0) &&
       DeviceLocationLogic.HasValidCoordinates(-6.2, 106.8),
    "Device coordinates must be range-checked before entering settings.");
Assert(DeviceLocationLogic.HasValidAccuracy(25.0) &&
       !DeviceLocationLogic.HasValidAccuracy(double.NaN) &&
       !DeviceLocationLogic.HasValidAccuracy(double.PositiveInfinity) &&
       !DeviceLocationLogic.HasValidAccuracy(-1.0) &&
       DeviceLocationLogic.AccuracyLabel(35.0).Contains("35 m", StringComparison.Ordinal) &&
       DeviceLocationLogic.AccuracyLabel(null).Contains("tidak dilaporkan", StringComparison.OrdinalIgnoreCase),
    "Reported accuracy must be validated and labeled honestly when unavailable.");
MethodInfo localLocationNameMethod = RequireMethod(
    typeof(DeviceLocationLogic), "FormatLocalLocationName", BindingFlags.Public | BindingFlags.Static);
string localGpsLocationName = (string)localLocationNameMethod.Invoke(null,
    [DeviceLocationSource.Satellite, -6.4336368, 107.1934549])!;
Assert(localGpsLocationName.Contains("GPS/GNSS", StringComparison.Ordinal) &&
       localGpsLocationName.Contains("-6.4336", StringComparison.Ordinal) &&
       localGpsLocationName.Contains("107.1935", StringComparison.Ordinal) &&
       !localGpsLocationName.Contains("Bojongmangu", StringComparison.OrdinalIgnoreCase) &&
       !localGpsLocationName.Contains("Bekasi", StringComparison.OrdinalIgnoreCase),
    "GPS location names must be generated locally from the reported source and coordinates, without inventing a city.");

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static JsonDocument NormalizeSettings(string json)
{
    AppSettings settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    settings.Normalize();
    return JsonDocument.Parse(JsonSerializer.Serialize(settings));
}

static bool IsJsonString(JsonElement root, string propertyName, string expected) =>
    root.TryGetProperty(propertyName, out JsonElement value) &&
    value.ValueKind == JsonValueKind.String &&
    string.Equals(value.GetString(), expected, StringComparison.Ordinal);

static XElement? FindImplicitStyle(XDocument document, XNamespace presentation, XNamespace xaml, string typeName) =>
    document.Descendants(presentation + "Style").SingleOrDefault(style =>
        (string?)style.Attribute("TargetType") == $"{{x:Type {typeName}}}" &&
        style.Attribute(xaml + "Key") is null);

static bool HasTriggerSetter(XElement? style, XNamespace presentation, string triggerProperty, string setterProperty, string setterValue) =>
    style?.Descendants(presentation + "Trigger").Any(trigger =>
        (string?)trigger.Attribute("Property") == triggerProperty &&
        trigger.Elements(presentation + "Setter").Any(setter =>
            (string?)setter.Attribute("Property") == setterProperty &&
            (string?)setter.Attribute("Value") == setterValue)) == true;

static PropertyInfo RequireProperty(Type type, string name, BindingFlags flags)
{
    PropertyInfo? property = type.GetProperty(name, flags);
    Assert(property is not null, $"Required test property '{type.Name}.{name}' is missing.");
    return property!;
}

static MethodInfo RequireMethod(Type type, string name, BindingFlags flags)
{
    MethodInfo? method = type.GetMethod(name, flags);
    Assert(method is not null, $"Required test method '{type.Name}.{name}' is missing.");
    return method!;
}

static double ContrastRatio(string foreground, string background)
{
    static double Luminance(string hex)
    {
        double Channel(int index)
        {
            double value = Convert.ToInt32(hex.Substring(index, 2), 16) / 255.0;
            return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(1) + 0.7152 * Channel(3) + 0.0722 * Channel(5);
    }

    double first = Luminance(foreground);
    double second = Luminance(background);
    return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
}

using (JsonDocument defaults = NormalizeSettings("{}"))
{
    Assert(IsJsonString(defaults.RootElement, "widget_mode", "essential") &&
           IsJsonString(defaults.RootElement, "widget_motion", "static") &&
           IsJsonString(defaults.RootElement, "widget_position", "tc") &&
           IsJsonString(defaults.RootElement, "theme", "light") &&
           defaults.RootElement.TryGetProperty("auto_lock_enabled", out JsonElement autoLockEnabled) &&
           autoLockEnabled.ValueKind == JsonValueKind.False &&
           defaults.RootElement.TryGetProperty("lock_enabled", out JsonElement lockEnabled) &&
           lockEnabled.ValueKind == JsonValueKind.False &&
           defaults.RootElement.TryGetProperty("lock_screen_opt_in", out JsonElement lockScreenOptIn) &&
           lockScreenOptIn.ValueKind == JsonValueKind.False &&
           defaults.RootElement.TryGetProperty("start_hidden", out JsonElement startHidden) &&
           startHidden.ValueKind == JsonValueKind.False &&
           IsJsonString(defaults.RootElement, "online_geocoding_endpoint", AppSettings.DefaultOnlineGeocodingEndpoint) &&
           !defaults.RootElement.TryGetProperty("online_geocoding_consent", out _),
        "New installs must default to the readable one-line widget, static text, top-center position, Light theme, lock screen and auto-lock disabled, startup visible, and explicit non-persisted geocoding consent.");
}

using (JsonDocument legacyMinimal = NormalizeSettings("{\"widget_mode\":\"minimal\"}"))
{
    Assert(IsJsonString(legacyMinimal.RootElement, "widget_mode", "essential") &&
           IsJsonString(legacyMinimal.RootElement, "widget_motion", "marquee"),
        "The legacy minimal marquee setting must migrate to the five-prayer essential mode without losing its motion preference.");
}

using (JsonDocument legacyNormal = NormalizeSettings("{\"widget_mode\":\"normal\"}"))
{
    Assert(IsJsonString(legacyNormal.RootElement, "widget_mode", "detail"),
        "The legacy normal schedule setting must migrate to the medium-detail mode.");
}

using (JsonDocument invalidSettings = NormalizeSettings("{\"widget_mode\":\"unknown\",\"widget_position\":\"invalid\",\"theme\":\"unknown\"}"))
{
    Assert(IsJsonString(invalidSettings.RootElement, "widget_mode", "essential") &&
           IsJsonString(invalidSettings.RootElement, "widget_position", "tc") &&
           IsJsonString(invalidSettings.RootElement, "theme", "light"),
        "Invalid widget positions, modes, and themes must normalize to safe defaults.");
}

using (JsonDocument persistedSettings = NormalizeSettings(
           "{\"widget_mode\":\"detail\",\"widget_motion\":\"marquee\",\"widget_position\":\"br\",\"theme\":\"nord\",\"unrelated_existing_setting\":\"preserve-me\"}"))
{
    Assert(IsJsonString(persistedSettings.RootElement, "widget_mode", "detail") &&
           IsJsonString(persistedSettings.RootElement, "widget_motion", "marquee") &&
           IsJsonString(persistedSettings.RootElement, "widget_position", "br") &&
           IsJsonString(persistedSettings.RootElement, "theme", "nord") &&
           IsJsonString(persistedSettings.RootElement, "unrelated_existing_setting", "preserve-me"),
        "Widget, position, motion, and theme choices must persist without dropping unrelated legacy config.");
}

Type? themeCatalogType = typeof(AppSettings).Assembly.GetType("PrayerAutomation.UI.ThemeCatalog");
Assert(themeCatalogType is not null, "A shared theme catalog must drive both configuration and WPF presentation.");
BindingFlags publicStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
BindingFlags publicInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
PropertyInfo paletteListProperty = RequireProperty(themeCatalogType!, "All", publicStatic);
var themePalettes = ((IEnumerable)paletteListProperty.GetValue(null)!).Cast<object>().ToArray();
string[] expectedThemeIds = ["light", "dark", "classic", "xp", "macos", "adwaita", "nord", "dracula"];
string[] actualThemeIds = themePalettes.Select(palette =>
    (string?)RequireProperty(palette.GetType(), "Id", publicInstance).GetValue(palette) ?? "").ToArray();
Assert(actualThemeIds.SequenceEqual(expectedThemeIds), "The theme catalog must contain all eight requested presets in stable order.");
MethodInfo normalizeThemeMethod = RequireMethod(themeCatalogType!, "NormalizeId", publicStatic);
Assert(string.Equals(normalizeThemeMethod.Invoke(null, ["unsupported"]) as string, "light", StringComparison.Ordinal),
    "Unknown theme identifiers must fail back to Light.");
foreach (object palette in themePalettes)
{
    string themeId = (string)RequireProperty(palette.GetType(), "Id", publicInstance).GetValue(palette)!;
    string PaletteColor(string name) => (string)RequireProperty(palette.GetType(), name, publicInstance).GetValue(palette)!;
    Assert(ContrastRatio(PaletteColor("PrimaryText"), PaletteColor("Canvas")) >= 4.5 &&
           ContrastRatio(PaletteColor("PrimaryText"), PaletteColor("Surface")) >= 4.5 &&
           ContrastRatio(PaletteColor("PrimaryText"), PaletteColor("SurfaceAlt")) >= 4.5 &&
           ContrastRatio(PaletteColor("MutedText"), PaletteColor("Surface")) >= 4.5 &&
           ContrastRatio(PaletteColor("MutedText"), PaletteColor("SurfaceAlt")) >= 4.5 &&
           ContrastRatio(PaletteColor("MutedText"), PaletteColor("Canvas")) >= 4.5 &&
           ContrastRatio(PaletteColor("AccentText"), PaletteColor("Accent")) >= 4.5 &&
           ContrastRatio(PaletteColor("ErrorText"), PaletteColor("Surface")) >= 4.5 &&
           ContrastRatio(PaletteColor("ErrorText"), PaletteColor("SurfaceAlt")) >= 4.5 &&
           ContrastRatio(PaletteColor("SuccessText"), PaletteColor("Surface")) >= 4.5 &&
           ContrastRatio(PaletteColor("SuccessText"), PaletteColor("SurfaceAlt")) >= 4.5 &&
           ContrastRatio(PaletteColor("WarningText"), PaletteColor("Surface")) >= 4.5 &&
           ContrastRatio(PaletteColor("WarningText"), PaletteColor("SurfaceAlt")) >= 4.5 &&
           ContrastRatio(PaletteColor("NoteText"), PaletteColor("Surface")) >= 4.5 &&
           ContrastRatio(PaletteColor("NoteText"), PaletteColor("SurfaceAlt")) >= 4.5 &&
           ContrastRatio(PaletteColor("WidgetText"), PaletteColor("WidgetBackground")) >= 4.5 &&
           ContrastRatio(PaletteColor("WidgetMuted"), PaletteColor("WidgetBackground")) >= 4.5 &&
           ContrastRatio(PaletteColor("WidgetAccent"), PaletteColor("WidgetBackground")) >= 4.5,
        $"Theme '{themeId}' must keep readable text and widget accents at WCAG AA contrast.");
}

DateTime themeToday = new(2026, 10, 19);
CalendarMarker themeSelection = PrayerUiLogic.CalendarMarkerFor(themeToday.AddDays(1), themeToday, isSelected: true, isHoliday: false);
CalendarMarker themeHoliday = PrayerUiLogic.CalendarMarkerFor(themeToday.AddDays(2), themeToday, isSelected: false, isHoliday: true);
string[] themePrayerKeys = ["Fajr", "Sunrise", "Dhuha", "Dhuhr", "Asr", "Maghrib", "Isha"];
foreach (ThemePalette palette in ThemeCatalog.All)
{
    CalendarThemeColors selectedColors = ThemeCatalog.CalendarColorsFor(palette, themeSelection);
    CalendarThemeColors holidayColors = ThemeCatalog.CalendarColorsFor(palette, themeHoliday);
    Assert(selectedColors.Background == palette.Accent && selectedColors.Foreground == palette.AccentText && selectedColors.Border == palette.Accent,
        $"Selected calendar dates must use the active '{palette.Id}' theme accent.");
    Assert(holidayColors.Background == palette.SurfaceAlt && holidayColors.Foreground == palette.ErrorText &&
           holidayColors.Border == palette.ErrorText && holidayColors.Marker == palette.ErrorText,
        $"Holiday calendar markers must use the active '{palette.Id}' theme error palette.");
    for (int index = 0; index < themePrayerKeys.Length; index++)
    {
        Assert(ThemeCatalog.PrayerColor(palette, themePrayerKeys[index]) == palette.PrayerColors[index],
            $"Prayer '{themePrayerKeys[index]}' must map to the active '{palette.Id}' color slot.");
        Assert(ContrastRatio(palette.PrayerColors[index], palette.SurfaceAlt) >= 4.5,
            $"Prayer '{themePrayerKeys[index]}' must remain readable on the actual '{palette.Id}' schedule-card surface.");
    }
}

XDocument xaml = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "MainWindow.xaml"));
XDocument appXaml = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "App.xaml"));
XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
string? rootBackground = (string?)xaml.Root?.Attribute("Background");
string? rootForeground = (string?)xaml.Root?.Attribute("Foreground");
Assert(rootBackground == "{DynamicResource AppBackgroundBrush}" && rootForeground == "{DynamicResource PrimaryTextBrush}",
    "The main window must bind to the active theme instead of a hard-coded color palette.");
var overviewGrid = xaml.Descendants(presentation + "Grid")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "ScheduleOverviewGrid");
Assert(overviewGrid is not null, "Overview schedule must use an explicit Grid with one vertical column.");
Assert(overviewGrid!.Element(presentation + "Grid.ColumnDefinitions")?.Elements(presentation + "ColumnDefinition").Count() == 1,
    "The overview schedule must render in a single vertical column.");
var overviewTab = xaml.Descendants(presentation + "TabItem")
    .SingleOrDefault(element => (string?)element.Attribute("AutomationProperties.AutomationId") == "OverviewTab");
Assert(overviewTab?.Descendants(presentation + "ScrollViewer").Any(element =>
           (string?)element.Attribute("VerticalScrollBarVisibility") == "Auto" &&
           (string?)element.Attribute("HorizontalScrollBarVisibility") == "Disabled") == true,
    "The complete overview must scroll vertically instead of clipping at short window heights.");
Assert(!overviewTab!.Descendants(presentation + "Grid").Any(element =>
           (element.Element(presentation + "Grid.ColumnDefinitions")?.Elements(presentation + "ColumnDefinition").Count() ?? 0) > 1),
    "All overview sections must remain in one vertical column.");

var settingsTab = xaml.Descendants(presentation + "TabItem")
    .SingleOrDefault(element => (string?)element.Attribute("AutomationProperties.AutomationId") == "SettingsTab");
var settingsStack = settingsTab?.Descendants(presentation + "ScrollViewer").SingleOrDefault()?.Element(presentation + "StackPanel");
string[] settingsSections = settingsStack?.Elements().Select(element =>
        (string?)element.Attribute("Name") ?? (string?)element.Attribute("Header") ?? "")
    .ToArray() ?? [];
Assert(settingsSections.SequenceEqual(new[]
       {
           "LocationAndEstimateRow", "CorrectionsRow", "Deteksi lokasi jaringan dan perangkat (GPS)",
           "Pengingat dan widget", "Tampilan live widget aktual", "Tampilan aplikasi", "Windows dan batas keselamatan"
       }),
    "Settings sections must be grouped in the requested order.");
var globalButtonStyle = xaml.Descendants(presentation + "Style")
    .SingleOrDefault(element => (string?)element.Attribute("TargetType") == "Button" &&
                                element.Attribute(XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml") + "Key") is null);
Assert(globalButtonStyle?.Elements(presentation + "Setter").Any(element =>
           (string?)element.Attribute("Property") == "Margin" && (string?)element.Attribute("Value") == "4") == true,
    "Standard buttons must retain a visible, consistent margin.");

var locationEstimate = xaml.Descendants(presentation + "Grid")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "LocationAndEstimateRow");
Assert(locationEstimate?.Element(presentation + "Grid.ColumnDefinitions")?.Elements(presentation + "ColumnDefinition").Count() == 2,
    "Location/calculation controls and today's estimate must be side-by-side.");
var correctionRow = xaml.Descendants(presentation + "Grid")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "CorrectionsRow");
Assert(correctionRow?.Element(presentation + "Grid.ColumnDefinitions")?.Elements(presentation + "ColumnDefinition").Count() == 2,
    "Prayer-time and calendar corrections must be side-by-side.");
foreach (string prayerKey in new[] { "Fajr", "Sunrise", "Dhuha", "Dhuhr", "Asr", "Maghrib", "Isha" })
{
    string correctionName = $"Correction{prayerKey}Box";
    string decreaseName = $"Correction{prayerKey}DecreaseButton";
    string increaseName = $"Correction{prayerKey}IncreaseButton";
    Assert(xaml.Descendants(presentation + "TextBox").Any(element =>
               (string?)element.Attribute("Name") == correctionName &&
               (string?)element.Attribute("AutomationProperties.AutomationId") == correctionName) &&
           xaml.Descendants(presentation + "Button").Any(element =>
               (string?)element.Attribute("Name") == decreaseName &&
               (string?)element.Attribute("AutomationProperties.AutomationId") == decreaseName) &&
           xaml.Descendants(presentation + "Button").Any(element =>
               (string?)element.Attribute("Name") == increaseName &&
               (string?)element.Attribute("AutomationProperties.AutomationId") == increaseName),
        $"Prayer correction '{prayerKey}' must expose stable input, decrement, and increment UI Automation controls.");
}
foreach ((string editorName, string decreaseName, string increaseName, string decreaseLabel, string increaseLabel) in new[]
         {
             ("CalendarDateCorrectionBox", "CalendarDateCorrectionDecreaseButton", "CalendarDateCorrectionIncreaseButton", "Kurangi koreksi tanggal Masehi", "Tambah koreksi tanggal Masehi"),
             ("HijriDateCorrectionBox", "HijriDateCorrectionDecreaseButton", "HijriDateCorrectionIncreaseButton", "Kurangi koreksi tanggal Hijriyah", "Tambah koreksi tanggal Hijriyah")
         })
{
    var editor = xaml.Descendants(presentation + "TextBox").SingleOrDefault(element =>
        (string?)element.Attribute("Name") == editorName &&
        (string?)element.Attribute("AutomationProperties.AutomationId") == editorName);
    var decrease = xaml.Descendants(presentation + "Button").SingleOrDefault(element =>
        (string?)element.Attribute("Name") == decreaseName &&
        (string?)element.Attribute("AutomationProperties.AutomationId") == decreaseName &&
        (string?)element.Attribute("AutomationProperties.Name") == decreaseLabel &&
        (string?)element.Attribute("Content") == "−");
    var increase = xaml.Descendants(presentation + "Button").SingleOrDefault(element =>
        (string?)element.Attribute("Name") == increaseName &&
        (string?)element.Attribute("AutomationProperties.AutomationId") == increaseName &&
        (string?)element.Attribute("AutomationProperties.Name") == increaseLabel &&
        (string?)element.Attribute("Content") == "+");
    Assert(editor is not null && decrease is not null && increase is not null,
        $"Calendar correction '{editorName}' must expose accessible decrement and increment helper buttons.");
}

var settingsPreview = xaml.Descendants().SingleOrDefault(element => (string?)element.Attribute("Name") == "SettingsSchedulePreviewGrid");
Assert(settingsPreview is not null, "Settings must show a live prayer-time estimate for the current form values.");
var detectButton = xaml.Descendants(presentation + "Button")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "DetectNetworkLocationButton");
Assert(detectButton is not null && (string?)detectButton.Attribute("Click") == "DetectNetworkLocation_Click",
    "Network location detection must remain an enabled, explicitly user-triggered action.");
var gpsButton = xaml.Descendants(presentation + "Button")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "DetectGpsLocationButton");
Assert(gpsButton is not null && (string?)gpsButton.Attribute("Click") == "DetectGpsLocation_Click",
    "GPS/device-location detection must be available from Settings.");
var widgetMode = xaml.Descendants(presentation + "ComboBox")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "WidgetModeBox");
string[] widgetModeTags = widgetMode?.Elements(presentation + "ComboBoxItem")
    .Select(item => (string?)item.Attribute("Tag") ?? "").ToArray() ?? [];
Assert(widgetModeTags.SequenceEqual(new[] { "essential", "minimalist", "detail" }),
    "Settings must expose the three distinct essential, minimalist, and detail widget modes.");
var widgetMotion = xaml.Descendants(presentation + "ComboBox")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "WidgetMotionBox");
string[] widgetMotionTags = widgetMotion?.Elements(presentation + "ComboBoxItem")
    .Select(item => (string?)item.Attribute("Tag") ?? "").ToArray() ?? [];
Assert(widgetMotionTags.SequenceEqual(new[] { "static", "marquee" }),
    "Essential mode must offer static and marquee motion settings.");
var widgetPosition = xaml.Descendants(presentation + "ComboBox")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "WidgetPositionBox");
string[] widgetPositionTags = widgetPosition?.Elements(presentation + "ComboBoxItem")
    .Select(item => (string?)item.Attribute("Tag") ?? "").ToArray() ?? [];
Assert(widgetPositionTags.SequenceEqual(new[] { "tc", "tl", "tr", "bl", "br" }),
    "The detail widget must offer top-center and all four screen corners.");
var themeBox = xaml.Descendants(presentation + "ComboBox")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "ThemeBox");
string[] themeTags = themeBox?.Elements(presentation + "ComboBoxItem")
    .Select(item => (string?)item.Attribute("Tag") ?? "").ToArray() ?? [];
Assert(themeTags.SequenceEqual(new[] { "light", "dark", "classic", "xp", "macos", "adwaita", "nord", "dracula" }),
    "Settings must expose the eight requested theme presets.");
Assert((string?)themeBox?.Attribute("SelectionChanged") == "ThemeBox_SelectionChanged",
    "Theme selection must preview immediately without persisting the unsaved value.");
var textBoxStyle = xaml.Descendants(presentation + "Style").SingleOrDefault(element =>
    (string?)element.Attribute("TargetType") == "TextBox" &&
    element.Attribute(XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml") + "Key") is null);
Assert(textBoxStyle?.Elements(presentation + "Setter").Any(setter =>
           (string?)setter.Attribute("Property") == "Background" &&
           (string?)setter.Attribute("Value") == "{DynamicResource SurfaceBrush}") == true &&
       textBoxStyle.Elements(presentation + "Setter").Any(setter =>
           (string?)setter.Attribute("Property") == "Foreground" &&
           (string?)setter.Attribute("Value") == "{DynamicResource PrimaryTextBrush}") == true,
    "TextBox foreground and background must follow the active theme palette.");
var comboBoxItemStyle = FindImplicitStyle(appXaml, presentation, xamlNamespace, "ComboBoxItem");
Assert(HasTriggerSetter(comboBoxItemStyle, presentation, "IsHighlighted", "Background", "{DynamicResource AccentBrush}") &&
       HasTriggerSetter(comboBoxItemStyle, presentation, "IsHighlighted", "Foreground", "{DynamicResource AccentTextBrush}") &&
       HasTriggerSetter(comboBoxItemStyle, presentation, "IsSelected", "Background", "{DynamicResource AccentBrush}") &&
       HasTriggerSetter(comboBoxItemStyle, presentation, "IsSelected", "Foreground", "{DynamicResource AccentTextBrush}") &&
       HasTriggerSetter(comboBoxItemStyle, presentation, "IsEnabled", "Foreground", "{DynamicResource MutedTextBrush}") &&
       HasTriggerSetter(comboBoxItemStyle, presentation, "IsKeyboardFocusWithin", "BorderBrush", "{DynamicResource AccentBrush}"),
    "ComboBox popup hover, selected, disabled, and keyboard-focus states must use explicit readable theme colors.");
var tabControlStyle = FindImplicitStyle(appXaml, presentation, xamlNamespace, "TabControl");
var tabItemStyle = FindImplicitStyle(appXaml, presentation, xamlNamespace, "TabItem");
Assert(tabItemStyle is not null && !tabItemStyle.Elements(presentation + "Setter").Any(setter =>
           (string?)setter.Attribute("Property") == "Template"),
    "Tab headers must retain the default Windows/WPF template and must not use rounded custom shapes.");
Assert(tabControlStyle is not null &&
       HasTriggerSetter(tabItemStyle, presentation, "IsMouseOver", "Background", "{DynamicResource SurfaceAltBrush}") &&
       HasTriggerSetter(tabItemStyle, presentation, "IsSelected", "Background", "{DynamicResource AccentBrush}") &&
       HasTriggerSetter(tabItemStyle, presentation, "IsSelected", "Foreground", "{DynamicResource AccentTextBrush}") &&
       HasTriggerSetter(tabItemStyle, presentation, "IsEnabled", "Foreground", "{DynamicResource MutedTextBrush}") &&
       HasTriggerSetter(tabItemStyle, presentation, "IsKeyboardFocusWithin", "BorderBrush", "{DynamicResource AccentBrush}"),
    "Tab headers must expose readable hover, selected, disabled, and keyboard-focus states from the active theme.");
var listBoxStyle = FindImplicitStyle(appXaml, presentation, xamlNamespace, "ListBox");
var listBoxItemStyle = FindImplicitStyle(appXaml, presentation, xamlNamespace, "ListBoxItem");
Assert(listBoxStyle?.Elements(presentation + "Setter").Any(setter =>
           (string?)setter.Attribute("Property") == "Background" &&
           (string?)setter.Attribute("Value") == "{DynamicResource SurfaceBrush}") == true &&
       listBoxStyle.Elements(presentation + "Setter").Any(setter =>
           (string?)setter.Attribute("Property") == "Foreground" &&
           (string?)setter.Attribute("Value") == "{DynamicResource PrimaryTextBrush}") == true &&
       HasTriggerSetter(listBoxItemStyle, presentation, "IsMouseOver", "Background", "{DynamicResource SurfaceAltBrush}") &&
       HasTriggerSetter(listBoxItemStyle, presentation, "IsSelected", "Background", "{DynamicResource AccentBrush}") &&
       HasTriggerSetter(listBoxItemStyle, presentation, "IsSelected", "Foreground", "{DynamicResource AccentTextBrush}") &&
       HasTriggerSetter(listBoxItemStyle, presentation, "IsEnabled", "Foreground", "{DynamicResource MutedTextBrush}") &&
       HasTriggerSetter(listBoxItemStyle, presentation, "IsKeyboardFocusWithin", "BorderBrush", "{DynamicResource AccentBrush}"),
    "Selection lists must have theme-aware, readable hover, selected, disabled, and keyboard-focus states.");
var lockScreen = xaml.Descendants(presentation + "CheckBox")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "LockScreenCheckBox");
Assert(lockScreen is not null && (string?)lockScreen.Attribute("IsEnabled") == "False",
    "Lock-screen writes must stay disabled while the active image cannot be safely snapshotted and restored.");
var autoLock = xaml.Descendants(presentation + "CheckBox")
    .SingleOrDefault(element => (string?)element.Attribute("Name") == "AutoLockEnabledCheckBox");
Assert(autoLock is not null &&
       (string?)autoLock.Attribute("AutomationProperties.AutomationId") == "AutoLockEnabledCheckBox" &&
       (((string?)autoLock.Attribute("Content")) ?? string.Join(" ", autoLock.Descendants(presentation + "TextBlock")
           .Select(element => (string?)element.Attribute("Text"))))
           .Contains("lima waktu", StringComparison.OrdinalIgnoreCase),
    "Auto-lock must be exposed as a separate, accessible five-prayer opt-in control.");

string codeBehind = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "MainWindow.xaml.cs"));
Assert(codeBehind.Contains("CorrectionInputBehavior.Attach", StringComparison.Ordinal) &&
       codeBehind.Contains("CorrectionInputBehavior.TryParseCorrection", StringComparison.Ordinal),
    "Prayer correction controls must use the filtered editor behavior and the same bounded save/preview parser.");
Assert(codeBehind.Contains("CorrectionInputBehavior.AttachDateCorrection(CalendarDateCorrectionBox, CalendarDateCorrectionDecreaseButton, CalendarDateCorrectionIncreaseButton)", StringComparison.Ordinal) &&
       codeBehind.Contains("CorrectionInputBehavior.AttachDateCorrection(HijriDateCorrectionBox, HijriDateCorrectionDecreaseButton, HijriDateCorrectionIncreaseButton)", StringComparison.Ordinal) &&
       codeBehind.Contains("CorrectionInputBehavior.TryParseDateCorrection(CalendarDateCorrectionBox.Text", StringComparison.Ordinal) &&
       codeBehind.Contains("CorrectionInputBehavior.TryParseDateCorrection(HijriDateCorrectionBox.Text", StringComparison.Ordinal),
    "Both calendar date corrections must use filtered ±7-day editors and the same bounded save parser.");
Assert(codeBehind.Contains("AutoLockEnabledCheckBox.IsChecked = _settings.AutoLockEnabled", StringComparison.Ordinal) &&
       codeBehind.Contains("candidate.AutoLockEnabled = AutoLockEnabledCheckBox.IsChecked == true", StringComparison.Ordinal) &&
       codeBehind.Contains("destination.AutoLockEnabled = source.AutoLockEnabled", StringComparison.Ordinal),
    "The auto-lock opt-in must load, persist, and copy independently of reminders and lock-screen images.");
int constructorStart = codeBehind.IndexOf("internal MainWindow(AppSettings settings)", StringComparison.Ordinal);
int initializeComponent = codeBehind.IndexOf("InitializeComponent();", constructorStart, StringComparison.Ordinal);
int loadingGuard = codeBehind.IndexOf("_loadingSettings = true;", constructorStart, StringComparison.Ordinal);
Assert(constructorStart >= 0 && loadingGuard >= constructorStart && initializeComponent > loadingGuard,
    "Initial XAML selection events must not replace the previously saved theme before settings are loaded.");
Assert(codeBehind.Contains("private async void DetectGpsLocation_Click", StringComparison.Ordinal),
    "The GPS button must have an asynchronous device-location handler.");
Assert(codeBehind.Contains("WidgetModeBox", StringComparison.Ordinal) &&
       codeBehind.Contains("WidgetMotionBox", StringComparison.Ordinal) &&
       codeBehind.Contains("WidgetPositionBox", StringComparison.Ordinal) &&
       codeBehind.Contains("ThemeBox", StringComparison.Ordinal) &&
       codeBehind.Contains("candidate.WidgetMode", StringComparison.Ordinal) &&
       codeBehind.Contains("candidate.WidgetMotion", StringComparison.Ordinal) &&
       codeBehind.Contains("candidate.WidgetPosition", StringComparison.Ordinal) &&
       codeBehind.Contains("candidate.ThemeId", StringComparison.Ordinal) &&
       codeBehind.Contains("destination.WidgetMode", StringComparison.Ordinal) &&
       codeBehind.Contains("destination.WidgetMotion", StringComparison.Ordinal) &&
       codeBehind.Contains("destination.WidgetPosition", StringComparison.Ordinal) &&
       codeBehind.Contains("destination.ThemeId", StringComparison.Ordinal),
    "Widget mode/motion/position and theme must be loaded, validated, persisted, and copied into active settings.");
string themeManagerSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ThemeManager.cs"));
Assert(themeManagerSource.Contains("Current = ThemeCatalog.Get(themeId)", StringComparison.Ordinal) &&
       themeManagerSource.Contains("resources[\"AppBackgroundBrush\"]", StringComparison.Ordinal) &&
       themeManagerSource.Contains("resources[\"AccentBrush\"]", StringComparison.Ordinal) &&
       themeManagerSource.Contains("resources[\"CardCornerRadius\"]", StringComparison.Ordinal) &&
       codeBehind.Contains("ThemeCatalog.CalendarColorsFor(ThemeManager.Current", StringComparison.Ordinal),
    "The saved theme must map to application resources and calendar markers.");
string reminderPopupSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ReminderPopup.cs"));
Assert(reminderPopupSource.Contains("ThemePalette palette = ThemeManager.Current", StringComparison.Ordinal) &&
       reminderPopupSource.Contains("ThemeManager.ToBrush(palette.Accent)", StringComparison.Ordinal) &&
       reminderPopupSource.Contains("ThemeManager.ToBrush(palette.PrimaryText)", StringComparison.Ordinal) &&
       reminderPopupSource.Contains("ThemeManager.ToBrush(palette.Border)", StringComparison.Ordinal),
    "Reminder popups must share the active application palette.");
Assert(!xaml.Descendants().Attributes().Any(attribute =>
           (attribute.Name.LocalName is "Background" or "Foreground" or "BorderBrush") && attribute.Value.StartsWith("#", StringComparison.Ordinal)),
    "Main-window presentation colors must be theme resources rather than fixed hex values.");
Assert(codeBehind.Contains("LockScreenCheckBox.IsChecked = _settings.LockScreenEnabled && _settings.LockScreenOptIn", StringComparison.Ordinal) &&
       codeBehind.Contains("candidate.LockScreenEnabled = lockScreenOptIn", StringComparison.Ordinal) &&
       codeBehind.Contains("candidate.LockScreenOptIn = lockScreenOptIn", StringComparison.Ordinal) &&
       codeBehind.Contains("LockScreenCheckBox.IsEnabled = safety.CanChangeTemporarily || alreadyEnabled", StringComparison.Ordinal) &&
       codeBehind.Contains("LockScreenSafetyProbe.Inspect().CanChangeTemporarily", StringComparison.Ordinal),
    "The explicit lock-screen opt-in must persist only when preflight indicates a safe snapshot and restore path.");
string lockScreenProbe = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "LockScreenSafetyProbe.cs"));
Assert(lockScreenProbe.Contains("UserProfilePersonalizationSettings.IsSupported", StringComparison.Ordinal) &&
       lockScreenProbe.Contains("LockScreen.OriginalImageFile", StringComparison.Ordinal) &&
       !lockScreenProbe.Contains("TrySetLockScreenImageAsync", StringComparison.Ordinal) &&
       !lockScreenProbe.Contains("SystemParametersInfo", StringComparison.Ordinal) &&
       !lockScreenProbe.Contains("LockWorkStation", StringComparison.Ordinal),
    "The lock-screen probe must stay read-only and must not use desktop wallpaper or session-lock APIs.");
string lockScreenBackend = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "WindowsLockScreenImageBackend.cs"));
string lockScreenWorkflow = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "LockScreenImageWorkflow.cs"));
string lockScreenScheduler = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "LockScreenImageScheduler.cs"));
string lockScreenStore = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "LockScreenRestoreStore.cs"));
string appStartup = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "App.xaml.cs"));
string uiProject = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "PrayerAutomation.UI.csproj"));
string lockScreenNative = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "LockScreenNativePlatform.cs"));
Assert(lockScreenNative.Contains("LockScreen.SetImageFileAsync", StringComparison.Ordinal) &&
       lockScreenNative.Contains("Registry.CurrentUser.OpenSubKey", StringComparison.Ordinal) &&
       lockScreenBackend.Contains("WriteModeFlags(snapshot.PreviousSlideshowEnabled, snapshot.PreviousSpotlightEnabled,", StringComparison.Ordinal) &&
       !lockScreenBackend.Contains("Control Panel\\Desktop", StringComparison.Ordinal) &&
       !lockScreenBackend.Contains("SystemParametersInfo", StringComparison.Ordinal) &&
       !lockScreenBackend.Contains("requireAdministrator", StringComparison.OrdinalIgnoreCase),
    "The Windows backend must touch only the lock-screen API and current-user mode flags, never desktop wallpaper or elevation.");
var lockScreenModeWriteCalls = System.Text.RegularExpressions.Regex.Matches(lockScreenBackend, @"_native\.WriteModeFlags\(([^;]*)\);");
Assert(lockScreenModeWriteCalls.Count == 3 &&
       lockScreenModeWriteCalls.Cast<System.Text.RegularExpressions.Match>()
           .All(write => write.Groups[1].Value.Contains("canWrite:", StringComparison.Ordinal)),
    "Apply, maintenance and restore must each pass an ownership guard to the native mode-write boundary.");
Assert(lockScreenWorkflow.Contains("TemporaryDuration = TimeSpan.FromMinutes(60)", StringComparison.Ordinal) &&
       lockScreenWorkflow.Contains("CheckTemporaryImage", StringComparison.Ordinal) &&
       lockScreenWorkflow.Contains("IsRestored", StringComparison.Ordinal) &&
       lockScreenScheduler.Contains("\"Fajr\", \"Dhuhr\", \"Asr\", \"Maghrib\", \"Isha\"", StringComparison.Ordinal) &&
       lockScreenScheduler.Contains("RestoreAsync(CancellationToken.None)", StringComparison.Ordinal),
    "The workflow must schedule only the five obligatory prayers, persist a bounded restoration deadline, and restore on exit.");
Assert(lockScreenStore.Contains("Flush(flushToDisk: true)", StringComparison.Ordinal) &&
       lockScreenStore.Contains("QuarantineInvalidJournal", StringComparison.Ordinal) &&
       appStartup.Contains("new LockScreenImageScheduler", StringComparison.Ordinal) &&
       appStartup.Contains("StartAsync(DateTimeOffset.UtcNow)", StringComparison.Ordinal) &&
       uiProject.Contains("../../sedang_sholat.png", StringComparison.Ordinal) &&
       uiProject.Contains("CopyToPublishDirectory=\"PreserveNewest\"", StringComparison.Ordinal),
    "The recovery journal must be durable, startup must recover it before scheduling, and the bundled image must ship with the portable build.");
string startupSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "StartupRegistration.cs"));
Assert(startupSource.Contains("Registry.CurrentUser.CreateSubKey", StringComparison.Ordinal) &&
       codeBehind.Contains("StartupRegistration.SetEnabled", StringComparison.Ordinal) &&
       codeBehind.Contains("StartupRegistration.IsEnabled", StringComparison.Ordinal),
    "Startup control must use the current-user Run key and read back the registration state.");
string widgetSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ScheduleWidgetWindow.cs"));
Assert(widgetSource.Contains("RenderEssentialSchedule", StringComparison.Ordinal) &&
       widgetSource.Contains("RenderMinimalistSchedule", StringComparison.Ordinal) &&
       widgetSource.Contains("RenderDetailSchedule", StringComparison.Ordinal) &&
       widgetSource.Contains("PrayerUiLogic.NextPrayerAfter", StringComparison.Ordinal) &&
       widgetSource.Contains("PrayerUiLogic.FormatCountdown", StringComparison.Ordinal) &&
       widgetSource.Contains("WidgetSettingsLogic.PlaceWidget", StringComparison.Ordinal) &&
       widgetSource.Contains("ThemeManager.Current", StringComparison.Ordinal),
    "The widget must have three distinct layouts, actual prayer countdowns, themed colors, and safe screen placement.");
Assert(widgetSource.Contains("PrayerUiLogic.EssentialPrayerTimes(entries)", StringComparison.Ordinal) &&
       widgetSource.Contains("for (int index = 0; index < essential.Count; index++)", StringComparison.Ordinal) &&
       widgetSource.Contains("Text = $\"{entry.Label}  {time}\"", StringComparison.Ordinal) &&
       widgetSource.Contains("new GridLength(1, GridUnitType.Star)", StringComparison.Ordinal) &&
       widgetSource.Contains("EssentialMarqueeViewport", StringComparison.Ordinal) &&
       widgetSource.Contains("Motion, \"marquee\"", StringComparison.Ordinal) &&
       widgetSource.Contains("_appearancePreview?.Motion ?? _settings.WidgetMotion", StringComparison.Ordinal),
    "Essential mode must render five prayer labels/times across the static layout and retain its marquee viewport.");
Assert(widgetSource.Contains("Berikutnya: {countdown.PrayerLabel} · {time}", StringComparison.Ordinal) &&
       widgetSource.Contains("_countdown.Text = remaining", StringComparison.Ordinal),
    "Minimalist mode must contain only the next obligatory prayer and its remaining countdown.");
Assert(widgetSource.Contains("JADWAL HARI INI", StringComparison.Ordinal) &&
       widgetSource.Contains("_clock.Text = now.ToString(\"HH:mm:ss\"", StringComparison.Ordinal) &&
       widgetSource.Contains("_date.Text = now.ToString(\"dddd, d MMM yyyy\"", StringComparison.Ordinal) &&
       widgetSource.Contains("pukul {time} · {remaining}", StringComparison.Ordinal),
    "Medium detail mode must show live clock, date, daily schedule, and next-prayer countdown.");
var countdownSchedule = new PrayerEntry[]
{
    new("Fajr", "Subuh", TimeSpan.FromHours(4)),
    new("Sunrise", "Terbit", TimeSpan.FromHours(5)),
    new("Dhuha", "Duha", TimeSpan.FromHours(6)),
    new("Dhuhr", "Dzuhur", TimeSpan.FromHours(12)),
    new("Asr", "Ashar", TimeSpan.FromHours(15)),
    new("Maghrib", "Maghrib", TimeSpan.FromHours(18)),
    new("Isha", "Isya", TimeSpan.FromHours(19))
};
MethodInfo essentialMethod = RequireMethod(typeof(PrayerUiLogic), "EssentialPrayerTimes", publicStatic);
var essentialSchedule = ((IEnumerable<PrayerEntry>)essentialMethod.Invoke(null, [countdownSchedule])!).ToArray();
Assert(essentialSchedule.Select(entry => entry.Key).SequenceEqual(new[] { "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha" }),
    "The essential one-line widget must include only the five daily obligatory prayers, in time order.");
Type? widgetSettingsLogicType = typeof(AppSettings).Assembly.GetType("PrayerAutomation.UI.WidgetSettingsLogic");
Assert(widgetSettingsLogicType is not null, "Widget mode and placement normalization must be shared and unit-testable.");
MethodInfo placeWidgetMethod = RequireMethod(widgetSettingsLogicType!, "PlaceWidget", publicStatic);
(object placement, double left, double top)[] placements =
[
    (placeWidgetMethod.Invoke(null, ["detail", "tc", 0d, 0d, 1920d, 1080d, 420d, 360d, 12d])!, 750d, 0d),
    (placeWidgetMethod.Invoke(null, ["detail", "tl", 0d, 0d, 1920d, 1080d, 420d, 360d, 12d])!, 12d, 0d),
    (placeWidgetMethod.Invoke(null, ["detail", "tr", 0d, 0d, 1920d, 1080d, 420d, 360d, 12d])!, 1488d, 0d),
    (placeWidgetMethod.Invoke(null, ["detail", "bl", 0d, 0d, 1920d, 1080d, 420d, 360d, 12d])!, 12d, 708d),
    (placeWidgetMethod.Invoke(null, ["detail", "br", 0d, 0d, 1920d, 1080d, 420d, 360d, 12d])!, 1488d, 708d),
    (placeWidgetMethod.Invoke(null, ["essential", "br", 0d, 0d, 1920d, 1080d, 420d, 64d, 12d])!, 1488d, 1004d)
];
foreach ((object placement, double expectedLeft, double expectedTop) in placements)
{
    double actualLeft = Convert.ToDouble(RequireProperty(placement.GetType(), "Left", publicInstance).GetValue(placement));
    double actualTop = Convert.ToDouble(RequireProperty(placement.GetType(), "Top", publicInstance).GetValue(placement));
    Assert(Math.Abs(actualLeft - expectedLeft) < 0.01 && Math.Abs(actualTop - expectedTop) < 0.01,
        "All widget modes must honor top-center defaults and the selected screen corners.");
}
object clippedPlacement = placeWidgetMethod.Invoke(null,
    ["detail", "br", 100d, 20d, 300d, 220d, 420d, 360d, 12d])!;
Assert(Convert.ToDouble(RequireProperty(clippedPlacement.GetType(), "Width", publicInstance).GetValue(clippedPlacement)) == 276d &&
       Convert.ToDouble(RequireProperty(clippedPlacement.GetType(), "Height", publicInstance).GetValue(clippedPlacement)) == 196d,
    "Widget placement must clamp medium-layout dimensions to the usable work area.");
PrayerCountdown? nextPrayer = PrayerUiLogic.NextPrayerAfter(
    new DateTime(2026, 5, 1, 19, 30, 0), _ => countdownSchedule);
Assert(nextPrayer is { PrayerLabel: "Subuh", Remaining: var remaining } && remaining == TimeSpan.FromHours(8.5) &&
       PrayerUiLogic.FormatCountdown(remaining) == "08:30:00",
    "The countdown must skip Sunrise/Dhuha and roll forward to the next mandatory prayer.");
string locationService = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "DeviceLocationService.cs"));
Assert(locationService.Contains("Geolocator.RequestAccessAsync", StringComparison.Ordinal) &&
       locationService.Contains("PositionSource", StringComparison.Ordinal),
    "Device location must request Windows permission and report the actual platform source.");
int locationMethodStart = locationService.IndexOf("public async Task<DeviceLocationResult> DetectAsync", StringComparison.Ordinal);
int locationRequest = locationService.IndexOf("Geolocator.RequestAccessAsync", locationMethodStart, StringComparison.Ordinal);
int locationTry = locationService.IndexOf("try", locationMethodStart, StringComparison.Ordinal);
Assert(locationMethodStart >= 0 && locationTry > locationMethodStart && locationRequest > locationTry &&
       locationService.Contains("catch (TimeoutException)", StringComparison.Ordinal) &&
       locationService.Contains("catch (OperationCanceledException)", StringComparison.Ordinal) &&
       locationService.Contains("DeviceLocationStatus.PermissionDenied", StringComparison.Ordinal),
    "Permission requests and position reads must map timeouts/cancellation and denied access to explicit statuses.");
int gpsHandlerStart = codeBehind.IndexOf("private async void DetectGpsLocation_Click", StringComparison.Ordinal);
int gpsHandlerEnd = codeBehind.IndexOf("private static string DescribeLocationProvider", gpsHandlerStart, StringComparison.Ordinal);
Assert(gpsHandlerStart >= 0 && gpsHandlerEnd > gpsHandlerStart &&
       codeBehind[gpsHandlerStart..gpsHandlerEnd].Contains("DeviceLocationLogic.FormatLocalLocationName", StringComparison.Ordinal) &&
       codeBehind[gpsHandlerStart..gpsHandlerEnd].Contains("GeoTimeZone.TimeZoneLookup.GetTimeZone", StringComparison.Ordinal),
    "GPS detection must update the local source/coordinate label and map timezone offline from coordinates.");
Assert(codeBehind.Contains("TodayRing-", StringComparison.Ordinal) &&
       codeBehind.Contains("HijriTodayRing-", StringComparison.Ordinal),
    "Today's date must receive a distinct marker in both Gregorian and Hijri calendars.");
Assert(codeBehind.Contains("PrayerUiLogic.PresentationFor", StringComparison.Ordinal),
    "The overview schedule must use the tested prayer-specific icon/color presentation.");

var positions = PrayerUiLogic.ColumnFirstPositions(7, 2);
var expectedPositions = new[]
{
    new GridCellPosition(0, 0), new GridCellPosition(0, 1), new GridCellPosition(0, 2),
    new GridCellPosition(0, 3), new GridCellPosition(1, 0), new GridCellPosition(1, 1),
    new GridCellPosition(1, 2)
};
Assert(positions.SequenceEqual(expectedPositions), "The two-column settings preview must fill its first column top-to-bottom before column two.");
Assert(WidgetLifecycleLogic.ForSettingsChange(false, false) == WidgetVisibilityAction.None &&
       WidgetLifecycleLogic.ForSettingsChange(false, true) == WidgetVisibilityAction.Show &&
       WidgetLifecycleLogic.ForSettingsChange(true, false) == WidgetVisibilityAction.Hide &&
       WidgetLifecycleLogic.ForSettingsChange(true, true) == WidgetVisibilityAction.None,
    "Widget visibility must change only when the saved enabled setting changes.");
string appSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "App.xaml.cs"));
Assert(appSource.Contains("new AutoLockScheduler(Settings)", StringComparison.Ordinal) &&
       appSource.Contains("_autoLock?.Refresh()", StringComparison.Ordinal) &&
       appSource.Contains("_autoLock?.Dispose()", StringComparison.Ordinal),
    "The separate auto-lock scheduler must start, refresh, and stop with the app lifecycle.");
Assert(appSource.Contains("WidgetLifecycleLogic.ForSettingsChange", StringComparison.Ordinal) &&
       appSource.Contains("ShowWidget();", StringComparison.Ordinal) &&
       appSource.Contains("_widget?.Hide();", StringComparison.Ordinal),
    "Saving widget settings must create/show it when enabled and hide it when disabled.");
Assert(appSource.Split("ThemeManager.Apply(this, Settings.ThemeId)", StringSplitOptions.None).Length >= 3 &&
       appSource.Contains("ApplyThemePreview(string themeId)", StringComparison.Ordinal) &&
       appSource.Contains("_mainWindow.RestoreSavedSettingsPreview();", StringComparison.Ordinal) &&
       codeBehind.Contains("internal void RestoreSavedSettingsPreview()", StringComparison.Ordinal) &&
       codeBehind.Contains("RestoreWidgetAppearancePreview();", StringComparison.Ordinal) &&
       codeBehind.Contains("ThemeBox_SelectionChanged", StringComparison.Ordinal) &&
       codeBehind.Contains("ApplyThemePreview(themeId)", StringComparison.Ordinal) &&
       codeBehind.Contains("RestoreSavedThemePreview();", StringComparison.Ordinal) &&
       appSource.Contains("_widget?.RefreshSchedule()", StringComparison.Ordinal) &&
       codeBehind.Contains("RefreshCalendarViews();", StringComparison.Ordinal),
    "Theme preview must update dependent views and restore the saved palette on Cancel, close, and tray reopen.");

string[] prayerKeys = ["Fajr", "Sunrise", "Dhuha", "Dhuhr", "Asr", "Maghrib", "Isha"];
var presentations = prayerKeys.Select(PrayerUiLogic.PresentationFor).ToArray();
Assert(presentations.Select(p => p.Foreground).Distinct(StringComparer.OrdinalIgnoreCase).Count() == prayerKeys.Length,
    "Each schedule entry must have its own color.");
Assert(presentations[0].IconKind == PrayerIconKind.Moon && presentations[6].IconKind == PrayerIconKind.Moon,
    "Fajr and Isha must use the moon icon.");
Assert(presentations.Skip(1).Take(5).All(p => p.IconKind == PrayerIconKind.Sun),
    "Sunrise through Maghrib must use the sun icon.");
Assert(presentations.Select(p => p.AccessibleDescription).All(s => !string.IsNullOrWhiteSpace(s)),
    "Every icon needs an accessible description.");

var today = new DateTime(2026, 10, 19);
var todayMarker = PrayerUiLogic.CalendarMarkerFor(today, today, isSelected: false, isHoliday: false);
Assert(todayMarker.IsToday && todayMarker.BorderThickness >= 3 && todayMarker.Border == "#0067B8",
    "Today must have a prominent blue ring even when it is not selected.");
var selectedMarker = PrayerUiLogic.CalendarMarkerFor(today.AddDays(1), today, isSelected: true, isHoliday: false);
Assert(!selectedMarker.IsToday && selectedMarker.IsSelected && selectedMarker.BorderThickness >= 2,
    "Selected dates must remain visually distinct from today.");
var ordinaryMarker = PrayerUiLogic.CalendarMarkerFor(today.AddDays(2), today, isSelected: false, isHoliday: false);
Assert(!ordinaryMarker.IsToday && ordinaryMarker.BorderThickness == 1,
    "Ordinary dates must retain a quiet baseline style.");

DateTime locationLocalNow = new(2026, 10, 19, 23, 55, 0);
Assert(PrayerUiLogic.PrayerScheduleDate(locationLocalNow) == new DateTime(2026, 10, 19),
    "Prayer times must be calculated for the location's actual date.");
Assert(PrayerUiLogic.CalendarDisplayDate(locationLocalNow, -1) == new DateTime(2026, 10, 18),
    "Calendar correction may shift displayed dates without changing the prayer schedule date.");

WidgetMarqueeContentSmoke.VerifyOfflineHadithContent(countdownSchedule);
Console.WriteLine("PASS: layout grouping/scrolling, widget setting lifecycle, countdown, GPS status handling, safe lock-screen gate, startup registration, calendar markers, and presentation logic.");
