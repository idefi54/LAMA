﻿using LAMA.Communicator;
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
        public Xamarin.Forms.Command DatabaseNameCommand { get; }
        public Xamarin.Forms.Command ServerLoginCommand { get; }

        public string DatabaseName { get; set; }
        private string _databaseNameDisplay;
        public string DatabaseNameDisplay { get { return _databaseNameDisplay; } set { SetProperty(ref _databaseNameDisplay, value); } }


        public string ServerName { get; set; }
        public string ServerNgrokEndpoint { get; set; }
        //public string ServerIP { get; set; }
        //public string ServerPort { get; set; }
        public string ServerPassword { get; set; }

        public string NickName { get; set; }

        public string PersonalPassword { get; set; }

        private string _errorLabel;
        public string ErrorLabel { get { return _errorLabel; } set { SetProperty(ref _errorLabel, value); } }

        private bool isNewServer;

        public ServerLoginViewModel(bool newServer)
        {
            ServerLoginCommand = new Xamarin.Forms.Command(OnServerLoginClicked);
            DatabaseNameCommand = new Xamarin.Forms.Command(OnChangeDatabaseName);
            DatabaseNameDisplay = $"Database Name: {SQLConnectionWrapper.databaseName}";
            isNewServer = newServer;
        }

        private void OnChangeDatabaseName()
        {
            SQLConnectionWrapper.databaseName = DatabaseName;
            if (SQLConnectionWrapper.connection == null)
            {
                DatabaseNameDisplay = $"Database Name: {DatabaseName}";
            }
        }

        private async void OnServerLoginClicked(object obj)
        {
            string name = ServerName;
            string nick = NickName;
            //string IP = ServerIP;
            string ngrokEndpoint = ServerNgrokEndpoint;
            string password = ServerPassword;
            string personalPassword = PersonalPassword;
            //int port;
            /*
            try
            {
                port = int.Parse(ServerPort);
            }
            catch (Exception e)
            {
                ErrorLabel = e.ToString();
                return;
            }
            */

            //name - libovolné (čísla, písmena, mezery, podtržítka, pomlčka)
            //IP - veřejná IP (je potřeba psát, nebo by šla někde vytáhnout?)
            //port
            //password - libovolné, klienti ho pak musí opakovat (u existujícího serveru [jméno] to pak při správném hesle edituje hodnoty)
            try
            {
                Console.WriteLine("Launching Communicator");
                new ServerCommunicator(name, ngrokEndpoint, password, personalPassword, nick, isNewServer);
                //new ServerCommunicator(name, IP, port, password);
                Console.WriteLine("Communicator launched");

            }
            catch (Exception e)
            {
                ErrorLabel = e.ToString();
                return;
            }

            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            if (Device.RuntimePlatform == Device.WPF)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new MapPage());
                //LAMA.App.Current.MainPage = new MapPage();
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}");
            }
        }
    }
}
