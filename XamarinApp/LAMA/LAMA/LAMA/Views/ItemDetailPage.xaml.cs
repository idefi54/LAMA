using LAMA.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace LAMA.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}