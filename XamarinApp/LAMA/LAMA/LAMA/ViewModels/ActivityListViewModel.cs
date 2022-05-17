using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class ActivityListViewModel
    {

        public Xamarin.Forms.Command AddActivityCommand { get; }
        public ObservableCollection<ActivityListItemViewModel> LarpActivityListItems { get; }

        INavigation Navigation;

        public ActivityListViewModel(INavigation navigation)
        {
            Navigation = navigation;

            LarpActivityListItems = new ObservableCollection<ActivityListItemViewModel>();

            //LarpActivity activity = new LarpActivity() { name = "Příprava přepadu", start = new Time(60 * 12 + 45), eventType = LarpActivity.EventType.preparation };

            LarpActivity larpActivity = new LarpActivity(0, "Příprava přepadu", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
                new Time(60 + 30), 0, new Time(60 * 12 + 45), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));
            
            larpActivity = new LarpActivity(0, "Přepad karavanu", "", "", LarpActivity.EventType.normal, new EventList<int>(),
                new Time(60 + 30), 0, new Time(60 * 14 + 15), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));
            
            larpActivity = new LarpActivity(0, "Příprava záchrany", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
                new Time(60 + 30), 0, new Time(60 * 14), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));
            
            larpActivity = new LarpActivity(0, "Záchrana kupce", "", "", LarpActivity.EventType.normal, new EventList<int>(),
                new Time(60 + 30), 0, new Time(60 * 16 + 10), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            larpActivity = new LarpActivity(0, "Úklid mrtvol hráčů", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
                new Time(60 + 30), 0, new Time(60 * 16 + 15), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            AddActivityCommand = new Xamarin.Forms.Command(OnAddActivityListItem);

            //LarpActivityListItems.Add(new LarpActivity() { name = "Příprava přepadu", start = new Time(60 * 12 + 45), eventType = LarpActivity.EventType.preparation });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Přepad karavanu", start = new Time(60 * 14 + 15), eventType = LarpActivity.EventType.normal });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Příprava záchrany", start = new Time(60 * 14), eventType = LarpActivity.EventType.preparation });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Záchrana kupce", start = new Time(60 * 16 + 10), eventType = LarpActivity.EventType.normal });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Úklid mrtvol hráčů", start = new Time(60 * 16 + 15), eventType = LarpActivity.EventType.preparation });
        }

        private async void OnAddActivityListItem(object obj)
        {
            //await Shell.Current.GoToAsync(nameof(NewActivityPage));

            await Navigation.PushAsync(new NewActivityPage());
            
            //LarpActivity larpActivity = new LarpActivity(0, "New Activity", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
            //    new Time(0), 0, new Time(0), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //LarpActivityListItems.Add(larpActivity);
        }
    }
}
