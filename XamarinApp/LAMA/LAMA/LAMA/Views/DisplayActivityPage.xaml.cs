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
    public partial class DisplayActivityPage : ContentPage
    {
        private MapView _mapView;
        private LarpActivity _activity;

        public DisplayActivityPage(LarpActivity activity)
        {
            InitializeComponent();
            BindingContext = new DisplayActivityViewModel(Navigation,activity);
            _activity = activity;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var scrollView = Content as ScrollView;
            var layout = scrollView.Children[0] as StackLayout;

            var mapHorizontalOption = LayoutOptions.Fill;
            var mapVerticalOption = LayoutOptions.Fill;
            int mapHeightRequest = 300;
            var mapBackgroundColor = Color.Gray;

            var activityIndicator = new ActivityIndicator
            {
                VerticalOptions = mapVerticalOption,
                HorizontalOptions = mapHorizontalOption,
                HeightRequest = mapHeightRequest,
                IsRunning = true
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

            if (_activity != null)
            {
                long id = _activity.ID;
                double lon = _activity.place.first;
                double lat = _activity.place.second;

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
            if (_mapView == null)
                return;

            var scrollView = Content as ScrollView;
            (scrollView.Children[0] as StackLayout).Children.Remove(_mapView);
            _mapView = null;
        }
    }
}