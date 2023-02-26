using System;
using Xamarin.Forms;
using Mapsui.UI.Forms;
using LAMA.Services;

namespace LAMA.ViewModels
{
    public class MapPageViewModel : BaseViewModel
    {
        private Func<MapView> _getMapView;
        private bool _showDropdown = false;
        public bool ShowDropdown
        {
            get => _showDropdown;
            set => SetProperty(ref _showDropdown, value, nameof(ShowDropdown));
        }
        public Command FilterDropdown { get; private set; }

        public MapPageViewModel(Func<MapView> getMapView)
        {
            FilterDropdown = new Command(SwitchFilterDropdown);
            _getMapView = getMapView;
        }

        public void SwitchFilterDropdown()
        {
            ShowDropdown = !ShowDropdown;
            if (!ShowDropdown)
                MapHandler.Instance.RefreshMapView(_getMapView());
        }
	}
}
