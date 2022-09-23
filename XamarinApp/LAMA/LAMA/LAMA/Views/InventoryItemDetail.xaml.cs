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
    public partial class InventoryItemDetail : ContentPage
    {
        public InventoryItemDetail(Models.InventoryItem item)
        {
            InitializeComponent();
            BindingContext = new ViewModels.InventoryItemDetailViewModel(Navigation, item);
        }
    }
}