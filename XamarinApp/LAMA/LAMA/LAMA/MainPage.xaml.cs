using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LAMA
{
    public partial class MainPage : ContentPage
    {
        private MapHandler mapHandler;

        public MainPage()
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
