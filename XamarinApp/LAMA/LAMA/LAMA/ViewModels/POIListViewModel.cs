using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    class POIListViewModel:BaseViewModel
    {
        INavigation navigation;

        public Command Create { get; set; }
        public bool CanCreate { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangePOI); }  }
        TrulyObservableCollection<POIViewModel> _PointsOfInterest = new TrulyObservableCollection<POIViewModel>();
        public TrulyObservableCollection<POIViewModel> PointsOfInterest { get { return _PointsOfInterest; } set { _PointsOfInterest = value; } }
        public Command<object> OpenDetails { get; set; }

        public POIListViewModel(INavigation navigation)
        {
            this.navigation = navigation;
            Create = new Command(onCreate);
            OpenDetails = new Command<object>(onOpenDetails);

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
            PointsOfInterest.Add(new POIViewModel(navigation, poi));
        }


        async void onCreate()
        {
            await navigation.PushAsync(new POIEditView());
        }
        async void onOpenDetails(object sender)
        {
            await navigation.PushAsync(new POIDetailsView((sender as POIViewModel).POI));
        }

    }
}
