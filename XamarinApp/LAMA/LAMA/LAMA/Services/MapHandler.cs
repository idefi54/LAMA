﻿using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI.Forms;
using Mapsui.Utilities;
using Mapsui.Projection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Essentials;
using System.Threading.Tasks;
using LAMA.Models;

using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
using Color = Xamarin.Forms.Color;
using ActivityStatus = LAMA.Models.LarpActivity.Status;

namespace LAMA.Services
{

    public class MapHandler
    {
        // Private fields
        private List<Feature> _symbols;
        private Dictionary<long, Pin> _activityPins;
        private Dictionary<ulong, Pin> _alertPins;
        private Dictionary<long, Pin> _cpPins;
        private Dictionary<long, Pin> _pointOfInterestPins;
        private List<Pin> _pins;
        private Pin _selectionPin;
        private ulong _alertID;
        private static MapHandler _instance;
        private Stopwatch _stopwatch;
        private long _time;
        private long _prevTime;
        private const long _doubleClickTime = 500;
        private const float _pinScale = 0.7f;
        private static Color _highlightColor = Color.FromHex("3A0E80");
        private Location _currentLocation;
        private EntityType _filter = EntityType.Nothing;

        /// <summary>
        /// Things that can be shown on the map.
        /// </summary>
        public enum EntityType
        {
            Nothing = 0b0,
            Activities = 0b1,
            Alerts = 0b10,
            CPs = 0b100,
            PointsOfIntrest = 0b1000
        }

        /// <summary>
        /// Filters the types in or out.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="show"></param>
        public void Filter(EntityType types, bool show)
        {
            if (show) FilterIn(types);
            else FilterOut(types);
        }

        /// <summary>
        /// Filters the types exclusively.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="show">
        ///     <br>True means show nothing but the types.</br>
        ///     <br>False means show everything but the types.</br>
        /// </param>
        public void FilterExclusive(EntityType types, bool show)
        {
            if (show) FilterInExclusive(types);
            else FilterOutExclusive(types);
        }

        /// <summary>
        /// Don't show these types on the map.
        /// </summary>
        /// <param name="types"></param>
        private void FilterOut(EntityType types) => _filter |= types;
        /// <summary>
        /// Show these types on the map.
        /// </summary>
        /// <param name="types"></param>
        private void FilterIn(EntityType types) => _filter &= ~types;
        /// <summary>
        /// Show ONLY these types on tha map;
        /// </summary>
        /// <param name="types"></param>
        private void FilterInExclusive(EntityType types) => _filter = ~types;
        /// <summary>
        /// Show EVERYTHING BUT these types.
        /// </summary>
        /// <param name="types"></param>
        private void FilterOutExclusive(EntityType types) => _filter = types;
        /// <summary>
        /// Are all of these types excluded from the map?
        /// </summary>
        /// <param name="types"></param>
        public bool IsFilteredOut(EntityType types) => (_filter & types) == types;
        /// <summary>
        /// Can all of these types be shown on the map?
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public bool IsFilteredIn(EntityType types) => (_filter & types) == EntityType.Nothing;
        /// <summary>
        /// Show everything.
        /// </summary>
        public void FilterClear() => _filter = EntityType.Nothing;

        // Events
        public delegate void PinClick(PinClickedEventArgs e, long activityID, bool doubleClick);
        public delegate void MapClick(MapClickedEventArgs e);
        public event PinClick OnPinClick;
        public event MapClick OnMapClick;

        /// <summary>
        /// Holds current location of this device.
        /// Might be changed on UpdateLocation() call.
        /// Can be set manually.
        /// </summary>
        public Location CurrentLocation
        {
            get => _currentLocation;
            set
            {
                _currentLocation = value;
                LocalStorage.cp.location = new Pair<double, double>(value.Longitude, value.Latitude);
            }
        }

        /// <summary>
        /// <para>
        /// Instance holds internal data on events, alerts and such, that should show on the map.
        /// </para>
        /// 
        /// Has methods that need to work with such data.
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
            _activityPins = new Dictionary<long, Pin>();
            _alertPins = new Dictionary<ulong, Pin>();
            _cpPins = new Dictionary<long, Pin>();
            _pointOfInterestPins = new Dictionary<long, Pin>();
            _selectionPin = new Pin();
            _selectionPin.Label = "temp";
            _selectionPin.Color = _highlightColor;
            _alertID = 0;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _time = 0;
            _prevTime = 0;
            _currentLocation = null;

            LoadActivities();
        }

        // Static methods do not need map data, they just work with given mapView.
        public static void SetZoomLimits(MapView view, double min, double max)
        {
            view.Map.Limiter.ZoomLimits = new Mapsui.UI.MinMax(min, max);
        }
        public static void SetPanLimits(MapView view, double minLon, double minLat, double maxLon, double maxLat)
        {
            view.Map.Limiter.PanLimits = new BoundingBox(minLon, minLat, maxLon, maxLat);
        }
        public static void CenterOn(MapView view, double longitude, double latitude)
        {
            Point p = SphericalMercator.FromLonLat(longitude, latitude);
            view.Navigator.CenterOn(p);
        }

