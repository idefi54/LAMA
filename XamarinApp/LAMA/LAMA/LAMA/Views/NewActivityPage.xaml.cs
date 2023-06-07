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
using LAMA.Models.DTO;

using Mapsui.UI.Forms;
using LAMA.Singletons;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewActivityPage : ContentPage
	{

        private MapView _mapView;
        private Button _expandButton;
        public LarpActivity Activity { get; set; }

		public NewActivityPage(Action<LarpActivityDTO> createNewActivity, LarpActivity activity = null)
        {
            InitializeComponent();
            Activity = activity;
			BindingContext = new NewActivityViewModel(Navigation, createNewActivity, activity);
		}

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as NewActivityViewModel).OnAppearing();

            MapHandler mapHandler = MapHandler.Instance;
            string expandText = "Zvětšit mapu";
            string hideText = "Zmenšit mapu";
            (_mapView, _expandButton) = await mapHandler.CreateAndAddMapView(
                MapLayout,
                LayoutOptions.Fill,
                LayoutOptions.Fill,
                300,
                DetailsLayout,
                expandText,
                hideText);

            if (Device.RuntimePlatform == Device.Android)
                _expandButton.Clicked += (object sender, System.EventArgs e)
                    => ScrollView.InputTransparent = ((Button)sender).Text == hideText;

            mapHandler.MapViewSetup(_mapView, showSelection: true, relocateSelection: true);

            if (Activity != null)
            {
                long id = Activity.ID;
                double lon = Activity.place.first;
                double lat = Activity.place.second;

                mapHandler.RemoveActivity(id, _mapView);
                mapHandler.SetSelectionPin(lon, lat);
                MapHandler.CenterOn(_mapView, lon, lat);
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            (BindingContext as NewActivityViewModel).OnDisappearing();
            MapHandler.Instance.RemoveMapView(_mapView, MapLayout, _expandButton);
            _mapView = null;
        }
    }
}