using System;
using Xamarin.Forms;
using Mapsui.UI.Forms;
using LAMA.Services;
using LAMA.Views;
using Xamarin.Essentials;

namespace LAMA.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        private Func<MapView> _getMapView;
        private IKeyboardService _keyboard;

        /// <summary>
        /// Dropdown for filtering map entities.
        /// </summary>
        private bool _showDropdown = false;
        public bool ShowDropdown
        {
            get => _showDropdown;
            set => SetProperty(ref _showDropdown, value, nameof(ShowDropdown));
        }
        public Command FilterDropdown { get; private set; }
        public Command SaveHome { get; private set; }

        /// <summary>
        /// Edit switch for adding polylines.
        /// </summary>
        private bool _editing;
        public bool Editing
        {
            get => _editing;
            set
            {
                SetProperty(ref _editing, value, nameof(Editing));
                if (!value) MapHandler.Instance.PolylineFlush();
            }
        }

        public MapViewModel(Func<MapView> getMapView)
        {
            FilterDropdown = new Command(HandleFilterDropdown);
            SaveHome = new Command(HandleSaveHome);
            _getMapView = getMapView;
            _keyboard = DependencyService.Get<IKeyboardService>();

            if (_keyboard == null)
                return;

            _keyboard.OnKeyDown += OnKeyDown;
            _keyboard.OnKeyUp += OnKeyUp;
        }

        public void HandleFilterDropdown()
        {
            ShowDropdown = !ShowDropdown;

            if (!ShowDropdown)
                MapHandler.Instance.RefreshMapView(_getMapView());
        }

        public async void HandleSaveHome()
        {
            Mapsui.Geometries.Point p = Mapsui.Projection.SphericalMercator.ToLonLat(_getMapView().Viewport.Center.X, _getMapView().Viewport.Center.Y);
            Location loc = new Location
            {
                Latitude = p.Y,
                Longitude = p.X
            };
            MapHandler.Instance.CurrentLocation = loc;
            await MapHandler.Instance.UpdateLocation(_getMapView(), false);
            MapHandler.Instance.SetLocationVisible(_getMapView(), MapHandler.Instance.CurrentLocation != null);
        }

        private void OnKeyDown(string key)
        {
            if (_getMapView() == null || !Editing)
                return;

            if (key == "LeftShift")
                MapHandler.Instance.PolylineStart(_getMapView());

            if (_keyboard.IsKeyPressed("LeftCtrl") && key == "Z")
                MapHandler.Instance.PolylineUndo(_getMapView());

            if (key == "System")
                MapHandler.Instance.PolylineDeletion();
        }

        private void OnKeyUp(string key)
        {
            if (_getMapView() == null || !Editing)
                return;

            if (key == "LeftShift")
                MapHandler.Instance.PolylineFinish(_getMapView());

            if (key == "System")
                MapHandler.Instance.PolylineStopDeletion();
        }
    }
}
