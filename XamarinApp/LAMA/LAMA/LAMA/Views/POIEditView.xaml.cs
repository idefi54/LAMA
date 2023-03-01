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
    public partial class POIEditView : ContentPage
    {
        public POIEditView()
        {
            InitializeComponent();
            BindingContext = new POIViewModel(Navigation, null);
        }
        public POIEditView(PointOfInterest poi)
        {
            InitializeComponent();
            BindingContext = new POIViewModel(Navigation, poi);
        }
    }
}