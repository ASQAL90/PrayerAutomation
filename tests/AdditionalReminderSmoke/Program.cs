using System.Text.Json;
using PrayerAutomation.UI;

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException("GAGAL: " + message);
    Console.WriteLine("OK: " + message);
}

Assert(AppDateTimeFormat.Date("2026-11-22") == "22 Nov 2026", "Tanggal snapshot memakai format 22 Nov 2026.");
Assert(AppDateTimeFormat.DateTime(new DateTime(2026, 11, 25, 8, 10, 0)) == "25 Nov 2026 08.10",
    "Waktu pengingat memakai format 25 Nov 2026 08.10.");
Assert(AppDateTimeFormat.TryParseTime("08.10", out TimeSpan eightTen) && eightTen == new TimeSpan(8, 10, 0),
    "Jam HH.mm dapat dibaca.");
Assert(AppDateTimeFormat.NormalizeTime("8:5", "08.10") == "08.05", "Jam fallback dinormalisasi ke HH.mm.");

AppSettings settings = AppSettings.CreateFreshInstall();
Assert(settings.SunnahFastingReminderTime == "16.00" && settings.PublicHolidayReminderTime == "08.10" &&
       settings.IslamicHolidayReminderTime == "08.10" && settings.NoFastReminderTime == "16.00" &&
       settings.MorningDzikirReminderTime == "08.10" &&
       settings.EveningDzikirReminderTime == "17.00", "Default 16.00 untuk puasa dan larangan puasa besok; 08.10/17.00 untuk lainnya.");
Assert(settings.ReminderRepeatSeconds == 300 && settings.ReminderRepeatCount == 3,
    "Jeda ulang default 300 detik dan maksimal 3 kali.");
Assert(settings.SunnahFastingReminderEnabled && settings.PublicHolidayReminderEnabled &&
       settings.IslamicHolidayReminderEnabled && settings.NoFastReminderEnabled &&
       settings.MorningDzikirReminderEnabled &&
       settings.EveningDzikirReminderEnabled, "Enam pengingat tambahan aktif pada instal baru.");

settings.ReminderRepeatSeconds = 1;
settings.ReminderRepeatCount = 99;
settings.MorningDzikirReminderTime = "25.90";
settings.Normalize();
Assert(settings.ReminderRepeatSeconds == 30 && settings.ReminderRepeatCount == 10 &&
       settings.MorningDzikirReminderTime == "08.10", "Nilai pengingat di luar rentang dinormalisasi.");

settings.FullOfflineDataEnabled = true;
string settingsJson = JsonSerializer.Serialize(settings);
AppSettings? restoredSettings = JsonSerializer.Deserialize<AppSettings>(settingsJson);
Assert(restoredSettings is not null && restoredSettings.FullOfflineDataEnabled,
    "Centang paket data lengkap ikut tersimpan di config.");

DateTime reminderDate = new(2026, 11, 25, 0, 0, 0, DateTimeKind.Unspecified);
CalendarObservance[] todayObservances =
[
    new(0, "Tahun Baru Masehi", "Tahun Baru Masehi", CalendarObservanceKind.PublicHoliday)
];
CalendarObservance[] tomorrowObservances =
[
    new(16, "Ayyamul Bidh — puasa sunnah (13–15 Hijriyah)", "Ayyamul Bidh", CalendarObservanceKind.SunnahFast)
];
IReadOnlyList<AdditionalReminderGroup> groups = AdditionalReminderLogic.BuildGroups(
    AppSettings.CreateFreshInstall(), reminderDate, todayObservances, tomorrowObservances);
Assert(groups.Count == 3, "08.10 gabungan observansi, 16.00 puasa besok, 17.00 dzikir petang.");
AdditionalReminderGroup morning = groups.Single(item => item.Time == new TimeSpan(8, 10, 0));
Assert(morning.Message.Contains("25 Nov 2026 08.10", StringComparison.Ordinal) &&
       morning.Message.Contains("Tahun Baru Masehi (hari libur nasional)", StringComparison.Ordinal) &&
       morning.Message.Contains("Dzikir pagi", StringComparison.Ordinal) &&
       !morning.Message.Contains("Ayyamul Bidh", StringComparison.Ordinal),
    "Popup 08.10 memuat kejadian hari ini tanpa menyertakan puasa besok.");
