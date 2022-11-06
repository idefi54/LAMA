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
    public partial class EncyclopedyCategoryView : ContentPage
    {
        public EncyclopedyCategoryView()
        {
            InitializeComponent();
            BindingContext = new LAMA.ViewModels.EncyclopedyCategoryViewModel(null);
        }
        public EncyclopedyCategoryView(EncyclopedyCategory category)
        {
            InitializeComponent();
            BindingContext = new LAMA.ViewModels.EncyclopedyCategoryViewModel(category);
        }
    }
}