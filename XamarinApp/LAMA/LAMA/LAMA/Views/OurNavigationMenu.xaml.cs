using LAMA.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OurNavigationMenu : StackLayout
    {
        public OurNavigationMenu()
        {
            InitializeComponent();
            this.BindingContext = new OurNavigationMenuViewModel();
        }
    }
}