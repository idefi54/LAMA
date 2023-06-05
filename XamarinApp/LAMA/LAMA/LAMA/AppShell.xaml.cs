using LAMA.Communicator;
using LAMA.Services;
using LAMA.Singletons;
using LAMA.ViewModels;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LAMA
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        const string STOP = "Zastavit sledování pozice na pozadí";
        const string START = "Spustit sledování pozice na pozadí";

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(DisplayActivityPage), typeof(DisplayActivityPage));
            LocationTracking.Text =
                GetLocationService.IsRunning
                ? START
                : STOP;

            MessagingCenter.Subscribe(this, "ServiceStarted", (StartServiceMessage message) => LocationTracking.Text = STOP);
            MessagingCenter.Subscribe(this, "ServiceStopped", (StopServiceMessage message) => LocationTracking.Text = START);
        }

        private void OnLocationTrackingClicked(object sender, EventArgs e)
        {

            var item = sender as MenuItem;

            if (Device.RuntimePlatform == Device.Android && item.Text == START)
            {
                StartService();
                item.Text = STOP;
                Debug.WriteLine("START");
            } else if (Device.RuntimePlatform == Device.Android)
            {
                StopService();
                item.Text = START;
                Debug.WriteLine("STOP");
            }
        }

        private void StartService()
        {
            var startServiceMessage = new StartServiceMessage();
            MessagingCenter.Send(startServiceMessage, "ServiceStarted");
        }

        private void StopService()
        {
            var stopServiceMessage = new StopServiceMessage();
            MessagingCenter.Send(stopServiceMessage, "ServiceStopped");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool result = await DisplayAlert("Odhlášení", "Opravdu se chcete odhlásit?", "Ano", "Ne");
            if (result)
            {
                CommunicationInfo.Instance.Communicator = null;
                CommunicationInfo.Instance.ServerName = "";
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

        private async void OpenActivityList(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ActivityListPage());
            Shell.Current.FlyoutIsPresented = false;
        }
        private async void OpenChat(object sender, EventArgs e)
        {
            Routing.RegisterRoute("//monkeydetails", typeof(ActivityListPage));
            await Shell.Current.GoToAsync("//monkeydetails");
            ContentPage page;


            //await Navigation.PushAsync(new ActivityListPage());
            //Shell.Current.FlyoutIsPresented = false;
            
        }
        private async void OpenActivityGraph(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ActivityListPage());
        }
        private async void OpenEncyclopedia(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ActivityListPage());
        }
        private async void OpenCPList(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ActivityListPage());
        }
        private async void OpenInventory(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ActivityListPage());
        }
        private async void OpenPOIList(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ActivityListPage());
        }
    }
}
