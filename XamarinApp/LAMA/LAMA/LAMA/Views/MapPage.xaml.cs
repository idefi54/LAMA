using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LAMA.Services;
using Xamarin.Essentials;
using Mapsui.UI.Forms;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private MapView _mapView;
        private Button _setHomeButton;

        public MapPage()
        {
            InitializeComponent();
            MapHandler.Instance.OnPinClick += OnPinClicked;
            _setHomeButton = new Button();
            _setHomeButton.Clicked += SetHomeButton_Clicked;
            _setHomeButton.Text = "Set Home Location";
            _setHomeButton.TextColor = Color.Black;
            _setHomeButton.BackgroundColor = Color.Yellow;
            var layout = (Content as StackLayout);
            layout.Children.Add(_setHomeButton);
        }

        private async void OnPinClicked(PinClickedEventArgs e, int activityID, bool doubleClick)
        {
            if (!doubleClick)
                return;
            Models.LarpActivity activity = DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.getByID(activityID);
            await Navigation.PushAsync(new DisplayActivityPage(activity));
            e.Handled = true;
        }

        private async Task<bool> CheckLocationAvailable()
        {
            if (Device.RuntimePlatform != Device.WPF)
            {
                PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                    return false;
            }
            else
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
            var layout = (Content as StackLayout);

            // Add activity indicator
            var activityIndicator = new ActivityIndicator
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };
            activityIndicator.IsRunning = true;
            layout.Children.Add(activityIndicator);

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

            // Init Map View
            _mapView = new MapView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Gray
            };
            MapHandler.Instance.MapViewSetup(_mapView);
            await MapHandler.Instance.UpdateLocation(_mapView, locationAvailable);
            MapHandler.Instance.SetLocationVisible(_mapView, MapHandler.Instance.CurrentLocation != null || locationAvailable);

            if (MapHandler.Instance.CurrentLocation != null)
            {
                MapHandler.CenterOn(_mapView, MapHandler.Instance.CurrentLocation.Longitude, MapHandler.Instance.CurrentLocation.Latitude);
                MapHandler.Zoom(_mapView, 75);
                MapHandler.SetZoomLimits(_mapView, 1, 100);
                _setHomeButton.BackgroundColor = Color.Gray;
                _setHomeButton.Text = "Change Home Location";
            }

            layout.Children.Add(_mapView);
            layout.Children.Remove(activityIndicator);
           
            // test
            MapHandler.Instance.AddActivity(0, 0, 125, _mapView);
        }

        private async void SetHomeButton_Clicked(object sender, System.EventArgs e)
        {
            _setHomeButton.BackgroundColor = Color.Gray;
            _setHomeButton.Text = "Change Home Location";

            Mapsui.Geometries.Point p = Mapsui.Projection.SphericalMercator.ToLonLat(_mapView.Viewport.Center.X, _mapView.Viewport.Center.Y);
            Location loc = new Location
            {
                Latitude = p.Y,
                Longitude = p.X
            };
            MapHandler.Instance.CurrentLocation = loc;
            MapHandler.Instance.UpdateLocation(_mapView, false);
            MapHandler.Instance.SetLocationVisible(_mapView, MapHandler.Instance.CurrentLocation != null);
            MapHandler.CenterOn(_mapView, MapHandler.Instance.CurrentLocation.Longitude, MapHandler.Instance.CurrentLocation.Latitude);
            MapHandler.Zoom(_mapView, 75);
            MapHandler.SetZoomLimits(_mapView, 1, 100);

            await DisplayAlert("Success", "The location has been set. Tap the button to return to location at any time.", "OK");
            return;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (_mapView == null)
                return;

            (Content as StackLayout).Children.Remove(_mapView);
            _mapView = null;
        }
    }
}