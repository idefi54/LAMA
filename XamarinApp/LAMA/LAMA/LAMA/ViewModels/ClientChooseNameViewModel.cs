using LAMA.Communicator;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class ClientChooseNameViewModel : BaseViewModel
    {
        private ClientCommunicator communicator;
        public Xamarin.Forms.Command LoginCommand { get; }

        private bool _loginEnabled;
        public bool LoginEnabled
        {
            get { return _loginEnabled; }
            set { SetProperty(ref _loginEnabled, value); }
        }

        private bool _creatingCP;
        public bool CreatingCP
        {
            get { return _creatingCP; }
            set { SetProperty(ref _creatingCP, value); }
        }

        public string ClientName { get; set; }
        public string ClientPassword { get; set; }

        private string _errorLabel;
        public string ErrorLabel { get { return _errorLabel; } set { SetProperty(ref _errorLabel, value); } }

        private bool isNew;

        public ClientChooseNameViewModel(ClientCommunicator communicator, bool newClient)
        {
            this.communicator = communicator;
            LoginCommand = new Xamarin.Forms.Command(OnLoginClicked);
            CreatingCP = false;
            LoginEnabled = true;
            isNew = newClient;
        }


        private async void OnLoginClicked(object obj)
        {
            Debug.WriteLine("OnLoginClicked");
            string password = ClientPassword;
            string clientName = ClientName;
            CreatingCP = true;
            LoginEnabled = false;

            //možnost vybrat
            try
            {
                //client Name - přezdívka klienta
                //password - heslo klienta
                communicator.LoginAsCP(clientName, password, isNew);
                float timer = 0.0f;
                while (true)
                {
                    await Task.Delay(500);
                    if (communicator.connected)
                    {
                        break;
                    }
                    else
                    {
                        timer += 0.5f;
                        if (timer > 5.0f)
                        {
                            throw new Exception("Nepodařilo se nastavit CP jméno. Zkuste se přihlásit znovu.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLabel = e.ToString();
                CreatingCP = false;
                LoginEnabled = true;
                return;
            }
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            if (Device.RuntimePlatform == Device.WPF)
            {
                App.Current.MainPage = new NavigationPage(new MapPage())
                {
                    BarBackground = new SolidColorBrush(new Color((double)33 / 255, (double)144 / 255, (double)243 / 255)),
                    BarBackgroundColor = new Color(33, 144, 243)
                };
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}");
            }
        }
    }
}
