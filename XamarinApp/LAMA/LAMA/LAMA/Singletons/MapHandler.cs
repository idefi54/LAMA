using Mapsui;
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
using XColor = Xamarin.Forms.Color;
using MColor = Mapsui.Styles.Color;
using XPoint = Xamarin.Forms.Point;
using MPoint = Mapsui.Geometries.Point;
using ActivityStatus = LAMA.Models.LarpActivity.Status;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using System.Reflection;
using System.IO;
using LAMA.Singletons;
using LAMA.Themes;
using LAMA.Extensions;

namespace LAMA.Singletons
{
    public class MapHandler
    {
        #region PRIVATE FIELDS
        // Containers
        private List<Feature> _symbols;
        private Dictionary<long, Pin> _activityPins;
        private Dictionary<ulong, Pin> _alertPins;
        private Dictionary<long, Pin> _cpPins;
        private Dictionary<long, Pin> _pointOfInterestPins;
        private Dictionary<long, Polyline> _roadPolyLines;
        private Stack<Polyline> _polylineBuffer;
        private List<Pin> _pins;

        // Pins
        private Pin _selectionPin;
        private bool _selectionPinVisible; // Because pin's property throws FRICKING exceptions.
        private Pin _polylinePin;
        private const float _pinScale = 0.7f;
        private static XColor _highlightColor = XColor.FromHex("3A0E80");

        // IDs
        private ulong _alertID;

        // Double clicking
        private Stopwatch _stopwatch;
        private long _time;
        private long _prevTime;
        private const long _doubleClickTime = 500;

        // Other
        private EntityType _filter = EntityType.Nothing;
        private bool _polylineAddition;
        private bool _polylineDeletion;
        private MapView _activeMapView;
        private Polyline _limitsVisual;
        public bool IsGlobalPanLimits =>
            !double.IsInfinity(LarpEvent.minX)
            && !double.IsInfinity(LarpEvent.maxX)
            && !double.IsInfinity(LarpEvent.minY)
            && !double.IsInfinity(LarpEvent.maxY);
        public bool IsGlobalZoomLimits => LarpEvent.maxZoom != -1 && LarpEvent.minZoom != -1;
        #endregion

        /// <summary>
        /// Use GPU for drawing all mapviews?
        /// WPF has a bug, where this has to be disabled.
        /// </summary>
        public static bool UseGPU
        {
            get => MapControl.UseGPU;
            set => MapControl.UseGPU = value;
        }

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
        private Location _currentLocation;

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
        private static MapHandler _instance;

        private MapHandler()
        {
            _symbols = new List<Feature>();
            _pins = new List<Pin>();
            _activityPins = new Dictionary<long, Pin>();
            _alertPins = new Dictionary<ulong, Pin>();
            _cpPins = new Dictionary<long, Pin>();
            _pointOfInterestPins = new Dictionary<long, Pin>();
            _roadPolyLines = new Dictionary<long, Polyline>();
            _polylineBuffer = new Stack<Polyline>();
            _polylinePin = new Pin();

            _selectionPin = new Pin();
            _selectionPin.Label = "temp";
            _selectionPin.Color = _highlightColor;
            _selectionPinVisible = false;

            _limitsVisual = new Polyline();
            _limitsVisual.StrokeColor = XColor.Red;
            _limitsVisual.StrokeWidth = 2;

            _alertID = 0;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _time = 0;
            _prevTime = 0;
            _currentLocation = null;

            SQLEvents.created += SQLEvents_created;
            SQLEvents.dataDeleted += SQLEvents_dataDeleted;
            SQLEvents.dataChanged += SQLEvents_dataChanged;
        }


        #region FILTER
        /// <summary>
        /// Things that can be shown on the map.
        /// </summary>
        public enum EntityType
        {
            Nothing = 0b0,
            Activities = 0b1,
            Alerts = 0b10,
            CPs = 0b100,
            PointsOfIntrest = 0b1000,
            Polylines = 0b10000
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
        #endregion

