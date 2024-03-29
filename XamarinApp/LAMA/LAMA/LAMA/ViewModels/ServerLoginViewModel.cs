﻿using LAMA.Themes;
using LAMA.Communicator;
using LAMA.Singletons;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class ServerLoginViewModel : BaseViewModel
    {
        public Xamarin.Forms.Command ServerLoginCommand { get; }

        public string ServerName { get; set; }
        public string ServerNgrokEndpoint { get; set; }
        public string ServerPassword { get; set; }

        public string NickName { get; set; }

        public string PersonalPassword { get; set; }

        private string _errorLabel;
        public string ErrorLabel { get { return _errorLabel; } set { SetProperty(ref _errorLabel, value); } }

        private bool isNewServer;

        public ServerLoginViewModel(bool newServer)
        {
            ServerLoginCommand = new Xamarin.Forms.Command(OnServerLoginClicked);
            isNewServer = false;
        }

        private async void OnServerLoginClicked(object obj)
        {
            //name - libovolné (čísla, písmena, mezery, podtržítka, pomlčka)
            //IP - veřejná IP (je potřeba psát, nebo by šla někde vytáhnout?)
            //port
            //password - libovolné, klienti ho pak musí opakovat (u existujícího serveru [jméno] to pak při správném hesle edituje hodnoty)
            string name = ServerName;
            string nick = NickName;
            string ngrokEndpoint = ServerNgrokEndpoint;
            string password = ServerPassword;
            string personalPassword = PersonalPassword;

            try
            {
                if (name == null || name.Trim() == "") throw new EntryMissingException("Jméno Serveru");
                if (ngrokEndpoint == null || ngrokEndpoint.Trim() == "") throw new EntryMissingException("Ngrok Endpoint");
                if (password == null || password.Trim() == "") throw new EntryMissingException("Serverové Heslo");
                if (personalPassword == null || personalPassword.Trim() == "") throw new EntryMissingException("Osobní Heslo");
                if (nick == null || nick.Trim() == "") throw new EntryMissingException("Přezdívka");

                if (name.Length > 100) throw new EntryTooLongException(100, "Jméno Serveru");
                if (ngrokEndpoint.Length > 100) throw new EntryTooLongException(100, "Ngrok Endpoint");
                if (password.Length > 100) throw new EntryTooLongException(100, "Serverové Heslo");
                if (personalPassword.Length > 100) throw new EntryTooLongException(100, "Osobní Heslo");
                if (nick.Length > 100) throw new EntryTooLongException(100, "Přezdívka");

                if (CommunicationInfo.Instance.Communicator != null) { CommunicationInfo.Instance.Communicator.EndCommunication(); }
                Console.WriteLine("Launching Communicator");
                new ServerCommunicator(name, ngrokEndpoint, password, personalPassword, nick, isNewServer);
                Console.WriteLine("Communicator launched");

            }
            catch (EntryMissingException e)
            {
                isNewServer = false;
                await App.Current.MainPage.DisplayAlert("Chybějící Údaj", $"Pole \"{e.Message}\" nebylo vyplněno!", "OK");
                return;
            }
            catch (EntryTooLongException e)
            {
                isNewServer = false;
                await App.Current.MainPage.DisplayAlert("Dlouhý Vstup", $"Příliš dlouhý vstup - pole \"{e.fieldName}\" může mít maximální délku {e.length} znaků!", "OK");
                return;
            }
            catch (PasswordTooShortException)
            {
                isNewServer = false;
                await App.Current.MainPage.DisplayAlert("Příliš Krátké Heslo", "Zadané heslo je příliš krátké, heslo musí mít minimálně 5 znaků.", "OK");
                return;
            }
            catch (CantConnectToCentralServerException)
            {
                isNewServer = false;
                await App.Current.MainPage.DisplayAlert("Připojení k Seznamu Serverů", "Nepodařilo se připrojit k centrálnímu seznamu serverů. Zkontrolujte internetové připojení.", "OK");
                return;
            }
            catch (WrongNgrokAddressFormatException)
            {
                isNewServer = false;
                await App.Current.MainPage.DisplayAlert("Ngrok Adresa", "Uvedená ngrok adresa není ve správném formátu - příklad validní ngrok adresy: tcp://2.tcp.eu.ngrok.io:19912", "OK");
                return;
            }
            catch (WrongCredentialsException e)
            {
                if (isNewServer)
                {
                    isNewServer = false;
                    await App.Current.MainPage.DisplayAlert("Jméno Serveru", "Server s tímto jménem už existuje. Zvolte jiné jméno, nebo se přihlašte jako existující server.", "OK");
                }
                else if (e.Message == "password")
                {
                    isNewServer = false;
                    await App.Current.MainPage.DisplayAlert("Přihlašovací Údaje", "Zadali jste špatné heslo.", "OK");
                }
                else
                {
                    isNewServer = await App.Current.MainPage.DisplayAlert("Neexistující Server", "Server s tímto jménem neexistuje. Chcete ho vytvořit?", "Ano", "Ne");
                    if (isNewServer)
                        OnServerLoginClicked(e);
                }
                return;
            }
            catch (Exception e)
            {
                isNewServer = false;
                ErrorLabel = e.ToString();
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
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}");
            }
        }
    }
}
