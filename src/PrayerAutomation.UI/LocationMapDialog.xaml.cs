// Code-behind for the offline-first location map picker.
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PrayerAutomation.UI;

/// <summary>
/// Themed dialog for choosing a map point. Tiles are never requested until the
/// user checks the consent box and presses "Muat peta online".
/// </summary>
internal partial class LocationMapDialog : Window
{
    private const double FallbackLatitude = -6.2;
    private const double FallbackLongitude = 106.8;
    private const int ZoomPercentageBase = 11;
    private const double DragThreshold = 3.0;
    private static readonly double[] LatitudeGridSteps =
        [30, 10, 5, 2, 1, 0.5, 0.2, 0.1, 0.05, 0.02, 0.01, 0.005, 0.002, 0.001];
    private readonly bool _onlineGeocodingEnabled;
    private readonly string _onlineEndpoint;
    private readonly OpenStreetMapTileService _tileService = new();
    private readonly DispatcherTimer _cursorResetTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(450),
    };
    private double _centerLatitude;
    private double _centerLongitude;
    private int _zoom = 11;
    private int _renderVersion;
    private int _searchVersion;
    private bool _tilesRequested;
    private bool _isPanning;
    private bool _dragMoved;
    private bool _suppressSearchSelection;
    private System.Windows.Point _dragStart;
    private System.Windows.Point _dragLast;
    private double? _pendingLatitude;
    private double? _pendingLongitude;
    private string _pendingName = "";

    /// <summary>Initializes the picker at the coordinates currently in the settings form.</summary>
    /// <param name="owner">Owning settings window.</param>
    /// <param name="latitude">Initial latitude in degrees.</param>
    /// <param name="longitude">Initial longitude in degrees.</param>
    /// <param name="onlineGeocodingEnabled">Whether the user enabled Nominatim in Settings.</param>
    /// <param name="onlineEndpoint">Configured Nominatim-compatible base URI.</param>
    internal LocationMapDialog(Window owner, double latitude, double longitude,
        bool onlineGeocodingEnabled, string onlineEndpoint)
    {
        InitializeComponent();
        Icon = App.LoadApplicationIcon();
        Title = $"{AppBranding.DisplayName} — Pilih lokasi pada peta";
        Owner = owner;
        _onlineGeocodingEnabled = onlineGeocodingEnabled;
        _onlineEndpoint = onlineEndpoint;
        _centerLatitude = double.IsFinite(latitude) ? MapProjection.ClampLatitude(latitude) : FallbackLatitude;
        _centerLongitude = double.IsFinite(longitude) ? MapProjection.NormalizeLongitude(longitude) : FallbackLongitude;
        UpdateZoomLabel();
        MapCanvas.Cursor = MapCursorFactory.Pin;
        _cursorResetTimer.Tick += (_, _) =>
        {
            _cursorResetTimer.Stop();
            if (!_isPanning) MapCanvas.Cursor = MapCursorFactory.Pin;
        };
        Loaded += (_, _) => RenderMap();
        Closed += (_, _) =>
        {
            _cursorResetTimer.Stop();
            _searchVersion++;
        };
    }

    /// <summary>True after the user clicked the map or chose a search result.</summary>
    internal bool HasSelection => _pendingLatitude.HasValue && _pendingLongitude.HasValue;

    /// <summary>Latitude of the confirmed point.</summary>
    internal double Latitude => _pendingLatitude ?? _centerLatitude;

    /// <summary>Longitude of the confirmed point.</summary>
    internal double Longitude => _pendingLongitude ?? _centerLongitude;

    /// <summary>Human-readable preview of the confirmed point, when known.</summary>
    internal string DisplayName => _pendingName;

    /// <summary>Redraws grid lines, tiles, and the pending marker.</summary>
    /// <param name="loadTiles">False during a drag so existing tiles can be shifted without refetching.</param>
    private void RenderMap(bool loadTiles = true)
    {
        if (!IsLoaded || MapCanvas.ActualWidth <= 0 || MapCanvas.ActualHeight <= 0) return;

        int version = ++_renderVersion;
        if (loadTiles) TileLayer.Children.Clear();
        GridLayer.Children.Clear();
        double width = MapCanvas.ActualWidth;
        double height = MapCanvas.ActualHeight;
        MapPoint centre = MapProjection.WorldPixel(_centerLatitude, _centerLongitude, _zoom);

        DrawGraticule(centre, width, height);
        UpdateMarker();
        if (loadTiles && _tilesRequested) LoadOnlineTiles(version, centre, width, height);
    }

    /// <summary>Updates the zoom badge with a percentage relative to the initial zoom level.</summary>
    private void UpdateZoomLabel()
    {
        double percent = Math.Pow(2.0, _zoom - ZoomPercentageBase) * 100.0;
        ZoomLevelText.Text = percent >= 10.0
            ? percent.ToString("0", CultureInfo.CurrentCulture) + "%"
            : percent.ToString("0.#", CultureInfo.CurrentCulture) + "%";
    }

    /// <summary>Moves already-loaded tiles with the pointer while a drag is in progress.</summary>
    /// <param name="dx">Horizontal pointer movement in device-independent pixels.</param>
    /// <param name="dy">Vertical pointer movement in device-independent pixels.</param>
    private void ShiftTiles(double dx, double dy)
    {
        foreach (UIElement child in TileLayer.Children)
        {
            double left = Canvas.GetLeft(child);
            double top = Canvas.GetTop(child);
            if (double.IsNaN(left)) left = 0;
            if (double.IsNaN(top)) top = 0;
            Canvas.SetLeft(child, left + dx);
            Canvas.SetTop(child, top + dy);
        }
    }

    /// <summary>Draws a coarse latitude/longitude grid so offline mode still reads as a map.</summary>
    /// <param name="centre">World pixel at the viewport centre.</param>
    /// <param name="width">Viewport width.</param>
    /// <param name="height">Viewport height.</param>
    private void DrawGraticule(MapPoint centre, double width, double height)
    {
        Brush stroke = TryFindResource("AppBorderBrush") as Brush ?? Brushes.Gray;
        double pixelsPerDegree = MapProjection.WorldScale(_zoom) / 360.0;
        double step = LatitudeGridSteps.FirstOrDefault(candidate => candidate * pixelsPerDegree >= 72,
            LatitudeGridSteps[^1]);
        var top = MapProjection.CoordinatesFromWorldPixel(new MapPoint(centre.X, centre.Y - height / 2.0), _zoom);
        var bottom = MapProjection.CoordinatesFromWorldPixel(new MapPoint(centre.X, centre.Y + height / 2.0), _zoom);
        var left = MapProjection.CoordinatesFromWorldPixel(new MapPoint(centre.X - width / 2.0, centre.Y), _zoom);
        var right = MapProjection.CoordinatesFromWorldPixel(new MapPoint(centre.X + width / 2.0, centre.Y), _zoom);

        for (double longitude = Math.Floor(left.Longitude / step) * step;
             longitude <= right.Longitude + step; longitude += step)
        {
            double x = MapProjection.LongitudeToWorldX(longitude, _zoom) - centre.X + width / 2.0;
            if (x < 0 || x > width) continue;
            GridLayer.Children.Add(new Line
            {
                X1 = x, X2 = x, Y1 = 0, Y2 = height, Stroke = stroke, StrokeThickness = 0.6
            });
        }
        for (double latitude = Math.Floor(bottom.Latitude / step) * step;
             latitude <= top.Latitude + step; latitude += step)
        {
            double y = MapProjection.LatitudeToWorldY(latitude, _zoom) - centre.Y + height / 2.0;
            if (y < 0 || y > height) continue;
            GridLayer.Children.Add(new Line
            {
                X1 = 0, X2 = width, Y1 = y, Y2 = y, Stroke = stroke, StrokeThickness = 0.6
            });
        }
    }

    /// <summary>Loads the visible tiles one at a time to respect OSM rate limits.</summary>
    /// <param name="version">Render generation captured before the first request.</param>
    /// <param name="centre">World pixel at the viewport centre.</param>
    /// <param name="width">Viewport width.</param>
    /// <param name="height">Viewport height.</param>
    private async void LoadOnlineTiles(int version, MapPoint centre, double width, double height)
    {
        try
        {
            if (MapOnlineConsentCheckBox.IsChecked != true) return;
            IReadOnlyList<MapTile> tiles =
                MapProjection.VisibleTiles(_centerLatitude, _centerLongitude, _zoom, width, height);
            foreach (MapTile tile in tiles)
            {
                if (version != _renderVersion || MapOnlineConsentCheckBox.IsChecked != true) return;
                byte[]? bytes = await _tileService.TryGetTileAsync(tile, userConsentGranted: true).ConfigureAwait(true);
                if (bytes is null) continue;
                if (version != _renderVersion || MapOnlineConsentCheckBox.IsChecked != true) return;

                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(bytes))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                var image = new Image
                {
                    Width = MapProjection.TileSize,
                    Height = MapProjection.TileSize,
                    Stretch = Stretch.Fill,
                    Source = bitmap,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(image, tile.X * MapProjection.TileSize - centre.X + width / 2.0);
                Canvas.SetTop(image, tile.Y * MapProjection.TileSize - centre.Y + height / 2.0);
                TileLayer.Children.Add(image);
            }
        }
        catch (Exception ex)
        {
            MapSelectionStatus.Text = "Tile peta online gagal dimuat. Titik tetap bisa dipilih tanpa peta dasar.";
            AppLogger.Write("map.tiles", ex);
        }
    }

    /// <summary>Positions the marker for the pending point.</summary>
    private void UpdateMarker()
    {
        if (!HasSelection)
        {
            MapMarker.Visibility = Visibility.Collapsed;
            return;
        }
        MapPoint centre = MapProjection.WorldPixel(_centerLatitude, _centerLongitude, _zoom);
        MapPoint point = MapProjection.WorldPixel(_pendingLatitude!.Value, _pendingLongitude!.Value, _zoom);
        double scale = MapProjection.WorldScale(_zoom);
        double x = point.X - centre.X + MapCanvas.ActualWidth / 2.0;
        double y = point.Y - centre.Y + MapCanvas.ActualHeight / 2.0;
        if (x < -scale / 2.0) x += scale;
        else if (x > scale / 2.0) x -= scale;

        Canvas.SetLeft(MapMarker, x - MapMarker.Width / 2.0);
        Canvas.SetTop(MapMarker, y - MapMarker.Height / 2.0);
        MapMarker.Visibility = Visibility.Visible;
    }

    /// <summary>Starts a potential drag or click and captures the pointer.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse button event arguments.</param>
    private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;
        HideSearchResults();
        _dragStart = _dragLast = e.GetPosition(MapCanvas);
        _isPanning = true;
        _dragMoved = false;
        _cursorResetTimer.Stop();
        MapCanvas.CaptureMouse();
        MapCanvas.Cursor = Cursors.SizeAll;
        e.Handled = true;
    }

    /// <summary>Pans the map while the left button is held and the pointer moves past a small threshold.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse event arguments.</param>
    private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isPanning || e.LeftButton != MouseButtonState.Pressed) return;

        System.Windows.Point current = e.GetPosition(MapCanvas);
        if (!_dragMoved)
        {
            if (Math.Abs(current.X - _dragStart.X) < DragThreshold &&
                Math.Abs(current.Y - _dragStart.Y) < DragThreshold)
                return;
            _dragMoved = true;
        }

        double dx = current.X - _dragLast.X;
        double dy = current.Y - _dragLast.Y;
        _dragLast = current;
        if (dx == 0 && dy == 0) return;

        MapPoint centre = MapProjection.WorldPixel(_centerLatitude, _centerLongitude, _zoom);
        (_centerLatitude, _centerLongitude) = MapProjection.CoordinatesFromWorldPixel(
            new MapPoint(centre.X - dx, centre.Y - dy), _zoom);
        ShiftTiles(dx, dy);
        RenderMap(loadTiles: false);
        e.Handled = true;
    }

    /// <summary>Finishes a drag, or treats an unmoved click as a pending location.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse button event arguments.</param>
    private void MapCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || !_isPanning) return;

        bool wasDrag = _dragMoved;
        _isPanning = false;
        _dragMoved = false;
        if (MapCanvas.IsMouseCaptured) MapCanvas.ReleaseMouseCapture();
        MapCanvas.Cursor = MapCursorFactory.Pin;

        if (wasDrag) RenderMap();
        else SelectPointAt(e.GetPosition(MapCanvas));
        e.Handled = true;
    }

    /// <summary>Recovers when pointer capture is lost mid-drag.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse event arguments.</param>
    private void MapCanvas_LostMouseCapture(object sender, MouseEventArgs e)
    {
        if (!_isPanning) return;
        _isPanning = false;
        _dragMoved = false;
        MapCanvas.Cursor = MapCursorFactory.Pin;
        RenderMap();
    }

    /// <summary>Zooms in or out with the mouse wheel while keeping the point under the cursor stable.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Mouse wheel event arguments.</param>
    private void MapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta == 0) return;

        MapCanvas.Cursor = e.Delta > 0 ? MapCursorFactory.ZoomIn : MapCursorFactory.ZoomOut;
        _cursorResetTimer.Stop();
        _cursorResetTimer.Start();

        int previousZoom = _zoom;
        int nextZoom = MapProjection.ClampZoom(_zoom + (e.Delta > 0 ? 1 : -1));
        if (nextZoom == previousZoom)
        {
            e.Handled = true;
            return;
        }

        double width = MapCanvas.ActualWidth;
        double height = MapCanvas.ActualHeight;
        if (width > 0 && height > 0)
        {
            System.Windows.Point position = e.GetPosition(MapCanvas);
            MapPoint centre = MapProjection.WorldPixel(_centerLatitude, _centerLongitude, previousZoom);
            var underCursor = new MapPoint(
                centre.X + position.X - width / 2.0,
                centre.Y + position.Y - height / 2.0);
            (double latitude, double longitude) =
                MapProjection.CoordinatesFromWorldPixel(underCursor, previousZoom);

            _zoom = nextZoom;
            MapPoint newUnderCursor = MapProjection.WorldPixel(latitude, longitude, _zoom);
            var newCentre = new MapPoint(
                newUnderCursor.X - position.X + width / 2.0,
                newUnderCursor.Y - position.Y + height / 2.0);
            (_centerLatitude, _centerLongitude) =
                MapProjection.CoordinatesFromWorldPixel(newCentre, _zoom);
        }
        else
        {
            _zoom = nextZoom;
        }

        UpdateZoomLabel();
        RenderMap();
        e.Handled = true;
    }

    /// <summary>Turns a click position on the canvas into a pending geographic point.</summary>
    /// <param name="position">Click position relative to the map canvas.</param>
    private void SelectPointAt(System.Windows.Point position)
    {
        MapPoint centre = MapProjection.WorldPixel(_centerLatitude, _centerLongitude, _zoom);
        var world = new MapPoint(
            centre.X + position.X - MapCanvas.ActualWidth / 2.0,
            centre.Y + position.Y - MapCanvas.ActualHeight / 2.0);
        (double latitude, double longitude) = MapProjection.CoordinatesFromWorldPixel(world, _zoom);
        _pendingLatitude = latitude;
        _pendingLongitude = longitude;
        _pendingName = "";
        ConfirmMapPointButton.IsEnabled = true;
        UpdateMarker();
        DescribePendingPoint(latitude, longitude);
    }

    /// <summary>Shows the nearest offline settlement for a pending point without filling the form.</summary>
    /// <param name="latitude">Pending latitude.</param>
    /// <param name="longitude">Pending longitude.</param>
    private async void DescribePendingPoint(double latitude, double longitude)
    {
        string coordinates = $"{latitude:F5}, {longitude:F5}";
        MapSelectionStatus.Text = $"Titik sementara: {coordinates}. Mencari permukiman terdekat…";
        try
        {
            OfflineLocationResult? place = await Task.Run(
                () => OfflineReverseGeocoder.Current.FindNearest(latitude, longitude));
            if (_pendingLatitude != latitude || _pendingLongitude != longitude) return;
            _pendingName = place?.DisplayName ?? "";
            MapSelectionStatus.Text = place is null
                ? $"Titik sementara: {coordinates}. Tidak ada permukiman dalam radius 75 km; nama akan diisi 'Lokasi manual'. Tekan Tetapkan titik untuk memakai koordinat ini."
                : $"Titik sementara: {place.DisplayName}, sekitar {place.DistanceKm:0.0} km (perkiraan). Tekan Tetapkan titik untuk mengisi form; belum disimpan.";
        }
        catch (Exception ex)
        {
            MapSelectionStatus.Text = $"Titik sementara: {coordinates}. Dataset lokal tidak dapat dibaca; koordinat tetap bisa dipakai.";
            AppLogger.Write("map.nearest", ex);
        }
    }

    /// <summary>Searches offline first and optionally via the already-consented Nominatim service.</summary>
    private async void SearchMapButton_Click(object sender, RoutedEventArgs e) => await SearchAsync();

    /// <summary>Runs a search when Enter is pressed in the search field.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Key event arguments.</param>
    private void MapSearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        _ = SearchAsync();
    }

    /// <summary>Executes the location search and shows the merged results.</summary>
    private async Task SearchAsync()
    {
        string query = MapSearchBox.Text.Trim();
        if (query.Length < 2)
        {
            MapSearchResults.ItemsSource = null;
            MapSearchResults.Visibility = Visibility.Collapsed;
            MapSelectionStatus.Text = "Ketik minimal dua karakter untuk mencari nama tempat.";
            return;
        }

        int version = ++_searchVersion;
        SearchMapButton.IsEnabled = false;
        SearchMapButton.Content = "Mencari…";
        try
        {
            IReadOnlyList<OfflineLocationSuggestion> offline = await Task.Run(
                () => OfflineReverseGeocoder.Current.Search(query, 8));
            IReadOnlyList<OfflineLocationSuggestion> online = [];
            string? onlineError = null;
            if (_onlineGeocodingEnabled)
            {
                try
                {
                    var service = new NominatimReverseGeocodingService();
                    online = await service.SearchAsync(query, _onlineEndpoint, userConsentGranted: true, limit: 5);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                    or OperationCanceledException or IOException or InvalidDataException or ArgumentException
                    or InvalidOperationException or System.Text.Json.JsonException)
                {
                    onlineError = ex.Message;
                }
            }
            if (version != _searchVersion) return;

            var merged = new List<OfflineLocationSuggestion>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (OfflineLocationSuggestion suggestion in offline.Concat(online))
            {
                if (seen.Add(suggestion.DisplayName)) merged.Add(suggestion);
            }
            MapSearchResults.ItemsSource = merged;
            MapSearchResults.Visibility = merged.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            MapSelectionStatus.Text = merged.Count > 0
                ? $"{merged.Count} hasil ditemukan. Pilih salah satu untuk memindahkan titik sementara; hasil belum mengubah form."
                : onlineError is not null
                    ? $"Tidak ada hasil lokal dan Nominatim gagal: {onlineError}"
                    : "Tidak ada hasil. Klik langsung pada peta untuk menetapkan titik secara manual.";
        }
        catch (Exception ex)
        {
            MapSelectionStatus.Text = "Pencarian lokal gagal. Klik langsung pada peta untuk menetapkan titik.";
            AppLogger.Write("map.search", ex);
        }
        finally
        {
            SearchMapButton.IsEnabled = true;
            SearchMapButton.Content = "Cari";
        }
    }

    /// <summary>Moves the pending marker to a chosen search result without filling the form.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Selection event arguments.</param>
    private void MapSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSearchSelection) return;
        if (MapSearchResults.SelectedItem is not OfflineLocationSuggestion suggestion) return;
        SetPendingPoint(suggestion.Latitude, suggestion.Longitude, suggestion.DisplayName);
        HideSearchResults();
    }

    /// <summary>Closes and clears the search-results list after a result is used.</summary>
    private void HideSearchResults()
    {
        if (MapSearchResults.Visibility == Visibility.Collapsed && MapSearchResults.SelectedItem is null)
            return;
        _suppressSearchSelection = true;
        try
        {
            MapSearchResults.SelectedItem = null;
            MapSearchResults.Visibility = Visibility.Collapsed;
        }
        finally
        {
            _suppressSearchSelection = false;
        }
    }

    /// <summary>Sets a draft point, centres the map, and redraws the marker.</summary>
    /// <param name="latitude">Latitude in degrees.</param>
    /// <param name="longitude">Longitude in degrees.</param>
    /// <param name="name">Known display name, if any.</param>
    private void SetPendingPoint(double latitude, double longitude, string name)
    {
        _centerLatitude = MapProjection.ClampLatitude(latitude);
        _centerLongitude = MapProjection.NormalizeLongitude(longitude);
        _pendingLatitude = _centerLatitude;
        _pendingLongitude = _centerLongitude;
        _pendingName = name;
        ConfirmMapPointButton.IsEnabled = true;
        MapSelectionStatus.Text = $"Titik sementara: {name}. Geser/klik peta bila perlu, lalu tekan Tetapkan titik; belum disimpan.";
        RenderMap();
    }

    /// <summary>Zooms one step out.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => ChangeZoom(-1);

    /// <summary>Zooms one step in.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => ChangeZoom(1);

    /// <summary>Applies a bounded zoom change and redraws the map.</summary>
    /// <param name="delta">Number of zoom levels to add.</param>
    private void ChangeZoom(int delta)
    {
        int nextZoom = MapProjection.ClampZoom(_zoom + delta);
        if (nextZoom == _zoom) return;
        _zoom = nextZoom;
        UpdateZoomLabel();
        RenderMap();
    }

    /// <summary>Enables OSM tiles only after the explicit consent checkbox is ticked.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void LoadOnlineTiles_Click(object sender, RoutedEventArgs e)
    {
        if (MapOnlineConsentCheckBox.IsChecked != true)
        {
            MapSelectionStatus.Text = "Centang izin penggunaan peta online terlebih dahulu; tidak ada permintaan yang dikirim.";
            return;
        }
        _tilesRequested = true;
        LoadOnlineTilesButton.IsEnabled = false;
        LoadOnlineTilesButton.Content = "Peta online aktif";
        MapSelectionStatus.Text = $"Memuat tile dari OpenStreetMap. {OpenStreetMapTileService.Attribution}";
        RenderMap();
    }

    /// <summary>Clears tiles immediately when consent is withdrawn.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void MapOnlineConsentCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (MapOnlineConsentCheckBox.IsChecked == true) return;
        _tilesRequested = false;
        _renderVersion++;
        TileLayer.Children.Clear();
        LoadOnlineTilesButton.IsEnabled = true;
        LoadOnlineTilesButton.Content = "Muat peta online";
    }

    /// <summary>Confirms the pending point and closes the dialog.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void ConfirmMapPoint_Click(object sender, RoutedEventArgs e)
    {
        if (!HasSelection)
        {
            MapSelectionStatus.Text = "Klik peta atau pilih hasil pencarian terlebih dahulu.";
            return;
        }
        DialogResult = true;
        Close();
    }

    /// <summary>Closes the dialog without changing the settings form.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Routed event arguments.</param>
    private void CancelMap_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>Closes the dialog on Escape without applying a point.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Key event arguments.</param>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape) return;
        DialogResult = false;
        Close();
        e.Handled = true;
    }

    /// <summary>Repositions grid and tiles when the map area changes size.</summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Size changed event arguments.</param>
    private void MapCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => RenderMap();
}
