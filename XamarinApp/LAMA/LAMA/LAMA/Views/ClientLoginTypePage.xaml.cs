using LAMA.Communicator;
using LAMA.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClientLoginTypePage : ContentPage
    {
        private static ClientLoginTypeViewModel viewModel;

        public static void InitPage(ClientCommunicator communicator)
        {
            viewModel = new ClientLoginTypeViewModel(communicator);
        }

        public ClientLoginTypePage()
        {
            InitializeComponent();
            this.BindingContext = viewModel;
        }
        public ClientLoginTypePage(ClientCommunicator communicator)
        {
            InitializeComponent();
            this.BindingContext = new ClientLoginTypeViewModel(communicator);
        }
    }
}