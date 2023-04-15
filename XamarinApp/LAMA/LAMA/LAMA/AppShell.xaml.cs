using LAMA.Communicator;
using LAMA.ViewModels;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace LAMA
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            Debug.WriteLine("App Shell");
            InitializeComponent();
            Routing.RegisterRoute(nameof(DisplayActivityPage), typeof(DisplayActivityPage));
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform == Device.WPF)
            {
                await App.Current.MainPage.Navigation.PushAsync(new ChooseClientServerPage());
            }
            else
            {
                await Shell.Current.GoToAsync("//ClientChooseServerPage");
            }
        }
    }
}
