using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace LAMA.Services
{
    public class MapHandler
    {
        private MemoryLayer layer;
        private MapView view;
        private List<Feature> points;
        private Pin _pin;
        private bool _createPinOnClick;


        public delegate void PinClick(PinClickedEventArgs e);
        public event PinClick OnPinClick;
        public bool CreatePinOnClick
        {
            get { return _createPinOnClick; }
            set
            {
                if (!value && _pin != null)
                {
                    view.Pins.Remove(_pin);
                    _pin = null;
                }

                _createPinOnClick = value;
            }
        }

        public int TestBitmapID { get; private set; }

        public MapHandler(MapView mapView)
        {
            points = new List<Feature>();
            layer = CreateMarkersLayer();
            view = mapView;

            Mapsui.Map map = new Mapsui.Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(layer);
            map.Home = n => n.NavigateTo(view.MyLocationLayer.MyLocation.ToMapsui(), map.Resolutions[5]);
            view.Map = map;

            view.PinClicked += HandlePinClicked;
            view.MapClicked += HandleMapClicked;
        }


        public void AddSymbol(double lon, double lat, IEnumerable<IStyle> styles = null)
        {
            var feature = new Feature();
            var point = Mapsui.Projection.SphericalMercator.FromLonLat(lon, lat);
            feature.Geometry = point;
            if (styles != null)
                foreach (Style style in styles)
                    feature.Styles.Add(style);

            points.Add(feature);
            layer.DataSource = new MemoryProvider(points);
            view.Refresh();
        }
        public void SetZoomLimits(double min, double max)
        {
            view.Map.Limiter.ZoomLimits = new Mapsui.UI.MinMax(min, max);
        }
        public void SetPanLimits(double minLon, double minLat, double maxLon, double maxLat)
        {
            view.Map.Limiter.PanLimits = new BoundingBox(minLon, minLat, maxLon, maxLat);
        }
        public Pin AddPin(double lon, double lat, string label)
        {
            Pin p = new Pin(view);
            p.Label = label;
            p.Position = new Position(lon, lat);
            view.Pins.Add(p);
            return p;
        }
        public void ShowNotification(double lon, double lat, string text)
        {
            Pin p = AddPin(lon, lat, "normal");
            p.Callout.Title = text;
            p.Color = Xamarin.Forms.Color.Green;
        }
        public void ShowImportantNotification(double lon, double lat, string text)
        {
            Pin p = AddPin(lon, lat, "important");
            p.Callout.Title = text;
            p.Color = Xamarin.Forms.Color.Red;
            p.Callout.Color = Xamarin.Forms.Color.Red;
        }
        /// <summary>
        /// Ties to "CreatePinOnClick".
        /// Returns null if no such pin is on the map.
        /// </summary>
        /// <returns></returns>
        public (double, double)? GetTempPinLocation()
        {
            if (_pin == null) return null;
            return (_pin.Position.Longitude, _pin.Position.Latitude);
        }

        public async void CenterOnLocation()
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            view.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude));
            view.Map.Home = n => n.CenterOn(location.Latitude, location.Longitude);
        }
        private MemoryLayer CreateMarkersLayer()
        {

            string path = "LAMA.marker.png";
            var assembly = typeof(MapHandler).Assembly;
            var image = assembly.GetManifestResourceStream(path);
            TestBitmapID = BitmapRegistry.Instance.Register(image);
            var bitmapHeight = 175;

            var style = new SymbolStyle
            {
                SymbolScale = 0.0,
                SymbolOffset = new Offset(0, bitmapHeight * 0.5)
            };

            return new MemoryLayer
            {
                Name = "markers",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider(points),
                Style = style

            };
        }
        private void HandlePinClicked(object sender, PinClickedEventArgs e)
        {
            if (e.NumOfTaps == 1)
                if (e.Pin.Callout.IsVisible)
                    e.Pin.HideCallout();
                else
                    e.Pin.ShowCallout();

            PinClick handler = OnPinClick;
            handler?.Invoke(e);

            e.Handled = true;
        }
        private void HandleMapClicked(object sender, MapClickedEventArgs e)
        {
            if (!_createPinOnClick)
                return;

            if (_pin == null)
                _pin = AddPin(e.Point.Longitude, e.Point.Latitude, "temp");
            else
                _pin.Position = new Position(e.Point.Latitude, e.Point.Longitude);

            e.Handled = true;
        }

    }
}
