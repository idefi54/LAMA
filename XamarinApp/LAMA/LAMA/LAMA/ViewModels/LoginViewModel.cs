using LAMA.Communicator;
using LAMA.Database;
using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Xamarin.Forms.Command DatabaseNameCommand { get; }
        public Xamarin.Forms.Command LoginCommand { get; }
        public Xamarin.Forms.Command ServerLoginCommand { get; }
        public Xamarin.Forms.Command FakeLoginCommand { get; }

        public string DatabaseName { get; set; }
        private string _databaseNameDisplay;
        public string DatabaseNameDisplay { get { return _databaseNameDisplay; } set { SetProperty(ref _databaseNameDisplay, value); } }
        public string ClientServerName { get; set; }
        public string ClientName { get; set; }
        public string ClientPassword { get; set; }

        public string ServerName { get; set; }
        public string ServerNgrokEndpoint { get; set; }
        //public string ServerIP { get; set; }
        //public string ServerPort { get; set; }
        public string ServerPassword { get; set; }
        public string ServerAdminPassword { get; set; }

        private string _errorLabel;
        public string ErrorLabel { get { return _errorLabel; } set { SetProperty(ref _errorLabel, value); } }

        public LoginViewModel()
        {
            LoginCommand = new Xamarin.Forms.Command(OnLoginClicked);
            FakeLoginCommand = new Xamarin.Forms.Command(OnFakeLoginClicked);
            ServerLoginCommand = new Xamarin.Forms.Command(OnServerLoginClicked);
            DatabaseNameCommand = new Xamarin.Forms.Command(OnChangeDatabaseName);
            DatabaseNameDisplay = $"Database Name: {SQLConnectionWrapper.databaseName}";
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
            string clientName = ClientName;


            //možnost vybrat
            try
            {
                //serverName - méno toho serveru (identifikátor)
                //heslo serveru
                //clentName
                new ClientCommunicator(serverName, password, clientName);

            }
            catch (Exception e)
            {
                ErrorLabel = e.ToString();
                return;
            }
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            if (Device.RuntimePlatform == Device.WPF)
            {
                await App.Current.MainPage.Navigation.PushAsync(new MapPage());
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}");
            }
        }

        private async void OnServerLoginClicked(object obj)
        {
            string name = ServerName;
            //string IP = ServerIP;
            string ngrokEndpoint = ServerNgrokEndpoint;
            string password = ServerPassword;
            string adminPassword = ServerAdminPassword;
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
                new ServerCommunicator(name, ngrokEndpoint, password, adminPassword, name, true);
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

        private async void OnFakeLoginClicked(object obj)
        {
            if (DatabaseHolder<CP, CPStorage>.Instance.rememberedList.Count == 0)
                DatabaseHolder<CP, CPStorage>.Instance.rememberedList.add(new CP(0, "Inco Gnito", "incognito", new EventList<string>(), "123456789", "", "", ""));

            LocalStorage.cpID = (int)DatabaseHolder<CP, CPStorage>.Instance.rememberedList[0].ID;

            if (Device.RuntimePlatform == Device.WPF)
            {
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
