using LAMA.Models;
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
    }
}