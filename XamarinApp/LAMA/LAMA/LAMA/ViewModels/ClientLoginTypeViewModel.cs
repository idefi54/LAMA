using LAMA.Communicator;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class ClientLoginTypeViewModel : BaseViewModel
    {
        public Xamarin.Forms.Command NewClientCommand { get; }
        public Xamarin.Forms.Command ExistingClientCommand { get; }

        private ClientCommunicator clientCommunicator;

        public ClientLoginTypeViewModel(ClientCommunicator communicator)
        {
            NewClientCommand = new Xamarin.Forms.Command(OnNewClient);
            ExistingClientCommand = new Xamarin.Forms.Command(OnExistingClient);
            clientCommunicator = communicator;
        }

        private async void OnNewClient(object obj)
        {
            if (Device.RuntimePlatform == Device.WPF)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new ClientChooseNamePage(clientCommunicator, true));
            }
            else
            {
                await Shell.Current.Navigation.PushAsync(new ClientChooseNamePage(clientCommunicator, true));
            }
        }

        private async void OnExistingClient(object obj)
        {
            if (Device.RuntimePlatform == Device.WPF)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new ClientChooseNamePage(clientCommunicator, false));
            }
            else
            {
                await Shell.Current.Navigation.PushAsync(new ClientChooseNamePage(clientCommunicator, false));
            }
        }
    }
}
