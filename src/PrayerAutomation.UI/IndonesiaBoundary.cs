using System.IO;

namespace PrayerAutomation.UI;

/// <summary>
/// Indonesia land boundary built from Natural Earth (public domain) and stored as a
/// compact binary of exterior rings. Used only to choose the detailed Indonesian
/// place layer; it is never a legal or navigational boundary.
/// </summary>
internal sealed class IndonesiaBoundary
{
    private const int MaximumPolygonCount = 100_000;
    private const int MaximumPointCount = 5_000_000;
    private const double KilometresPerDegreeLatitude = 110.574;
    private const double KilometresPerDegreeLongitude = 111.320;

    private readonly double[][] _rings;
    private readonly double[] _minimumLongitude;
    private readonly double[] _minimumLatitude;
    private readonly double[] _maximumLongitude;
    private readonly double[] _maximumLatitude;

    private IndonesiaBoundary(double[][] rings)
    {
        _rings = rings;
        _minimumLongitude = new double[rings.Length];
        _minimumLatitude = new double[rings.Length];
        _maximumLongitude = new double[rings.Length];
        _maximumLatitude = new double[rings.Length];
        for (int index = 0; index < rings.Length; index++)
        {
            double[] ring = rings[index];
            double minLon = double.PositiveInfinity;
            double minLat = double.PositiveInfinity;
            double maxLon = double.NegativeInfinity;
            double maxLat = double.NegativeInfinity;
            for (int point = 0; point < ring.Length; point += 2)
            {
                double lon = ring[point];
                double lat = ring[point + 1];
                if (lon < minLon) minLon = lon;
                if (lon > maxLon) maxLon = lon;
                if (lat < minLat) minLat = lat;
                if (lat > maxLat) maxLat = lat;
            }
            _minimumLongitude[index] = minLon;
            _minimumLatitude[index] = minLat;
            _maximumLongitude[index] = maxLon;
            _maximumLatitude[index] = maxLat;
        }
    }

    /// <summary>Loads the bundled boundary asset.</summary>
    /// <param name="stream">Binary source (AJB1 format).</param>
    /// <returns>The loaded boundary.</returns>
    /// <exception cref="InvalidDataException">The header or a polygon is malformed.</exception>
    internal static IndonesiaBoundary Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        Span<byte> magic = stackalloc byte[4];
        if (reader.Read(magic) != 4 || magic[0] != (byte)'A' || magic[1] != (byte)'J' ||
            magic[2] != (byte)'B' || magic[3] != (byte)'1')
        {
            throw new InvalidDataException("Berkas batas Indonesia tidak dikenali.");
        }

        int polygonCount = reader.ReadInt32();
        if (polygonCount is <= 0 or > MaximumPolygonCount)
            throw new InvalidDataException("Jumlah poligon batas Indonesia tidak valid.");

        var rings = new double[polygonCount][];
        long totalPoints = 0;
        for (int index = 0; index < polygonCount; index++)
        {
            int pointCount = reader.ReadInt32();
            if (pointCount < 3 || totalPoints + pointCount > MaximumPointCount)
                throw new InvalidDataException("Jumlah titik batas Indonesia tidak valid.");
            totalPoints += pointCount;
            var ring = new double[pointCount * 2];
            for (int point = 0; point < pointCount; point++)
            {
                double longitude = reader.ReadDouble();
                double latitude = reader.ReadDouble();
                if (!double.IsFinite(longitude) || !double.IsFinite(latitude) ||
                    latitude is < -90 or > 90 || longitude is < -180 or > 180)
                {
                    throw new InvalidDataException("Koordinat batas Indonesia tidak valid.");
                }
                ring[point * 2] = longitude;
                ring[point * 2 + 1] = latitude;
            }
            rings[index] = ring;
        }

