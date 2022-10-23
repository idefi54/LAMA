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
    public partial class ActivitySelectionPage : ContentPage
    {
        public ActivitySelectionPage(Action<LarpActivity> callback, Func<LarpActivity, bool> filter = null)
        {
            InitializeComponent();

            BindingContext = new ActivitySelectionViewModel(callback, filter is null ? (_) => { return true; } : filter);
        }
    }
}