using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAMA.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CPListPage : ContentPage
    {
        public CPListPage()
        {
            InitializeComponent();
            BindingContext = new CPListViewModel(Navigation);
        }
    }
}