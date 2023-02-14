using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class ChooseClientServerViewModel : BaseViewModel
    {
        public Xamarin.Forms.Command ChooseServerCommand { get; }
        public Xamarin.Forms.Command ChooseClientCommand { get; }

        public ChooseClientServerViewModel()
        {
            ChooseServerCommand = new Xamarin.Forms.Command(OnServerClicked);
            ChooseClientCommand = new Xamarin.Forms.Command(OnClientClicked);
        }

        private async void OnServerClicked()
        {
            if (Device.RuntimePlatform == Device.WPF)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new ServerLoginTypePage());
            }
            else {
                await Shell.Current.GoToAsync($"//{nameof(ServerLoginTypePage)}");
            }
        }

        private async void OnClientClicked()
        {
            if (Device.RuntimePlatform == Device.WPF)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new ClientChooseServerPage());
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(ClientChooseServerPage)}");
            }
        }
    }
}