        #region STATIC METHOS
        // Static methods do not need map data, they just work with given mapView.
        public static void SetZoomLimits(MapView view, double min, double max)
        {
            if (min <= 0)
            {
                int last = view.Map.Resolutions.Count - 1;
                min = view.Map.Resolutions[last];
            }

            view.Map.Limiter.ZoomLimits = new Mapsui.UI.MinMax(min, max);
        }
        public static void SetPanLimits(MapView view, double minX, double minY, double maxX, double maxY)
        {
            view.Map.Limiter.PanLimits = new BoundingBox(minX, minY, maxX, maxY);
        }
        public static void CenterOn(MapView view, double longitude, double latitude)
        {
            MPoint p = SphericalMercator.FromLonLat(longitude, latitude);
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

        public static string PointOfInterestIcon(int iconID)
        {
            return IconLibrary.GetIconsByClass<PointOfInterest>()[iconID];
        }

        public static void SetGlobalBoundsFromView(MapView view)
        {
            BoundingBox bounds = view.Viewport.Extent;
            var zoom = view.Viewport.Resolution;

            LarpEvent.minX = bounds.MinX;
            LarpEvent.maxX = bounds.MaxX;
            LarpEvent.minY = bounds.MinY;
            LarpEvent.maxY = bounds.MaxY;
            LarpEvent.minZoom = 0;
            LarpEvent.maxZoom = zoom;
        }

        public static void RemoveGlobalBounds()
        {
            LarpEvent.minX = float.PositiveInfinity;
            LarpEvent.maxX = float.NegativeInfinity;
            LarpEvent.minY = float.PositiveInfinity;
            LarpEvent.maxY = float.NegativeInfinity;
            LarpEvent.minZoom = -1;
            LarpEvent.maxZoom = -1;
        }
        #endregion

        #region GENERAL MAPVIEW HANDLING

        public async Task<(MapView, Button)> CreateAndAddMapView(Layout<View> layout, LayoutOptions horizontalOptions, LayoutOptions verticalOptions, double heightRequest, Layout<View> hidableView = null)
        {
            var activityIndicator = new ActivityIndicator
            {
                VerticalOptions = verticalOptions,
                HorizontalOptions = horizontalOptions,
                HeightRequest = heightRequest,
                IsRunning = true,
                IsVisible = true
            };
            layout.Children.Add(activityIndicator);

            await Task.Delay(500);

            var view = new MapView
            {
                VerticalOptions = verticalOptions,
                HorizontalOptions = horizontalOptions,
                HeightRequest = heightRequest,
                BackgroundColor = XColor.Gray
            };

            const string EXPAND = "Expand Map";
            const string HIDE = "Hide Map";

            Button button = null;
            if (hidableView != null)
            {
                button = new Button();
                button.Text = EXPAND;
                button.Clicked += (object sender, EventArgs args) =>
                {
                    var b = (Button)sender;

                    if (b.Text == HIDE)
                    {
                        hidableView.IsVisible = true;
                        button.Text = EXPAND;
                        view.HeightRequest = heightRequest;
                        view.HorizontalOptions = LayoutOptions.Fill;
                        view.VerticalOptions = LayoutOptions.Fill;
                    } else
                    {
                        hidableView.IsVisible = false;
                        button.Text = HIDE;
                        view.HeightRequest = -1;
                        view.HorizontalOptions = LayoutOptions.FillAndExpand;
                        view.VerticalOptions = LayoutOptions.FillAndExpand;
                    }
                };

                layout.Children.Add(button);
            }

            layout.Children.Remove(activityIndicator);
            layout.Children.Add(view);
            return (view, button);
        }

        public void RemoveMapView(MapView view, Layout<View> layout, Button expandButton = null)
        {
            if (view != null)
                layout.Children.Remove(view);

            if (expandButton != null)
                layout.Children.Remove(expandButton);
        }

        /// <summary>
        /// General MapView setup. Creates a Map, loads data and adds events.
        /// </summary>
        /// <param name="view">MapView you want to setup.</param>
        /// <param name="showSelection">Show the selection pin on the map.</param>
        /// <param name="relocateSelection">Can relocate the selection pin by clicking on the map.</param>
        public void MapViewSetup(MapView view, bool showSelection = false, bool relocateSelection = false)
        {
            view.Map = CreateMap();
            view.PinClicked += HandlePinClicked;
            if (relocateSelection) view.MapClicked += HandleMapClickedSelection;
            else view.MapClicked += HandleMapClicked;

            _selectionPinVisible = showSelection;

            if (_currentLocation == null)
            {
                SetLocationVisible(view, false);
                SetSelectionPin(0, 0);
            } else
                SetSelectionPin(_currentLocation.Longitude, _currentLocation.Latitude);

            ReloadMapView(view);
            RefreshMapView(view);
            _activeMapView = view;
        }

        /// <summary>
        /// Call on exiting the page.
        /// Dumpts the temporary data into the database.
        /// </summary>
        public void MapDataSave()
        {
            _polylineAddition = false;
            _polylinePin.IsVisible = false;
            // TODO - save things
            PolylineFlush(_activeMapView);
            //foreach (long id in _roadPolyLines.Keys)
            //    SavePolyline(id);

            _activeMapView = null;
        }

        /// <summary>
        /// Reloades all data not tagged in <see cref="_filter"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="activity"></param>
        public void ReloadMapView(MapView view)
        {
            SetPanLimits(view, view.Map.Envelope.Bottom, view.Map.Envelope.Left, view.Map.Envelope.Top, view.Map.Envelope.Right);
            SetZoomLimits(view, view.Map.Resolutions[view.Map.Resolutions.Count - 1], view.Map.Resolutions[2]);
            Zoom(view);

            bool isClient = LocalStorage.cpID != 0 || LocalStorage.clientID != 0;
            if (isClient && IsGlobalPanLimits)
                SetPanLimits(view, LarpEvent.minX, LarpEvent.minY, LarpEvent.maxX, LarpEvent.maxY);

            if (isClient && IsGlobalZoomLimits)
            {
                SetZoomLimits(view, LarpEvent.minZoom, LarpEvent.maxZoom);
                Zoom(view, LarpEvent.maxZoom);
            }

            if (CurrentLocation != null)
                view.MyLocationLayer.UpdateMyLocation(new Position(CurrentLocation.Latitude, CurrentLocation.Longitude));

            view.Pins.Clear();
            view.Drawables.Clear();

            _activityPins.Clear();
            _cpPins.Clear();
            _alertPins.Clear();
            _pointOfInterestPins.Clear();
            _roadPolyLines.Clear();

            LoadActivities();
            LoadCPs();
            LoadPointsOfIntrest();
            LoadRoads();
        }

        /// <summary>
        /// Visually refreshes entities on themap.
        /// </summary>
        /// <param name="view"></param>
        public void RefreshMapView(MapView view)
        {
            view.Pins.Clear();
            view.HideCallouts();
            view.Drawables.Clear();

            if (IsFilteredIn(EntityType.Activities))
                foreach (Pin pin in _activityPins.Values)
                    view.Pins.Add(pin);

            if (IsFilteredIn(EntityType.Alerts))
                foreach (Pin pin in _alertPins.Values)
                    view.Pins.Add(pin);

            if (IsFilteredIn(EntityType.CPs))
                foreach (long id in _cpPins.Keys)
                    view.Pins.Add(_cpPins[id]);

            if (IsFilteredIn(EntityType.PointsOfIntrest))
                foreach (Pin pin in _pointOfInterestPins.Values)
                    view.Pins.Add(pin);

            if (IsFilteredIn(EntityType.Polylines))
            {
                foreach (Polyline polyline in _roadPolyLines.Values)
                    view.Drawables.Add(polyline);

                foreach (Polyline polyline in _polylineBuffer)
                    view.Drawables.Add(polyline);
            }

            if (IsGlobalPanLimits)
            {
                MPoint topLeft = SphericalMercator.ToLonLat(LarpEvent.minX, LarpEvent.minY);
                MPoint botRight = SphericalMercator.ToLonLat(LarpEvent.maxX, LarpEvent.maxY);
                _limitsVisual.Positions.Clear();
                _limitsVisual.Positions.Add(new Position(topLeft.Y, topLeft.X));
                _limitsVisual.Positions.Add(new Position(topLeft.Y, botRight.X));
                _limitsVisual.Positions.Add(new Position(botRight.Y, botRight.X));
                _limitsVisual.Positions.Add(new Position(botRight.Y, topLeft.X));
                _limitsVisual.Positions.Add(new Position(topLeft.Y, topLeft.X));
                view.Drawables.Add(_limitsVisual);
            }


            view.Pins.Add(_selectionPin);
            _selectionPin.IsVisible = _selectionPinVisible;

            _polylinePin.Label = "temp";
            view.Pins.Add(_polylinePin);
            _polylinePin.Scale = 0.3f;
            _polylinePin.Color = XColor.Indigo;
            _polylinePin.IsVisible = false;

            view.Refresh();
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
        /// Enables or disables the icon showing our location from the gps.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="visible"></param>
        public void SetLocationVisible(MapView view, bool visible)
        {
            view.MyLocationEnabled = visible;
            view.MyLocationLayer.Opacity = visible ? 1 : 0;
        }
        #endregion

        #region ADDING AND REMOVING ENTITIES
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
                foreach (var style in styles)
                    feature.Styles.Add(style);

            _symbols.Add(feature);
        }

        /// <summary>
        /// Adds a general pin to the internal data.
        /// Also adds the pin to a MapView if specfied.
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
        /// Adds the Activity to the internal data.
        /// Also adds a pin to a MapView if specified.
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="view"></param>
        public void AddActivity(LarpActivity activity, MapView view = null)
        {
            Pin pin = CreatePin(activity.place.first, activity.place.second, "Activity");
            pin.Callout.Title = $"{activity.name}";
            pin.Callout.Subtitle =
                $"Status: {activity.status}\n"
                + $"Description: {activity.description}\n"
                + "\n"
                + $"Double click to show the activity.";

            PinSetIcon(pin, activity.GetIconByteArray());
            _activityPins[activity.ID] = pin;

            if (view != null && IsFilteredIn(EntityType.Activities))
                view.Pins.Add(pin);
        }

        /// <summary>
        /// Adds the CP to the internal data.
        /// Also adds a pin to a MapView if specified.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="view"></param>
        public void AddCP(CP cp, MapView view = null)
        {
            if (cp.ID == LocalStorage.cpID)
                return;

            Pin pin = CreatePin(cp.location.first, cp.location.second, "CP");
            PinSetIcon(pin, IconLibrary.GetIconsByClass<CP>()[0]);
            pin.Scale = 0.8f;
            pin.Color = XColor.Orange;
            pin.Callout.Title = cp.name;

            _cpPins[cp.ID] = pin;

            if (view != null && IsFilteredIn(EntityType.CPs))
                view.Pins.Add(pin);
        }

        /// <summary>
        /// Adds the PointOfInterest to the internal data.
        /// Also adds a pin to a MapView if specified.
        /// </summary>
        /// <param name="poi"></param>
        /// <param name="view"></param>
        public void AddPointOfInterest(PointOfInterest poi, MapView view = null)
        {
            Pin pin = CreatePin(poi.Coordinates.first, poi.Coordinates.second, "POI", view);
            PinSetIcon(pin, PointOfInterestIcon(poi.Icon));
            pin.Scale = 0.8f;
            pin.Color = XColor.Beige;
            pin.Callout.Title = poi.Name;

            _pointOfInterestPins[poi.ID] = pin;

            if (view != null && IsFilteredIn(EntityType.PointsOfIntrest))
                view.Pins.Add(pin);
        }

        /// <summary>
        /// Adds the Road to the internal data.
        /// Also adds Polyline to a MapView if specified.
        /// </summary>
        /// <param name="road"></param>
        /// <param name="view"></param>
        public void AddRoad(Road road, MapView view = null)
        {
            var polyline = new Polyline();
            polyline.StrokeWidth = (float)road.Thickness;
            polyline.StrokeColor = new XColor(
                road.Color[0], // R
                road.Color[1], // G
                road.Color[2], // B
                road.Color[3]  // A
                );

            foreach (var coords in road.Coordinates)
                polyline.Positions.Add(new Position(coords.second, coords.first));

            _roadPolyLines.Add(road.ID, polyline);
            view?.Drawables.Add(polyline);
        }

        /// <summary>
        /// Adds an alert to the internal data.
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
            PinSetIcon(p, "LAMA.Resources.Icons.message_2_exp-1");
            p.Callout.Title = text;
            p.Color = XColor.Red;
            p.Callout.Color = XColor.Red;
            _alertPins[_alertID] = p;

            if (view != null && IsFilteredIn(EntityType.Alerts))
                view.Pins.Add(p);

            //p.Scale = _pinScale;
            return _alertID++;
        }

