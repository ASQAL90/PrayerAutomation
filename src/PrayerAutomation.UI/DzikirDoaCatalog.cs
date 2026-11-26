using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace PrayerAutomation.UI;

/// <summary>Local, embedded collection of dzikir and supplication text with source credits.</summary>
internal sealed class DzikirDoaCatalog
{
    private const string ResourceName = "PrayerAutomation.UI.Data.dzikir-doa.json";

    [JsonPropertyName("schema")] public int Schema { get; set; }
    [JsonPropertyName("generated_at")] public string GeneratedAt { get; set; } = string.Empty;
    [JsonPropertyName("sections")] public List<DzikirDoaSection> Sections { get; set; } = [];

    /// <summary>Loads the embedded data file and validates its minimum shape.</summary>
    /// <returns>The parsed catalog.</returns>
    /// <exception cref="InvalidDataException">The embedded data is missing or invalid.</exception>
    internal static DzikirDoaCatalog Load()
    {
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
        if (stream is null)
            throw new InvalidDataException("Data dzikir dan doa tidak ditemukan di dalam aplikasi.");
        DzikirDoaCatalog? catalog;
        try
        {
            catalog = JsonSerializer.Deserialize<DzikirDoaCatalog>(stream);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Data dzikir dan doa tidak dapat dibaca.", ex);
        }
        if (catalog is null || catalog.Schema != 1 || catalog.Sections.Count == 0)
            throw new InvalidDataException("Data dzikir dan doa tidak lengkap.");
        foreach (DzikirDoaSection section in catalog.Sections)
        {
            foreach (DzikirDoaEntry entry in section.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.SourceUrl)) entry.SourceUrl = section.SourceUrl;
                if (string.IsNullOrWhiteSpace(entry.SourceTitle))
                    entry.SourceTitle = string.IsNullOrWhiteSpace(section.SourceUrl)
                        ? "Rumaysho.Com"
                        : section.Title;
            }
        }
        return catalog;
    }

    /// <summary>Returns one section by its stable identifier.</summary>
    /// <param name="id">Section identifier.</param>
    /// <returns>The matching section, or null when absent.</returns>
    internal DzikirDoaSection? Find(string id) =>
        Sections.FirstOrDefault(section => string.Equals(section.Id, id, StringComparison.OrdinalIgnoreCase));
}

/// <summary>One tab of dzikir or supplication entries.</summary>
internal sealed class DzikirDoaSection
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("source_url")] public string SourceUrl { get; set; } = string.Empty;
    [JsonPropertyName("credit")] public string Credit { get; set; } = string.Empty;
    [JsonPropertyName("entries")] public List<DzikirDoaEntry> Entries { get; set; } = [];
}

/// <summary>One Arabic recitation with transliteration and meaning.</summary>
internal sealed class DzikirDoaEntry
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("entry_title")] public string EntryTitle { get; set; } = string.Empty;
    [JsonPropertyName("arabic")] public string Arabic { get; set; } = string.Empty;
    [JsonPropertyName("latin")] public string Latin { get; set; } = string.Empty;
    [JsonPropertyName("meaning")] public string Meaning { get; set; } = string.Empty;
    [JsonPropertyName("note")] public string Note { get; set; } = string.Empty;
    [JsonPropertyName("source_url")] public string SourceUrl { get; set; } = string.Empty;
    [JsonPropertyName("source_title")] public string SourceTitle { get; set; } = string.Empty;

    /// <summary>Visibility of the authored descriptive header.</summary>
    [JsonIgnore]
    public Visibility TitleVisibility => string.IsNullOrWhiteSpace(EntryTitle) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Visibility of the optional short reading note.</summary>
    [JsonIgnore]
    public Visibility NoteVisibility => string.IsNullOrWhiteSpace(Note) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>Short link label shown below each entry.</summary>
    [JsonIgnore]
    public string LinkLabel => string.IsNullOrWhiteSpace(SourceTitle)
        ? "Lihat sumber lengkap di Rumaysho.Com"
        : $"Sumber: {SourceTitle}";
}
