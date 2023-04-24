using LAMA.Colors;
using LAMA.Communicator;
using LAMA.Singletons;
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
            string password = ClientPassword;
            string serverName = ClientServerName;
            TryingToConnect = true;
            LoginEnabled = false;
            FakeLoginEnabled = false;
            DatabaseEnabled = false;

            //možnost vybrat
            try
            {
                if (CommunicationInfo.Instance.Communicator != null) { CommunicationInfo.Instance.Communicator.EndCommunication(); }
                //serverName - jméno toho serveru (identifikátor)
                //heslo serveru
                //clentName
                ClientCommunicator communicator = new ClientCommunicator(serverName, password);
                float timer = 0.0f;
                while (true)
                {
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
                        BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                        BarBackgroundColor = ColorPalette.PrimaryColor
                    };
                }
                else
                {
                    //ClientLoginTypePage.InitPage(communicator);
                    await Shell.Current.Navigation.PushAsync(new ClientLoginTypePage(communicator));
                    //await Shell.Current.GoToAsync($"{nameof(ClientLoginTypePage)}?communicator={communicator}");
                    //await Shell.Current.GoToAsync($"//{nameof(ClientLoginTypePage)}");
                }
            }
            catch (CantConnectToCentralServerException)
            {
                await App.Current.MainPage.DisplayAlert("Chyba Připojení", "Nepodařilo se připojit k seznamu existujících serverů. Zkontrolujte své připojení a pokuste se přihlásit znovu.", "OK");
                TryingToConnect = false;
                LoginEnabled = true;
                FakeLoginEnabled = true;
                DatabaseEnabled = true;
                return;
            }
            catch (CantConnectToDatabaseException)
            {
                await App.Current.MainPage.DisplayAlert("Chyba Připojení", "Nepodařilo se připojit k databázi existujících serverů. Zkuste opakovat zadání později.", "OK");
                TryingToConnect = false;
                LoginEnabled = true;
                FakeLoginEnabled = true;
                DatabaseEnabled = true;
                return;
            }
            catch (WrongCredentialsException)
            {
                await App.Current.MainPage.DisplayAlert("Chybné Přihlašovací Údaje", "Server neexistuje, nebo jste zadali špatné heslo.", "OK");
                TryingToConnect = false;
                LoginEnabled = true;
                FakeLoginEnabled = true;
                DatabaseEnabled = true;
                return;
            }
            catch (ServerConnectionRefusedException)
            {
                await App.Current.MainPage.DisplayAlert("Server Odmítl Připojení", "Server odmítl připojení, zkuste se znovu přihlásit později.", "OK");
                TryingToConnect = false;
                LoginEnabled = true;
                FakeLoginEnabled = true;
                DatabaseEnabled = true;
                return;
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
                await App.Current.MainPage.Navigation.PushAsync(new TestPage());
            }
            else
            {
                // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
                await Shell.Current.GoToAsync($"//{nameof(TestPage)}");
            }
        }
    }
}