        /// <summary>
        /// Removes the Road from the internal data.
        /// Immediately removes from MapView, if specified.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="view"></param>
        public void RemoveRoad(long id, MapView view = null)
        {
            if (!_roadPolyLines.ContainsKey(id))
                return;

            view?.Drawables.Remove(_roadPolyLines[id]);
            _roadPolyLines.Remove(id);
        }

        /// <summary>
        /// Removes the Activity from the internal data.
        /// Also removes it from MapView if specified.
        /// </summary>
        /// <param name="activityID"></param>
        /// <param name="view"></param>
        public void RemoveActivity(long activityID, MapView view = null)
        {
            if (!_activityPins.ContainsKey(activityID))
                return;

            view?.Pins.Remove(_activityPins[activityID]);
            _activityPins.Remove(activityID);
        }

        /// <summary>
        /// Removes the Alert from the internal data.
        /// Also removes it from a MapView if specified.
        /// </summary>
        /// <param name="alertID"></param>
        /// <param name="view"></param>
        public void RemoveAlert(ulong alertID, MapView view = null)
        {
            if (!_alertPins.ContainsKey(alertID))
                return;

            view?.Pins.Remove(_alertPins[alertID]);
            _alertPins.Remove(alertID);
        }

        /// <summary>
        /// Removes the PointOfInterest from the internal data.
        /// Also removes it from a MapView if specified.
        /// </summary>
        /// <param name="poiID"></param>
        /// <param name="view"></param>
        public void RemovePointOfInterest(long poiID, MapView view = null)
        {
            if (!_pointOfInterestPins.ContainsKey(poiID))
                return;

            view?.Pins.Remove(_pointOfInterestPins[poiID]);
            _pointOfInterestPins.Remove(poiID);
        }

