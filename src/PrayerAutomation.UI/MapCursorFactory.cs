// Runtime-generated map cursors: pin for placement, magnifier for zoom.
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32.SafeHandles;

namespace PrayerAutomation.UI;

/// <summary>
/// Builds small themed cursors so the map can signal pin placement and the
/// zoom direction without shipping binary cursor assets or a new dependency.
/// Creation only runs on Windows; any failure falls back to built-in cursors.
/// </summary>
internal static class MapCursorFactory
{
    private const int CursorSize = 32;
    private static readonly Lazy<Cursor> PinLazy = new(() => Create(CreatePinDrawing(), 16, 30));
    private static readonly Lazy<Cursor> ZoomInLazy = new(() => Create(CreateZoomDrawing(zoomIn: true), 13, 13));
    private static readonly Lazy<Cursor> ZoomOutLazy = new(() => Create(CreateZoomDrawing(zoomIn: false), 13, 13));

    /// <summary>Cursor shown while a click would drop the location pin.</summary>
    internal static Cursor Pin => PinLazy.Value;

    /// <summary>Cursor shown briefly when the wheel zooms in.</summary>
    internal static Cursor ZoomIn => ZoomInLazy.Value;

    /// <summary>Cursor shown briefly when the wheel zooms out.</summary>
    internal static Cursor ZoomOut => ZoomOutLazy.Value;

    private static Drawing CreatePinDrawing()
    {
        var group = new DrawingGroup();

        var body = new StreamGeometry();
        using (StreamGeometryContext context = body.Open())
        {
            context.BeginFigure(new Point(16, 2.5), true, true);
            context.BezierTo(new Point(8.8, 2.5), new Point(3, 8.3), new Point(3, 15.5), true, true);
            context.BezierTo(new Point(3, 20.6), new Point(6.6, 24.8), new Point(10.6, 27.4), true, true);
            context.LineTo(new Point(16, 31), true, true);
            context.LineTo(new Point(21.4, 27.4), true, true);
            context.BezierTo(new Point(25.4, 24.8), new Point(29, 20.6), new Point(29, 15.5), true, true);
            context.BezierTo(new Point(29, 8.3), new Point(23.2, 2.5), new Point(16, 2.5), true, true);
        }

        body.Freeze();
        group.Children.Add(new GeometryDrawing(
            new SolidColorBrush(Color.FromRgb(0xE5, 0x48, 0x4D)),
            new Pen(new SolidColorBrush(Color.FromRgb(0x5B, 0x11, 0x16)), 1.2),
            body));
        group.Children.Add(new GeometryDrawing(
            Brushes.White, null, new EllipseGeometry(new Point(16, 14.5), 3.6, 3.6)));
        return group;
    }

    private static Drawing CreateZoomDrawing(bool zoomIn)
    {
        var group = new DrawingGroup();
        var outline = new SolidColorBrush(Color.FromRgb(0x1F, 0x29, 0x37));
        var accent = new SolidColorBrush(zoomIn
            ? Color.FromRgb(0x12, 0xA1, 0x50)
            : Color.FromRgb(0xD9, 0x30, 0x25));

        group.Children.Add(new GeometryDrawing(
            new SolidColorBrush(Color.FromArgb(240, 0xFF, 0xFF, 0xFF)),
            new Pen(outline, 2.6),
            new EllipseGeometry(new Point(13, 13), 8.5, 8.5)));

        var handlePen = new Pen(outline, 3.2)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
        };
        group.Children.Add(new GeometryDrawing(
            null, handlePen, new LineGeometry(new Point(19.4, 19.4), new Point(28, 28))));

        var markPen = new Pen(accent, 2.6)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
        };
        group.Children.Add(new GeometryDrawing(
            null, markPen, new LineGeometry(new Point(9, 13), new Point(17, 13))));
        if (zoomIn)
        {
            group.Children.Add(new GeometryDrawing(
                null, markPen, new LineGeometry(new Point(13, 9), new Point(13, 17))));
        }

        return group;
    }

    private static Cursor Create(Drawing drawing, int hotspotX, int hotspotY)
    {
        try
        {
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
                context.DrawDrawing(drawing);

            var bitmap = new RenderTargetBitmap(
                CursorSize, CursorSize, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();

            Cursor? cursor = CreateFromBitmap(bitmap, hotspotX, hotspotY);
            return cursor ?? Cursors.Cross;
        }
        catch (Exception)
        {
            // Cursor creation must never block the map picker.
            return Cursors.Cross;
        }
    }

    private static Cursor? CreateFromBitmap(BitmapSource bitmap, int hotspotX, int hotspotY)
    {
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;
        int stride = width * 4;
        byte[] pixels = new byte[stride * height];
        bitmap.CopyPixels(pixels, stride, 0);

        // DIB sections are bottom-up by default.
        byte[] flipped = new byte[pixels.Length];
        for (int y = 0; y < height; y++)
            Buffer.BlockCopy(pixels, y * stride, flipped, (height - 1 - y) * stride, stride);

        var info = new BitmapInfo
        {
            Header = new BitmapInfoHeader
            {
                Size = Marshal.SizeOf<BitmapInfoHeader>(),
                Width = width,
                Height = height,
                Planes = 1,
                BitCount = 32,
                Compression = 0,
            },
        };

        IntPtr colorBitmap = CreateDIBSection(
            IntPtr.Zero, ref info, 0, out IntPtr bits, IntPtr.Zero, 0);
        if (colorBitmap == IntPtr.Zero) return null;

        IntPtr maskBitmap = IntPtr.Zero;
        IntPtr cursor = IntPtr.Zero;
        try
        {
            Marshal.Copy(flipped, 0, bits, flipped.Length);

            int maskStride = ((width + 15) / 16) * 2;
            byte[] mask = new byte[maskStride * height];
            maskBitmap = CreateBitmap(width, height, 1, 1, mask);
            if (maskBitmap == IntPtr.Zero) return null;

            var iconInfo = new IconInfo
            {
                IsIcon = 0,
                HotspotX = hotspotX,
                HotspotY = hotspotY,
                MaskBitmap = maskBitmap,
                ColorBitmap = colorBitmap,
            };
            cursor = CreateIconIndirect(ref iconInfo);
            if (cursor == IntPtr.Zero) return null;

            return CursorInteropHelper.Create(new SafeCursorHandle(cursor));
        }
        finally
        {
            if (colorBitmap != IntPtr.Zero) DeleteObject(colorBitmap);
            if (maskBitmap != IntPtr.Zero) DeleteObject(maskBitmap);
        }
    }

    /// <summary>Keeps the generated HICON alive for the lifetime of the process.</summary>
    private sealed class SafeCursorHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeCursorHandle(IntPtr handle) : base(true) => SetHandle(handle);

        protected override bool ReleaseHandle() => true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        internal int Size;
        internal int Width;
        internal int Height;
        internal short Planes;
        internal short BitCount;
        internal int Compression;
        internal int SizeImage;
        internal int XPelsPerMeter;
        internal int YPelsPerMeter;
        internal int ClrUsed;
        internal int ClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        internal BitmapInfoHeader Header;
        internal uint Colors;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IconInfo
    {
        internal int IsIcon;
        internal int HotspotX;
        internal int HotspotY;
        internal IntPtr MaskBitmap;
        internal IntPtr ColorBitmap;
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateDIBSection(
        IntPtr hdc, ref BitmapInfo bitmapInfo, uint usage,
        out IntPtr bits, IntPtr section, uint offset);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateBitmap(
        int width, int height, uint planes, uint bitCount, byte[]? bits);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr handle);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateIconIndirect(ref IconInfo iconInfo);
}