AdditionalReminderGroup fasting = groups.Single(item => item.Time == new TimeSpan(16, 0, 0));
Assert(fasting.Key == "2026-11-25|16.00" &&
       fasting.Message.Contains("25 Nov 2026 16.00", StringComparison.Ordinal) &&
       fasting.Message.Contains("Besok, 26 Nov 2026: Ayyamul Bidh (puasa sunnah)", StringComparison.Ordinal),
    "Puasa sunnah mengingatkan 26 Nov 2026 pukul 16.00 dengan key tanggal eksekusi 25 Nov 2026.");
Assert(groups.Single(item => item.Time == new TimeSpan(17, 0, 0)).Message.Contains("Dzikir petang", StringComparison.Ordinal),
    "Dzikir petang tetap menjadi popup terpisah pukul 17.00.");
Assert(fasting.NavigationTarget is null && morning.NavigationTarget == "morning" &&
       morning.NavigationLabel == "Buka dzikir pagi" &&
       groups.Single(item => item.Time == new TimeSpan(17, 0, 0)).NavigationTarget == "evening",
    "Popup dzikir membawa target navigasi pagi/petang sesuai konteks.");

CalendarObservance[] tasyrikTomorrow =
[
    new(15, "Hari Tasyriq — tidak berpuasa (11–13 Zulhijah)", "Tasyrik", CalendarObservanceKind.NoFast)
];
IReadOnlyList<AdditionalReminderGroup> tasyrikGroups = AdditionalReminderLogic.BuildGroups(
    AppSettings.CreateFreshInstall(), reminderDate, [], tasyrikTomorrow);
AdditionalReminderGroup tasyrik = tasyrikGroups.Single(item => item.Time == new TimeSpan(16, 0, 0));
Assert(tasyrik.Message.Contains("Besok, 26 Nov 2026: Tasyrik (dilarang berpuasa)", StringComparison.Ordinal) &&
       tasyrikGroups.Where(item => item.Time == new TimeSpan(8, 10, 0))
           .All(item => !item.Message.Contains("Tasyrik", StringComparison.Ordinal)),
    "Hari Tasyriq masuk pengingat larangan puasa besok, bukan peringatan hari ini.");

CalendarObservance[] maulidToday =
[
    new(6, "Maulid Nabi Muhammad SAW — perkiraan", "Maulid Nabi", CalendarObservanceKind.ReligiousEvent)
];
AdditionalReminderGroup maulid = AdditionalReminderLogic.BuildGroups(
        AppSettings.CreateFreshInstall(), reminderDate, maulidToday, [])
    .Single(item => item.Time == new TimeSpan(8, 10, 0));
Assert(maulid.Message.Contains("Maulid Nabi (peringatan Islam)", StringComparison.Ordinal) &&
       !maulid.Message.Contains("puasa sunnah", StringComparison.Ordinal),
    "Peringatan Islam hanya muncul sebagai peringatan hari ini.");

CalendarObservance[] eidToday =
[
    new(8, "Idulfitri — perkiraan; tidak berpuasa", "Idulfitri", CalendarObservanceKind.EidNoFast)
];
IReadOnlyList<AdditionalReminderGroup> eidGroups = AdditionalReminderLogic.BuildGroups(
    AppSettings.CreateFreshInstall(), reminderDate, eidToday, []);
Assert(eidGroups.Single(item => item.Time == new TimeSpan(8, 10, 0)).Message
           .Contains("Idulfitri (hari raya Id)", StringComparison.Ordinal) &&
       eidGroups.All(item => item.Time != new TimeSpan(16, 0, 0)),
    "Idulfitri masuk kategori hari libur/Id, bukan puasa sunnah atau peringatan Islam.");

