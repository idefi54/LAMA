using LAMA.Models;
using LAMA.Services;
using LAMA.Themes;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    class POIViewModel : BaseViewModel
    {
        INavigation navigation;
        public PointOfInterest POI;

        string name = string.Empty;
        public string Name { get { return name; } set { SetProperty(ref name, value); } }
        string description = string.Empty;
        public string Description { get { return description; } set { SetProperty(ref description, value); } }

        private string[] _icons;
        private int _currentIconIndex;
        private int CurrentIconIndex
        {
            get => _currentIconIndex;
            set
            {
                _currentIconIndex = value;
                CurrentIcon = IconLibrary.GetImageSourceFromResourcePath(_icons[value]);
            }
        }
        private ImageSource _currentIcon;
        public ImageSource CurrentIcon
        {
            get
            {
                return _currentIcon;
            }
            set
            {
                SetProperty(ref _currentIcon, value);
            }
        }

        public Command Save { get; set; }
        public Command Cancel { get; set; }
        public Command Edit { get; set; }
        public Command IconChange { get; set; }
        public bool CanChange { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangePOI); } }
        public POIViewModel(INavigation navigation, PointOfInterest POI)
        {
            this.navigation = navigation;
            this.POI = POI;
            // Icons need to be assigned before assigning icon index
            _icons = IconLibrary.GetIconsByClass<PointOfInterest>();
            CurrentIconIndex = 0;

            if (POI != null)
            {
                Name = POI.Name;
                Description = POI.Description;
                CurrentIconIndex = POI.Icon;
                POI.IGotUpdated += onPOIChanged;
            }

            Save = new Command(onSave);
            Cancel = new Command(onCancel);
            Edit = new Command(onEdit);
            IconChange = new Command(onIconChange);
        }

        private void onPOIChanged(object sender, int e)
        {
            var poi = sender as PointOfInterest;

            switch (e)
            {
                case 2:
                    CurrentIconIndex = poi.Icon;
                    break;
                case 3:
                    Name = poi.Name;
                    break;
                case 4:
                    Description = poi.Description;
                    break;
            }
        }

        async void onSave()
        {
            if (POI != null)
            {
                POI.Name = name;
                POI.description = description;
                POI.Icon = CurrentIconIndex;
            }
            else
            {
                var list = DatabaseHolder<PointOfInterest, PointOfInterestStorage>.Instance.rememberedList;
                (double lon, double lat) = MapHandler.Instance.GetSelectionPin();
                list.add(new PointOfInterest(list.nextID(), new Pair<double, double>(lon, lat), CurrentIconIndex, name, description));
            }
            await navigation.PopAsync();
        }
        async void onCancel()
        {
            await navigation.PopAsync();
        }
        async void onEdit()
        {
            await navigation.PushAsync(new POIEditView(POI));
        }
        async void onIconChange()
        {
            CurrentIconIndex = await IconSelectionPage.ShowIconSelectionPage(navigation, _icons);
        }
    }
}
