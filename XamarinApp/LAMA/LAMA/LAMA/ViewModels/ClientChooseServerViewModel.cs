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
    public class ClientChooseServerViewModel : BaseViewModel
    {
        public Xamarin.Forms.Command DatabaseNameCommand { get; }
        public Xamarin.Forms.Command LoginCommand { get; }
        public Xamarin.Forms.Command FakeLoginCommand { get; }
        
        private bool _loginEnabled;
        public bool LoginEnabled { 
            get { return _loginEnabled; } 
            set { SetProperty(ref _loginEnabled, value); } 
        }

        private bool _fakeLoginEnabled;
        public bool FakeLoginEnabled { 
            get { return _fakeLoginEnabled; } 
            set { SetProperty(ref _fakeLoginEnabled, value); } 
        }

        private bool _databaseEnabled;
        public bool DatabaseEnabled { 
            get { return _databaseEnabled; } 
            set { SetProperty(ref _databaseEnabled, value); } 
        }
        private bool _tryingToConnect;
        public bool TryingToConnect
        {
            get { return _tryingToConnect; }
            set { SetProperty(ref _tryingToConnect, value); }
        }

        public string DatabaseName { get; set; }
        private string _databaseNameDisplay;
        public string DatabaseNameDisplay { get { return _databaseNameDisplay; } set { SetProperty(ref _databaseNameDisplay, value); } }
        public string ClientServerName { get; set; }
        public string ClientPassword { get; set; } 

        private string _errorLabel;
        public string ErrorLabel { get { return _errorLabel; } set { SetProperty(ref _errorLabel, value); } }

        public ClientChooseServerViewModel()
        {
            LoginCommand = new Xamarin.Forms.Command(OnLoginClicked);
            FakeLoginCommand = new Xamarin.Forms.Command(OnFakeLoginClicked);
            DatabaseNameCommand = new Xamarin.Forms.Command(OnChangeDatabaseName);
            DatabaseNameDisplay = $"Database Name: {SQLConnectionWrapper.databaseName}";
            TryingToConnect = false;
            LoginEnabled = true;
            FakeLoginEnabled = true;
            DatabaseEnabled = true;
        }

        private void OnChangeDatabaseName()
        {
            SQLConnectionWrapper.databaseName = DatabaseName;
            if (SQLConnectionWrapper.connection == null)
            {
                DatabaseNameDisplay = $"Database Name: {DatabaseName}";
            }
        }


        private async void OnLoginClicked(object obj)
        {
            Debug.WriteLine("OnLoginClicked");
            string password = ClientPassword;
            string serverName = ClientServerName;
            TryingToConnect = true;
            LoginEnabled = false;
            FakeLoginEnabled = false;
            DatabaseEnabled = false;

            //možnost vybrat
            try
            {
                //serverName - jméno toho serveru (identifikátor)
                //heslo serveru
                //clentName
                ClientCommunicator communicator = new ClientCommunicator(serverName, password);
                float timer = 0.0f;
                while (true) {
                    await Task.Delay(500);
                    if (ClientCommunicator.s.Connected)
                    {
                        break;
                    }
                    else
                    {
                        timer += 0.5f;
                        if (timer > 5.0f)
                        {
                            communicator.KillConnectionTimer();
                            throw new Exception("Nepodařilo se připojit k serveru");
                        }
                    }
                }
                if (Device.RuntimePlatform == Device.WPF)
                {
                    App.Current.MainPage = new NavigationPage(new ClientLoginTypePage(communicator))
                    {
                        BarBackground = new SolidColorBrush(new Color((double)33 / 255, (double)144 / 255, (double)243 / 255)),
                        BarBackgroundColor = new Color(33, 144, 243)
                    };
                }
                else
                {
                    //ClientLoginTypePage.InitPage(communicator);
                    await Shell.Current.Navigation.PushAsync(new ClientLoginTypePage(communicator));
                    //await Shell.Current.GoToAsync($"//{nameof(ClientLoginTypePage)}");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception Caught");
                Debug.WriteLine(e.ToString());
                Debug.WriteLine(e.Message);
                ErrorLabel = e.ToString();
                TryingToConnect = false;
                LoginEnabled = true;
                FakeLoginEnabled = true;
                DatabaseEnabled = true;
                return;
            }
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            //App.Current.MainPage = new NavigationPage(new ClientChooseNamePage(communicator));
            /*
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(ClientChooseNamePage)}");
            }
            */
        }

        private async void OnFakeLoginClicked(object obj)
        {
            if (Device.RuntimePlatform == Device.WPF)
            {
                //App.Current.MainPage = new TestPage();
                //await App.Current.MainPage.Navigation.PopToRootAsync();
                await App.Current.MainPage.Navigation.PushAsync(new TestPage());
                //LAMA.App.Current.MainPage = new TestPage();
            }
            else
            {
                // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
                await Shell.Current.GoToAsync($"//{nameof(TestPage)}");
            }
        }
    }
}