CalendarObservance[] seninToday =
[
    new(18, "Puasa sunnah Senin", "Puasa Senin", CalendarObservanceKind.SunnahFast)
];
Assert(AdditionalReminderLogic.BuildGroups(AppSettings.CreateFreshInstall(), reminderDate, seninToday, [])
        .All(item => !item.Message.Contains("Puasa Senin", StringComparison.Ordinal)),
    "Puasa Senin bukan hari besar Islam dan tidak muncul pada popup hari ini.");

DateTime lateTick = new(2026, 11, 25, 18, 23, 0, DateTimeKind.Unspecified);
IReadOnlyList<AdditionalReminderGroup> lateGroups = AdditionalReminderLogic.BuildGroups(
    AppSettings.CreateFreshInstall(), lateTick, todayObservances, tomorrowObservances);
Assert(lateGroups.Single(item => item.Time == new TimeSpan(16, 0, 0)).Message
        .Contains("25 Nov 2026 16.00", StringComparison.Ordinal),
    "Stempel popup memakai jam jadwal, bukan jam saat pemeriksaan berjalan.");

AppSettings combinedDzikir = AppSettings.CreateFreshInstall();
combinedDzikir.SunnahFastingReminderEnabled = false;
combinedDzikir.PublicHolidayReminderEnabled = false;
combinedDzikir.IslamicHolidayReminderEnabled = false;
combinedDzikir.EveningDzikirReminderTime = combinedDzikir.MorningDzikirReminderTime;
IReadOnlyList<AdditionalReminderGroup> combinedGroups = AdditionalReminderLogic.BuildGroups(
    combinedDzikir, reminderDate, [], []);
Assert(combinedGroups.Count == 1 &&
       combinedGroups[0].NavigationTarget == "dzikir" &&
       combinedGroups[0].NavigationLabel == "Buka Dzikir & Doa" &&
       combinedGroups[0].Message.Contains("Dzikir pagi", StringComparison.Ordinal) &&
       combinedGroups[0].Message.Contains("Dzikir petang", StringComparison.Ordinal),
    "Dzikir pagi dan petang yang bersamaan menjadi satu popup dengan navigasi tab Dzikir & Doa.");

AppSettings customTimes = AppSettings.CreateFreshInstall();
customTimes.PublicHolidayReminderTime = "07.00";
IReadOnlyList<AdditionalReminderGroup> splitGroups = AdditionalReminderLogic.BuildGroups(
    customTimes, reminderDate, todayObservances, tomorrowObservances);
Assert(splitGroups.Count == 4,
    "Jadwal pada jam berbeda tetap menghasilkan popup terpisah.");

DateTime scheduled = reminderDate.AddHours(16);
Assert(AdditionalReminderLogic.DueCycle(scheduled.AddMinutes(-1), scheduled, 300, 3) == -1,
    "Jadwal yang belum tiba belum menghasilkan pengingat.");
Assert(AdditionalReminderLogic.DueCycle(scheduled.AddMinutes(2), scheduled, 300, 3) == 0,
    "Jadwal yang baru terlewat dua menit berada pada siklus awal.");
Assert(AdditionalReminderLogic.DueCycle(scheduled.AddMinutes(15), scheduled, 300, 3) == 3,
    "Perulangan dihitung sejak jadwal terlewat, bukan sejak popup ditutup.");
Assert(AdditionalReminderLogic.DueCycle(scheduled.AddMinutes(15), scheduled, 300, 0) == 0,
    "Jadwal terlewat tetap muncul sekali walaupun pengulangan dimatikan.");

string popupSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ReminderPopup.cs"));
Assert(popupSource.Contains("public static void ShowCustom", StringComparison.Ordinal) &&
       popupSource.Contains("Hentikan pengingat ini untuk hari ini", StringComparison.Ordinal) &&
       popupSource.Contains("Ingatkan lagi", StringComparison.Ordinal),
    "Popup pengingat tambahan memakai tombol ✕ hentikan dan tombol ingat lagi.");
