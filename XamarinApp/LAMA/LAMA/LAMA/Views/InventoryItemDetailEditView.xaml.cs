using LAMA.Models;
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
    public partial class InventoryItemDetailEditView : ContentPage
    {
        public InventoryItemDetailEditView(InventoryItem item)
        {
            InitializeComponent();
            BindingContext = new InventoryItemDetailEditViewModel(Navigation, item);
        }
    }
}