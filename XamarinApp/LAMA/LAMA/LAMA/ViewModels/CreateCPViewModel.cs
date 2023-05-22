using LAMA.Models;
using LAMA.Singletons;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace LAMA.ViewModels
{
    internal class CreateCPViewModel : BaseViewModel
    {
        INavigation navigation;

        public Command CancelCommand { get; private set; }
        public Command CreateCommand { get; private set; }

        string _name, _nick, _password, _roles, _phone, _facebook, _discord, _notes;

        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
        public string Roles { get { return _roles; } set { SetProperty(ref _roles, value); } }
        public string Nick { get { return _nick; } set { SetProperty(ref _nick, value); } }
        public string Password { get { return _password; } set { SetProperty(ref _password, value); } }
        public string Phone { get { return _phone; } set { SetProperty(ref _phone, value); } }
        public string Facebook { get { return _facebook; } set { SetProperty(ref _facebook, value); } }
        public string Discord { get { return _discord; } set { SetProperty(ref _discord, value); } }
        public string Notes { get { return _notes; } set { SetProperty(ref _notes, value); } }


        public CreateCPViewModel(INavigation navigation)
        {
            this.navigation = navigation;

            CancelCommand = new Command(OnCancel);
            CreateCommand = new Command(OnSave);
        }

        void OnCancel()
        {
            navigation.PopAsync();
        }
        async void OnSave()
        {
            bool cpNameValid = InputChecking.CheckInput(_name, "Jméno CP", 50);
            if (!cpNameValid) return;
            bool cpNickValid = InputChecking.CheckInput(_nick, "Přezdívka CP", 50);
            if (!cpNickValid) return;
            bool passwordValid = _password != null && _password.Trim().Length >= 5 && InputChecking.CheckInput(_password, "Heslo", 100);
            if (!passwordValid) return;
            bool cpRolesValid = InputChecking.CheckInput(_roles, "Role", 200, true);
            if (!cpRolesValid) return;
            bool cpPhoneValid = InputChecking.CheckInput(_phone, "Telefon", 20, true);
            if (!cpPhoneValid) return;
            bool cpFacebookValid = InputChecking.CheckInput(_facebook, "Facebook", 100, true);
            if (!cpFacebookValid) return;
            bool cpDiscordValid = InputChecking.CheckInput(_discord, "Discord", 100, true);
            if (!cpDiscordValid) return;
            bool cpNotesValid = InputChecking.CheckInput(_notes, "Role", 1000, true);
            if (!cpNotesValid) return;

            IsBusy = true;
            var list = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;

            var toAdd = new CP(list.nextID(), _name, _nick, Helpers.readStringField(_roles), _phone, _facebook, _discord, _notes);
            var cpRememberedList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;
            bool found = false;
            for (int i = 0; i < cpRememberedList.Count; i++)
            {
                if (cpRememberedList[i].nick == _nick) found = true;
            }
            if (found)
            {
                await App.Current.MainPage.DisplayAlert("Duplikát", "CP s toutou přezdívkou už existuje zvolte jinou přezdívku", "OK");
                IsBusy = false;
                return;
            }
            if (CommunicationInfo.Instance.IsServer)
            {
                list.add(toAdd);
                toAdd.password = Communicator.Encryption.EncryptPassword(Communicator.Encryption.EncryptPassword(_password));
            }
            else
            {
                try
                {
                    ((Communicator.ClientCommunicator)CommunicationInfo.Instance.Communicator).LoginAsCP(_name, Communicator.Encryption.EncryptPassword(_password), true);
                    float timer = 0.0f;
                    while (true)
                    {
                        await Task.Delay(500);

                        cpRememberedList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;
                        found = false;
                        for (int i = 0; i < cpRememberedList.Count; i++)
                        {
                            if (cpRememberedList[i].nick == _nick) found = true;
                        }
                        if (found)
                        {
                            break;
                        }
                        else if (((Communicator.ClientCommunicator)CommunicationInfo.Instance.Communicator).clientRefusedMessage != "")
                        {
                            throw new Communicator.ClientRefusedException(((Communicator.ClientCommunicator)CommunicationInfo.Instance.Communicator).clientRefusedMessage);
                        }
                        else
                        {
                            timer += 0.5f;
                            if (timer > 5.0f)
                            {
                                throw new TimeoutException("Nepodařilo se vytvořit CP. Zkontrolujte si internetové připojení.");
                            }
                        }
                    }
                }
                catch (TimeoutException)
                {
                    await App.Current.MainPage.DisplayAlert("Chyba Připojení", "Server neodpověděl na požadavek na vytvoření CP. Zkontrolujte si internetové připojení.", "OK");
                    IsBusy = false;
                    return;
                }
                catch (Communicator.ClientRefusedException)
                {
                    await App.Current.MainPage.DisplayAlert("Odmítnutí Serveru", "Server odmítl požadavek na vytvoření klienta.", "OK");
                    IsBusy = false;
                    return;
                }
            }
            IsBusy = false;
            await navigation.PopAsync();
        }

    }
}