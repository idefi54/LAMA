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
		private MapHandler mapHandler;

		public MapPage()
		{
			InitializeComponent();

			mapHandler = new MapHandler(mapView);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            while (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Error", "Please, have your location turned on and accept the permission.", "OK");
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            mapHandler.CenterOnLocation();

        }
    }
}