Assert(popupSource.Contains("MaxVisiblePopups = 3", StringComparison.Ordinal) &&
       popupSource.Contains("PendingPrayerPopups", StringComparison.Ordinal) &&
       popupSource.Contains("PendingAdditionalPopups", StringComparison.Ordinal) &&
       popupSource.Contains("QueuePopup(popup)", StringComparison.Ordinal) &&
       popupSource.Contains("PumpQueue();", StringComparison.Ordinal),
    "Maksimal 3 popup aktif; antrean sholat diprioritaskan dan antrean tambahan terpisah.");
Assert(popupSource.Contains("requiredHeight > availableHeight", StringComparison.Ordinal) &&
       popupSource.Contains("popup.Hide();", StringComparison.Ordinal) &&
       popupSource.Contains("PopupGap = 8", StringComparison.Ordinal) &&
       popupSource.Contains("ScreenMargin = 16", StringComparison.Ordinal),
    "Popup baru masuk antrean bila ruang vertikal tidak cukup, sehingga tidak bertabrakan.");
Assert(popupSource.Contains("ReminderPopupNavigate", StringComparison.Ordinal) &&
       popupSource.Contains("_onNavigate?.Invoke()", StringComparison.Ordinal) &&
       popupSource.Contains("if (popup._stopKey is null) popup._closeTimer?.Start();", StringComparison.Ordinal),
    "Tombol buka dzikir tersedia dan timer auto-close 12 detik hanya berjalan saat popup sholat tampil.");
string additionalSchedulerSource = File.ReadAllText(
    Path.Combine(AppContext.BaseDirectory, "AdditionalReminderScheduler.cs"));
Assert(additionalSchedulerSource.Contains("now.Date.AddDays(1)", StringComparison.Ordinal) &&
       additionalSchedulerSource.Contains("OpenDzikirTab", StringComparison.Ordinal) &&
       additionalSchedulerSource.Contains("group.NavigationTarget", StringComparison.Ordinal),
    "Penjadwal memakai observansi besok dan meneruskan navigasi dzikir ke popup.");
string loggerSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ReminderScheduler.cs"));
Assert(loggerSource.Contains("DateTimeOffset.Now:O} INFO", StringComparison.Ordinal) &&
       !loggerSource.Contains("DateTimeOffset.UtcNow:O} INFO", StringComparison.Ordinal),
    "Semua baris INFO log memakai waktu lokal sistem.");
string offlineSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "OfflineDataDownloadService.cs"));
Assert(offlineSource.Contains("installed.json", StringComparison.Ordinal) &&
       offlineSource.Contains("WriteInstalledMetadata(staging, manifest)", StringComparison.Ordinal) &&
       !offlineSource.Contains("tersedia dari cache", StringComparison.Ordinal),
    "Metadata snapshot ditulis saat unduh dan status lama dihapus.");
string mainSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "MainWindow.xaml.cs"));
Assert(mainSource.Contains("PersistFullOfflineDataChoice", StringComparison.Ordinal) &&
       mainSource.Contains("InstalledStatus(snapshot)", StringComparison.Ordinal) &&
       mainSource.Contains("DzikirDoaLink_RequestNavigate", StringComparison.Ordinal),
    "Checkbox, status snapshot, dan tautan kredit tersambung di jendela utama.");
Assert(mainSource.Contains("_savedFormBaseline[FullOfflineDataDraftIndex] = enabled", StringComparison.Ordinal),
    "Centang paket data hanya memperbarui baseline checkbox, bukan seluruh form.");
Assert(mainSource.Contains("RestoreLastTabs()", StringComparison.Ordinal) &&
       mainSource.Contains("_settings.LastMainTabIndex", StringComparison.Ordinal) &&
       mainSource.Contains("_settings.LastDzikirTabIndex", StringComparison.Ordinal) &&
       mainSource.Contains("MainTabs.SelectedIndex = 2", StringComparison.Ordinal) &&
       mainSource.Contains("MainTabs.SelectedIndex = 3", StringComparison.Ordinal),
    "Tab terakhir dipulihkan; Dzikir memakai indeks 2 dan Pengaturan menjadi tab terakhir (3).");
Assert(mainSource.Contains("TrySaveSettings();", StringComparison.Ordinal) &&
       mainSource.Contains("InitializeReminderTimePickers()", StringComparison.Ordinal),
    "Pengaturan disimpan saat menutup ke systray dan picker jam dipasang saat jendela dibuat.");