        /// <summary>
        /// Resolution is -1 by default. This will zoom to default resolution.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="resolution"></param>
        public static void Zoom(MapView view, double resolution = -1)
        {
            if (resolution == -1)
                resolution = view.Map.Resolutions[view.Map.Resolutions.Count - 7];
            view.Navigator.ZoomTo(resolution);
        }

        // Public methods need to work with map data.
        //============================================

        /// <summary>
        /// Sets up MapView for normal use.
        /// </summary>
        /// <param name="view"></param>
        public void MapViewSetup(MapView view, LarpActivity activity = null)
        {
            view.Map = CreateMap();
            view.PinClicked += HandlePinClicked;
            view.MapClicked += HandleMapClicked;

            ReloadMapView(view);
        }

        /// <summary>
        /// Sets up MapView with selection pin. Click on map to relocate the pin.
        /// Use GetSelectionPin() method to get the pin location.
        /// </summary>
        /// <param name="view"></param>
        public void MapViewSetupSelection(MapView view, LarpActivity activity = null)
        {
            view.Map = CreateMap();
            view.PinClicked += HandlePinClicked;
            view.MapClicked += HandleMapClickedAdding;

            ReloadMapView(view, activity);

            if (!view.Pins.Contains(_selectionPin))
                view.Pins.Add(_selectionPin);
        }

        /// <summary>
        /// Visually refreshes entities on themap.
        /// </summary>
        /// <param name="view"></param>
        public void RefreshMapView(MapView view)
        {
            view.Pins.Clear();
            view.HideCallouts();

            if (IsFilteredIn(EntityType.Activities))
                foreach (Pin pin in _activityPins.Values)
                {
                    view.Pins.Add(pin);
                    pin.Scale = _pinScale;
                }


            if (IsFilteredIn(EntityType.Alerts))
                foreach (Pin pin in _alertPins.Values)
                {
                    view.Pins.Add(pin);
                    pin.Scale = _pinScale;
                }


            if (IsFilteredIn(EntityType.CPs))
                foreach (Pin pin in _cpPins.Values)
                {
                    view.Pins.Add(pin);
                    pin.Scale = 0.5f;
                }

            if (IsFilteredIn(EntityType.PointsOfIntrest))
                foreach (Pin pin in _cpPins.Values)
                {
                    view.Pins.Add(pin);
                    pin.Scale = _pinScale;
                }

            view.Refresh();
        }

