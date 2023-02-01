using LAMA.Communicator;
using LAMA.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClientChooseNamePage : ContentPage
    {
        public ClientChooseNamePage(ClientCommunicator communicator, bool newClient)
        {
            InitializeComponent();
            this.BindingContext = new ClientChooseNameViewModel(communicator, newClient);
        }
    }
}