using LAMA.Models;
using LAMA.Services;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    class POIViewModel : BaseViewModel, INotifyPropertyChanged
    {
        INavigation navigation;
        public PointOfInterest POI;

        string name = string.Empty;
        public string Name { get { return name; } set { SetProperty(ref name, value); } }
        string description = string.Empty;
        public string Description { get { return description; } set { SetProperty(ref description, value); } }

        public bool Icon0Checked { get; set; }
        public bool Icon1Checked { get; set; }
        public bool Icon2Checked { get; set; }
        public bool Icon3Checked { get; set; }

        public Command Save { get; set; }
        public Command Cancel { get; set; }
        public Command Edit { get; set; }
        public bool CanChange { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangePOI); } }
        public POIViewModel(INavigation navigation, PointOfInterest POI)
        {
            this.navigation = navigation;
            this.POI = POI;

            if (POI != null)
            {
                POI.IGotUpdated += onChange;
                Name = POI.Name;
                Description = POI.Description;
            }
            Save = new Command(onSave);
            Cancel = new Command(onCancel);
            Edit = new Command(onEdit);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void onChange(object sender, int index)
        {
            string propName = "";

            switch (index)
            {
                case 3:
                    propName = nameof(Name);
                    Name = POI.Name;
                    break;
                case 4:
                    propName = nameof(Description);
                    Description = POI.Description;
                    break;
                default:
                    return;
            }

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        async void onSave()
        {
            if (POI != null)
            {
                POI.Name = name;
                POI.description = description;
                POI.Icon = GetIcon();
            }
            else
            {
                var list = DatabaseHolder<PointOfInterest, PointOfInterestStorage>.Instance.rememberedList;
                (double lon, double lat) = MapHandler.Instance.GetSelectionPin();
                list.add(new PointOfInterest(list.nextID(), new Pair<double, double>(lon, lat), GetIcon(), name, description));
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

        private int GetIcon()
        {
            if (Icon0Checked) return 0;
            if (Icon1Checked) return 1;
            if (Icon2Checked) return 2;
            if (Icon3Checked) return 3;
            return 0;
        }
    }
}
