using LAMA.Communicator;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Xamarin.Forms.Command LoginCommand { get; }
        public Xamarin.Forms.Command FakeLoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Xamarin.Forms.Command(OnLoginClicked);
            FakeLoginCommand = new Xamarin.Forms.Command(OnFakeLoginClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            string name = "";
            string IP = "";
            int port = 0;
            string password = "";
            string serverName = "";
            string clientName = "";

            new ServerCommunicator(name, IP, port, password);
            new ClientCommunicator(serverName, password, clientName);
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            await Shell.Current.GoToAsync($"//{nameof(MapPage)}");
        }

        private async void OnFakeLoginClicked(object obj)
        {
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            await Shell.Current.GoToAsync($"//{nameof(MapPage)}");
        }
    }
}