Assert(mainSource.Contains("candidate.NoFastReminderEnabled", StringComparison.Ordinal) &&
       mainSource.Contains("candidate.NoFastReminderTime", StringComparison.Ordinal) &&
       mainSource.Contains("destination.NoFastReminderTime", StringComparison.Ordinal) &&
       mainSource.Contains("LaranganPuasaReminderCheckBox.IsChecked", StringComparison.Ordinal),
    "Pengingat larangan puasa dimuat, divalidasi, disimpan, dan disalin.");
Assert(mainSource.Contains("_hasHijriSelection", StringComparison.Ordinal) &&
       mainSource.Contains("UpdateHijriSelection();", StringComparison.Ordinal) &&
       mainSource.Contains("UpdateHijriDateButton(cellDate, button, today);", StringComparison.Ordinal) &&
       mainSource.Contains("\"HijriTodayRing-\"", StringComparison.Ordinal) &&
       mainSource.Contains("RestoreHijriSelection(year, calendarToday)", StringComparison.Ordinal) &&
       mainSource.Contains("SelectHijriDate(todayHijri.Year, todayHijri.Month, todayHijri.Day, calendarToday.Date)", StringComparison.Ordinal),
    "Klik tanggal Hijriyah mewarnai ulang cell terpilih, dan tampilan pertama menyorot tanggal hari ini seperti kalender Masehi.");

string mainXaml = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "MainWindow.xaml"));
string[] hourIds = ["SunnahFastingTimeBox", "PublicHolidayTimeBox", "IslamicHolidayTimeBox",
    "LaranganPuasaTimeBox", "MorningDzikirTimeBox", "EveningDzikirTimeBox"];
string[] minuteIds = ["SunnahFastingMinuteBox", "PublicHolidayMinuteBox", "IslamicHolidayMinuteBox",
    "LaranganPuasaMinuteBox", "MorningDzikirMinuteBox", "EveningDzikirMinuteBox"];
Assert(hourIds.All(id => mainXaml.Contains($"AutomationProperties.AutomationId=\"{id}\"", StringComparison.Ordinal)) &&
       minuteIds.All(id => mainXaml.Contains($"AutomationProperties.AutomationId=\"{id}\"", StringComparison.Ordinal)) &&
       mainXaml.Contains("AutomationProperties.AutomationId=\"LaranganPuasaReminderCheckBox\"", StringComparison.Ordinal),
    "Enam pemilih jam dan menit serta checkbox larangan puasa tersedia.");
Assert(mainXaml.Contains("DzikirDoaLink", StringComparison.Ordinal) &&
       mainXaml.Contains("DynamicResource AccentBrush", StringComparison.Ordinal) &&
       mainXaml.Contains("{Binding EntryTitle}", StringComparison.Ordinal) &&
       mainXaml.Contains("FontSize=\"20\"", StringComparison.Ordinal) &&
       mainXaml.Contains("SelectionChanged=\"DzikirDoaTabs_SelectionChanged\"", StringComparison.Ordinal),
    "Hyperlink mengikuti tema, judul doa terikat dan diperbesar, dan sub-tab dzikir tercatat.");
Assert(mainXaml.Contains("Pengingat puasa untuk esok hari.", StringComparison.Ordinal) &&
       mainXaml.Contains("Pengingat larangan berpuasa untuk esok hari.", StringComparison.Ordinal) &&
       mainXaml.Contains("Batas waktu dzikir pagi.", StringComparison.Ordinal) &&
       mainXaml.Contains("Pengingat peringatan Islam hari ini.", StringComparison.Ordinal) &&
       !mainXaml.Contains("10 menit setelah jam masuk kantor", StringComparison.Ordinal) &&
       !mainXaml.Contains("selepas Ashar sebelum menjelang malam", StringComparison.Ordinal),
    "Teks bantuan Pengaturan ringkas tanpa detail jam kantor atau jam default.");
