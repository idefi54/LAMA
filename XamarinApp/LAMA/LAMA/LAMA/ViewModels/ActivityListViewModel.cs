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

        public Command<object> LarpActivityTapped { get; private set; }

        public Command<object> RemoveLarpActivity { get; private set; }

        INavigation Navigation;

        int maxId = 0;

        public ActivityListViewModel(INavigation navigation)
        {
            Navigation = navigation;

            LarpActivityListItems = new ObservableCollection<ActivityListItemViewModel>();

            for (int i = 0; i < DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.Count; i++)
            {
                LarpActivityListItems.Add(new ActivityListItemViewModel(DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList[i]));
            }

            foreach(ActivityListItemViewModel item in LarpActivityListItems)
            {
                if(item.LarpActivity.ID > maxId)
                    maxId = item.LarpActivity.ID;
            }

            //LarpActivity activity = new LarpActivity() { name = "Příprava přepadu", start = new Time(60 * 12 + 45), eventType = LarpActivity.EventType.preparation };

            //LarpActivity larpActivity = new LarpActivity(0, "Příprava přepadu", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
            //    new Time(60 + 30), 0, new Time(60 * 12 + 45), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            //LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            //larpActivity = new LarpActivity(1, "Přepad karavanu", "", "", LarpActivity.EventType.normal, new EventList<int>(),
            //    new Time(60 + 30), 0, new Time(60 * 14 + 15), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            //LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            //larpActivity = new LarpActivity(2, "Příprava záchrany", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
            //    new Time(60 + 30), 0, new Time(60 * 14), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            //LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            //larpActivity = new LarpActivity(3, "Záchrana kupce", "", "", LarpActivity.EventType.normal, new EventList<int>(),
            //    new Time(60 + 30), 0, new Time(60 * 16 + 10), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            //LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            //larpActivity = new LarpActivity(4, "Úklid mrtvol hráčů", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
            //    new Time(60 + 30), 0, new Time(60 * 16 + 15), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            //LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            AddActivityCommand = new Xamarin.Forms.Command(OnAddActivityListItem);
            LarpActivityTapped = new Command<object>(DisplayActivity);
            RemoveLarpActivity = new Command<object>(RemoveActivity);

            //LarpActivityListItems.Add(new LarpActivity() { name = "Příprava přepadu", start = new Time(60 * 12 + 45), eventType = LarpActivity.EventType.preparation });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Přepad karavanu", start = new Time(60 * 14 + 15), eventType = LarpActivity.EventType.normal });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Příprava záchrany", start = new Time(60 * 14), eventType = LarpActivity.EventType.preparation });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Záchrana kupce", start = new Time(60 * 16 + 10), eventType = LarpActivity.EventType.normal });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Úklid mrtvol hráčů", start = new Time(60 * 16 + 15), eventType = LarpActivity.EventType.preparation });
        }

        private async void DisplayActivity(object obj)
        {
            if (obj.GetType() != typeof(ActivityListItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(ActivityListItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            ActivityListItemViewModel activityViewModel = (ActivityListItemViewModel)obj;

            LarpActivity activity = activityViewModel.LarpActivity;

            await Navigation.PushAsync(new DisplayActivityPage(activity));
        }

        private async void RemoveActivity(object obj)
        {
            if (obj.GetType() != typeof(ActivityListItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(ActivityListItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            ActivityListItemViewModel activityViewModel = (ActivityListItemViewModel)obj;

            bool result = await App.Current.MainPage.DisplayAlert("Smazat aktivitu", "Opravdu chcete smazat aktivitu " + activityViewModel.LarpActivity.name + ".", "Smazat", "zrušit");

            if(result)
            {
                LarpActivityListItems.Remove(activityViewModel);
                DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.removeByID(activityViewModel.LarpActivity.getID());
            }
        }

        private async void OnAddActivityListItem(object obj)
        {
            //await Shell.Current.GoToAsync(nameof(NewActivityPage));

            await Navigation.PushAsync(new NewActivityPage(CreateNewActivity));
            
            //LarpActivity larpActivity = new LarpActivity(0, "New Activity", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
            //    new Time(0), 0, new Time(0), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //LarpActivityListItems.Add(larpActivity);
        }

        private void CreateNewActivity(LarpActivity larpActivity)
        {
            //larpActivity.ID = ++maxId;

            LarpActivity activity = new LarpActivity(++maxId, larpActivity.name, larpActivity.description, larpActivity.preparationNeeded, larpActivity.eventType,
                larpActivity.prerequisiteIDs, larpActivity.duration, larpActivity.day, larpActivity.start, larpActivity.place, larpActivity.status,
                larpActivity.requiredItems, larpActivity.roles, larpActivity.registrationByRole);

            LarpActivityListItems.Add(new ActivityListItemViewModel(activity));
            DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.add(activity);
        }
    }
}
