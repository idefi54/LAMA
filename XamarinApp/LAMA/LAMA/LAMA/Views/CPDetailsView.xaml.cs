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
    public partial class CPDetailsView : ContentPage
    {
        public CPDetailsView(Models.CP cp)
        {
            InitializeComponent();
            BindingContext = new ViewModels.CPDetailsViewModel(Navigation, cp);
        }
    }
}