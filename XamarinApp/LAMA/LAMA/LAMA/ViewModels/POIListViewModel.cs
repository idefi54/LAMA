using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    class POIListViewModel : BaseViewModel
    {
        INavigation navigation;

        public bool CanChange { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangePOI); } }
        TrulyObservableCollection<POIViewModel> _PointsOfInterest = new TrulyObservableCollection<POIViewModel>();
        public TrulyObservableCollection<POIViewModel> PointsOfInterest { get { return _PointsOfInterest; } set { _PointsOfInterest = value; } }

        public Command Create { get; set; }
        public Command<object> OpenDetails { get; set; }
        public Command<object> Remove { get; set; }
        public POIListViewModel(INavigation navigation)
        {
            this.navigation = navigation;
            Create = new Command(onCreate);
            OpenDetails = new Command<object>(onOpenDetails);
            Remove = new Command<object>(onRemove);

            var list = DatabaseHolder<PointOfInterest, PointOfInterestStorage>.Instance.rememberedList;

            for (int i = 0; i < list.Count; ++i)
            {
                PointsOfInterest.Add(new POIViewModel(navigation, list[i]));
            }


            SQLEvents.created += onCreated;
            SQLEvents.dataDeleted += onDeleted;
        }

        void onCreated(Serializable created)
        {
            if (created.GetType() != typeof(PointOfInterest))
                return;
            var poi = (PointOfInterest)created;
            PointsOfInterest.Add(new POIViewModel(navigation, poi));
        }
        void onDeleted(Serializable deleted)
        {
            if (deleted.GetType() != typeof(PointOfInterest))
                return;
            var poi = (PointOfInterest)deleted;
            POIViewModel item = PointsOfInterest.Where(x => x.POI.ID == poi.ID).FirstOrDefault();

            if (item != null)
                PointsOfInterest.Remove(item);
        }

        async void onCreate()
        {
            await navigation.PushAsync(new POIEditView());
        }
        async void onOpenDetails(object sender)
        {
            await navigation.PushAsync(new POIDetailsView((sender as POIViewModel).POI));
        }
        async void onRemove(object sender)
        {
            var poiViewModel = sender as POIViewModel;
            if (poiViewModel == null)
            {
                await App.Current.MainPage.DisplayAlert(
                    "Message",
                    "Object is of wrong type\n" +
                    $"Expected: {typeof(POIViewModel).Name}\n" +
                    $"Actual: {sender.GetType().Name}",
                    "OK"
                    );

                return;
            }

            bool result = await App.Current.MainPage.DisplayAlert(
                "Smazat Point of Interest",
                $"Opravdu chcete smazat Point of interest {poiViewModel.Name}?",
                "Smazat", "Zrušit");

            if (!result)
                return;

            PointsOfInterest.Remove(poiViewModel);
            DatabaseHolder<PointOfInterest, PointOfInterestStorage>.Instance
                .rememberedList.removeByID(poiViewModel.POI.ID);
        }

    }
}