        /// <summary>
        /// Removes the CP from the internal data.
        /// Also removes it from a MapView if specified.
        /// </summary>
        /// <param name="cpID"></param>
        /// <param name="view"></param>
        public void RemoveCP(long cpID, MapView view = null)
        {
            if (!_cpPins.ContainsKey(cpID))
                return;

            view?.Pins.Remove(_cpPins[cpID]);
            _cpPins.Remove(cpID);
        }

        /// <summary>
        /// Removes the pin from the internal data.
        /// Also removes it from a MapView if specified.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="view"></param>
        public void RemovePin(Pin pin, MapView view = null)
        {
            if (!_pins.Contains(pin))
                return;

            _pins.Remove(pin);
            view?.Pins.Remove(pin);
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
        #endregion

        #region POLYLINES
        /// <summary>
        /// Adds polyline into a buffer. Adds point into the polyline on map click.
        /// Adds a marker for the last clicked position.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public void PolylineStart(MapView view = null)
        {
            var polyline = new Polyline();
            polyline.IsClickable = true;
            polyline.Clicked += (object sender, DrawableClickedEventArgs e) =>
                PolyLineClick(e.Point, sender as MapView);

            //_polyLines.Add(_polyLineID++, polyline);
            _polylineBuffer.Push(polyline);
            view?.Drawables.Add(polyline);
            _polylineAddition = true;
        }

        /// <summary>
        /// Map click no longer interacts with polyline.
        /// Removes the temporary marker.
        /// </summary>
        /// <param name="view"></param>
        public void PolylineFinish(MapView view = null)
        {
            _polylinePin.IsVisible = false;
            _polylineAddition = false;
        }

        /// <summary>
        /// Removes a point from the last polyline in the buffer.
        /// Removes the whole polyline if < 2.
        /// </summary>
        /// <param name="view"></param>
        public void PolylineUndo(MapView view = null)
        {
            if (_polylineBuffer.Count == 0)
                return;

            var positions = _polylineBuffer.Peek().Positions;
            positions.RemoveAt(positions.Count - 1);

            if (positions.Count < 2)
            {
                var polyline = _polylineBuffer.Pop();
                view?.Drawables.Remove(polyline);
            }

            view?.RefreshGraphics();
        }

        /// <summary>
        /// Adds polyline to internal data and creates an ID.
        /// Immediately draws on MapView, if specified.
        /// </summary>
        /// <returns></returns>
        public long AddPolyline(Polyline polyline, MapView view = null)
        {
            long id = DatabaseHolder<Road, RoadStorage>.Instance.rememberedList.nextID();
            _roadPolyLines.Add(id, polyline);
            view?.Drawables.Add(polyline);
            return id;
        }

        /// <summary>
        /// Fulshes the polyline buffer into the internal data.
        /// </summary>
        public void PolylineFlush(MapView view = null)
        {
            foreach (var polyline in _polylineBuffer)
            {
                view?.Drawables.Remove(polyline);
                SavePolyline(polyline);
            }

            _polylineBuffer.Clear();
        }

        public void PolylineDeletion() { _polylineDeletion = true; }
        public void PolylineStopDeletion() { _polylineDeletion = false; }

        private void SavePolyline(Polyline polyline)
        {
            var road = new Road();
            var c = polyline.StrokeColor;
            var rememberedList = DatabaseHolder<Road, RoadStorage>.Instance.rememberedList;

            road.ID = rememberedList.nextID();
            road.Thickness = polyline.StrokeWidth;
            road.Color.Clear();
            road.Color.Add(c.R);
            road.Color.Add(c.G);
            road.Color.Add(c.B);
            road.Color.Add(c.A);

            foreach (var pos in polyline.Positions)
                road.Coordinates.Add(new Pair<double, double>(pos.Longitude, pos.Latitude));

            rememberedList.add(road);
        }
        private long LoadRoad(Road road)
        {
            AddRoad(road);
            return road.ID;
        }
        #endregion

        #region PRIVATE METHODS
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
            //map.Layers.Add(CreateMarkersLayer());

            return map;
        }
        private Pin CreatePin(double lon, double lat, string label, MapView view = null)
        {
            Pin p = new Pin(view);
            p.Label = label;
            p.Position = new Position(lat, lon);
            p.Callout.ArrowAlignment = Mapsui.Rendering.Skia.ArrowAlignment.Right;
            p.Callout.Type = Mapsui.Rendering.Skia.CalloutType.Detail;

            p.Callout.TitleFontName = "Verdana";
            p.Callout.TitleFontAttributes = FontAttributes.Bold;
            p.Callout.TitleFontSize *= 0.8;

            p.Callout.SubtitleFontName = "Verdana";
            p.Callout.SubtitleFontAttributes = FontAttributes.Italic;
            p.Callout.SubtitleFontSize = p.Callout.TitleFontSize * 0.7f;

            //if (view != null) p.Scale = 0.7f; // Scale setter can throw null exception - WHY THE ACTUAL FRICK
            return p;
        }

