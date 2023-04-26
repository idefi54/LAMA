using LAMA.Communicator;
using LAMA.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClientChooseNamePage : ContentPage
    {
        private static bool newClient = true;
        private static ClientCommunicator communicator = null;

        public static void InitPage(ClientCommunicator communicator, bool newClient)
        {
            ClientChooseNamePage.communicator = communicator;
            ClientChooseNamePage.newClient = newClient;
        }

        public ClientChooseNamePage()
        {
            InitializeComponent();
            this.BindingContext = new ClientChooseNameViewModel(communicator, newClient);
        }

        public ClientChooseNamePage(ClientCommunicator communicator, bool newClient)
        {
            ClientChooseNamePage.communicator = communicator;
            ClientChooseNamePage.newClient = newClient;
            InitializeComponent();
            this.BindingContext = new ClientChooseNameViewModel(communicator, newClient);
        }
    }
}