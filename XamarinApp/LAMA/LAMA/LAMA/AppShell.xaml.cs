using LAMA.Communicator;
using LAMA.Singletons;
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
            InitializeComponent();
            Routing.RegisterRoute(nameof(DisplayActivityPage), typeof(DisplayActivityPage));
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
    }
}