        private void PinSetIcon(Pin pin, byte[] icon)
        {
            pin.Type = PinType.Icon;
            pin.Icon = icon;
            pin.Anchor = new XPoint(0, -pin.Height / 4.0);
        }

        private void PinSetIcon(Pin pin, string iconName)
        {
            byte[] icon = GetIcon(iconName);
            PinSetIcon(pin, icon);
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
                AddCP(rememberedList[i], view);
        }
        private void LoadPointsOfIntrest(MapView view = null)
        {
            var rememberedList = DatabaseHolder<PointOfInterest, PointOfInterestStorage>.Instance.rememberedList;

            for (int i = 0; i < rememberedList.Count; i++)
                AddPointOfInterest(rememberedList[i], view);
        }
        private void LoadRoads(MapView view = null)
        {
            var rememberedList = DatabaseHolder<Road, RoadStorage>.Instance.rememberedList;

            for (int i = 0; i < rememberedList.Count; i++)
                LoadRoad(rememberedList[i]);
        }
        private byte[] GetIcon(string path)
        {
            var stream = typeof(MapHandler).GetTypeInfo().Assembly.GetManifestResourceStream(path);
            return stream.ToBytes();
        }
        #endregion

        #region EVENTS
        public delegate void PinClick(PinClickedEventArgs e, long activityID, bool doubleClick);
        public delegate void MapClick(Position pos);
        public event PinClick OnPinClick;
        public event MapClick OnMapClick;

