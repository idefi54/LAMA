using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using LAMA.Models;
using LAMA.ViewModels;
using LAMA.Services;

using Mapsui;
using Mapsui.Projection;
using Mapsui.Utilities;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewActivityPage : ContentPage
	{
		public LarpActivity Activity { get; set; }

		public NewActivityPage()
        {
            InitializeComponent();
			BindingContext = new NewActivityViewModel();

            MapHandler.Instance.MapViewSetupAdding(mapView);
		}
    }
}