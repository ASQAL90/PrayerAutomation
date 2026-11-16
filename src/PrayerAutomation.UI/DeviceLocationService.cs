using Windows.Devices.Geolocation;

namespace PrayerAutomation.UI;

internal sealed class DeviceLocationService
{
    private static readonly TimeSpan MaximumPositionAge = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PositionTimeout = TimeSpan.FromSeconds(30);

    public async Task<DeviceLocationResult> DetectAsync()
    {
        // RequestAccessAsync must be called by the foreground UI thread before reading location.
        try
        {
            GeolocationAccessStatus accessStatus = await Geolocator.RequestAccessAsync();
            if (accessStatus == GeolocationAccessStatus.Denied)
                return new DeviceLocationResult(DeviceLocationStatus.PermissionDenied);
            if (accessStatus != GeolocationAccessStatus.Allowed)
                return new DeviceLocationResult(DeviceLocationStatus.PermissionUnspecified);

            var geolocator = new Geolocator { DesiredAccuracyInMeters = 100 };
            Geoposition position = await geolocator.GetGeopositionAsync(MaximumPositionAge, PositionTimeout);
            Geocoordinate coordinate = position.Coordinate;
            double latitude = coordinate.Point.Position.Latitude;
            double longitude = coordinate.Point.Position.Longitude;
            if (!DeviceLocationLogic.HasValidCoordinates(latitude, longitude))
                return new DeviceLocationResult(DeviceLocationStatus.InvalidPosition);

            double? accuracy = DeviceLocationLogic.HasValidAccuracy(coordinate.Accuracy)
                ? coordinate.Accuracy
                : null;
            return new DeviceLocationResult(
                DeviceLocationStatus.Success,
                latitude,
                longitude,
                accuracy,
                MapSource(coordinate.PositionSource));
        }
        catch (TimeoutException)
        {
            return new DeviceLocationResult(DeviceLocationStatus.TimedOut);
        }
        catch (OperationCanceledException)
        {
            return new DeviceLocationResult(DeviceLocationStatus.TimedOut);
        }
        catch (UnauthorizedAccessException)
        {
            return new DeviceLocationResult(DeviceLocationStatus.PermissionDenied);
        }
        catch (System.Runtime.InteropServices.COMException ex)
            when (ex.HResult == unchecked((int)0x80070005))
        {
            return new DeviceLocationResult(DeviceLocationStatus.PermissionDenied);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or ArgumentException or System.Runtime.InteropServices.COMException)
        {
            return new DeviceLocationResult(DeviceLocationStatus.Unavailable);
        }
    }

    private static DeviceLocationSource MapSource(PositionSource source) => source switch
    {
        PositionSource.Satellite => DeviceLocationSource.Satellite,
        PositionSource.WiFi => DeviceLocationSource.WiFi,
        PositionSource.Cellular => DeviceLocationSource.Cellular,
        PositionSource.IPAddress => DeviceLocationSource.IPAddress,
        PositionSource.Default => DeviceLocationSource.Default,
        _ => DeviceLocationSource.Unknown
    };
}