        /// <summary>
        /// Reloades all data not tagged in <see cref="_filter"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="activity"></param>
        public void ReloadMapView(MapView view, LarpActivity activity = null)
        {
            SetPanLimits(view, view.Map.Envelope.Bottom, view.Map.Envelope.Left, view.Map.Envelope.Top, view.Map.Envelope.Right);
            SetZoomLimits(view, view.Map.Resolutions[view.Map.Resolutions.Count - 1], view.Map.Resolutions[2]);
            Zoom(view);

            if (CurrentLocation != null)
                view.MyLocationLayer.UpdateMyLocation(new Position(CurrentLocation.Latitude, CurrentLocation.Longitude));

            view.Pins.Clear();

            _activityPins.Clear();
            _cpPins.Clear();
            _alertPins.Clear();

            LoadActivities();
            LoadCPs();

            // TODO: load alerts when they are saved
            // TODO: load points of interest when they are saved

            if (activity != null)
            {
                CenterOn(view, activity.place.first, activity.place.second);
                _activityPins.Remove(activity.ID);
                SetSelectionPin(activity.place.first, activity.place.second);
            }

            // Zoom in when clicked home button
            view.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == "MyLocationFollow")
                    Zoom(view, view.Map.Resolutions[view.Map.Resolutions.Count - 7]);
            };

            RefreshMapView(view);
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
            var point = SphericalMercator.FromLonLat(lon, lat);
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
        /// Adds the event to internal data.
        /// Also adds a pin to a MapView if specified.
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="view"></param>
        public void AddActivity(LarpActivity activity, MapView view = null)
        {
            Pin pin = CreatePin(activity.place.first, activity.place.second, "normal");
            pin.Callout.Title = $"{activity.name}";
            pin.Callout.Subtitle =
                $"Status: {activity.status}\n"
                + $"Description: {activity.description}\n"
                + "\n"
                + $"Double click to show the activity.";

            switch (activity.status)
            {
                case ActivityStatus.awaitingPrerequisites:
                    pin.Color = Color.Gray; break;

                case ActivityStatus.readyToLaunch:
                    pin.Color = Color.FromHex("668067"); break; // LimeGreen

                case ActivityStatus.launched:
                    pin.Color = Color.ForestGreen; break;

                case ActivityStatus.inProgress:
                    pin.Color = Color.DeepSkyBlue; break;

                case ActivityStatus.completed:
                    pin.Color = Color.Black; break;

                default:
                    break;
            }

            _activityPins[activity.ID] = pin;

            if (view != null && IsFilteredIn(EntityType.Activities))
                view.Pins.Add(pin);
        }

        public void AddCP(CP cp, MapView view = null)
        {
            Pin pin = CreatePin(cp.location.first, cp.location.second, "CP");
            pin.Scale = 0.2f;
            pin.Color = Color.Orange;
            pin.Callout.Title = cp.name;

            _cpPins[cp.ID] = pin;

            if (view != null && IsFilteredIn(EntityType.CPs))
                view.Pins.Add(pin);
        }


        /// <summary>
        /// Adds the alert to the internal data.
        /// Also adds a pin to a MapView if specified.
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <param name="text"></param>
        /// <param name="view"></param>
        /// <returns>Returns alert id, if you want to remove it later.</returns>
        public ulong AddAlert(double lon, double lat, string text, MapView view = null)
        {
            Pin p = CreatePin(lon, lat, "important");
            p.Callout.Title = text;
            p.Color = Color.Red;
            p.Callout.Color = Color.Red;
            _alertPins[_alertID] = p;

            if (view != null && IsFilteredIn(EntityType.Alerts))
                view.Pins.Add(p);

            return _alertID++;
        }

        /// <summary>
        /// Removes the event from the internal data.
        /// Also removes it from MapView if specified.
        /// </summary>
        /// <param name="activityID"></param>
        /// <param name="view"></param>
        public void RemoveActivity(int activityID, MapView view = null)
        {
            view?.Pins.Remove(_activityPins[activityID]);
            _activityPins.Remove(activityID);
        }

        /// <summary>
        /// Removes the alert from the internal data.
        /// Also removes it from a MapView if specified.
        /// </summary>
        /// <param name="noficationID"></param>
        /// <param name="view"></param>
        public void RemoveAlert(ulong noficationID, MapView view = null)
        {
            view?.Pins.Remove(_alertPins[noficationID]);
            _alertPins.Remove(noficationID);
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
        public async Task UpdateLocation(MapView view, bool gpsAvailable = true)
        {
            var location = gpsAvailable ? await Geolocation.GetLastKnownLocationAsync() : CurrentLocation;
            if (location == null) return;
            view.MyLocationLayer.UpdateMyLocation(new Position(location.Latitude, location.Longitude), false);
            CurrentLocation = location;
        }

        /// <summary>
        /// Gets the location of selection pin.
        /// See MapViewSetupAdding() method for more information.
        /// Always returns last location from the last mapView where it was used.
        /// </summary>
        /// <returns></returns>
        public (double, double) GetSelectionPin() => (_selectionPin.Position.Longitude, _selectionPin.Position.Latitude);

        /// <summary>
        /// Sets the location of selection pin.
        /// See MapViewSetupAdding() method for more information.
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        public void SetSelectionPin(double lon, double lat) => _selectionPin.Position = new Position(lat, lon);

        /// <summary>
        /// Enables or disables the icon showing our location from the gps.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="visible"></param>
        public void SetLocationVisible(MapView view, bool visible)
        {
            view.MyLocationEnabled = visible;
            view.MyLocationLayer.Opacity = visible ? 1 : 0;
        }


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
        private Pin CreatePin(double lon, double lat, string label, MapView view = null)
        {
            Pin p = new Pin(view);
            p.Label = label;
            p.Position = new Position(lat, lon);
            p.Callout.ArrowAlignment = Mapsui.Rendering.Skia.ArrowAlignment.Right;
            p.Callout.Type = Mapsui.Rendering.Skia.CalloutType.Detail;

            p.Callout.TitleFontAttributes = Xamarin.Forms.FontAttributes.Bold;
            p.Callout.TitleFontName = "Verdana";
            p.Callout.TitleFontSize *= 0.8;

            p.Callout.SubtitleFontName = "Verdana";
            p.Callout.SubtitleFontAttributes = Xamarin.Forms.FontAttributes.Italic;
            p.Callout.SubtitleFontSize = p.Callout.TitleFontSize * 0.7f;

            if (view != null) p.Scale = 0.7f; // Scale setter can throw null exception - WHY THE ACTUAL FRICK
            return p;
        }
        private void LoadActivities(MapView view = null)
        {
            var rememberedList = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList;

            for (int i = 0; i < rememberedList.Count; i++)
                AddActivity(rememberedList[i], view);
        }

        private void LoadCPs(MapView view = null)
        {
            var rememberedList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;

            for (int i = 0; i < rememberedList.Count; i++)
                if (rememberedList[i].ID != LocalStorage.cpID)
                    AddCP(rememberedList[i], view);
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

            foreach (int id in _activityPins.Keys)
                if (_activityPins[id] == e.Pin)
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
            _selectionPin.Position = new Position(e.Point.Latitude, e.Point.Longitude);
            OnMapClick?.Invoke(e);
            e.Handled = true;
        }
    }
}
