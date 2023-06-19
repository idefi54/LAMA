using LAMA.Themes;
using LAMA.Communicator;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using LAMA.Services;
using LAMA.Singletons;

namespace LAMA.ViewModels
{
    public class ClientChooseNameViewModel : BaseViewModel
    {
        private ClientCommunicator communicator;
        public Xamarin.Forms.Command LoginCommand { get; }

        public Xamarin.Forms.Command Back { get; }

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
            Back = new Xamarin.Forms.Command(OnGoBack);
            CreatingCP = false;
            LoginEnabled = true;
            isNew = newClient;
        }

        private async void OnGoBack(object obj)
        {
            bool result = await Application.Current.MainPage.DisplayAlert("Návrat", "Opravdu se chcete vrátit na výběr serveru (bude nutné opětovné zadání hesla)?", "Ano", "Ne");
            if (result)
            {
                string serverName = CommunicationInfo.Instance.ServerName;
                CommunicationInfo.Instance.Communicator = null;
                CommunicationInfo.Instance.ServerName = "";
                if (Device.RuntimePlatform == Device.WPF)
                {

                    App.Current.MainPage = new NavigationPage(new ChooseClientServerPage())
                    {
                        BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                        BarBackgroundColor = ColorPalette.PrimaryColor
                    };
                    await App.Current.MainPage.Navigation.PushAsync(new ClientChooseServerPage(serverName));
                }
                else
                {
                    await Shell.Current.GoToAsync($"//ClientChooseServerPage?serverName={serverName}");
                }
            }
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

                if (clientName.Length > 100) throw new EntryTooLongException(100, "Přezdívka");
                if (password.Length > 100) throw new EntryTooLongException(100, "Heslo");

                communicator.LoginAsCP(clientName, password, isNew);
                float timer = 0.0f;
                while (true)
                {
                    await Task.Delay(500);
                    if (communicator.loggedIn)
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
                        if (timer > 10.0f)
                        {
                            throw new TimeoutException("Nepodařilo se vytvořit CP klienta. Zkuste se přihlásit znovu.");
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
            catch (EntryTooLongException e)
            {
                await App.Current.MainPage.DisplayAlert("Dlouhý Vstup", $"Příliš dlouhý vstup - pole \"{e.fieldName}\" může mít maximální délku {e.length} znaků!", "OK");
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
                    await App.Current.MainPage.DisplayAlert("Přihlášení Se Nezdařilo", "CP s touto přezdívkou už existuje, zvolte prosím jiné jméno", "OK");
                } 
                else if (communicator.clientRefusedMessage == "Client")
                {
                    choseToCreateNewCP = await App.Current.MainPage.DisplayAlert("Neexistující CP", "CP s touto přezdívkou neexistuje, chcete ho vytvořit?", "Ano", "Ne");
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
                await Shell.Current.GoToAsync($"//MapPage");
                var startServiceMessage = new StartServiceMessage();
                MessagingCenter.Send(startServiceMessage, "ServiceStarted");
            }
        }
    }
}
