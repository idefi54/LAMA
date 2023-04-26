using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAMA.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LAMA
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            App.Current.Resources["CommandBarBackgroundColor"] = new SolidColorBrush(Colors.ColorPalette.PrimaryColor);
            App.Current.Resources["DefaultTitleBarBackgroundColor"] = new SolidColorBrush(Colors.ColorPalette.PrimaryColor);
            App.Current.Resources["DefaultTabbedBarBackgroundColor"] = new SolidColorBrush(Colors.ColorPalette.PrimaryColor);
            App.Current.Resources["AccentColor"] = new SolidColorBrush(Colors.ColorPalette.PrimaryColor);

            Debug.WriteLine("Main Page");
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

            MapHandler.Instance.UpdateLocation(mapView);
        }
    }
}
