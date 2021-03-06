using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Essentials;

namespace LAMA.Services
{
    public class MapHandler
    {
        // Private fields
        private List<Feature> _symbols;
        private Dictionary<int, Pin> _activities;
        private Dictionary<ulong, Pin> _notifications;
        private List<Pin> _pins;
        private Pin _pin;
        private ulong _notificationID;
        private static MapHandler _instance;
        private Stopwatch _stopwatch;
        private long _time;
        private long _prevTime;
        private const long _doubleClickTime = 500;

        // Events
        public delegate void PinClick(PinClickedEventArgs e, int activityID, bool doubleClick);
        public delegate void MapClick(MapClickedEventArgs e);
        public event PinClick OnPinClick;
        public event MapClick OnMapClick;


        /// <summary>
        /// <para>
        /// Instance holds internal data on events, notifications and such, that should show on the map.
        /// </para>
        /// 
        /// Has methods that need to work with this data.
        /// Static methods work just with a given MapView.
        /// </summary>
        public static MapHandler Instance
        {
            get
            {
                if (_instance == null) _instance = new MapHandler();
                return _instance;
            }
        }

        private MapHandler()
        {
            _symbols = new List<Feature>();
            _pins = new List<Pin>();
            _activities = new Dictionary<int, Pin>();
            _notifications = new Dictionary<ulong, Pin>();
            _pin = new Pin();
            _pin.Label = "temp";
            _notificationID = 0;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _time = 0;
            _prevTime = 0;
        }

        // Static methods do not need the map, they just work with given mapView.
        public static void SetZoomLimits(MapView view, double min, double max)
        {
            view.Map.Limiter.ZoomLimits = new Mapsui.UI.MinMax(min, max);
        }
        public static void SetPanLimits(MapView view, double minLon, double minLat, double maxLon, double maxLat)
        {
            view.Map.Limiter.PanLimits = new BoundingBox(minLon, minLat, maxLon, maxLat);
        }


        // Public methods need to work with the map.
        //============================================

        /// <summary>
        /// Sets up MapView for normal use.
        /// </summary>
        /// <param name="view"></param>
        public void MapViewSetup(MapView view)
        {
            view.Map = CreateMap();
            view.PinClicked += HandlePinClicked;
            view.MapClicked += HandleMapClicked;

            foreach (Pin pin in _activities.Values)
                view.Pins.Add(pin);

            foreach (Pin pin in _notifications.Values)
                view.Pins.Add(pin);
        }

        /// <summary>
        /// Sets up MapView with temporary pin. Click on map to relocate the pin.
        /// Use GetTemporaryPin() method to get the pin location.
        /// </summary>
        /// <param name="view"></param>
        public void MapViewSetupAdding(MapView view)
        {
            view.Map = CreateMap();
            view.PinClicked += HandlePinClicked;
            view.MapClicked += HandleMapClickedAdding;

            _pin.Position = new Position(0, 0);
            if (!view.Pins.Contains(_pin))
                view.Pins.Add(_pin);

            foreach (Pin pin in _activities.Values)
                view.Pins.Add(pin);

            foreach (Pin pin in _notifications.Values)
                view.Pins.Add(pin);
        }

        /// <summary>
        /// Removes everything from the map and adds it again to ensure only relevant data is on the map.
        /// </summary>
        /// <param name="view"></param>
        public void RefreshMapView(MapView view)
        {
            view.Pins.Clear();
            view.HideCallouts();

            foreach (Pin pin in _activities.Values) view.Pins.Add(pin);
            foreach (Pin pin in _notifications.Values) view.Pins.Add(pin);
            foreach (Pin pin in _pins) view.Pins.Add(pin);
        }

        /// <summary>
        /// Symbols are background things that are non-clickable.
        /// Has very wide variety of what to show on the map, not only icons.
        /// Not usable right now and might remove later.
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="styles"></param>
        public void AddSymbol(double lon, double lat, IEnumerable<IStyle> styles = null)
        {
            var feature = new Feature();
            var point = Mapsui.Projection.SphericalMercator.FromLonLat(lon, lat);
            feature.Geometry = point;
            if (styles != null)
                foreach (Style style in styles)
                    feature.Styles.Add(style);

            _symbols.Add(feature);
        }

        /// <summary>
        /// Adds a general pin to the internal data.
        /// Also adds the pin to a MapView if specfied.
        /// 
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="label"></param>
        /// <param name="view"></param>
        /// <returns> Returns the pin and you can edit it however you want afterwards.</returns>
        public Pin AddPin(double lon, double lat, string label, MapView view = null)
        {
            Pin p = new Pin();
            p.Label = label;
            p.Position = new Position(lat, lon);
            view?.Pins.Add(p);
            return p;
        }