        private void HandlePinClicked(object sender, PinClickedEventArgs e)
        {
            Debug.WriteLine("CLICK");
            _time = _stopwatch.ElapsedMilliseconds;

            if (e.NumOfTaps == 1 && e.Pin.Label != "temp")
                if (e.Pin.Callout.IsVisible)
                    e.Pin.HideCallout();
                else
                    e.Pin.ShowCallout();

            if (e.Pin.Label == "Activity")
                foreach (long id in _activityPins.Keys)
                    if (_activityPins[id] == e.Pin)
                    {
                        OnPinClick?.Invoke(e, id, _time - _prevTime < _doubleClickTime);
                        break;
                    }

            if (e.Pin.Label == "POI")
                foreach (long id in _pointOfInterestPins.Keys)
                    if (_pointOfInterestPins[id] == e.Pin)
                    {
                        OnPinClick?.Invoke(e, id, _time - _prevTime < _doubleClickTime);
                        break;
                    }

            if (e.Pin.Label == "CP")
                foreach (long id in _cpPins.Keys)
                    if (_cpPins[id] == e.Pin)
                    {
                        OnPinClick?.Invoke(e, id, _time - _prevTime < _doubleClickTime);
                        break;
                    }

            _prevTime = _time;
            e.Handled = true;
        }

