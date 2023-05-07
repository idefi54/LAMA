using LAMA.Themes;
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
            string password = ClientPassword;
            string clientName = ClientName;
            CreatingCP = true;
            LoginEnabled = false;
            try
            {
                if (clientName == null || clientName.Trim() == "") throw new EntryMissingException("Přezdívka");
                if (password == null || password.Trim() == "") throw new EntryMissingException("Heslo");
                if (password.Length < 5)
                {
                    throw new PasswordTooShortException();
                }
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
                    else if (communicator.clientRefusedMessage != "")
                    {
                        throw new ClientRefusedException(communicator.clientRefusedMessage);
                    }
                    else
                    {
                        timer += 0.5f;
                        if (timer > 5.0f)
                        {
                            throw new TimeoutException("Nepodařilo se nastavit CP jméno. Zkuste se přihlásit znovu.");
                        }
                    }
                }
            }
            catch (EntryMissingException e)
            {
                await App.Current.MainPage.DisplayAlert("Chybějící Údaj", $"Pole \"{e.Message}\" nebylo vyplněno!", "OK");
                CreatingCP = false;
                LoginEnabled = true;
                communicator.clientRefusedMessage = "";
                isNew = false;
                return;
            }
            catch (PasswordTooShortException)
            {
                await App.Current.MainPage.DisplayAlert("Příliš Krátké Heslo", "Zadané heslo je příliš krátké, heslo musí obsahovat alespoň 5 znaků", "OK");
                CreatingCP = false;
                LoginEnabled = true;
                communicator.clientRefusedMessage = "";
                isNew = false;
                return;
            }
            catch (ClientRefusedException)
            {
                bool choseToCreateNewCP = false;
                if (isNew) {
                    await App.Current.MainPage.DisplayAlert("Přihlášení Se Nezdařilo", "CP s tímto jménem už existuje, zvolte prosím jiné jméno", "OK");
                } 
                else if (communicator.clientRefusedMessage == "Client")
                {
                    choseToCreateNewCP = await App.Current.MainPage.DisplayAlert("Existující CP", "CP s tímto jménem neexistuje, chcete ho vytvořit", "Ano", "Ne");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Přihlášení Se Nezdařilo", "Zadal/a jste špatné heslo", "OK");
                }

                CreatingCP = false;
                communicator.clientRefusedMessage = "";
                if (choseToCreateNewCP)
                {
                    isNew = true;
                    OnLoginClicked(obj);
                    return;
                }
                else
                {
                    LoginEnabled = true;
                    return;
                }
            }
            catch (TimeoutException)
            {
                await App.Current.MainPage.DisplayAlert("Přihlášení Se Nezdařilo", "Server nezareagoval na přihlášení, zkontrolujte připojení nebo opakujte přihlášení později.", "OK");
                CreatingCP = false;
                LoginEnabled = true;
                communicator.clientRefusedMessage = "";
                isNew = false;
                return;
            }
            catch (Exception e)
            {
                ErrorLabel = e.ToString();
                CreatingCP = false;
                LoginEnabled = true;
                communicator.clientRefusedMessage = "";
                isNew = false;
                return;
            }
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            if (Device.RuntimePlatform == Device.WPF)
            {
                App.Current.MainPage = new NavigationPage(new MapPage())
                {
                    BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                    BarBackgroundColor = ColorPalette.PrimaryColor
                };
            }
            else
            {
                await Shell.Current.GoToAsync($"//ChatChannels");
            }
        }
    }
}
