// In-app changelog sourced from the embedded docs/CHANGELOG.md resource.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace PrayerAutomation.UI;

/// <summary>Read-only window showing every released version, newest first.</summary>
internal partial class ReleaseNotesWindow : Window
{
    private const string ResourceName = "PrayerAutomation.UI.CHANGELOG.md";
    private const string FallbackText = "Riwayat perubahan tidak tersedia pada paket ini.";

    /// <summary>Loads the embedded changelog and renders it.</summary>
    internal ReleaseNotesWindow()
    {
        InitializeComponent();
        NotesList.ItemsSource = BuildLines(ReadChangelog());
    }

    /// <summary>Reads the resource without touching the file system.</summary>
    /// <returns>Changelog text, or a short fallback message.</returns>
    private static string ReadChangelog()
    {
        try
        {
            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
            if (stream is null) return FallbackText;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        catch (Exception)
        {
            return FallbackText;
        }
    }

    /// <summary>Turns the Markdown changelog into display rows.</summary>
    /// <param name="changelog">Raw changelog text.</param>
    /// <returns>Rows for version headings and bullets.</returns>
    private static IReadOnlyList<ReleaseNoteDisplay> BuildLines(string changelog)
    {
        var lines = new List<ReleaseNoteDisplay>();
        foreach (string raw in changelog.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (lines.Count >= 600) break;
            string trimmed = raw.Trim();
            if (trimmed.StartsWith("## v", StringComparison.OrdinalIgnoreCase))
            {
                lines.Add(new ReleaseNoteDisplay(trimmed[3..].Trim(),
                    new Thickness(0, lines.Count == 0 ? 0 : 12, 0, 4), FontWeights.SemiBold));
            }
            else if (trimmed.StartsWith("- ", StringComparison.Ordinal))
            {
                string text = PlainText(trimmed[2..]);
                if (text.Length == 0) continue;
                // The TextBlock wraps, so keep the whole bullet. The old 220-character
                // cut made newer changelog entries unreadable ("..."), even though the
                // source document was complete.
                if (text.Length > 2000) text = text[..1999].TrimEnd() + "…";
                lines.Add(new ReleaseNoteDisplay("• " + text, new Thickness(12, 0, 0, 2), FontWeights.Normal));
            }
        }
        return lines;
    }

    /// <summary>Removes Markdown decoration while keeping identifiers such as <c>&lt;Version&gt;</c>.</summary>
    /// <param name="value">Raw bullet text from the changelog.</param>
    /// <returns>Plain text ready for a wrapping TextBlock.</returns>
    private static string PlainText(string value) => value
        .Replace("`", string.Empty, StringComparison.Ordinal)
        .Replace("**", string.Empty, StringComparison.Ordinal)
        .Replace("*", string.Empty, StringComparison.Ordinal)
        .Replace("~~", string.Empty, StringComparison.Ordinal)
        .Replace("_", string.Empty, StringComparison.Ordinal);

    /// <summary>Closes the window.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    /// <summary>Closes the window with Escape.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Key arguments.</param>
    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape) return;
        e.Handled = true;
        Close();
    }

    /// <summary>Allows dragging the borderless window by its header.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse button arguments.</param>
    private void OnHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
