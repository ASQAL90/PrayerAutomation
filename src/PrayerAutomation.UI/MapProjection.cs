namespace PrayerAutomation.UI;

/// <summary>A point in world-pixel space at a Web Mercator zoom level.</summary>
/// <param name="X">Horizontal pixel position from the left edge of the world.</param>
/// <param name="Y">Vertical pixel position from the top edge of the world.</param>
internal readonly record struct MapPoint(double X, double Y);

/// <summary>One square tile in the Web Mercator tile pyramid.</summary>
/// <param name="X">Column, wrapped into the valid range for the zoom level.</param>
/// <param name="Y">Row, north to south.</param>
/// <param name="Zoom">Web Mercator zoom level.</param>
internal readonly record struct MapTile(int X, int Y, int Zoom);

/// <summary>
/// Pure Web Mercator projection used by the location picker. It has no WPF or
/// network dependency so it can be exercised by a plain smoke test.
/// </summary>
internal static class MapProjection
{
    /// <summary>Side length of one raster tile in device-independent pixels.</summary>
    internal const int TileSize = 256;
    /// <summary>Lowest supported zoom; the whole world still fits in a small viewport.</summary>
    internal const int MinZoom = 2;
    /// <summary>Highest supported zoom; above this OSM detail no longer helps a prayer-time location.</summary>
    internal const int MaxZoom = 18;
    /// <summary>Latitude at which Web Mercator is clipped.</summary>
    internal const double MaxLatitude = 85.05112878;
    private const int MaxTilesPerView = 64;

    /// <summary>Clamps a zoom level to the supported range.</summary>
    /// <param name="zoom">Requested zoom.</param>
    /// <returns>Supported zoom.</returns>
    internal static int ClampZoom(int zoom) => Math.Clamp(zoom, MinZoom, MaxZoom);

    /// <summary>Clamps a latitude to the Web Mercator range.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <returns>Clamped latitude.</returns>
    internal static double ClampLatitude(double latitude) => Math.Clamp(latitude, -MaxLatitude, MaxLatitude);

    /// <summary>Wraps a longitude into the range [-180, 180).</summary>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <returns>Wrapped longitude.</returns>
    internal static double NormalizeLongitude(double longitude) =>
        ((longitude + 180.0) % 360.0 + 360.0) % 360.0 - 180.0;

    /// <summary>Number of tiles on one axis at a zoom level.</summary>
    /// <param name="zoom">Zoom level.</param>
    /// <returns>Tile count per axis.</returns>
    internal static int TileCount(int zoom) => 1 << ClampZoom(zoom);

    /// <summary>World pixel width/height at a zoom level.</summary>
    /// <param name="zoom">Zoom level.</param>
    /// <returns>World scale in pixels.</returns>
    internal static double WorldScale(int zoom) => TileSize * Math.Pow(2.0, ClampZoom(zoom));

    /// <summary>Converts longitude directly to world X without wrapping, for drawing grid lines.</summary>
    /// <param name="longitude">Longitude in degrees, possibly outside the usual range.</param>
    /// <param name="zoom">Zoom level.</param>
    /// <returns>Horizontal world pixel.</returns>
    internal static double LongitudeToWorldX(double longitude, int zoom) =>
        (longitude + 180.0) / 360.0 * WorldScale(zoom);

    /// <summary>Converts latitude directly to world Y without wrapping.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="zoom">Zoom level.</param>
    /// <returns>Vertical world pixel.</returns>
    internal static double LatitudeToWorldY(double latitude, int zoom)
    {
        double sine = Math.Sin(ClampLatitude(latitude) * Math.PI / 180.0);
        return (0.5 - Math.Log((1.0 + sine) / (1.0 - sine)) / (4.0 * Math.PI)) * WorldScale(zoom);
    }

    /// <summary>Converts geographic coordinates to world pixels.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="zoom">Zoom level.</param>
    /// <returns>World pixel position.</returns>
    internal static MapPoint WorldPixel(double latitude, double longitude, int zoom) =>
        new(LongitudeToWorldX(NormalizeLongitude(longitude), zoom), LatitudeToWorldY(latitude, zoom));

    /// <summary>Converts a world pixel back to geographic coordinates.</summary>
    /// <param name="pixel">World pixel position.</param>
    /// <param name="zoom">Zoom level.</param>
    /// <returns>Latitude and longitude in degrees.</returns>
    internal static (double Latitude, double Longitude) CoordinatesFromWorldPixel(MapPoint pixel, int zoom)
    {
        double scale = WorldScale(zoom);
        double x = ((pixel.X % scale) + scale) % scale;
        double y = Math.Clamp(pixel.Y, 0.0, scale);
        double latitude = Math.Atan(Math.Sinh(Math.PI - 2.0 * Math.PI * y / scale)) * 180.0 / Math.PI;
        double longitude = x / scale * 360.0 - 180.0;
        return (ClampLatitude(latitude), NormalizeLongitude(longitude));
    }

    /// <summary>Lists the tiles needed to cover a viewport, west to east and north to south.</summary>
    /// <param name="latitude">Viewport centre latitude.</param>
    /// <param name="longitude">Viewport centre longitude.</param>
    /// <param name="zoom">Zoom level.</param>
    /// <param name="viewportWidth">Viewport width in device-independent pixels.</param>
    /// <param name="viewportHeight">Viewport height in device-independent pixels.</param>
    /// <returns>Tiles to request, capped at a small number per redraw.</returns>
    internal static IReadOnlyList<MapTile> VisibleTiles(
        double latitude, double longitude, int zoom, double viewportWidth, double viewportHeight)
    {
        if (viewportWidth <= 0 || viewportHeight <= 0) return [];

        int clampedZoom = ClampZoom(zoom);
        double scale = WorldScale(clampedZoom);
        MapPoint centre = WorldPixel(latitude, longitude, clampedZoom);
        int firstX = (int)Math.Floor((centre.X - viewportWidth / 2.0) / TileSize);
        int lastX = (int)Math.Floor((centre.X + viewportWidth / 2.0 - 0.001) / TileSize);
        int firstY = Math.Max(0, (int)Math.Floor((centre.Y - viewportHeight / 2.0) / TileSize));
        int lastY = Math.Min(TileCount(clampedZoom) - 1,
            (int)Math.Floor((centre.Y + viewportHeight / 2.0 - 0.001) / TileSize));

        var tiles = new List<MapTile>();
        for (int y = firstY; y <= lastY; y++)
        {
            for (int x = firstX; x <= lastX; x++)
            {
                if (tiles.Count >= MaxTilesPerView) return tiles;
                int wrappedX = ((x % TileCount(clampedZoom)) + TileCount(clampedZoom)) % TileCount(clampedZoom);
                tiles.Add(new MapTile(wrappedX, y, clampedZoom));
            }
        }
        return tiles;
    }
}
