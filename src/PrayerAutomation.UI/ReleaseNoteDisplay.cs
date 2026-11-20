// Shared display row for release-note panels. WPF-only types are kept out of
// the headless parser so its smoke test stays free of UI dependencies.
using System.Windows;

namespace PrayerAutomation.UI;

/// <summary>One rendered line of release notes or changelog.</summary>
/// <param name="Text">Display text, already cleaned by the parser.</param>
/// <param name="Margin">Indent used for section headings versus bullets.</param>
/// <param name="Weight">Font weight used for section headings versus bullets.</param>
internal sealed record ReleaseNoteDisplay(string Text, Thickness Margin, FontWeight Weight);
