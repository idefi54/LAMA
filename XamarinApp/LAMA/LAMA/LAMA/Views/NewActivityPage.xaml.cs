using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using LAMA.Models;
using LAMA.ViewModels;

using Mapsui;
using Mapsui.Projection;
using Mapsui.Utilities;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewActivityPage : ContentPage
	{
		//public LAMA.LarpActivity Activity { get; set; }

		public NewActivityPage()
        {
            InitializeComponent();
			BindingContext = new NewActivityViewModel();

			var map = new Map
			{
				CRS = "EPSG:3857",
				Transformation = new MinimalTransformation()
			};

			var tileLayer = OpenStreetMap.CreateTileLayer();

			map.Layers.Add(tileLayer);
			map.Widgets.Add(new Mapsui.Widgets.ScaleBar.ScaleBarWidget(map) { TextAlignment = Mapsui.Widgets.Alignment.Center, HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom });

			mapView.Map = map;
		}
    }
}