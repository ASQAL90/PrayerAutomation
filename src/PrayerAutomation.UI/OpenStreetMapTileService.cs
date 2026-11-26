using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PrayerAutomation.UI;

/// <summary>
/// Optional OpenStreetMap raster tile loader. Nothing is fetched until the user
/// gives explicit consent and asks for online tiles; the picker stays usable
/// offline with coordinates, grid lines, and a marker.
/// </summary>
internal sealed class OpenStreetMapTileService
{
    /// <summary>Attribution required whenever OSM tiles are displayed.</summary>
    internal const string Attribution = "© OpenStreetMap contributors";
    /// <summary>OSM copyright page shown in the picker.</summary>
    internal const string AttributionUri = "https://www.openstreetmap.org/copyright";
    private const string TileTemplate = "https://tile.openstreetmap.org/{0}/{1}/{2}.png";
    private const int MaxTileBytes = 256 * 1024;
    private const int MaxCachedTiles = 256;
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(24);
    private static readonly TimeSpan MinimumRequestInterval = TimeSpan.FromMilliseconds(600);
    private static readonly HttpClient SharedHttpClient = new() { Timeout = TimeSpan.FromSeconds(15) };
    private static readonly SemaphoreSlim RequestGate = new(1, 1);
    private static readonly object CacheLock = new();
    private static readonly Dictionary<string, TileCacheEntry> MemoryCache = new(StringComparer.Ordinal);
    private static readonly string UserAgent =
        $"AyoJamaah/{typeof(OpenStreetMapTileService).Assembly.GetName().Version} (https://github.com/ASQAL90/PrayerAutomation)";
    private static DateTimeOffset _lastRequestUtc = DateTimeOffset.MinValue;

    /// <summary>Builds and validates the HTTPS URI for one OSM tile.</summary>
    /// <param name="tile">Tile to fetch.</param>
    /// <returns>An absolute HTTPS URI on tile.openstreetmap.org.</returns>
    internal static Uri BuildTileUri(MapTile tile)
    {
        int count = 1 << tile.Zoom;
        if (tile.Zoom < MapProjection.MinZoom || tile.Zoom > MapProjection.MaxZoom ||
            tile.X < 0 || tile.X >= count || tile.Y < 0 || tile.Y >= count)
            throw new ArgumentOutOfRangeException(nameof(tile), "Tile berada di luar rentang Web Mercator.");
        return new Uri(string.Format(CultureInfo.InvariantCulture, TileTemplate, tile.Zoom, tile.X, tile.Y), UriKind.Absolute);
    }

    /// <summary>
    /// Loads one tile from the in-memory cache or OSM. Network failures return
    /// null so the picker can continue with its offline grid and marker.
    /// </summary>
    /// <param name="tile">Tile to fetch.</param>
    /// <param name="userConsentGranted">Must be true after an explicit user action.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>PNG bytes, or null when the tile could not be loaded.</returns>
    internal async Task<byte[]?> TryGetTileAsync(
        MapTile tile, bool userConsentGranted, CancellationToken cancellationToken = default)
    {
        if (!userConsentGranted)
            throw new InvalidOperationException("Tile peta online memerlukan persetujuan eksplisit.");

        Uri uri = BuildTileUri(tile);
        string key = uri.AbsoluteUri;
        if (TryGetCached(key, out byte[]? cached)) return cached;

        await RequestGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (TryGetCached(key, out cached)) return cached;

            TimeSpan elapsed = DateTimeOffset.UtcNow - _lastRequestUtc;
            TimeSpan wait = MinimumRequestInterval - elapsed;
            if (wait > TimeSpan.Zero)
                await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/png"));
            _lastRequestUtc = DateTimeOffset.UtcNow;

            using HttpResponseMessage response = await SharedHttpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;

            string? mediaType = response.Content.Headers.ContentType?.MediaType;
            if (mediaType is null || !mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) return null;
            if (response.Content.Headers.ContentLength > MaxTileBytes) return null;

            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var buffer = new MemoryStream();
            byte[] chunk = new byte[8192];
            int total = 0;
            while (true)
            {
                int read = await stream.ReadAsync(chunk, cancellationToken).ConfigureAwait(false);
                if (read == 0) break;
                total += read;
                if (total > MaxTileBytes) return null;
                buffer.Write(chunk, 0, read);
            }
            byte[] bytes = buffer.ToArray();
            AddToCache(key, bytes);
            return bytes;
        }
        catch (Exception ex) when ((ex is HttpRequestException or IOException or InvalidDataException or OperationCanceledException)
                                   && !cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        finally
        {
            RequestGate.Release();
        }
    }

    /// <summary>Looks up a fresh tile in the bounded memory cache.</summary>
    /// <param name="key">Tile URI.</param>
    /// <param name="bytes">Cached PNG bytes.</param>
    /// <returns>True on a cache hit.</returns>
    private static bool TryGetCached(string key, out byte[]? bytes)
    {
        lock (CacheLock)
        {
            if (MemoryCache.TryGetValue(key, out TileCacheEntry? entry))
            {
                if (entry.ExpiresAtUtc > DateTimeOffset.UtcNow)
                {
                    bytes = entry.Bytes;
                    return true;
                }
                MemoryCache.Remove(key);
            }
        }
        bytes = null;
        return false;
    }

    /// <summary>Adds a tile to the bounded memory cache, dropping the oldest entries first.</summary>
    /// <param name="key">Tile URI.</param>
    /// <param name="bytes">PNG bytes.</param>
    private static void AddToCache(string key, byte[] bytes)
    {
        lock (CacheLock)
        {
            while (MemoryCache.Count >= MaxCachedTiles)
            {
                string? oldest = MemoryCache.OrderBy(pair => pair.Value.ExpiresAtUtc)
                    .Select(pair => pair.Key).FirstOrDefault();
                if (oldest is null) return;
                MemoryCache.Remove(oldest);
            }
            MemoryCache[key] = new TileCacheEntry(bytes, DateTimeOffset.UtcNow + CacheLifetime);
        }
    }

    private sealed record TileCacheEntry(byte[] Bytes, DateTimeOffset ExpiresAtUtc);
}
