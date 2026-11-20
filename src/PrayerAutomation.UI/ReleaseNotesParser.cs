// Parses release-note Markdown into short, display-safe sections.
// No WPF dependency: the same logic is exercised by the headless smoke test.
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PrayerAutomation.UI;

/// <summary>One recognised release-note section.</summary>
/// <param name="Heading">Canonical Indonesian heading.</param>
/// <param name="Items">Display-ready bullet lines.</param>
internal sealed record ReleaseNotesSection(string Heading, IReadOnlyList<string> Items);

/// <summary>Reads the small subset of Markdown used by release notes.</summary>
internal static class ReleaseNotesParser
{
    private const int MaxItems = 40;
    private const int MaxItemLength = 220;
    private const int MaxTotalLength = 4000;

    private static readonly Dictionary<string, string> SectionNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Fitur baru"] = "Fitur baru",
        ["Features"] = "Fitur baru",
        ["Perbaikan"] = "Perbaikan",
        ["Fixes"] = "Perbaikan",
        ["Bug fixes"] = "Perbaikan",
        ["Perubahan"] = "Perubahan",
        ["Changes"] = "Perubahan",
        ["Catatan"] = "Catatan",
        ["Notes"] = "Catatan"
    };

    private static readonly Regex HtmlComment = new("<!--.*?-->", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex Link = new(@"!?\[([^\]]+)\]\([^)]+\)", RegexOptions.Compiled);
    private static readonly Regex HtmlTag = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex Emphasis = new(@"[*`_~]{1,3}", RegexOptions.Compiled);
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    /// <summary>Parses a GitHub release body into display sections.</summary>
    /// <param name="body">Raw release body, or null when unavailable.</param>
    /// <returns>Recognised sections that contain at least one bullet.</returns>
    internal static IReadOnlyList<ReleaseNotesSection> Parse(string? body)
    {
        if (string.IsNullOrWhiteSpace(body)) return Array.Empty<ReleaseNotesSection>();

        var sections = new List<ReleaseNotesSection>();
        string? current = null;
        var items = new List<string>();
        int total = 0;

        void Flush()
        {
            if (current is not null && items.Count > 0) sections.Add(new ReleaseNotesSection(current, items.ToArray()));
            items.Clear();
        }

        foreach (string raw in body.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            string trimmed = raw.Trim();
            if (trimmed.StartsWith('#'))
            {
                Flush();
                string heading = trimmed.TrimStart('#').Trim().TrimEnd(':').Trim();
                current = SectionNames.TryGetValue(heading, out string? canonical) ? canonical : null;
                continue;
            }
            if (current is null) continue;

            bool bullet = trimmed.StartsWith("- ", StringComparison.Ordinal) ||
                          trimmed.StartsWith("* ", StringComparison.Ordinal);
            if (bullet)
            {
                string text = Clean(trimmed[2..]);
                if (text.Length == 0 || items.Count >= MaxItems || total + text.Length > MaxTotalLength) continue;
                items.Add(Truncate(text, MaxItemLength));
                total += Math.Min(text.Length, MaxItemLength);
            }
            else if (items.Count > 0 && raw.Length > 0 && char.IsWhiteSpace(raw[0]))
            {
                string text = Clean(trimmed);
                if (text.Length == 0 || total + text.Length > MaxTotalLength) continue;
                int index = items.Count - 1;
                int previousLength = items[index].Length;
                items[index] = Truncate(items[index] + " " + text, MaxItemLength);
                total += items[index].Length - previousLength;
            }
        }
        Flush();
        return sections;
    }

    /// <summary>Strips Markdown and HTML decoration from one bullet.</summary>
    /// <param name="value">Raw bullet text.</param>
    /// <returns>Plain display text.</returns>
    private static string Clean(string value)
    {
        string text = HtmlComment.Replace(value, string.Empty);
        text = Link.Replace(text, "$1");
        text = HtmlTag.Replace(text, string.Empty);
        text = Emphasis.Replace(text, string.Empty);
        text = Whitespace.Replace(text, " ").Trim();
        if (text.StartsWith("Full Changelog", StringComparison.OrdinalIgnoreCase)) return string.Empty;
        return text;
    }

    /// <summary>Limits a bullet to the display budget.</summary>
    /// <param name="value">Clean text.</param>
    /// <param name="limit">Maximum length.</param>
    /// <returns>Truncated text with an ellipsis when needed.</returns>
    private static string Truncate(string value, int limit)
    {
        if (value.Length <= limit) return value;
        return value[..(limit - 1)].TrimEnd() + "…";
    }
}
