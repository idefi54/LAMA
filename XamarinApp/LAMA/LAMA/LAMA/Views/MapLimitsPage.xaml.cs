using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using LAMA.Services;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapLimitsPage : ContentPage
    {
        public MapLimitsPage()
        {
            InitializeComponent();

            double minExt = Math.Min(mapView.Viewport.Extent.MinX, mapView.Viewport.Extent.MinY);
            double maxExt = Math.Max(mapView.Viewport.Extent.MaxX, mapView.Viewport.Extent.MaxY);
            MapHandler.SetPanLimits(mapView, minExt, minExt, maxExt, maxExt);
        }

        async void OnSaveLimits(object sender, EventArgs e)
        {
            MapHandler.globalZoomLimit = mapView.Viewport.Resolution;
            MapHandler.globalPanLimt = mapView.Viewport.Extent;
            await Navigation.PopAsync();
        }
    }
}