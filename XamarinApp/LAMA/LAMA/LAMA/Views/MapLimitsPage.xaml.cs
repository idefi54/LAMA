using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LAMA.Services;
using System.Threading.Tasks;
using Mapsui;
using Mapsui.UI.Forms;
using Xamarin.Essentials;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapLimitsPage : ContentPage
    {
        private MapView mapView;
        public MapLimitsPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var layout = Content as StackLayout;

            var indicator = new ActivityIndicator();
            indicator.IsRunning = true;
            indicator.IsVisible = true;
            layout.Children.Add(indicator);

            await Task.Delay(1000);
            mapView = new MapView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.Gray,
                HeightRequest = 500
            };

            MapHandler.Instance.MapViewSetup(mapView);
            MapHandler.Instance.SetLocationVisible(mapView, false);

            layout.Children.Add(mapView);
            layout.Children.Remove(indicator);
            indicator.IsRunning = false;
        }

        async void OnSaveLimits(object sender, EventArgs e)
        {
            Mapsui.Geometries.Point p = Mapsui.Projection.SphericalMercator.ToLonLat(mapView.Viewport.Center.X, mapView.Viewport.Center.Y);
            Location loc = new Location
            {
                Latitude = p.Y,
                Longitude = p.X
            };
            MapHandler.Instance.CurrentLocation = loc;
            (Content as StackLayout).Children.Remove(mapView);
            mapView = null;
            await Navigation.PushAsync(new MapPage());
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }
}