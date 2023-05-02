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
                await Application.Current.MainPage.Navigation.PushAsync(new ServerLoginPage(false));
            }
            else {
                await Shell.Current.GoToAsync($"//{nameof(ServerLoginPage)}?newServer={false}");
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