string legend = "Larangan puasa / Tasyrik";
string islamicLegend = "Peringatan Islam";
Assert(mainXaml.Split(legend, StringSplitOptions.None).Length - 1 == 2 &&
       mainXaml.Split(islamicLegend, StringSplitOptions.None).Length - 1 == 2 &&
       !mainXaml.Contains("Tasyrik / larangan puasa", StringComparison.Ordinal) &&
       !mainXaml.Contains("Text=\"Peringatan\"", StringComparison.Ordinal),
    "Legenda Masehi dan Hijriyah memakai istilah yang sama.");
string traySource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TrayService.cs"));
string appSource = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "App.xaml.cs"));
Assert(traySource.Contains("Pengaturan", StringComparison.Ordinal) &&
       traySource.Contains("Dzikir & Doa", StringComparison.Ordinal),
    "Menu systray memuat Pengaturan dan shortcut Dzikir & Doa.");
Assert(appSource.Contains("internal void OpenDzikir()", StringComparison.Ordinal) &&
       appSource.Contains("internal void OpenDzikirTab(string target)", StringComparison.Ordinal),
    "App membuka tab Dzikir dari systray dan sub-tab dari popup pengingat.");

using JsonDocument catalog = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "dzikir-doa.json")));
JsonElement root = catalog.RootElement;
Assert(root.GetProperty("schema").GetInt32() == 1, "Skema data dzikir-doa adalah 1.");
JsonElement sections = root.GetProperty("sections");
Assert(sections.GetArrayLength() == 3, "Data memuat tiga bagian: dzikir pagi, dzikir petang, dan doa.");
foreach (JsonElement section in sections.EnumerateArray())
{
    string sectionId = section.GetProperty("id").GetString() ?? string.Empty;
    Assert(new[] { "dzikir-pagi", "dzikir-petang", "doa" }.Contains(sectionId),
        "Bagian " + sectionId + " dikenal.");
    Assert(!string.IsNullOrWhiteSpace(section.GetProperty("credit").GetString()), "Bagian " + sectionId + " memuat kredit.");
    int count = 0;
    foreach (JsonElement entry in section.GetProperty("entries").EnumerateArray())
    {
        count++;
        Assert(!string.IsNullOrWhiteSpace(entry.GetProperty("arabic").GetString()),
            "Entri " + sectionId + "#" + count + " memuat teks Arab.");
        Assert(!string.IsNullOrWhiteSpace(entry.GetProperty("meaning").GetString()),
            "Entri " + sectionId + "#" + count + " memuat arti.");
        string? entryTitle = entry.TryGetProperty("entry_title", out JsonElement titleElement)
            ? titleElement.GetString()
            : null;
        if (sectionId == "doa")
        {
            Assert(!string.IsNullOrWhiteSpace(entryTitle) &&
                   !string.Equals(entryTitle, entry.GetProperty("source_title").GetString(), StringComparison.Ordinal),
                "Entri doa " + count + " memakai judul deskriptif buatan, bukan judul halaman sumber.");
        }
        else
        {
            Assert(string.IsNullOrWhiteSpace(entryTitle),
                "Entri dzikir " + sectionId + "#" + count + " tidak diberi judul buatan.");
        }
        string source = entry.GetProperty("source_url").GetString() ?? string.Empty;
        Assert(Uri.TryCreate(source, UriKind.Absolute, out Uri? sourceUri) &&
               sourceUri.Scheme == Uri.UriSchemeHttps &&
               string.Equals(sourceUri.Host, "rumaysho.com", StringComparison.OrdinalIgnoreCase),
            "Entri " + sectionId + "#" + count + " memuat tautan kredit Rumaysho.");
    }
    Assert(count > 0, "Bagian " + sectionId + " tidak kosong.");
    Console.WriteLine("INFO: " + sectionId + " memuat " + count + " entri.");
}

Console.WriteLine("Semua pemeriksaan tambahan 2.6.0 lulus.");

namespace PrayerAutomation.UI
{
    // Minimal stand-in: PrayerUiLogic needs this record, not the full schedule service.
    internal sealed record PrayerEntry(string Key, string Label, TimeSpan Time);
}
