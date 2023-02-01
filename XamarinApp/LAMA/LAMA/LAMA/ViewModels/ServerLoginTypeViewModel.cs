using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class ServerLoginTypeViewModel : BaseViewModel
    {
        public Xamarin.Forms.Command NewServerCommand { get; }
        public Xamarin.Forms.Command ExistingServerCommand { get; }

        public ServerLoginTypeViewModel()
        {
            NewServerCommand = new Xamarin.Forms.Command(OnNewServer);
            ExistingServerCommand = new Xamarin.Forms.Command(OnExistingServer);
        }

        private async void OnNewServer()
        {
            if (Device.RuntimePlatform == Device.WPF)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new ServerLoginPage(true));
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(ServerLoginPage)}?newServer={true}");
            }
        }

        private async void OnExistingServer()
        {
            if (Device.RuntimePlatform == Device.WPF)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new ServerLoginPage(false));
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(ServerLoginPage)}?newServer={false}");
            }
        }
    }
}
