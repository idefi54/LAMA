using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LAMA.Services;
using Xamarin.Essentials;
using Mapsui.UI.Forms;
using LAMA.ViewModels;
using System;
using LAMA.Models;
using System.Collections.Generic;
using System.Diagnostics;
using LAMA.Singletons;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private MapView _mapView;
        private bool disappearing = false;

        public MapPage()
        {
            InitializeComponent();
            BindingContext = new MapViewModel(() => _mapView);

            // Add options into the filter
            int row = 0;
            foreach (MapHandler.EntityType type in Enum.GetValues(typeof(MapHandler.EntityType)))
            {
                if (type == MapHandler.EntityType.Nothing)
                    continue;

                LayoutOptions center = LayoutOptions.Center;

                var label = new Label
                {
                    Text = $"{type}:",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = center,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.White,
                };

                var checkBox = new CheckBox
                {
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = center,
                    IsChecked = true,
                    Color = Color.Green,
                    BackgroundColor = Color.Black,
                };
                checkBox.CheckedChanged += (object sender, CheckedChangedEventArgs e) =>
                {
                    MapHandler.Instance.Filter(type, e.Value);
                    MapHandler.Instance.RefreshMapView(_mapView);
                };

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
            disappearing = false;

            // Handle permissions and location
            if (Device.RuntimePlatform != Device.WPF)
            {
                await Permissions.RequestAsync<Permissions.StorageWrite>();
                await Permissions.RequestAsync<Permissions.StorageRead>();
            }
            bool locationAvailable = await CheckLocationAvailable();

            if (!locationAvailable && MapHandler.Instance.CurrentLocation == null)
            {
                await DisplayAlert("Lokace není přístupná",
                    "Prosíme, zapněte si lokaci nebo zadejte domovskou lokaci.", "OK");
            }

            MapHandler mapHandler = MapHandler.Instance;
            (_mapView, _) = await mapHandler.CreateAndAddMapView(MapLayout, LayoutOptions.FillAndExpand, LayoutOptions.FillAndExpand, 300, null);
            mapHandler.MapViewSetup(_mapView, showSelection: false, relocateSelection: false);

            MapHandler.Instance.OnPinClick += OnPinClicked;
            await MapHandler.Instance.UpdateLocation(_mapView, locationAvailable);
            MapHandler.Instance.SetLocationVisible(_mapView, MapHandler.Instance.CurrentLocation != null || locationAvailable);

            if (MapHandler.Instance.CurrentLocation != null)
            {
                MapHandler.CenterOn(_mapView, MapHandler.Instance.CurrentLocation.Longitude, MapHandler.Instance.CurrentLocation.Latitude);
                MapHandler.Zoom(_mapView);
                SetHomeLocationButton.BackgroundColor = Color.Gray;
                SetHomeLocationButton.Text = "Zadejte domovskou lokaci";
            }
            SetHomeLocationButton.Clicked += SetHomeClicked;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MapHandler.Instance.MapDataSave();

            if (_mapView == null)
                return;

            MapLayout.Children.Remove(_mapView);
            _mapView = null;
        }

        private void SetHomeClicked(object sender, EventArgs e)
        {
            SetHomeLocationButton.BackgroundColor = Color.Gray;
            SetHomeLocationButton.Text = "Změnit Domovskou lokaci";
        }

        private async void OnPinClicked(PinClickedEventArgs e, long id, bool doubleClick)
        {
            if (!doubleClick || disappearing)
                return;

            if (e.Pin.Label == "Activity")
            {
                var rememberedList = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList;
                LarpActivity activity = rememberedList.getByID(id);
                disappearing = true;
                await Navigation.PushAsync(new ActivityDetailsPage(activity));
            }

            if (e.Pin.Label == "POI")
            {
                var rememberedList = DatabaseHolder<PointOfInterest, PointOfInterestStorage>.Instance.rememberedList;
                PointOfInterest pointOfInterest = rememberedList.getByID(id);
                disappearing = true;
                await Navigation.PushAsync(new POIDetailsView(pointOfInterest));
            }

            if (e.Pin.Label == "CP")
            {
                var rememberedList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;
                CP cp = rememberedList.getByID(id);
                disappearing = true;
                await Navigation.PushAsync(new CPDetailsView(cp));
            }

            e.Handled = true;
        }
    }
}