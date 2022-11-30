using LAMA.Services;
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
        public CPDetailsView(Models.CP cp)
        {
            InitializeComponent();
            BindingContext = new ViewModels.CPDetailsViewModel(Navigation, cp);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            var scrollView = Content as ScrollView;
            var layout = scrollView.Children[0] as StackLayout;

            var mapHorizontalOption = LayoutOptions.Fill;
            var mapVerticalOption = LayoutOptions.Fill;
            int mapHeightRequest = 300;
            var mapBackgroundColor = Color.Gray;

            var activityIndicator = new ActivityIndicator
            {
                VerticalOptions = mapVerticalOption,
                HorizontalOptions = mapHorizontalOption,
                HeightRequest = mapHeightRequest,
                IsRunning = true,
                IsVisible = true,
            };
            layout.Children.Add(activityIndicator);

            await Task.Delay(500);
            _mapView = new MapView
            {
                VerticalOptions = mapVerticalOption,
                HorizontalOptions = mapHorizontalOption,
                HeightRequest = mapHeightRequest,
                BackgroundColor = mapBackgroundColor
            };
            MapHandler.Instance.MapViewSetup(_mapView);
            layout.Children.Remove(activityIndicator);
            layout.Children.Add(_mapView);
        }
    }
}