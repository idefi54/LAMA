﻿using LAMA.Models;
using LAMA.Services;
using LAMA.Singletons;
using LAMA.ViewModels;
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
    public partial class DisplayActivityPage : ContentPage
    {
        private MapView _mapView;
        private LarpActivity _activity;
        private Button _expandButton;

        public DisplayActivityPage(LarpActivity activity)
        {
            InitializeComponent();
            BindingContext = new DisplayActivityViewModel(Navigation,activity);
            _activity = activity;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            MapHandler mapHandler = MapHandler.Instance;
            (_mapView, _expandButton) = await mapHandler.CreateAndAddMapView(MapLayout, LayoutOptions.Fill, LayoutOptions.Fill, 300, DetailsLayout);
            mapHandler.MapViewSetup(_mapView, showSelection: true, relocateSelection: false);

            if (_activity != null)
            {
                long id = _activity.ID;
                double lon = _activity.place.first;
                double lat = _activity.place.second;

                mapHandler.RemoveActivity(id, _mapView);
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