extern alias ProductUi;

using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using Expression = System.Linq.Expressions.Expression;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

// Copy-only probes use actual portable IL and actual compiled MainWindow BAML.
// Native condition forcing, Windows location acquisition, and product startup are forbidden.
internal static class CopyHumanizeSmoke
{
    private const BindingFlags I = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags S = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly string[] Themes = ["light", "dark", "classic", "xp", "macos", "adwaita", "nord", "dracula"];
    private static readonly string[] SafetyFlags = ["RemindersEnabled", "AutoLockEnabled", "LockScreenEnabled",
        "LockScreenOptIn", "StartupEnabled", "StartHidden", "WidgetEnabled"];
    private static readonly Assembly Product = typeof(ProductUi::PrayerAutomation.UI.App).Assembly;

    public static void RunOnlineAdvanced(Application application)
    {
        string directory = OutputDirectory("online-advanced");
        var rows = new List<object>();
        using (var fixture = new CopyFixture(application, directory))
        {
            // Disconnect acquisition/network owners in this synthetic Window only.
            foreach (string name in new[] { "_deviceLocationService", "_locationDetectionService", "_onlineReverseGeocoder" })
            {
                FieldInfo service = fixture.MainWindow.GetType().GetField(name, I)
                    ?? throw new InvalidOperationException("Fail closed: missing acquisition/network owner.");
                service.SetValue(fixture.MainWindow, null);
                Assert(service.GetValue(fixture.MainWindow) is null, "Fail closed: acquisition/network owner must stay disconnected.");
            }
            var advanced = fixture.MainWindow.FindName("OnlineGeocodingAdvancedExpander") as Expander;
            Assert(advanced is not null && !advanced.IsExpanded,
                "ONLINE_ADVANCED_DEFAULT: the online server, consent, disclosures and action must be behind a closed-by-default advanced toggle.");
            var expander = advanced!;
            var header = fixture.MainWindow.FindName("OnlineGeocodingAdvancedHeader") as TextBlock;
            Assert(header is not null && header.Text == "Lanjutan — pencarian nama online",
                "ONLINE_ADVANCED_HEADER: use a concise, explicit Indonesian label.");
            string[] onlineNames = ["OnlineGeocodingEndpointBox", "OnlineGeocodingEndpointInfo", "OnlineGeocodingConsentDisclosure",
                "OnlineReverseGeocodeConsentCheckBox", "OnlineReverseGeocodeButton", "OnlineReverseGeocodeStatus"];
            FrameworkElement[] online = onlineNames.Select(name => fixture.Field<FrameworkElement>(name)).ToArray();
            var consent = fixture.Field<CheckBox>("OnlineReverseGeocodeConsentCheckBox");
            var button = fixture.Field<Button>("OnlineReverseGeocodeButton");
            var endpoint = fixture.Field<TextBox>("OnlineGeocodingEndpointBox");
            Assert(expander.Content is StackPanel panel && panel.Children.Count == 11,
                "ONLINE_ADVANCED_CONTENT: the entire existing online block must be retained, including unnamed privacy/policy/attribution paragraphs.");
            foreach (string theme in Themes)
            foreach (int width in new[] { 940, 1180 })
            {
                fixture.SelectTheme(theme);
                fixture.MainWindow.Width = width;
                fixture.MainWindow.Height = width == 940 ? 650 : 820;
                Pump(application, fixture);
                Assert(!expander.IsExpanded && online.All(control => !control.IsVisible),
                    "ONLINE_ADVANCED_COLLAPSED: online fields/actions must be effectively invisible, not simply disabled or offscreen.");
                Assert(new[] { "DetectNetworkLocationButton", "DetectGpsLocationButton", "OfflineReverseGeocodeButton" }
                    .All(name => fixture.Field<Button>(name).IsVisible && fixture.Field<Button>(name).IsEnabled),
                    "ONLINE_ADVANCED_NORMAL: network/device/GPS and offline actions must remain outside the advanced section.");
                expander.BringIntoView();
                Pump(application, fixture);
                fixture.MainWindow.UpdateLayout();
                double collapsedHeight = expander.ActualHeight;
                Assert(collapsedHeight > 0 && collapsedHeight <= 60 && header!.IsVisible,
                    "ONLINE_ADVANCED_COMPACT: closed section must consume header height only.");
                string Stable() => JsonSerializer.Serialize(new
                {
                    settings = JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType),
                    form = new[] { "LocationNameBox", "LatitudeBox", "LongitudeBox", "TimeZoneBox", "LockScreenDurationBox" }
                        .Select(name => new { name, text = name == "TimeZoneBox" ? fixture.Field<ComboBox>(name).Text : fixture.Field<TextBox>(name).Text }),
                    endpoint = endpoint.Text, consent = consent.IsChecked, actionEnabled = button.IsEnabled,
                    status = fixture.Field<TextBlock>("OnlineReverseGeocodeStatus").Text,
                    fixture.SaveWrites, fixture.StartupWrites
                });
                string stable = Stable();
                expander.IsExpanded = true;
                Pump(application, fixture);
                fixture.MainWindow.UpdateLayout();
                Assert(online.All(control => control.IsVisible && ReferenceEquals(Ancestor<Expander>(control), expander)),
                    "ONLINE_ADVANCED_EXPANDED: the existing online block must become visible inside the actual Expander.");
                Assert(expander.ActualHeight > collapsedHeight + 100 && Stable() == stable && !button.IsEnabled && consent.IsChecked is false,
                    "ONLINE_ADVANCED_NO_SIDE_EFFECT: opening must not grant consent, acquire location, request online, change drafts/settings or save.");
                var body = (StackPanel)expander.Content;
                TextBlock[] help = body.Children.OfType<TextBlock>()
                    .Where(text => ReferenceEquals(text.Style, fixture.MainWindow.FindResource("FormHelpText"))).ToArray();
                Assert(help.Length == 7 && help.All(text => text.TextWrapping == TextWrapping.Wrap && text.FontSize == 14 &&
                    text.LineHeight == 20 && text.TextAlignment == TextAlignment.Left),
                    "ONLINE_ADVANCED_PARAGRAPHS: retain shared readable wrapping/spacing for all existing help and disclosure text.");
                double expandedHeight = expander.ActualHeight;
                expander.BringIntoView();
                Pump(application, fixture);
                Color foreground = ((SolidColorBrush)header!.Foreground).Color;
                Color background = ((SolidColorBrush)expander.Background).Color;
                double contrast = Contrast(foreground, background);
                Assert(contrast >= 4.5, "ONLINE_ADVANCED_HEADER_CONTRAST: real theme header text must remain readable.");
                string capture = Path.Combine(directory, theme + "-" + width + "-header.png");
                int glyphs = CaptureGlyphs(header, expander.Background, foreground, capture);
                Assert(glyphs > 0, "ONLINE_ADVANCED_HEADER_RENDER: actual header glyphs must be rendered, not only correct brush properties.");
                expander.IsExpanded = false;
                Pump(application, fixture);
                fixture.MainWindow.UpdateLayout();
                Assert(online.All(control => !control.IsVisible) && Math.Abs(expander.ActualHeight - collapsedHeight) < .5 && Stable() == stable,
                    "ONLINE_ADVANCED_RECLOSE: collapse must remove the online area and blank height without changing consent or other state.");
                VerifySaveReachable(application, fixture);
                rows.Add(new { theme, formWidth = width, collapsedHeight, expandedHeight, contrast, headerGlyphPixels = glyphs,
                    capture, captureSha256 = HashFile(capture), entireOnlineBlockToggles = true, normalActionsVisible = true,
                    draftAndSavedStateUnchanged = true, consentStillFalse = true });
            }
            expander.IsExpanded = true;
            Pump(application, fixture);
            // The actual wired click is forced while disabled to check the early no-consent branch.
            // All acquisition/network owner fields were disconnected before this event.
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Pump(application, fixture);
            Assert(fixture.Field<TextBlock>("OnlineReverseGeocodeStatus").Text.StartsWith("Centang", StringComparison.Ordinal) &&
                consent.IsChecked is false && !button.IsEnabled,
                "ONLINE_ADVANCED_CONSENT_REQUIRED: a forced actual click without consent must return before any acquisition/request.");
            consent.IsChecked = true;
            Pump(application, fixture);
            Assert(button.IsEnabled, "ONLINE_ADVANCED_CONSENT_WIRING: valid endpoint plus explicit consent must retain original action enablement.");
            endpoint.Text = "http://example.invalid";
            Pump(application, fixture);
            Assert(consent.IsChecked is false && !button.IsEnabled,
                "ONLINE_ADVANCED_ENDPOINT_REVOKES: changing the endpoint must revoke consent and keep invalid HTTP unavailable.");
            Assert(fixture.SaveWrites == 0 && fixture.StartupWrites == 0 && !File.Exists(fixture.SavePath),
                "Online advanced interaction must not save even through mock boundaries.");
            Assert(fixture.MainWindow.GetType().GetField("_onlineGeocodeCancellation", I)!.GetValue(fixture.MainWindow) is null,
                "No online lookup may start in this isolated toggle/early-consent test.");
        }
        WpfApplicationIsolation.Verify(application);
        Assert(rows.Count == 16, "Advanced realization coverage must be 8 themes x 2 widths.");
        WriteEvidence(directory, "online-advanced", rows, new HashSet<string>(), new
        {
            themes = 8, widths = new[] { 940, 1180 }, realizations = rows.Count, actualCompiledMainWindowBaml = true,
            normalActionsRemainVisible = true, collapsedAndExpandedExercised = true,
            actualConsentAndEndpointEventsExercised = true, deviceAcquisitionAndNetworkRequestExercised = false,
            note = "Uses existing pre-constructor false-flag/scratch-save/startup-mock fixture; existing constructor read-only capability inspection remains; acquisition/network owners are disconnected before toggle/early-consent events."
        });
        Console.WriteLine("ONLINE_ADVANCED PASS: default closed; complete block open/close; 16 actual-WPF theme/width cases; header glyphs/contrast; GPS/offline preserved; actual consent/endpoint events, no acquisition/request/save.");
    }

    public static void RunSafety(Application application)
    {
        var failures = new HashSet<string>(StringComparer.Ordinal);
        var rows = new List<object>();
        string directory = OutputDirectory("safety");
        MethodInfo inspect = Product.GetType("PrayerAutomation.UI.LockScreenSafetyProbe", true)!.GetMethod("Inspect", S)!;
        // Compiled ldstr order is inspected, not a source parse or a reconstructed expected template.
        string[] messages = Decode(inspect).Where(x => x.OpCode == OpCodes.Ldstr)
            .Select(x => (string)x.Operand!).Where(x => x.EndsWith(".", StringComparison.Ordinal)).ToArray();
        Assert(messages.Length == 8 && messages[1].StartsWith("Mode Spotlight", StringComparison.Ordinal) &&
            messages[2].StartsWith("Mode slideshow", StringComparison.Ordinal),
            "Fail closed: compiled Inspect status-literal shape has changed; do not invoke native detection to find it.");
        var cases = new[]
        {
            new SafetyCase("unsupported", 0, "Windows belum mengonfirmasi dukungan perubahan gambar lock screen. Fitur tetap nonaktif."),
            new SafetyCase("spotlight", 1, "Mode Spotlight: gambar saja bisa diganti sementara. Setelah durasi pilihan habis, aplikasi mencoba kembali ke Spotlight. Fitur gambar tidak mengunci komputer."),
            new SafetyCase("slideshow", 2, "Mode slideshow: gambar saja bisa diganti sementara. Setelah durasi pilihan habis, aplikasi mencoba kembali ke slideshow. Fitur gambar tidak mengunci komputer."),
            new SafetyCase("missing-local-image", 5, "File gambar lock screen lokal untuk pemulihan tidak tersedia. Fitur tetap nonaktif.")
        };
        using (var fixture = new CopyFixture(application, directory))
        {
            TextBlock text = fixture.Field<TextBlock>("LockScreenSafetyText");
            foreach (string theme in Themes)
            foreach (int width in new[] { 940, 1180 })
            {
                fixture.SelectTheme(theme);
                fixture.MainWindow.Width = width;
                fixture.MainWindow.Height = width == 940 ? 650 : 820;
                Pump(application, fixture);
                string before = NonTextState(fixture);
                foreach (SafetyCase item in cases)
                {
                    text.Text = messages[item.LiteralIndex]; // Actual SUT text, never item.Expected.
                    var measured = RealizeParagraph(application, fixture, text,
                        Path.Combine(directory, theme + "-" + width + "-" + item.Key + ".png"));
                    Assert(NonTextState(fixture) == before,
                        "Safety copy must not change saved flags, control enabled/checked state, duration/defaults, consent, settings or neighboring statuses.");
                    if (text.Text != item.Expected)
                        failures.Add("COPY_SAFETY_WORDING " + item.Key + ": actual compiled status retains jargon or a stronger restoration promise; expected exactly the approved humanized text.");
                    rows.Add(new { theme, formWidth = width, status = item.Key, actualCompiledText = text.Text,
                        textMatches = text.Text == item.Expected, measured, nonTextStateUnchanged = true,
                        nonTextStateSha256 = HashText(before) });
                }
                VerifySaveReachable(application, fixture);
            }
            Assert(fixture.SaveWrites == 0 && fixture.StartupWrites == 0 && !File.Exists(fixture.SavePath),
                "Copy rendering must not save settings or call startup setters, even the mock ones.");
        }
        WpfApplicationIsolation.Verify(application);
        Assert(rows.Count == 64, "Safety realization coverage must be exactly 4 statuses x 8 themes x 2 widths.");
        WriteEvidence(directory, "safety", rows, failures, new
        {
            statusCount = 4, themes = 8, widths = new[] { 940, 1180 }, realizations = rows.Count,
            compiledMethod = inspect.DeclaringType!.FullName + ".Inspect",
            sutTextOrigin = "actual compiled ldstr; placed on actual LockScreenSafetyText",
            nativeFourConditionBranchesExercised = false,
            conditionForcing = false,
            note = "The original Window constructor performs its existing read-only capability inspection; no target native condition is forced. All controlled copy cases extract IL without invoking Inspect."
        });
        Assert(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Console.WriteLine("COPY_SAFETY PASS: actual compiled 4 statuses, 64 WPF realizations, 8 themes/2 widths; unchanged nontext state, wrap/typography/glyph contrast; native four-condition acceptance not exercised.");
    }

    public static void RunGps(Application application)
    {
        var failures = new HashSet<string>(StringComparer.Ordinal);
        var rows = new List<object>();
        string directory = OutputDirectory("gps");
        GpsResumeRoute route = GuardGpsResumeRoute(); // Fail closed BEFORE any MoveNext or permission/acquisition call.
        Type resultType = route.FirstAwaiter.FieldType.GenericTypeArguments.Single();
        Type statusType = resultType.GetProperty("Status", I)!.PropertyType;
        Type sourceType = resultType.GetProperty("Source", I)!.PropertyType;
        var cases = new[]
        {
            new GpsCase("satellite", "Success", "Satellite", 8.4, "GPS/GNSS (satelit)", "akurasi dilaporkan Windows ±8 m"),
            new GpsCase("wifi", "Success", "WiFi", 123.6, "Wi-Fi (bukan GPS)", "akurasi dilaporkan Windows ±124 m"),
            new GpsCase("cellular", "Success", "Cellular", 55.5, "jaringan seluler (bukan GPS)", "akurasi dilaporkan Windows ±56 m"),
            new GpsCase("ip-no-accuracy", "Success", "IPAddress", null, "alamat IP (perkiraan; bukan GPS)", "akurasi tidak dilaporkan Windows"),
            new GpsCase("unknown-zero", "Success", "Unknown", 0, "sumber tidak diketahui (bukan GPS terverifikasi)", "akurasi dilaporkan Windows ±0 m"),
            new GpsCase("default-invalid-accuracy", "Success", "Default", -1, "lokasi default Windows (sumber tidak dipastikan)", "akurasi tidak dilaporkan Windows"),
            new GpsCase("ocean-no-place", "Success", "Unknown", null, "sumber tidak diketahui (bukan GPS terverifikasi)", "akurasi tidak dilaporkan Windows", 0, -140),
            new GpsCase("denied-result", "PermissionDenied", ExpectedError: "Akses lokasi ditolak. Periksa Pengaturan Windows > Privasi & keamanan > Lokasi, lalu coba lagi."),
            new GpsCase("unspecified-result", "PermissionUnspecified", ExpectedError: "Windows belum memberi status izin lokasi. Periksa layanan lokasi Windows dan coba lagi."),
            new GpsCase("timeout-result", "TimedOut", ExpectedError: "Lokasi belum didapat dalam 30 detik. Coba lagi di area dengan sinyal lebih baik; GPS dapat memerlukan waktu akuisisi."),
            new GpsCase("invalid-result", "InvalidPosition", ExpectedError: "Windows mengembalikan koordinat tidak valid; form tidak diubah."),
            new GpsCase("unavailable-default", "Unavailable", ExpectedError: "Lokasi perangkat tidak tersedia. Periksa layanan dan izin lokasi Windows atau isi secara manual."),
            new GpsCase("denied-catch", Fault: new UnauthorizedAccessException("synthetic denied awaiter"), ExpectedError: "Akses ditolak oleh pengaturan privasi lokasi Windows."),
            new GpsCase("generic-catch", Fault: new InvalidOperationException("synthetic failed awaiter"), ExpectedError: "Windows tidak dapat membaca lokasi perangkat. Periksa layanan dan izin lokasi.")
        };
        using (var fixture = new CopyFixture(application, directory))
        {
            // Defense in depth: the existing real service objects are disconnected only in this isolated test Window.
            // A compiler shape change cannot accidentally reach a Windows/network service through the owner fields.
            foreach (string field in new[] { "_deviceLocationService", "_locationDetectionService", "_onlineReverseGeocoder" })
            {
                FieldInfo service = fixture.MainWindow.GetType().GetField(field, I)
                    ?? throw new InvalidOperationException("Fail closed: expected acquisition/network field is absent.");
                service.SetValue(fixture.MainWindow, null);
                Assert(service.GetValue(fixture.MainWindow) is null, "Fail closed: isolated acquisition/network field must be disconnected.");
            }
            foreach (string theme in Themes)
            foreach (int width in new[] { 940, 1180 })
            {
                fixture.SelectTheme(theme);
                fixture.MainWindow.Width = width;
                fixture.MainWindow.Height = width == 940 ? 650 : 820;
                Pump(application, fixture);
                string stable = GpsStableState(fixture);
                string? actualSatelliteBody = null;
                foreach (GpsCase item in cases)
                {
                    TextBlock text = fixture.Field<TextBlock>("GpsLocationStatus");
                    string[] beforeLocation = LocationFields(fixture);
                    string beforeTimeZone = fixture.Field<ComboBox>("TimeZoneBox").Text;
                    string beforeOffline = fixture.Field<TextBlock>("OfflineReverseGeocodeStatus").Text;
                    object result = Activator.CreateInstance(resultType, I, null,
                        [Enum.Parse(statusType, item.Status), item.Status == "Success" ? item.Latitude : null,
                            item.Status == "Success" ? item.Longitude : null, item.Accuracy, Enum.Parse(sourceType, item.Source)], null)!;
                    ResumeGps(application, fixture, route, 0, result, item.Fault, null);
                    Assert(GpsStableState(fixture) == stable,
                        "Actual GPS completion must preserve unsaved settings, flags, ranges/defaults, consent, online/network status, loading state and unrelated controls.");
                    bool success = item.Status == "Success" && item.Fault is null;
                    bool copyMatches = true;
                    if (success)
                    {
                        Assert(fixture.Field<TextBox>("LatitudeBox").Text == item.Latitude.ToString("G", CultureInfo.InvariantCulture) &&
                            fixture.Field<TextBox>("LongitudeBox").Text == item.Longitude.ToString("G", CultureInfo.InvariantCulture),
                            "Actual compiled successful completion must populate synthetic coordinate form fields without saving.");
                        Type lookup = Assembly.Load("GeoTimeZone").GetType("GeoTimeZone.TimeZoneLookup", throwOnError: true)!;
                        object mappedZone = lookup.GetMethod("GetTimeZone", [typeof(double), typeof(double)])!.Invoke(null, [item.Latitude, item.Longitude])!;
                        Assert(fixture.Field<ComboBox>("TimeZoneBox").Text == (string)mappedZone.GetType().GetProperty("Result")!.GetValue(mappedZone)!,
                            "Actual GPS completion must replace the previous timezone with the offline coordinate mapping.");
                        string expected = ExpectedGpsBody(item);
                        copyMatches = text.Text.StartsWith(expected, StringComparison.Ordinal);
                        if (!copyMatches) failures.Add("COPY_GPS_WORDING " + item.Key + ": actual compiled successful completion still uses the old jargon/privacy/timezone body; approved humanized body required.");
                        string actualBody = ActualGpsBody(text.Text);
                        Assert(actualBody.Contains(item.SourceLabel, StringComparison.Ordinal) && actualBody.Contains(item.AccuracyLabel, StringComparison.Ordinal),
                            "SourceLabel/AccuracyLabel interpolation results must retain their exact existing source/accuracy meanings, including unavailable/invalid accuracy.");
                        VerifyOfflineAppend(fixture, item, text.Text);
                        if (item.Key == "satellite") actualSatelliteBody = actualBody;
                    }
                    else
                    {
                        Assert(text.Text == item.ExpectedError && LocationFields(fixture).SequenceEqual(beforeLocation) &&
                            fixture.Field<ComboBox>("TimeZoneBox").Text == beforeTimeZone &&
                            fixture.Field<TextBlock>("OfflineReverseGeocodeStatus").Text == beforeOffline,
                            "Actual result-default and exception/catch branches must retain exact messages, unchanged form fields and restored button state.");
                    }
                    var measured = RealizeParagraph(application, fixture, text,
                        Path.Combine(directory, theme + "-" + width + "-" + item.Key + ".png"));
                    Assert(GpsStableState(fixture) == stable, "Realizing actual GPS copy must not alter nontext state.");
                    rows.Add(new { theme, formWidth = width, input = item.Key, item.Status, item.Source, item.Accuracy,
                        coordinatesAreSynthetic = true, actualCompiledText = text.Text, copyMatches,
                        route = "first awaiter completion, state 0; acquisition and initial event path bypassed",
                        measured, unsavedSettingsPreserved = true, nonTextStateSha256 = HashText(stable) });
                }
                // Exercise the unchanged inner offline-error branch without damaging a dataset or requiring an external lookup.
                // Resume the actual second awaiter with a fault, retaining actual successful SUT body from the satellite case.
                Assert(actualSatelliteBody is not null, "Fail closed: second-awaiter fixture requires preceding actual successful SUT output.");
                GpsCase satellite = cases[0];
                object satelliteResult = Activator.CreateInstance(resultType, I, null,
                    [Enum.Parse(statusType, "Success"), satellite.Latitude, satellite.Longitude, satellite.Accuracy, Enum.Parse(sourceType, satellite.Source)], null)!;
                string[] errorBeforeLocation = LocationFields(fixture);
                ResumeGps(application, fixture, route, 1, satelliteResult, new IOException("synthetic offline dataset read failure"), actualSatelliteBody);
                TextBlock errorText = fixture.Field<TextBlock>("GpsLocationStatus");
                Assert(errorText.Text == actualSatelliteBody &&
                    fixture.Field<TextBlock>("OfflineReverseGeocodeStatus").Text == "Dataset GeoNames offline tidak dapat dibaca; koordinat GPS tetap di form dan tidak dikirim ke internet." &&
                    LocationFields(fixture).SequenceEqual(errorBeforeLocation) && GpsStableState(fixture) == stable,
                    "Actual offline-error continuation must retain SUT body and form values and set only its unchanged safe offline failure status.");
                var errorMeasured = RealizeParagraph(application, fixture, errorText,
                    Path.Combine(directory, theme + "-" + width + "-offline-error.png"));
                rows.Add(new { theme, formWidth = width, input = "offline-error", coordinatesAreSynthetic = true,
                    actualCompiledText = errorText.Text, copyMatches = errorText.Text == ExpectedGpsBody(satellite),
                    route = "second awaiter fault, state 1; actual earlier successful SUT body, no lookup damage",
                    measured = errorMeasured, unsavedAndTimezonePreserved = true, nonTextStateSha256 = HashText(stable) });
                VerifySaveReachable(application, fixture);
            }
            Assert(fixture.SaveWrites == 0 && fixture.StartupWrites == 0 && !File.Exists(fixture.SavePath),
                "Actual GPS completion must not save settings or call startup setters, even mocked ones.");
        }
        WpfApplicationIsolation.Verify(application);
        Assert(rows.Count == 240, "GPS realization coverage must be exactly 15 synthetic branches x 8 themes x 2 widths.");
        WriteEvidence(directory, "gps", rows, failures, new
        {
            themes = 8, widths = new[] { 940, 1180 }, realizations = rows.Count, syntheticInputs = 15,
            sourceAccuracyVariants = 6, noPlaceFallback = true, fiveUnavailableStatusBranches = true,
            accessDeniedAndGenericCatch = true, actualOfflineErrorContinuation = true,
            route.StateType.FullName, firstResumeOffset = route.FirstResumeOffset,
            secondResumeOffset = route.SecondResumeOffset, acquisitionCallOffset = route.AcquisitionOffset,
            actualCompiledCompletionPathExercised = true, actualClickInitialPathExercised = false,
            physicalLocationAcquisitionExercised = false, deviceAndNetworkFieldsDisconnected = true,
            nativePermissionOrGeolocationCalls = 0, externalRequests = 0, offlineBundledLookupOnly = true
        });
        Assert(failures.Count == 0, string.Join(Environment.NewLine, failures));
        Console.WriteLine("COPY_GPS PASS: actual compiled safe async completion; 240 WPF realizations; 6 source/accuracy variants, offline/no-place/error/default/catches, unsaved/timezone/consent preserved; no native acquisition or initial click path.");
    }

    private sealed record GpsCase(string Key, string Status = "Unavailable", string Source = "Unknown", double? Accuracy = null,
        string SourceLabel = "", string AccuracyLabel = "", double Latitude = -6.2, double Longitude = 106.8,
        Exception? Fault = null, string ExpectedError = "");
    private sealed record GpsResumeRoute(Type StateType, FieldInfo State, FieldInfo Builder, FieldInfo Owner,
        FieldInfo Closure, FieldInfo Button, FieldInfo OriginalContent, FieldInfo FirstAwaiter, FieldInfo SecondAwaiter,
        int FirstResumeOffset, int SecondResumeOffset, int AcquisitionOffset);

    private static GpsResumeRoute GuardGpsResumeRoute()
    {
        Type window = Product.GetType("PrayerAutomation.UI.MainWindow", true)!;
        MethodInfo handler = window.GetMethod("DetectGpsLocation_Click", I)!; // NEVER invoked.
        Type machine = handler.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType
            ?? throw new InvalidOperationException("Fail closed: actual GPS state machine metadata is required.");
        Assert(machine.IsValueType && typeof(IAsyncStateMachine).IsAssignableFrom(machine), "Fail closed: GPS continuation must be the inspected compiler-generated value type.");
        FieldInfo F(string name) => machine.GetField(name, I) ?? throw new InvalidOperationException("Fail closed: actual GPS continuation field is absent: " + name);
        FieldInfo state = F("<>1__state"), owner = F("<>4__this"), first = F("<>u__1"), second = F("<>u__2"), closure = F("<>8__1");
        FieldInfo? requestVersion = machine.GetField("<requestVersion>5__4", I);
        Assert(machine.GetFields(I).Length == (requestVersion is null ? 10 : 11) &&
            (requestVersion is null || requestVersion.FieldType == typeof(int) && window.GetField("_locationRequestVersion", I)?.FieldType == typeof(int)) &&
            state.FieldType == typeof(int) && owner.FieldType == window &&
            F("<>t__builder").FieldType == typeof(AsyncVoidMethodBuilder) &&
            first.FieldType.GetGenericTypeDefinition() == typeof(TaskAwaiter<>) && second.FieldType.GetGenericTypeDefinition() == typeof(TaskAwaiter<>),
            "Fail closed: compiler continuation field count/type/awaiter shape changed.");
        Instruction[] il = Decode(machine.GetMethod("MoveNext", I)!).ToArray();
        OpCode[] prefix = [OpCodes.Ldarg_0, OpCodes.Ldfld, OpCodes.Stloc_0, OpCodes.Ldarg_0, OpCodes.Ldfld,
            OpCodes.Stloc_1, OpCodes.Ldloc_0, OpCodes.Ldc_I4_1, OpCodes.Ble_Un_S];
        Assert(il.Take(prefix.Length).Select(x => x.OpCode).SequenceEqual(prefix) &&
            SameField(il[1].Operand, state) && SameField(il[4].Operand, owner),
            "Fail closed: state 0/1 must branch over every initial UI/permission/acquisition operation.");
        int dispatchOffset = (int)il[8].Operand!;
        int dispatch = Array.FindIndex(il, x => x.Offset == dispatchOffset);
        Assert(dispatch >= 0 && il[dispatch].OpCode == OpCodes.Nop && il[dispatch + 1].OpCode == OpCodes.Ldloc_0 &&
            il[dispatch + 2].OpCode == OpCodes.Brfalse_S && il[dispatch + 3].OpCode == OpCodes.Ldloc_0 &&
            il[dispatch + 4].OpCode == OpCodes.Ldc_I4_1 && il[dispatch + 5].OpCode == OpCodes.Beq,
            "Fail closed: inspected dispatcher must jump directly to the first (0) or second (1) completed awaiter.");
        int firstOffset = (int)il[dispatch + 2].Operand!, secondGateOffset = (int)il[dispatch + 5].Operand!;
        int secondGate = Array.FindIndex(il, x => x.Offset == secondGateOffset);
        Assert(secondGate >= 0 && il[secondGate].OpCode == OpCodes.Nop && il[secondGate + 1].OpCode == OpCodes.Ldloc_0 &&
            il[secondGate + 2].OpCode == OpCodes.Ldc_I4_1 && il[secondGate + 3].OpCode == OpCodes.Beq_S,
            "Fail closed: state 1 must take the inspected inner offline awaiter gate without executing Task.Run.");
        int secondOffset = (int)il[secondGate + 3].Operand!;
        Instruction acquisition = il.Single(x => x.Operand is MethodInfo method && method.Name == "DetectAsync" &&
            method.DeclaringType?.FullName == "PrayerAutomation.UI.DeviceLocationService");
        Assert(acquisition.Offset < firstOffset && secondOffset > firstOffset &&
            il.Any(x => x.Offset < firstOffset && x.OpCode == OpCodes.Ldfld && x.Operand is FieldInfo f && f.Name == "_deviceLocationService"),
            "Fail closed: the only Windows DetectAsync acquisition must be before both synthetic resume targets.");
        VerifyAwaiter(firstOffset, first);
        VerifyAwaiter(secondOffset, second);
        foreach (Instruction item in il.Where(x => x.Offset >= firstOffset))
        {
            if (item.OpCode.FlowControl is FlowControl.Branch or FlowControl.Cond_Branch)
            {
                int[] targets = item.Operand is int[] many ? many : [(int)item.Operand!];
                Assert(targets.All(target => target >= firstOffset), "Fail closed: no resumed branch may return to initial acquisition.");
            }
            if (item.Operand is MethodBase call)
            {
                string type = call.DeclaringType?.FullName ?? "";
                Assert(!type.StartsWith("Windows.", StringComparison.Ordinal) &&
                    !type.Contains("DeviceLocationService", StringComparison.Ordinal) &&
                    !type.Contains("LocationDetectionService", StringComparison.Ordinal) &&
                    !type.Contains("OnlineReverseGeocode", StringComparison.Ordinal) &&
                    !type.StartsWith("System.Net.", StringComparison.Ordinal),
                    "Fail closed: resumed compiled GPS IL may not contain a native/acquisition/network service call.");
            }
        }
        Assert(closure.FieldType.GetFields(I).Length == 1 && closure.FieldType.GetField("result", I)?.FieldType == first.FieldType.GenericTypeArguments.Single() &&
            closure.FieldType.IsDefined(typeof(CompilerGeneratedAttribute), false),
            "Fail closed: offline-only closure must be compiler generated and contain just the actual DeviceLocationResult.");
        ConstructorInfo closureConstructor = closure.FieldType.GetConstructor(I, null, Type.EmptyTypes, null)!;
        byte[] constructorIl = closureConstructor.GetMethodBody()!.GetILAsByteArray()!;
        Assert(constructorIl.Length == 7 && constructorIl[0] == 0x02 && constructorIl[1] == 0x28 && constructorIl[6] == 0x2a &&
            closureConstructor.Module.ResolveMethod(BitConverter.ToInt32(constructorIl, 2))?.DeclaringType == typeof(object),
            "Fail closed: synthetic closure construction may call only object's constructor.");
        MethodInfo offline = closure.FieldType.GetMethods(I).Single(x => x.Name.Contains("b__", StringComparison.Ordinal));
        Assert(Decode(offline).Where(x => x.Operand is MethodInfo).All(x =>
            ((MethodInfo)x.Operand!).DeclaringType!.FullName is "PrayerAutomation.UI.OfflineReverseGeocoder" or "PrayerAutomation.UI.DeviceLocationResult" ||
            ((MethodInfo)x.Operand!).DeclaringType!.IsGenericType && ((MethodInfo)x.Operand!).DeclaringType!.GetGenericTypeDefinition() == typeof(Nullable<>)),
            "Fail closed: the sole background callback must stay bundled offline lookup and result/nullable getters only.");
        Console.WriteLine("GPS_RESUME_GUARD PASS: state 0/1 bypass acquisition; compiled DetectAsync=" + acquisition.Offset +
            " firstResume=" + firstOffset + " secondResume=" + secondOffset + "; no backward or native/network calls; only offline callback.");
        return new(machine, state, F("<>t__builder"), owner, closure, F("<button>5__2"), F("<originalContent>5__3"), first, second,
            firstOffset, secondOffset, acquisition.Offset);
        void VerifyAwaiter(int offset, FieldInfo awaiter)
        {
            int index = Array.FindIndex(il, x => x.Offset == offset);
            Assert(index >= 0 && il[index].OpCode == OpCodes.Ldarg_0 && il[index + 1].OpCode == OpCodes.Ldfld && SameField(il[index + 1].Operand, awaiter),
                "Fail closed: resume must begin by reading the injected typed completed awaiter.");
            Instruction[] resume = il.Skip(index).TakeWhile(x => x.Operand is not MethodInfo m || m.Name != "GetResult").ToArray();
            Assert(resume.Length < 20 && resume.All(x => x.Operand is not MethodBase) &&
                resume.Where(x => x.OpCode == OpCodes.Stfld).All(x => SameField(x.Operand, state)) &&
                il[index + resume.Length].Operand is MethodInfo get && get.Name == "GetResult" && get.DeclaringType == awaiter.FieldType,
                "Fail closed: no native or user call may occur before the injected GetResult.");
        }
        static bool SameField(object? candidate, FieldInfo field) => candidate is FieldInfo actual &&
            actual.Module == field.Module && actual.MetadataToken == field.MetadataToken;
    }

    private static void ResumeGps(Application application, CopyFixture fixture, GpsResumeRoute route,
        int resumeState, object result, Exception? fault, string? actualEarlierBody)
    {
        Assert(SynchronizationContext.Current is DispatcherSynchronizationContext && resumeState is 0 or 1,
            "Fail closed: safe actual async completion requires DispatcherSynchronizationContext and an inspected suspended state, never -1.");
        Button button = fixture.Field<Button>("DetectGpsLocationButton");
        TextBlock text = fixture.Field<TextBlock>("GpsLocationStatus");
        object? originalContent = button.Content;
        button.IsEnabled = false;
        button.Content = "SYNTHETIC_COMPLETED_AWAITER";
        text.Text = actualEarlierBody ?? "SYNTHETIC_COMPLETED_AWAITER";
        FieldInfo awaiterField = resumeState == 0 ? route.FirstAwaiter : route.SecondAwaiter;
        Type awaitedType = awaiterField.FieldType.GenericTypeArguments.Single();
        MethodInfo factory = typeof(Task).GetMethods(S).Single(x => x.Name == (fault is null ? "FromResult" : "FromException") && x.IsGenericMethodDefinition);
        object task = factory.MakeGenericMethod(awaitedType).Invoke(null, [fault is null ? result : fault])!;
        Assert(((Task)task).IsCompleted, "Fail closed: only an already completed synthetic awaiter may be injected.");
        object awaiter = task.GetType().GetMethod("GetAwaiter", I)!.Invoke(task, null)!;
        Assert(awaiter.GetType() == awaiterField.FieldType, "Fail closed: actual typed awaiter identity mismatch.");
        object machine = Activator.CreateInstance(route.StateType)!;
        object closure = Activator.CreateInstance(route.Closure.FieldType, nonPublic: true)!;
        route.Closure.FieldType.GetField("result", I)!.SetValue(closure, result);
        route.State.SetValue(machine, resumeState);
        route.Builder.SetValue(machine, AsyncVoidMethodBuilder.Create());
        route.Owner.SetValue(machine, fixture.MainWindow);
        route.Closure.SetValue(machine, closure);
        route.Button.SetValue(machine, button);
        route.OriginalContent.SetValue(machine, originalContent);
        awaiterField.SetValue(machine, awaiter);
        FieldInfo? requestVersion = route.StateType.GetField("<requestVersion>5__4", I);
        if (requestVersion is not null)
        {
            FieldInfo ownerVersion = fixture.MainWindow.GetType().GetField("_locationRequestVersion", I)!;
            Assert(requestVersion.FieldType == typeof(int) && ownerVersion.FieldType == typeof(int), "Fail closed: exact request-version owner/type required.");
            requestVersion.SetValue(machine, ownerVersion.GetValue(fixture.MainWindow));
        }
        Assert((int)route.State.GetValue(machine)! == resumeState && ReferenceEquals(route.Owner.GetValue(machine), fixture.MainWindow) &&
            ReferenceEquals(route.Button.GetValue(machine), button) && ReferenceEquals(route.OriginalContent.GetValue(machine), originalContent),
            "Fail closed: injected owner/state/button fields must read back exactly before executing actual MoveNext.");
        var frame = new DispatcherFrame();
        bool timedOut = false;
        int initialPermissionStatusObserved = 0;
        var enabledDescriptor = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(UIElement.IsEnabledProperty, typeof(Button))!;
        var textDescriptor = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock))!;
        EventHandler enabled = (_, _) => { if (button.IsEnabled) frame.Continue = false; };
        EventHandler observed = (_, _) => { if (text.Text.StartsWith("Meminta izin lokasi Windows.", StringComparison.Ordinal)) initialPermissionStatusObserved++; };
        var deadline = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromSeconds(10) };
        deadline.Tick += (_, _) => { timedOut = true; frame.Continue = false; deadline.Stop(); };
        enabledDescriptor.AddValueChanged(button, enabled);
        textDescriptor.AddValueChanged(text, observed);
        try
        {
            ((IAsyncStateMachine)machine).MoveNext(); // Actual compiled SUT, never actual handler or DetectAsync.
            if (!button.IsEnabled) { deadline.Start(); Dispatcher.PushFrame(frame); }
        }
        finally
        {
            deadline.Stop();
            enabledDescriptor.RemoveValueChanged(button, enabled);
            textDescriptor.RemoveValueChanged(text, observed);
        }
        Pump(application, fixture);
        Assert(!timedOut && button.IsEnabled && ReferenceEquals(button.Content, originalContent) && initialPermissionStatusObserved == 0,
            "Actual resumed completion must terminate via its real finally, restoring button content/enabled state without entering the initial permission path.");
    }

    private static string ExpectedGpsBody(GpsCase item) => "Koordinat, nama, dan zona waktu terisi, belum disimpan. Sumber: " + item.SourceLabel + "; " + item.AccuracyLabel +
        ". Koordinat tidak dikirim ke layanan lokasi jaringan atau pencarian nama online. Zona waktu dicocokkan secara offline dari koordinat GPS. Periksa sebelum simpan.";
    private static string ActualGpsBody(string actual)
    {
        int place = actual.IndexOf(" Nama permukiman terdekat dari GeoNames lokal: ", StringComparison.Ordinal);
        int fallback = actual.IndexOf(" Tidak ada permukiman GeoNames dalam radius 75 km;", StringComparison.Ordinal);
        int end = place >= 0 ? place : fallback >= 0 ? fallback : actual.Length;
        return actual[..end]; // Actual SUT output only; never expected-new templates used as rendered text.
    }
    private static void VerifyOfflineAppend(CopyFixture fixture, GpsCase item, string actual)
    {
        string body = ActualGpsBody(actual);
        Type geocoder = Product.GetType("PrayerAutomation.UI.OfflineReverseGeocoder", true)!;
        object bundled = geocoder.GetProperty("Bundled", S)!.GetValue(null)!;
        object? place = geocoder.GetMethod("FindNearest", I)!.Invoke(bundled, [item.Latitude, item.Longitude, 75d]);
        string offline = fixture.Field<TextBlock>("OfflineReverseGeocodeStatus").Text;
        if (place is null)
        {
            string name = "Lokasi perangkat (" + item.SourceLabel + ") — " + item.Latitude.ToString("0.####", CultureInfo.InvariantCulture) + ", " + item.Longitude.ToString("0.####", CultureInfo.InvariantCulture);
            Assert(actual == body + " Tidak ada permukiman GeoNames dalam radius 75 km; label lokal awal dipertahankan." &&
                fixture.Field<TextBox>("LocationNameBox").Text == name &&
                offline == "Tidak ada permukiman GeoNames dalam radius 75 km; label lokal awal dipertahankan. Tidak ada permintaan internet.",
                "The actual 75-km fallback must retain local source label and its unchanged exact appended/neighboring text.");
        }
        else
        {
            string name = (string)place.GetType().GetProperty("DisplayName", I)!.GetValue(place)!;
            string distance = ((double)place.GetType().GetProperty("DistanceKm", I)!.GetValue(place)!).ToString("0.0", CultureInfo.CurrentCulture);
            Assert(actual == body + " Nama permukiman terdekat dari GeoNames lokal: " + name + " (sekitar " + distance + " km); ini label perkiraan, bukan alamat." &&
                fixture.Field<TextBox>("LocationNameBox").Text == name &&
                offline == "Nama lokasi perangkat dicocokkan secara lokal dengan GeoNames: " + name + ", sekitar " + distance + " km. Tidak ada permintaan internet.",
                "Actual GeoNames local name, attribution and distance append must remain exact and match the unchanged offline result.");
        }
    }
    private static string[] LocationFields(CopyFixture fixture) => new[] { "LocationNameBox", "LatitudeBox", "LongitudeBox" }
        .Select(name => fixture.Field<TextBox>(name).Text).ToArray();
    private static string GpsStableState(CopyFixture fixture)
    {
        var root = System.Text.Json.Nodes.JsonNode.Parse(NonTextState(fixture))!;
        var values = (System.Text.Json.Nodes.JsonArray)root["values"]!;
        foreach (var item in values.Where(x => x!["name"]!.GetValue<string>() is "LocationNameBox" or "LatitudeBox" or "LongitudeBox" or "TimeZoneBox").ToArray()) values.Remove(item);
        var statuses = (System.Text.Json.Nodes.JsonArray)root["neighboringStatuses"]!;
        foreach (var item in statuses.Where(x => x!["name"]!.GetValue<string>() == "OfflineReverseGeocodeStatus").ToArray()) statuses.Remove(item);
        return root.ToJsonString(); // Only populated location/timezone fields and expected offline status may vary.
    }

    private sealed record SafetyCase(string Key, int LiteralIndex, string Expected);

    private static object RealizeParagraph(Application application, CopyFixture fixture, TextBlock text, string capture)
    {
        Pump(application, fixture);
        text.UpdateLayout();
        ScrollViewer scroll = Ancestor<ScrollViewer>(text);
        Visual content = (Visual)scroll.Content;
        Rect contentBounds = Bounds(text, content);
        scroll.ScrollToVerticalOffset(contentBounds.Top);
        Pump(application, fixture);
        fixture.MainWindow.UpdateLayout();
        Rect viewport = Bounds(text, scroll);
        Assert(text.Visibility == Visibility.Visible && text.IsVisible && text.ActualWidth > 100 && text.ActualHeight > 0,
            "The actual target status must be realized and visible on the real settings tab.");
        Assert(text.TextWrapping == TextWrapping.Wrap && text.TextTrimming == TextTrimming.None &&
            text.FontSize == 14 && text.LineHeight == 20 && text.LineStackingStrategy == LineStackingStrategy.BlockLineHeight &&
            text.TextAlignment == TextAlignment.Left && text.Margin == new Thickness(3, 4, 3, 8) &&
            ReferenceEquals(text.Style, fixture.MainWindow.FindResource("FormHelpText")),
            "The actual status must keep shared FormHelpText typography, wrapping, margins and style identity.");
        var formatted = new FormattedText(text.Text, CultureInfo.CurrentCulture, text.FlowDirection,
            new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize,
            text.Foreground, VisualTreeHelper.GetDpi(text).PixelsPerDip)
        { MaxTextWidth = text.ActualWidth, LineHeight = text.LineHeight, TextAlignment = text.TextAlignment };
        Assert(text.ActualHeight + 1 >= formatted.Height && viewport.Left >= -1 &&
            viewport.Right <= scroll.ActualWidth + 1 && viewport.Top >= -1 && viewport.Bottom <= scroll.ViewportHeight + 3,
            "Realized status must wrap without cropped ink or horizontal/viewport overflow.");
        var foreground = (SolidColorBrush)text.Foreground;
        var background = (SolidColorBrush)Ancestor<GroupBox>(text).Background;
        Assert(foreground.Color == ((SolidColorBrush)fixture.MainWindow.FindResource("MutedTextBrush")).Color,
            "The existing theme's muted status foreground must stay unchanged.");
        double contrast = Contrast(foreground.Color, background.Color);
        Assert(contrast >= 4.5, "Actual status glyph and card foreground/background must remain readable in every theme.");
        int glyphPixels = CaptureGlyphs(text, background, foreground.Color, capture);
        Assert(glyphPixels >= 10, "Actual rendered status must contain readable foreground glyph pixels, not an empty substitute.");
        Console.WriteLine("COPY_REALIZATION " + text.Name + " width=" + fixture.MainWindow.Width.ToString(CultureInfo.InvariantCulture) +
            " textWidth=" + text.ActualWidth.ToString("0.##", CultureInfo.InvariantCulture) + " height=" + text.ActualHeight.ToString("0.##", CultureInfo.InvariantCulture) +
            " contrast=" + contrast.ToString("0.000", CultureInfo.InvariantCulture) + " glyphPixels=" + glyphPixels);
        return new { textWidth = text.ActualWidth, textHeight = text.ActualHeight, formattedHeight = formatted.Height,
            fontSize = text.FontSize, lineHeight = text.LineHeight, fontFamily = text.FontFamily.Source,
            wrap = text.TextWrapping.ToString(), style = "actual FormHelpText", contrast, glyphPixels,
            viewport = new { viewport.X, viewport.Y, viewport.Width, viewport.Height },
            dpiScale = VisualTreeHelper.GetDpi(text).DpiScaleX, capture, captureSha256 = HashFile(capture) };
    }

    private static int CaptureGlyphs(TextBlock text, Brush background, Color foreground, string path)
    {
        int width = (int)Math.Ceiling(text.ActualWidth), height = (int)Math.Ceiling(text.ActualHeight);
        FrameworkElement? root = null;
        DependencyObject? parent = text;
        while ((parent = VisualTreeHelper.GetParent(parent)) is not null)
            if (parent is System.Windows.Documents.AdornerDecorator decorator) root = decorator;
        if (root is null) throw new InvalidOperationException("Fail closed: capture requires the real status adorner root, not an offset child Visual.");
        var whole = new RenderTargetBitmap((int)Math.Ceiling(root.ActualWidth), (int)Math.Ceiling(root.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        whole.Render(root);
        Rect bounds = Bounds(text, root);
        var crop = new Int32Rect((int)Math.Round(bounds.Left), (int)Math.Round(bounds.Top), width, height);
        if (crop.X < 0 || crop.Y < 0 || crop.X + width > whole.PixelWidth || crop.Y + height > whole.PixelHeight)
            throw new InvalidOperationException("Fail closed: actual status glyph crop must stay inside the realized root.");
        var cropped = new CroppedBitmap(whole, crop);
        byte[] pixels = new byte[width * height * 4];
        cropped.CopyPixels(pixels, width * 4, 0);
        int glyphs = 0;
        for (int p = 0; p < pixels.Length; p += 4)
            if (pixels[p + 3] >= 250 && Math.Abs(pixels[p] - foreground.B) <= 12 &&
                Math.Abs(pixels[p + 1] - foreground.G) <= 12 && Math.Abs(pixels[p + 2] - foreground.R) <= 12) glyphs++;
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(cropped));
        using FileStream file = File.Create(path);
        encoder.Save(file);
        return glyphs;
    }

    private static double Contrast(Color a, Color b)
    {
        static double L(Color c)
        {
            static double C(byte v) { double x = v / 255d; return x <= .04045 ? x / 12.92 : Math.Pow((x + .055) / 1.055, 2.4); }
            return .2126 * C(c.R) + .7152 * C(c.G) + .0722 * C(c.B);
        }
        double x = L(a), y = L(b);
        return (Math.Max(x, y) + .05) / (Math.Min(x, y) + .05);
    }

    private static string NonTextState(CopyFixture fixture) => JsonSerializer.Serialize(new
    {
        savedSettings = JsonSerializer.Serialize(fixture.Settings, fixture.SettingsType),
        checkBoxes = new[] { "StartupEnabledCheckBox", "StartHiddenCheckBox", "AutoLockEnabledCheckBox", "LockScreenCheckBox",
            "ReminderEnabledCheckBox", "WidgetEnabledCheckBox", "OnlineReverseGeocodeConsentCheckBox" }
            .Select(name => { CheckBox x = fixture.Field<CheckBox>(name); return new { name, x.IsChecked, x.IsEnabled,
                x.Visibility, templateIdentity = RuntimeHelpers.GetHashCode(x.Template) }; }),
        values = new[] { "LocationNameBox", "LatitudeBox", "LongitudeBox", "TimeZoneBox", "LockScreenDurationBox",
            "ReminderMinutesBox", "HijriDateCorrectionBox", "CorrectionFajrBox" }
            .Select(name => { System.Windows.Controls.Control x = name == "TimeZoneBox" ? fixture.Field<ComboBox>(name) : fixture.Field<TextBox>(name);
                string text = x is ComboBox combo ? combo.Text : ((TextBox)x).Text; return new { name, Text = text, x.IsEnabled,
                x.MinHeight, width = double.IsNaN(x.Width) ? "Auto" : x.Width.ToString(CultureInfo.InvariantCulture),
                templateIdentity = x.Template is null ? 0 : RuntimeHelpers.GetHashCode(x.Template) }; }),
        neighboringStatuses = new[] { "NetworkLocationStatus", "OnlineReverseGeocodeStatus", "OfflineReverseGeocodeStatus", "SettingsStatusLabel" }
            .Select(name => new { name, text = fixture.Field<TextBlock>(name).Text }),
        onlineButtonEnabled = fixture.Field<Button>("OnlineReverseGeocodeButton").IsEnabled,
        tab = fixture.Field<TabControl>("MainTabs").SelectedIndex,
        loading = fixture.MainWindow.GetType().GetField("_loadingSettings", I)!.GetValue(fixture.MainWindow),
        fixture.SaveWrites, fixture.StartupWrites
    });

    private static void VerifySaveReachable(Application application, CopyFixture fixture)
    {
        Button button = fixture.Field<Button>("SaveSettingsButton");
        Pump(application, fixture);
        Rect bounds = Bounds(button, fixture.MainWindow);
        Assert(button.IsEnabled && button.IsVisible && bounds.Left >= 0 && bounds.Top >= 0 &&
            bounds.Right <= fixture.MainWindow.ActualWidth && bounds.Bottom <= fixture.MainWindow.ActualHeight,
            "The unchanged real Save button must stay reachable at both actual form widths.");
    }

    // Same isolated lifecycle as UiMinorSmoke's Fixture, copied only to add pre-
    // constructor deserialized risk assertions and keep every synthetic path in this workspace.
    private sealed class CopyFixture : IDisposable
    {
        private readonly Application _application;
        private readonly object _previousSettings;
        private readonly Window? _previousMainWindow;
        private readonly Type _appType = typeof(ProductUi::PrayerAutomation.UI.App);
        internal CopyFixture(Application application, string directory)
        {
            _application = application;
            WpfApplicationIsolation.Verify(application);
            Assert(application.MainWindow is null && AppField("_mainWindow") is null && AppField("_widget") is null,
                "Fail closed: a copy fixture must never take over or close an existing app window.");
            _previousMainWindow = application.MainWindow;
            _appType.GetMethod("InitializeComponent", I)!.Invoke(application, null);
            SettingsType = Product.GetType("PrayerAutomation.UI.AppSettings", true)!;
            object seed = Activator.CreateInstance(SettingsType, nonPublic: true)!;
            foreach (string name in SafetyFlags) SettingsType.GetProperty(name, I)!.SetValue(seed, false);
            SettingsType.GetProperty("ThemeId", I)!.SetValue(seed, "light");
            Settings = JsonSerializer.Deserialize(JsonSerializer.Serialize(seed, SettingsType), SettingsType)!;
            VerifyFlags(); // BEFORE actual MainWindow construction, after deserialization.
            _previousSettings = _appType.GetProperty("Settings", I)!.GetValue(application)!;
            _appType.GetProperty("Settings", I)!.SetValue(application, Settings);
            _appType.GetMethod("ApplyThemePreview", I)!.Invoke(application, ["light"]);
            SavePath = Path.Combine(directory, "config.json");
            Type saverType = typeof(Action<>).MakeGenericType(SettingsType);
            ParameterExpression candidate = Expression.Parameter(SettingsType, "candidate");
            Delegate saver = Expression.Lambda(saverType,
                Expression.Call(Expression.Constant(this), GetType().GetMethod(nameof(CaptureSave), I)!,
                    Expression.Convert(candidate, typeof(object))), candidate).Compile();
            Action<bool, bool> startup = (enabled, hidden) => { Assert(!enabled && !hidden, "Fail closed: mock startup values must stay false."); StartupWrites++; };
            Func<bool> startupRead = () => false;
            Type windowType = Product.GetType("PrayerAutomation.UI.MainWindow", true)!;
            ConstructorInfo constructor = windowType.GetConstructor(I, null,
                [SettingsType, saverType, typeof(Action<bool, bool>), typeof(Func<bool>)], null)
                ?? throw new InvalidOperationException("Fail closed: existing isolated Save/startup constructor is required.");
            MainWindow = (Window)constructor.Invoke([Settings, saver, startup, startupRead]);
            ((DispatcherTimer)windowType.GetField("_clockTimer", I)!.GetValue(MainWindow)!).Stop();
            MainWindow.Opacity = 0;
            MainWindow.Topmost = false;
            MainWindow.ShowActivated = false;
            MainWindow.ShowInTaskbar = false;
            MainWindow.IsHitTestVisible = false;
            _appType.GetField("_mainWindow", I)!.SetValue(application, MainWindow);
            application.MainWindow = MainWindow;
            MainWindow.Show();
            Field<TabControl>("MainTabs").SelectedIndex = 2;
            Pump(application, this);
            Assert(Field<TextBox>("LockScreenDurationBox").Text == "60" &&
                !Field<CheckBox>("OnlineReverseGeocodeConsentCheckBox").IsChecked.GetValueOrDefault() &&
                !Field<Button>("OnlineReverseGeocodeButton").IsEnabled,
                "Fail closed: actual duration default/online consent and action must remain safe.");
            Console.WriteLine("COPY_FIXTURE_FLAGS PASS: deserialized 7 consequential/start-hidden/widget flags=false before Window; scratch saver/mock startup; clock stopped.");
        }
        internal Type SettingsType { get; }
        internal object Settings { get; }
        internal Window MainWindow { get; }
        internal string SavePath { get; }
        internal int SaveWrites { get; private set; }
        internal int StartupWrites { get; private set; }
        internal object? AppField(string name) => _appType.GetField(name, I)!.GetValue(_application);
        internal T Field<T>(string name) => (T)MainWindow.GetType().GetField(name, I)!.GetValue(MainWindow)!;
        internal void SelectTheme(string tag)
        {
            ComboBox selector = Field<ComboBox>("ThemeBox");
            selector.SelectedItem = selector.Items.OfType<ComboBoxItem>().Single(x => x.Tag as string == tag);
            Pump(_application, this);
        }
        internal void VerifyFlags()
        {
            foreach (string name in SafetyFlags)
                Assert(SettingsType.GetProperty(name, I)!.GetValue(Settings) is false,
                    "Fail closed: deserialized synthetic risk flag must stay false: " + name);
        }
        private void CaptureSave(object candidate)
        {
            foreach (string name in SafetyFlags)
                Assert(SettingsType.GetProperty(name, I)!.GetValue(candidate) is false, "Fail closed: unsafe synthetic save.");
            SaveWrites++;
            File.WriteAllText(SavePath, JsonSerializer.Serialize(candidate, SettingsType));
        }
        public void Dispose()
        {
            VerifyFlags();
            Assert(AppField("_widget") is null, "Copy fixture must not create any widget or close a preexisting one.");
            MainWindow.GetType().GetMethod("AllowClose", I)!.Invoke(MainWindow, null);
            MainWindow.Close(); // Only this transparent owned test Window; no app/native exit restore.
            Assert(!((DispatcherTimer)MainWindow.GetType().GetField("_clockTimer", I)!.GetValue(MainWindow)!).IsEnabled,
                "The isolated test Window clock must remain stopped at teardown.");
            _appType.GetField("_mainWindow", I)!.SetValue(_application, null);
            _appType.GetProperty("Settings", I)!.SetValue(_application, _previousSettings);
            _application.MainWindow = _previousMainWindow;
            WpfApplicationIsolation.Verify(_application);
        }
    }

    private static void Pump(Application application, CopyFixture fixture)
    {
        Assert(SynchronizationContext.Current is DispatcherSynchronizationContext,
            "Fail closed: install DispatcherSynchronizationContext before actual async UI code.");
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
        WpfApplicationIsolation.Verify(application);
        fixture.VerifyFlags();
        Assert(fixture.AppField("_widget") is null, "Fail closed: copy-only tests must never start widget/lifecycle actions.");
    }
    private static T Ancestor<T>(DependencyObject element) where T : DependencyObject
    {
        DependencyObject? current = element;
        while ((current = VisualTreeHelper.GetParent(current)) is not null)
            if (current is T result) return result;
        throw new InvalidOperationException("Fail closed: the actual status is missing its owning visual ancestor.");
    }
    private static Rect Bounds(FrameworkElement element, Visual ancestor) => element.TransformToAncestor(ancestor).TransformBounds(new Rect(element.RenderSize));
    private static string OutputDirectory(string phase)
    {
        string root = Environment.GetEnvironmentVariable("SMOKE_SCRATCH")
            ?? throw new InvalidOperationException("Fail closed: explicit owned scratch fixture directory is required.");
        string approved = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrayerAutomationSmoke", "scratch") + Path.DirectorySeparatorChar;
        Assert(Path.GetFullPath(root).StartsWith(approved, StringComparison.OrdinalIgnoreCase), "Fail closed: no original profile or system-temp fixture paths.");
        string output = Path.Combine(root, "copy-" + phase + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(output);
        return output;
    }
    private static string HashFile(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    private static string HashText(string value) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static void WriteEvidence(string directory, string phase, List<object> rows, HashSet<string> failures, object coverage)
    {
        string report = Path.Combine(directory, "actual-wpf-copy.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new
        {
            passed = failures.Count == 0, phase, actualUiAssembly = Product.Location,
            assemblySha256 = HashFile(Product.Location), coverage, rows, failures,
            originalSettingsOrNativeWrites = false, productExecutableLaunched = false,
            implicitStartupAbortedBeforePump = true, dispatcherContextInstalled = true
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("COPY_EVIDENCE " + JsonSerializer.Serialize(new { phase, report, reportSha256 = HashFile(report),
            assemblySha256 = HashFile(Product.Location), realizations = rows.Count, failedCopyAssertions = failures.Count }));
    }
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed record Instruction(int Offset, OpCode OpCode, object? Operand);
    private static IEnumerable<Instruction> Decode(MethodInfo method)
    {
        byte[] bytes = method.GetMethodBody()!.GetILAsByteArray()!;
        Dictionary<short, OpCode> codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!)
            .GroupBy(x => x.Value).ToDictionary(g => g.Key, g => g.First());
        int cursor = 0;
        while (cursor < bytes.Length)
        {
            int offset = cursor;
            short value = bytes[cursor++];
            if (value == 0xfe) value = unchecked((short)(0xfe00 | bytes[cursor++]));
            OpCode op = codes[value];
            object? operand = null;
            switch (op.OperandType)
            {
                case OperandType.InlineNone: break;
                case OperandType.ShortInlineI: operand = (sbyte)bytes[cursor++]; break;
                case OperandType.ShortInlineVar: operand = bytes[cursor++]; break;
                case OperandType.InlineVar: operand = BitConverter.ToUInt16(bytes, cursor); cursor += 2; break;
                case OperandType.InlineI: operand = ReadInt(); break;
                case OperandType.InlineI8: operand = BitConverter.ToInt64(bytes, cursor); cursor += 8; break;
                case OperandType.ShortInlineR: operand = BitConverter.ToSingle(bytes, cursor); cursor += 4; break;
                case OperandType.InlineR: operand = BitConverter.ToDouble(bytes, cursor); cursor += 8; break;
                case OperandType.ShortInlineBrTarget: int delta = (sbyte)bytes[cursor++]; operand = cursor + delta; break;
                case OperandType.InlineBrTarget: int distance = ReadInt(); operand = cursor + distance; break;
                case OperandType.InlineSwitch:
                    int count = ReadInt(); int end = cursor + count * 4; var targets = new int[count];
                    for (int x = 0; x < count; x++) targets[x] = end + ReadInt();
                    operand = targets; break;
                case OperandType.InlineString: operand = method.Module.ResolveString(ReadInt()); break;
                case OperandType.InlineField: operand = method.Module.ResolveField(ReadInt()); break;
                case OperandType.InlineMethod: operand = method.Module.ResolveMethod(ReadInt()); break;
                case OperandType.InlineType: operand = method.Module.ResolveType(ReadInt()); break;
                case OperandType.InlineTok: operand = method.Module.ResolveMember(ReadInt()); break;
                case OperandType.InlineSig: operand = ReadInt(); break;
                default: throw new InvalidOperationException("Fail closed: unsupported actual compiled IL operand.");
            }
            yield return new Instruction(offset, op, operand);
        }
        int ReadInt() { int value = BitConverter.ToInt32(bytes, cursor); cursor += 4; return value; }
    }
}
