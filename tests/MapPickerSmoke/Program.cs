using PrayerAutomation.UI;

/// <summary>Requires the condition and throws otherwise.</summary>
/// <param name="condition">Condition that must hold.</param>
/// <param name="message">Failure message used when the condition is false.</param>
static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

foreach ((double latitude, double longitude) in new[]
         {
             (-6.21462, 106.84513), (51.5074, -0.1278), (0.0, 0.0), (-33.8688, 151.2093), (35.6895, 139.6917)
         })
{
    MapPoint pixel = MapProjection.WorldPixel(latitude, longitude, 12);
    (double decodedLatitude, double decodedLongitude) = MapProjection.CoordinatesFromWorldPixel(pixel, 12);
    Require(Math.Abs(decodedLatitude - latitude) < 0.00001 &&
            Math.Abs(decodedLongitude - longitude) < 0.00001,
        $"Web Mercator round trip must preserve {latitude:F5}, {longitude:F5}.");
}

Require(Math.Abs(MapProjection.ClampLatitude(91.0) - MapProjection.MaxLatitude) < 0.00001 &&
        Math.Abs(MapProjection.ClampLatitude(-91.0) + MapProjection.MaxLatitude) < 0.00001,
    "Latitude must be clipped to the Web Mercator range.");
Require(MapProjection.ClampZoom(1) == MapProjection.MinZoom &&
        MapProjection.ClampZoom(19) == MapProjection.MaxZoom,
    "Zoom must stay inside the supported range.");
Require(Math.Abs(MapProjection.NormalizeLongitude(181.0) + 179.0) < 0.00001,
    "Longitude must wrap across the antimeridian.");

IReadOnlyList<MapTile> tiles = MapProjection.VisibleTiles(-6.21462, 106.84513, 11, 640, 420);
Require(tiles.Count > 0 && tiles.Count <= 12 && tiles.All(tile => tile.Zoom == 11),
    "A normal viewport must request a small, single-zoom tile set.");
Require(tiles.All(tile => tile.X >= 0 && tile.X < MapProjection.TileCount(11) &&
                          tile.Y >= 0 && tile.Y < MapProjection.TileCount(11)),
    "Every visible tile must be inside the Web Mercator tile grid.");
IReadOnlyList<MapTile> wrapped = MapProjection.VisibleTiles(0.0, 179.99, 3, 256, 256);
Require(wrapped.Any(tile => tile.X == 0),
    "Tiles crossing the antimeridian must wrap to column zero.");
Require(MapProjection.VisibleTiles(0.0, 0.0, 11, 0, 0).Count == 0,
    "An empty viewport must not schedule tile requests.");

Uri uri = OpenStreetMapTileService.BuildTileUri(new MapTile(3, 5, 11));
Require(uri.Scheme == Uri.UriSchemeHttps && uri.Host == "tile.openstreetmap.org" &&
        uri.AbsolutePath == "/11/3/5.png",
    "Tile URIs must be fixed to HTTPS on the OSM tile host.");
foreach (MapTile invalid in new[]
         {
             new MapTile(-1, 0, 11), new MapTile(0, -1, 11),
             new MapTile(1 << 11, 0, 11), new MapTile(0, 0, 1), new MapTile(0, 0, 19)
         })
{
    bool rejected = false;
    try { OpenStreetMapTileService.BuildTileUri(invalid); }
    catch (ArgumentOutOfRangeException) { rejected = true; }
    Require(rejected, $"Invalid tile {invalid} must be rejected before any request.");
}

var service = new OpenStreetMapTileService();
bool consentRejected = false;
try { await service.TryGetTileAsync(new MapTile(3, 5, 11), userConsentGranted: false); }
catch (InvalidOperationException) { consentRejected = true; }
Require(consentRejected, "Tile loading without explicit consent must be rejected before network access.");

Console.WriteLine("PASS: Web Mercator projection, viewport tiles, OSM URI validation, and tile consent gate.");
