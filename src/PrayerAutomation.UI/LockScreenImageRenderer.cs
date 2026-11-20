// Renders the dynamic lock-screen prayer image: the bundled "sedang_sholat" artwork with
// the prayer name, its scheduled time, and the app logo, saved as a managed temporary file.
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
using Application = System.Windows.Application;

namespace PrayerAutomation.UI;

/// <summary>Builds the temporary lock-screen image shown during prayer times.</summary>
internal static class LockScreenImageRenderer
{
    // The artwork (1920x1080) has an empty inner dome spanning x 664..1255 and
    // y 706..839. Text centred inside it stays clear of the minarets and roof, and
    // survives the side/top cropping Windows applies on non-16:9 displays.
    private const double DomeLeftFraction = 664.0 / 1920.0;
    private const double DomeTopFraction = 706.0 / 1080.0;
    private const double DomeWidthFraction = (1255.0 - 664.0) / 1920.0;
    private const double DomeHeightFraction = (839.0 - 706.0) / 1080.0;
    // Windows fills the lock screen by cropping the 16:9 artwork to the monitor's
    // aspect ratio (and Win11 adds a slight zoom/pan). A logo at 2.7%/1.6% sat
    // inside that crop and arrived clipped on non-16:9 displays. ~12%/15% adds
    // extra margin beyond the 10%/13% trial while staying in the upper left and
    // clear of the top-centre system clock and the dome text.
    private const double LogoFractionOfWidth = 0.12;
    private const double LogoTopFraction = 0.15;
    private const double LogoFractionOfHeight = 0.077;
    private const double BrandTextFractionOfHeight = 0.0165;
    private const double BrandGapFractionOfHeight = 0.006;
    // Prayer name and time scale with the dome instead of the whole image.
    private const double PrayerNameFractionOfDome = 0.16;
    private const double PrayerTimeFractionOfDome = 0.24;
    private const double PrayerGapFractionOfDome = 0.06;

    /// <summary>Renders the prayer image for the given trigger and returns its path.</summary>
    /// <param name="triggerKey">Prayer key (e.g. Fajr, Dhuhr).</param>
    /// <param name="scheduledTime">Prayer time shown on the image, not the render time.</param>
    /// <returns>Path to the rendered PNG inside the managed directory.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a required asset is missing.</exception>
    internal static string RenderPrayerImage(string triggerKey, TimeSpan scheduledTime)
    {
        BitmapSource target = RenderBitmap(triggerKey, scheduledTime, 0);
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

    /// <summary>Renders the same layout at preview resolution for the settings window.</summary>
    /// <param name="triggerKey">Prayer key (e.g. Fajr, Dhuhr).</param>
    /// <param name="scheduledTime">Prayer time shown on the image.</param>
    /// <returns>The rendered bitmap, frozen for cross-thread use.</returns>
    internal static BitmapSource RenderPreview(string triggerKey, TimeSpan scheduledTime) =>
        RenderBitmap(triggerKey, scheduledTime, 1280);

    /// <summary>Renders the overlay at native or preview width.</summary>
    /// <param name="triggerKey">Prayer key to label.</param>
    /// <param name="scheduledTime">Scheduled prayer time to display.</param>
    /// <param name="previewWidth">Target width, or 0 for the artwork's native width.</param>
    /// <returns>The rendered bitmap.</returns>
    private static BitmapSource RenderBitmap(string triggerKey, TimeSpan scheduledTime, int previewWidth)
    {
        string backgroundPath = Path.Combine(AppContext.BaseDirectory, "sedang_sholat.png");
        string logoPath = Path.Combine(AppContext.BaseDirectory, "AyoJamaah-logo-white.png");
        if (!File.Exists(backgroundPath))
            throw new FileNotFoundException("The bundled prayer lock-screen image is missing.", backgroundPath);

        BitmapImage background = LoadBitmap(backgroundPath);
        int width = previewWidth > 0 ? previewWidth : Math.Max(1, background.PixelWidth);
        int height = Math.Max(1, (int)Math.Round(width * (double)background.PixelHeight / Math.Max(1, background.PixelWidth)));
        DrawingVisual visual = new();
        using (DrawingContext dc = visual.RenderOpen())
        {
            dc.DrawImage(background, new Rect(0, 0, width, height));
            double pixelsPerDip = ResolvePixelsPerDip();
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
                dc.DrawText(brand, new Point(logoX + (logoSize - brand.Width) / 2,
                    logoY + logoSize + height * BrandGapFractionOfHeight));
            }

            double domeLeft = width * DomeLeftFraction;
            double domeTop = height * DomeTopFraction;
            double domeWidth = width * DomeWidthFraction;
            double domeHeight = height * DomeHeightFraction;
            string prayerName = PrayerScheduleService.LabelFor(triggerKey)?.ToUpper(culture)
                ?? triggerKey.ToUpper(culture);
            string prayerTime = scheduledTime.ToString(@"hh\.mm", CultureInfo.InvariantCulture);

            var name = new FormattedText(prayerName, culture,
                FlowDirection.LeftToRight, typefaceBold, domeHeight * PrayerNameFractionOfDome,
                new SolidColorBrush(Color.FromArgb(235, 226, 175, 74)), pixelsPerDip);
            var time = new FormattedText(prayerTime, culture,
                FlowDirection.LeftToRight, typefaceBlack, domeHeight * PrayerTimeFractionOfDome,
                new SolidColorBrush(Color.FromRgb(246, 250, 254)), pixelsPerDip);
            double gap = domeHeight * PrayerGapFractionOfDome;
            double blockHeight = name.Height + gap + time.Height;
            double blockTop = domeTop + (domeHeight - blockHeight) / 2;
            double centerX = domeLeft + domeWidth / 2;
            dc.DrawText(name, new Point(centerX - name.Width / 2, blockTop));
            dc.DrawText(time, new Point(centerX - time.Width / 2, blockTop + name.Height + gap));
        }

        RenderTargetBitmap target = new(width, height, 96, 96, PixelFormats.Pbgra32);
        target.Render(visual);
        target.Freeze();
        return target;
    }

    /// <summary>Resolves the monitor DPI used for glyph hinting, falling back to 96 DPI.</summary>
    /// <returns>Pixels per device-independent pixel.</returns>
    private static double ResolvePixelsPerDip()
    {
        try
        {
            Window? window = Application.Current?.MainWindow;
            if (window is not null)
                return VisualTreeHelper.GetDpi(window).PixelsPerDip;
        }
        catch (Exception)
        {
            // Rendering must not fail because the window is closing or unavailable.
        }
        return 1.0;
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
