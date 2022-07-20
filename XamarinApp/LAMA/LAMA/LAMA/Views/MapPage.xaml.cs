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

        public MapPage()
        {
            InitializeComponent();
            MapHandler.Instance.OnPinClick += OnPinClicked;
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
            PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
                return false;

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
            await Permissions.RequestAsync<Permissions.StorageWrite>();
            await Permissions.RequestAsync<Permissions.StorageRead>();
            bool locationAvailable = await CheckLocationAvailable();

            if (!locationAvailable && MapHandler.Instance.CurrentLocation == null)
            {
                await DisplayAlert("Location not available", "If you want your location to be visible on the map, please have your location turned on and grant the permission to use it.", "OK");
                await Navigation.PushModalAsync(new MapLimitsPage());
                layout.Children.Remove(activityIndicator);
                return;
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
            MapHandler.Instance.UpdateLocation(_mapView, locationAvailable);
            MapHandler.Instance.SetLocationVisible(_mapView, locationAvailable);
            MapHandler.CenterOn(_mapView, MapHandler.Instance.CurrentLocation.Longitude, MapHandler.Instance.CurrentLocation.Latitude);
            MapHandler.Zoom(_mapView, 75);
            MapHandler.SetZoomLimits(_mapView, 1, 100);

            layout.Children.Add(_mapView);
            layout.Children.Remove(activityIndicator);
           
            // test
            MapHandler.Instance.AddActivity(0, 0, 125, _mapView);
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