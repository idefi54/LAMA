using LAMA.Models;
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
    public partial class EncyclopediaCategoryPage : ContentPage
    {
        public EncyclopediaCategoryPage()
        {
            InitializeComponent();
            BindingContext = new LAMA.ViewModels.EncyclopedyCategoryViewModel(null, Navigation);
        }
        public EncyclopediaCategoryPage(EncyclopedyCategory category)
        {
            InitializeComponent();
            BindingContext = new LAMA.ViewModels.EncyclopedyCategoryViewModel(category, Navigation);
        }
    }
}