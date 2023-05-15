using LAMA.Services;
using LAMA.Singletons;
using Mapsui.UI.Forms;
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
    public partial class CPDetailsView : ContentPage
    {
        private MapView _mapView;
        private Button _expandButton;
        private Models.CP _cp;
        public CPDetailsView(Models.CP cp)
        {
            InitializeComponent();
            BindingContext = new ViewModels.CPDetailsViewModel(Navigation, cp);
            _cp = cp;
        }
        
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            MapHandler mapHandler = MapHandler.Instance;
            (_mapView, _expandButton) = await mapHandler.CreateAndAddMapView(MapLayout, LayoutOptions.Fill, LayoutOptions.Fill, 300, DetailsLayout);
            mapHandler.MapViewSetup(_mapView, showSelection: true, relocateSelection: false);

            if (_cp != null)
            {
                long id = _cp.ID;
                double lon = _cp.location.first;
                double lat = _cp.location.second;

                mapHandler.RemoveCP(id, _mapView);
                mapHandler.SetSelectionPin(lon, lat);
                MapHandler.CenterOn(_mapView, lon, lat);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MapHandler.Instance.RemoveMapView(_mapView, MapLayout, _expandButton);
            _mapView = null;
        }
        
    }
}