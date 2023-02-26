using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LAMA.Services;
using Xamarin.Essentials;
using Mapsui.UI.Forms;
using LAMA.ViewModels;
using System;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private MapView _mapView;

        public MapPage()
        {
            InitializeComponent();
            BindingContext = new MapPageViewModel(() => _mapView);

            int row = 0;
            foreach (MapHandler.EntityType type in Enum.GetValues(typeof(MapHandler.EntityType)))
            {
                LayoutOptions center = LayoutOptions.Center;

                var label = new Label
                {
                    Text = $"{type}:",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = center,
                    FontAttributes = FontAttributes.Bold,
                };

                var checkBox = new CheckBox
                {
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = center,
                    IsChecked = true,
                    Color = Color.Green,
                };
                checkBox.CheckedChanged += (object sender, CheckedChangedEventArgs e) =>
                    MapHandler.Instance.Filter(type, e.Value);

                FilterGrid.Children.Add(label, 0, row);
                FilterGrid.Children.Add(checkBox, 1, row);
                row++;
            }
        }



        private async Task<bool> CheckLocationAvailable()
        {
            if (Device.RuntimePlatform != Device.WPF)
            {
                PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                    return false;
            } else
            {
                return false;
            }

            // try if the location is truly ON
            try
            {
                await Geolocation.GetLocationAsync();
            } catch (FeatureNotEnabledException)
            {
                return false;
            }

            return true;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            // Add activity indicator
            var activityIndicator = new ActivityIndicator
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                IsRunning = true
            };
            MapLayout.Children.Add(activityIndicator);

            // Handle permissions and location
            if (Device.RuntimePlatform != Device.WPF)
            {
                await Permissions.RequestAsync<Permissions.StorageWrite>();
                await Permissions.RequestAsync<Permissions.StorageRead>();
            }
            bool locationAvailable = await CheckLocationAvailable();

            if (!locationAvailable && MapHandler.Instance.CurrentLocation == null)
            {
                await DisplayAlert("Location not available",
                    "Please, turn on your location or set your home location.", "OK");
            }

            // Handle the fucking map
            await Task.Delay(500);

            // test
            MapHandler.Instance.AddAlert(20, 20, "ALERT");

            // Init Map View
            _mapView = new MapView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Gray,
                //HeightRequest = Application.Current.MainPage.Height
            };
            MapHandler.Instance.MapViewSetup(_mapView);
            await MapHandler.Instance.UpdateLocation(_mapView, locationAvailable);
            MapHandler.Instance.SetLocationVisible(_mapView, MapHandler.Instance.CurrentLocation != null || locationAvailable);

            if (MapHandler.Instance.CurrentLocation != null)
            {
                MapHandler.CenterOn(_mapView, MapHandler.Instance.CurrentLocation.Longitude, MapHandler.Instance.CurrentLocation.Latitude);
                MapHandler.Zoom(_mapView);
                SetHomeLocationButton.BackgroundColor = Color.Gray;
                SetHomeLocationButton.Text = "Change Home Location";
            }
            SetHomeLocationButton.Clicked += SetHomeClicked;

            MapLayout.Children.Remove(activityIndicator);
            MapLayout.Children.Add(_mapView);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (_mapView == null)
                return;

            MapLayout.Children.Remove(_mapView);
            _mapView = null;
        }

        private async void SetHomeClicked(object sender, System.EventArgs e)
        {
            SetHomeLocationButton.BackgroundColor = Color.Gray;
            SetHomeLocationButton.Text = "Change Home Location";

            Mapsui.Geometries.Point p = Mapsui.Projection.SphericalMercator.ToLonLat(_mapView.Viewport.Center.X, _mapView.Viewport.Center.Y);
            Location loc = new Location
            {
                Latitude = p.Y,
                Longitude = p.X
            };
            MapHandler.Instance.CurrentLocation = loc;
            await MapHandler.Instance.UpdateLocation(_mapView, false);
            MapHandler.Instance.SetLocationVisible(_mapView, MapHandler.Instance.CurrentLocation != null);
            MapHandler.CenterOn(_mapView, MapHandler.Instance.CurrentLocation.Longitude, MapHandler.Instance.CurrentLocation.Latitude);
            MapHandler.Zoom(_mapView);

            if (Device.RuntimePlatform != Device.WPF)
            {
                await DisplayAlert("Success", "The location has been set. Tap the button to return to location at any time.", "OK");
            }
            return;
        }
    }
}