        private void HandleMapClicked(object sender, MapClickedEventArgs e)
        {
            if (_polylineAddition || _polylineDeletion)
                PolyLineClick(e.Point, sender as MapView);

            OnMapClick?.Invoke(e.Point);
            e.Handled = true;
        }

        private void PolyLineClick(Position pos, MapView view)
        {
            if (!_polylineDeletion)
            {
                _polylineBuffer.Peek().Positions.Add(pos);
                _polylinePin.Position = pos;
                _polylinePin.IsVisible = true;
                return;
            }

            float deletionDistance = 10;
            List<Position> toRemove = new List<Position>();

            if (view == null) view = _activeMapView;
            if (view == null) return;

            MPoint point1 = SphericalMercator.FromLonLat(pos.Longitude, pos.Latitude);
            double resolution = view.GetMapInfo(point1).Resolution;

            foreach (var polyline in _polylineBuffer)
            {
                for (int i = 0; i < polyline.Positions.Count; i++)
                {
                    var position = polyline.Positions[i];
                    MPoint point2 = position.ToMapsui();

                    if (point1.Distance(point2) / resolution < deletionDistance)
                        toRemove.Add(position);
                }

                foreach (var position in toRemove)
                    polyline.Positions.Remove(position);

                toRemove.Clear();
            }

            Func<(long, int)?> getRemoveIndex = () =>
            {
                foreach (var kvp in _roadPolyLines)
                {
                    long id = kvp.Key;
                    Polyline polyline = kvp.Value;

                    for (int i = 0; i < polyline.Positions.Count; i++)
                    {
                        var position = polyline.Positions[i];
                        MPoint point2 = position.ToMapsui();

                        if (point1.Distance(point2) / resolution < deletionDistance)
                            return (id, i);
                    }
                }

                return null;
            };

            (long, int)? roadToRemove = getRemoveIndex();
            if (roadToRemove == null) return;

            (long rID, int rIndex) = roadToRemove.Value;
            var rememberedList = DatabaseHolder<Road, RoadStorage>.Instance.rememberedList;
            var road = rememberedList.getByID(rID);

            if (road.Coordinates.Count > 2)
                rememberedList.getByID(rID).Coordinates.RemoveAt(rIndex);
            else
                rememberedList.removeByID(rID);
        }

