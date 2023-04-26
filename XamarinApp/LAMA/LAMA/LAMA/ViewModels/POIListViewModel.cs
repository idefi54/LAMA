using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
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
        public TrulyObservableCollection<POIViewModel> PointsOfInterest { get; set; }
        public Command<object> OpenDetails { get; set; }

        public POIListViewModel(INavigation navigation)
        {
            this.navigation = navigation;
            Create = new Command(onCreate);
            OpenDetails = new Command<object>(onOpenDetails);
        }

        async void onCreate()
        {
            await navigation.PushAsync(new POIEditView());
        }
        async void onOpenDetails(object sender)
        {
            navigation.PushAsync(new POIDetailsView((sender as POIViewModel).POI));
        }

    }
}