        /// <summary>
        /// Adds the event to the internal data.
        /// Also adds a pin to a MapView if specified.
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="activityID"></param>
        /// <param name="view"></param>
        public void AddActivity(double lon, double lat, int activityID, MapView view = null)
        {
            Pin pin = CreatePin(lon, lat, "normal");
            _activities.Add(activityID, pin);
            pin.Callout.Title =
                $"Activity id: {activityID}\n" +
                $"Double click to show the activity";
            pin.Color = Xamarin.Forms.Color.Green;
            view?.Pins.Add(pin);
        }

        /// <summary>
        /// Adds the notification to the internal data.
        /// Also adds a pin to a MapView if specified.
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="text"></param>
        /// <param name="view"></param>
        /// <returns>Returns notification id, if you want to remove it later.</returns>
        public ulong AddNotification(double lon, double lat, string text, MapView view = null)
        {
            Pin p = CreatePin(lon, lat, "important");
            p.Callout.Title = text;
            p.Color = Xamarin.Forms.Color.Red;
            p.Callout.Color = Xamarin.Forms.Color.Red;
            _notifications.Add(_notificationID, p);
            view?.Pins.Add(p);

            return _notificationID++;
        }

        /// <summary>
        /// Removes the event from the internal data.
        /// Also removes it from MapView if specified.
        /// </summary>
        /// <param name="activityID"></param>
        /// <param name="view"></param>
        public void RemoveActivity(int activityID, MapView view = null)
        {
            view?.Pins.Remove(_activities[activityID]);
            _activities.Remove(activityID);
        }

        /// <summary>
        /// Removes the notification from the internal data.
        /// Also removes it from a MapView if specified.
        /// </summary>
        /// <param name="noficationID"></param>
        /// <param name="view"></param>
        public void RemoveNotification(ulong noficationID, MapView view = null)
        {
            view?.Pins.Remove(_notifications[noficationID]);
            _notifications.Remove(noficationID);
        }

        /// <summary>
        /// Removes the pin from the internal data.
        /// Also removes it from a MapView if specified.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="view"></param>
        public void RemovePin(Pin pin, MapView view = null)
        {
            _pins.Remove(pin);
            view?.Pins.Remove(pin);
        }

        /// <summary>
        /// Calls Xamarin.Essentials.Geolocation to get last known location.
        /// Then updates it on the map.
        /// Needs to be async.
        /// </summary>
        /// <param name="view"></param>
        public async void UpdateLocation(MapView view)
        {
            if (await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() != PermissionStatus.Granted)
                return;

            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
                return;

            view.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude));
        }

        /// <summary>
        /// Gets the location of temporary pin.
        /// See MapViewSetupAdding() method for more information.
        /// Always returns last location from the last mapView where it was used.
        /// </summary>
        /// <returns></returns>
        public (double, double) GetTemporaryPin() => (_pin.Position.Longitude, _pin.Position.Latitude);

        /// <summary>
        /// Enables or disables the icon showing our location from the gps.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="visible"></param>
        public void SetLocationVisible(MapView view, bool visible) => view.MyLocationEnabled = visible;


        // Private methods
        //===================
        private MemoryLayer CreateMarkersLayer()
        {
            string path = "LAMA.marker.png";
            var assembly = typeof(MapHandler).Assembly;
            var image = assembly.GetManifestResourceStream(path);
            var TestBitmapID = BitmapRegistry.Instance.Register(image);
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
                DataSource = new MemoryProvider(_symbols),
                Style = style
            };
        }
        private Mapsui.Map CreateMap()
        {
            var map = new Mapsui.Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateMarkersLayer());

            return map;
        }
        private Pin CreatePin(double lon, double lat, string label)
        {
            Pin p = new Pin();
            p.Label = label;
            p.Position = new Position(lat, lon);
            p.Callout.ArrowAlignment = Mapsui.Rendering.Skia.ArrowAlignment.Right;
            return p;
        }

        // Event Handlers
        //===================
        private void HandlePinClicked(object sender, PinClickedEventArgs e)
        {
            _time = _stopwatch.ElapsedMilliseconds;

            if (e.NumOfTaps == 1 && e.Pin.Label != "temp")
                if (e.Pin.Callout.IsVisible)
                    e.Pin.HideCallout();
                else
                    e.Pin.ShowCallout();

            foreach (int id in _activities.Keys)
                if (_activities[id] == e.Pin)
                {
                    OnPinClick?.Invoke(e, id, _time - _prevTime < _doubleClickTime);
                    break;
                }
            _prevTime = _time;
            e.Handled = true;
        }
        private void HandleMapClicked(object sender, MapClickedEventArgs e)
        {
            OnMapClick?.Invoke(e);
            e.Handled = true;
        }
        private void HandleMapClickedAdding(object sender, MapClickedEventArgs e)
        {
            _pin.Position = new Position(e.Point.Latitude, e.Point.Longitude);
            OnMapClick?.Invoke(e);
            e.Handled = true;
        }
    }
}
