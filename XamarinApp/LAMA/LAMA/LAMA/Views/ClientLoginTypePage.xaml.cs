using LAMA.Communicator;
using LAMA.ViewModels;
using System;
using System.Collections.Generic;
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
        public ClientLoginTypePage(ClientCommunicator communicator)
        {
            InitializeComponent();
            this.BindingContext = new ClientLoginTypeViewModel(communicator);
        }
    }
}