        return new IndonesiaBoundary(rings);
    }

    /// <summary>Checks whether a coordinate lies inside the boundary, optionally within a tolerance.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="toleranceKm">Allowed distance outside the coastline in kilometers.</param>
    /// <returns>True when inside or within the tolerance.</returns>
    internal bool Contains(double latitude, double longitude, double toleranceKm = 0)
    {
        if (!double.IsFinite(latitude) || !double.IsFinite(longitude) || toleranceKm < 0)
            return false;

        // Convert the coastline tolerance from kilometres to degrees before applying it
        // to either axis. Longitude degrees shrink toward the poles.
        double latitudeTolerance = toleranceKm / KilometresPerDegreeLatitude;
        double longitudeTolerance = toleranceKm / KilometresPerDegreeLongitude;
        double cosine = Math.Cos(latitude * (Math.PI / 180.0));
        if (cosine > 0.01)
            longitudeTolerance /= cosine;

        for (int index = 0; index < _rings.Length; index++)
        {
            if (longitude < _minimumLongitude[index] - longitudeTolerance ||
                longitude > _maximumLongitude[index] + longitudeTolerance ||
                latitude < _minimumLatitude[index] - latitudeTolerance ||
                latitude > _maximumLatitude[index] + latitudeTolerance)
            {
                continue;
            }

            double[] ring = _rings[index];
            if (IsInside(ring, latitude, longitude))
                return true;
            if (toleranceKm > 0 && DistanceToRingKm(ring, latitude, longitude) <= toleranceKm)
                return true;
        }

        return false;
    }

    /// <summary>Ray-casting point-in-polygon test on a flat lon/lat ring.</summary>
    /// <param name="ring">Flat coordinate ring.</param>
    /// <param name="latitude">Point latitude.</param>
    /// <param name="longitude">Point longitude.</param>
    /// <returns>True when the point is inside.</returns>
    private static bool IsInside(double[] ring, double latitude, double longitude)
    {
        bool inside = false;
        int count = ring.Length / 2;
        int previous = count - 1;
        for (int current = 0; current < count; current++)
        {
            double currentLon = ring[current * 2];
            double currentLat = ring[current * 2 + 1];
            double previousLon = ring[previous * 2];
            double previousLat = ring[previous * 2 + 1];
            if ((currentLat > latitude) != (previousLat > latitude) &&
                longitude < (previousLon - currentLon) * (latitude - currentLat) /
                    (previousLat - currentLat) + currentLon)
            {
                inside = !inside;
            }
            previous = current;
        }
        return inside;
    }

    /// <summary>Approximate distance from a point to the nearest ring segment, in kilometers.</summary>
    /// <param name="ring">Flat coordinate ring.</param>
    /// <param name="latitude">Point latitude.</param>
    /// <param name="longitude">Point longitude.</param>
    /// <returns>Distance in kilometers.</returns>
    private static double DistanceToRingKm(double[] ring, double latitude, double longitude)
    {
        double longitudeScale = Math.Max(0.01, Math.Cos(latitude * (Math.PI / 180.0)));
        double minimum = double.PositiveInfinity;
        int count = ring.Length / 2;
        int previous = count - 1;
        for (int current = 0; current < count; current++)
        {
            double ax = (ring[previous * 2] - longitude) * KilometresPerDegreeLongitude * longitudeScale;
            double ay = (ring[previous * 2 + 1] - latitude) * KilometresPerDegreeLatitude;
            double bx = (ring[current * 2] - longitude) * KilometresPerDegreeLongitude * longitudeScale;
            double by = (ring[current * 2 + 1] - latitude) * KilometresPerDegreeLatitude;
            double dx = bx - ax;
            double dy = by - ay;
            double lengthSquared = dx * dx + dy * dy;
            double t = lengthSquared <= double.Epsilon ? 0 :
                Math.Clamp(-(ax * dx + ay * dy) / lengthSquared, 0, 1);
            double px = ax + t * dx;
            double py = ay + t * dy;
            double distance = Math.Sqrt(px * px + py * py);
            if (distance < minimum)
                minimum = distance;
            previous = current;
        }
        return minimum;
    }
}
