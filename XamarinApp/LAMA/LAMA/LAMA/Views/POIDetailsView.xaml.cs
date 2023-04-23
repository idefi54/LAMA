using LAMA.Models;
using LAMA.Services;
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
            var layout = Content as StackLayout;

            var mapHorizontalOption = LayoutOptions.Fill;
            var mapVerticalOption = LayoutOptions.Fill;
            int mapHeightRequest = 300;
            var mapBackgroundColor = Color.Gray;

            var activityIndicator = new ActivityIndicator
            {
                VerticalOptions = mapVerticalOption,
                HorizontalOptions = mapHorizontalOption,
                HeightRequest = mapHeightRequest,
                IsRunning = true,
                IsVisible = true
            };
            layout.Children.Add(activityIndicator);

            await Task.Delay(500);
            _mapView = new MapView
            {
                VerticalOptions = mapVerticalOption,
                HorizontalOptions = mapHorizontalOption,
                HeightRequest = mapHeightRequest,
                BackgroundColor = mapBackgroundColor
            };

            MapHandler mapHandler = MapHandler.Instance;
            mapHandler.MapViewSetup(_mapView, showSelection: true, relocateSelection: false);

            if (_poi != null)
            {
                long id = _poi.ID;
                double lon = _poi.Coordinates.first;
                double lat = _poi.Coordinates.second;

                mapHandler.RemoveActivity(id, _mapView);
                mapHandler.SetSelectionPin(lon, lat);
                MapHandler.CenterOn(_mapView, lon, lat);
            }

            layout.Children.Remove(activityIndicator);
            layout.Children.Add(_mapView);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            (Content as StackLayout).Children.Remove(_mapView);
            _mapView = null;
        }

    }
}