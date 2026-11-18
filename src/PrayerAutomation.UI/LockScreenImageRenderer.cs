// Renders the dynamic lock-screen prayer image: the bundled "sedang_sholat" artwork with
// the current prayer name, time, and the app logo, saved as a managed temporary file.
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
using FlowDirection = System.Windows.FlowDirection;
using FontFamily = System.Windows.Media.FontFamily;

namespace PrayerAutomation.UI;

/// <summary>Builds the temporary lock-screen image shown during prayer times.</summary>
internal static class LockScreenImageRenderer
{
    private const double LogoFractionOfHeight = 0.077;
    private const double LogoFractionOfWidth = 0.027;
    private const double LogoTopFraction = 0.016;
    private const double BrandTextFractionOfHeight = 0.0165;
    private const double PrayerTextFractionOfHeight = 0.068;
    private const double PrayerTextTopFraction = 0.166;

    /// <summary>Renders the prayer image for the given trigger and returns its path.</summary>
    /// <param name="triggerKey">Prayer key (e.g. Fajr, Dhuhr).</param>
    /// <returns>Path to the rendered PNG inside the managed directory.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a required asset is missing.</exception>
    internal static string RenderPrayerImage(string triggerKey)
    {
        string backgroundPath = Path.Combine(AppContext.BaseDirectory, "sedang_sholat.png");
        string logoPath = Path.Combine(AppContext.BaseDirectory, "AyoJamaah-logo-white.png");
        if (!File.Exists(backgroundPath))
            throw new FileNotFoundException("The bundled prayer lock-screen image is missing.", backgroundPath);

        BitmapImage background = LoadBitmap(backgroundPath);
        int width = Math.Max(1, background.PixelWidth);
        int height = Math.Max(1, background.PixelHeight);
        DrawingVisual visual = new();
        using (DrawingContext dc = visual.RenderOpen())
        {
            dc.DrawImage(background, new Rect(0, 0, width, height));
            double pixelsPerDip = 96;
            var typefaceBlack = new Typeface(new FontFamily("Segoe UI Black"),
                FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
            var typefaceBold = new Typeface(new FontFamily("Segoe UI"),
                FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
            var culture = CultureInfo.GetCultureInfo("id-ID");

            if (File.Exists(logoPath))
            {
                BitmapImage logo = LoadBitmap(logoPath);
                double logoSize = height * LogoFractionOfHeight;
                double logoX = width * LogoFractionOfWidth;
                double logoY = height * LogoTopFraction;
                dc.DrawImage(logo, new Rect(logoX, logoY, logoSize, logoSize));
                var brand = new FormattedText("AYO JAMA'AH!", culture,
                    FlowDirection.LeftToRight, typefaceBold, height * BrandTextFractionOfHeight,
                    new SolidColorBrush(Color.FromArgb(215, 246, 250, 254)), pixelsPerDip);
                dc.DrawText(brand, new Point(logoX + logoSize / 2 - brand.Width / 2,
                    logoY + logoSize + height * 0.006));
            }

            string prayerText = $"{PrayerScheduleService.LabelFor(triggerKey)?.ToUpper(culture) ?? triggerKey.ToUpper(culture)} · "
                + DateTime.Now.ToString("HH:mm", culture);
            var prayer = new FormattedText(prayerText, culture,
                FlowDirection.LeftToRight, typefaceBlack, height * PrayerTextFractionOfHeight,
                new SolidColorBrush(Color.FromRgb(226, 175, 74)), pixelsPerDip);
            dc.DrawText(prayer, new Point((width - prayer.Width) / 2, height * PrayerTextTopFraction));
        }

        RenderTargetBitmap target = new(width, height, 96, 96, PixelFormats.Pbgra32);
        target.Render(visual);
        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(target));
        string directory = LockScreenStatePaths.DefaultDirectory;
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory,
            LockScreenStatePaths.TemporaryImagePrefix + Guid.NewGuid().ToString("N") + ".png");
        using FileStream stream = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        encoder.Save(stream);
        return path;
    }

    /// <summary>Decodes a PNG from disk into a usable bitmap image.</summary>
    /// <param name="path">Path of the PNG file.</param>
    /// <returns>The decoded bitmap.</returns>
    private static BitmapImage LoadBitmap(string path)
    {
        BitmapImage image = new();
        using FileStream stream = File.OpenRead(path);
        image.BeginInit();
        image.StreamSource = stream;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