        private void HandleMapClickedSelection(object sender, MapClickedEventArgs e)
        {
            _selectionPin.Position = new Position(e.Point.Latitude, e.Point.Longitude);
            OnMapClick?.Invoke(e.Point);
            e.Handled = true;
        }

        private void SQLEvents_created(Serializable created)
        {
            if (created is LarpActivity activity)
                AddActivity(activity, _activeMapView);

            if (created is PointOfInterest poi)
                AddPointOfInterest(poi, _activeMapView);

            if (created is CP cp)
                AddCP(cp, _activeMapView);

            if (created is Road road)
                AddRoad(road, _activeMapView);
        }

        private void SQLEvents_dataDeleted(Serializable deleted)
        {
            if (deleted is LarpActivity activity)
                RemoveActivity(activity.ID, _activeMapView);

            if (deleted is PointOfInterest poi)
                RemovePointOfInterest(poi.ID, _activeMapView);

            if (deleted is CP cp)
                RemoveCP(cp.ID, _activeMapView);

            if (deleted is Road road)
                RemoveRoad(road.ID, _activeMapView);
        }

        private void SQLEvents_dataChanged(Serializable changed, int changedAttributeIndex)
        {
            if (changed is LarpEvent && _activeMapView != null)
            {
                bool isClient = LocalStorage.cpID != 0 || LocalStorage.clientID != 0;
                if (isClient && IsGlobalPanLimits)
                    SetPanLimits(_activeMapView, LarpEvent.minX, LarpEvent.minY, LarpEvent.maxX, LarpEvent.maxY);
                else if (!IsGlobalPanLimits)
                    SetPanLimits(_activeMapView, _activeMapView.Map.Envelope.Bottom, _activeMapView.Map.Envelope.Left, _activeMapView.Map.Envelope.Top, _activeMapView.Map.Envelope.Right);
                

                if (isClient && IsGlobalZoomLimits)
                {
                    SetZoomLimits(_activeMapView, LarpEvent.minZoom, LarpEvent.maxZoom);
                    Zoom(_activeMapView, LarpEvent.maxZoom);
                } else if (!IsGlobalZoomLimits)
                {
                    SetZoomLimits(_activeMapView, _activeMapView.Map.Resolutions[_activeMapView.Map.Resolutions.Count - 1], _activeMapView.Map.Resolutions[2]);
                }

                MPoint topLeft = SphericalMercator.ToLonLat(LarpEvent.minX, LarpEvent.minY);
                MPoint botRight = SphericalMercator.ToLonLat(LarpEvent.maxX, LarpEvent.maxY);
                _limitsVisual.Positions.Clear();

                if (IsGlobalPanLimits)
                {
                    _limitsVisual.Positions.Add(new Position(topLeft.Y, topLeft.X));
                    _limitsVisual.Positions.Add(new Position(topLeft.Y, botRight.X));
                    _limitsVisual.Positions.Add(new Position(botRight.Y, botRight.X));
                    _limitsVisual.Positions.Add(new Position(botRight.Y, topLeft.X));
                    _limitsVisual.Positions.Add(new Position(topLeft.Y, topLeft.X));
                }
            }

            if (changed is LarpActivity activity)
            {
                RemoveActivity(activity.ID, _activeMapView);
                AddActivity(activity, _activeMapView);
            }

            if (changed is PointOfInterest poi)
            {
                RemovePointOfInterest(poi.ID, _activeMapView);
                AddPointOfInterest(poi, _activeMapView);
            }

            if (changed is CP cp)
            {
                RemoveCP(cp.ID, _activeMapView);
                AddCP(cp, _activeMapView);
            }

            if (changed is Road road)
            {
                RemoveRoad(road.ID, _activeMapView);
                AddRoad(road, _activeMapView);
            }
        }
        #endregion
    }
}
