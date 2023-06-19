using LAMA.Themes;
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
        public Xamarin.Forms.Command LoginCommand { get; }
        
        private bool _loginEnabled;
        public bool LoginEnabled { 
            get { return _loginEnabled; } 
            set { SetProperty(ref _loginEnabled, value); } 
        }

        private bool _tryingToConnect;
        public bool TryingToConnect
        {
            get { return _tryingToConnect; }
            set { SetProperty(ref _tryingToConnect, value); }
        }

        public string ClientServerName { get; set; }
        public string ClientPassword { get; set; } 

        private string _errorLabel;
        public string ErrorLabel { get { return _errorLabel; } set { SetProperty(ref _errorLabel, value); } }

        public ClientChooseServerViewModel(string serverName = "")
        {
            LoginCommand = new Xamarin.Forms.Command(OnLoginClicked);
            TryingToConnect = false;
            LoginEnabled = true;
            ClientServerName = serverName;
        }


        private async void OnLoginClicked(object obj)
        {
            string password = ClientPassword;
            string serverName = ClientServerName;
            TryingToConnect = true;
            LoginEnabled = false;

            //možnost vybrat
            try
            {
                //serverName - jméno toho serveru (identifikátor)
                //heslo serveru
                //clentName
                if (serverName == null || serverName.Trim() == "") throw new EntryMissingException("Jméno Serveru");
                if (password == null || password.Trim() == "") throw new EntryMissingException("Heslo");
                if (serverName.Length > 100) throw new EntryTooLongException(100, "Jméno Serveru");
                if (password.Length > 100) throw new EntryTooLongException(100, "Heslo");

                if (CommunicationInfo.Instance.Communicator != null) { CommunicationInfo.Instance.Communicator.EndCommunication(); }
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
                    App.Current.MainPage = new NavigationPage(new ClientChooseNamePage(communicator, false))
                    {
                        BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                        BarBackgroundColor = ColorPalette.PrimaryColor
                    };
                }
                else
                {
                    await Shell.Current.Navigation.PushAsync(new ClientChooseNamePage(communicator, false));
                }
            }
            catch (EntryMissingException e)
            {
                await App.Current.MainPage.DisplayAlert("Chybějící Údaj", $"Pole \"{e.Message}\" nebylo vyplněno!", "OK");
                TryingToConnect = false;
                LoginEnabled = true;
                return;
            }
            catch (EntryTooLongException e)
            {
                await App.Current.MainPage.DisplayAlert("Dlouhý Vstup", $"Příliš dlouhý vstup - pole \"{e.fieldName}\" může mít maximální délku {e.length} znaků!", "OK");
                TryingToConnect = false;
                LoginEnabled = true;
                return;
            }
            catch (CantConnectToCentralServerException)
            {
                await App.Current.MainPage.DisplayAlert("Chyba Připojení", "Nepodařilo se připojit k seznamu existujících serverů. Zkontrolujte své připojení a pokuste se přihlásit znovu.", "OK");
                TryingToConnect = false;
                LoginEnabled = true;
                return;
            }
            catch (CantConnectToDatabaseException)
            {
                await App.Current.MainPage.DisplayAlert("Chyba Připojení", "Nepodařilo se připojit k databázi existujících serverů. Zkuste opakovat zadání později.", "OK");
                TryingToConnect = false;
                LoginEnabled = true;
                return;
            }
            catch (WrongCredentialsException)
            {
                await App.Current.MainPage.DisplayAlert("Chybné Přihlašovací Údaje", "Server neexistuje, nebo jste zadali špatné heslo.", "OK");
                TryingToConnect = false;
                LoginEnabled = true;
                return;
            }
            catch (ServerConnectionRefusedException)
            {
                await App.Current.MainPage.DisplayAlert("Server Odmítl Připojení", "Server odmítl připojení, zkuste se znovu přihlásit později.", "OK");
                TryingToConnect = false;
                LoginEnabled = true;
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
                return;
            }
        }
    }
}
