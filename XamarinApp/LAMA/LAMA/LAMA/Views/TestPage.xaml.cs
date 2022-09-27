using LAMA.Models;
using LAMA.Models.DTO;
using LAMA.ViewModels;
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
    public partial class TestPage : ContentPage
    {
        LarpActivity activity;

        public TestPage()
        {
            InitializeComponent();

            activity = new LarpActivity(4, "Testování Xamarin Aplikace",
                "Je potřeba otestovat zda aplikace funguje.\nProjít každou funkcionalitu a zda tam nejsou chyby.",
                "Udělat aplikaci.\nZaplnit ji daty.\nVyrazit do přírody.",
                LarpActivity.EventType.normal, new EventList<int>(),
                new Time(60 + 30), 0, new Time(60 * 16 + 15), new Pair<double, double>(0, 0), LarpActivity.Status.launched,
                new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());

        }

        async void OnDisplayActivity(object sender, EventArgs args)
        {
            //await Shell.Current.GoToAsync($"//{nameof(DisplayActivityPage)}");
            await Navigation.PushAsync(new DisplayActivityPage(activity));
        }

        async void OnNewActivity(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new NewActivityPage(DummyUpdateActivity));
        }

        async void OnEditActivity(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new NewActivityPage(DummyUpdateActivity, activity));
        }
        async void OnInventory (object sender, EventArgs args)
        {
            await Navigation.PushAsync(new InventoryView());
        }
        
        async void OnActivitySelector(object sender, EventArgs args)
        {
			ActivitySelectionPage page = new ActivitySelectionPage(_displayName);
            await Navigation.PushAsync(page);
            

            void _displayName(LarpActivity activity)
			{

                if(activity != null)
			        _ = DisplayAlert("Activity", activity.name, "OK");
                else
			        _ = DisplayAlert("Problem", "Something went wrong and no activity is present.", "BUMMER");


			}
        }
        async void OnInventory (object sender, EventArgs args)
        {
            await Navigation.PushAsync(new InventoryView());
        }

        private void DummyUpdateActivity(LarpActivityDTO larpActivity)
        {
            //activity.name = larpActivity.name;
            //activity.description = larpActivity.description;


            //activity.duration = larpActivity.duration;
            //activity.start = larpActivity.start;
            //activity.day = larpActivity.day;
            //activity.preparationNeeded = larpActivity.preparationNeeded;
            //activity.place = larpActivity.place;



            //activity = new LarpActivity(activity.ID, larpActivity.name, larpActivity.description, larpActivity.preparationNeeded, larpActivity.eventType,
            //    larpActivity.prerequisiteIDs, larpActivity.duration, larpActivity.day, larpActivity.start, larpActivity.place, larpActivity.status,
            //    larpActivity.requiredItems, larpActivity.roles, larpActivity.registrationByRole);

            activity = larpActivity.CreateLarpActivity();
        }
    }
}