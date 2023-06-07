using LAMA.Models;
using LAMA.Services;
using LAMA.Singletons;
using LAMA.ViewModels;
using Mapsui.UI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class POIDetailsView : ContentPage
    {
        PointOfInterest _poi;
        private MapView _mapView;
        private Button _expandButton;

        public POIDetailsView()
        {
            InitializeComponent();
            BindingContext = new POIViewModel(Navigation, null);
        }
        public POIDetailsView(PointOfInterest poi)
        {
            InitializeComponent();
            BindingContext = new POIViewModel(Navigation, poi);
            _poi = poi;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var mapHandler = MapHandler.Instance;
            (_mapView, _expandButton)  = await mapHandler.CreateAndAddMapView(
                layout: MapLayout,
                horizontalOptions: LayoutOptions.Fill,
                verticalOptions: LayoutOptions.Fill,
                heightRequest: 300,
                hidableView: DetailsView);

            mapHandler.MapViewSetup(_mapView, showSelection:true);

            if (_poi != null)
            {
                long id = _poi.ID;
                double lon = _poi.Coordinates.first;
                double lat = _poi.Coordinates.second;

                mapHandler.RemovePointOfInterest(id, _mapView);
                mapHandler.SetSelectionPin(lon, lat);
                MapHandler.CenterOn(_mapView, lon, lat);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MapHandler.Instance.RemoveMapView(_mapView, MapLayout, _expandButton);
            _mapView = null;
        }
    }
}