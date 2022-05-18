using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Mapsui;
using Mapsui.Projection;
using Mapsui.Utilities;
using LAMA.Services;
using Xamarin.Essentials;

namespace LAMA.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        private bool UserAnswered = false;
		public MapPage()
		{
			InitializeComponent();
            MapHandler.Instance.MapViewSetup(mapView);
            MapHandler.Instance.AddEvent(0, 0, 125, mapView);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();


            // Just to be sure but it did not fixed the problem.
            await Permissions.RequestAsync<Permissions.StorageWrite>();
            await Permissions.RequestAsync<Permissions.StorageRead>();

            if (status != PermissionStatus.Granted)
            {
                if (!UserAnswered)
                {
                    await DisplayAlert("Location not available", "If you want your location to be visible on the map, please have your location turned on and grant the permission to use it.", "OK");
                    UserAnswered = true;
                }

                MapHandler.Instance.SetLocationVisible(mapView, false);
                return;
            }

            MapHandler.Instance.UpdateLocation(mapView);
            MapHandler.Instance.SetLocationVisible(mapView, true);
        }
    }
}