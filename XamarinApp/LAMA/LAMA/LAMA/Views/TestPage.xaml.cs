using LAMA.Models;
using LAMA.Models.DTO;
using LAMA.Services;
using LAMA.ViewModels;
using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
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
                LarpActivity.EventType.normal, new EventList<long>(),
                999, 0, 666, new Pair<double, double>(0, 0), LarpActivity.Status.launched,
                new EventList<Pair<long, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<long, string>>());

            var me = DatabaseHolder<CP, CPStorage>.Instance.rememberedList.getByID(666);
            if (me == null)
            {
                me = new CP(666, "me", "test", new EventList<string>(), "", "", "", "");
                DatabaseHolder<CP, CPStorage>.Instance.rememberedList.add(me);

            }

            me.MakeMeAdmin();
            

            LocalStorage.cpID = 666;

            if (Device.RuntimePlatform == Device.Android)
            {
                MessagingCenter.Subscribe<LocationMessage>(this, "Location",
                    message =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            var cpList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;
                            cpList.getByID(LocalStorage.cpID).location = new Pair<double, double>(message.Longitude, message.Latitude);

                        });
                    });

                MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped",
                    message =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            // What now?
                            Debug.Print("LOCATION STOPPED");
                        });
                    });

                MessagingCenter.Subscribe<LocationErrorMessage>(this, "LocationError",
                    message =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Debug.Print("There was an error updating location");
                        });
                    });
            }
        }

        async void OnDisplayActivity(object sender, EventArgs args)
        {
            //await Shell.Current.GoToAsync($"//{nameof(DisplayActivityPage)}");
            await Navigation.PushAsync(new DisplayActivityPage(activity));
        }

        async void OnNewActivity(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new NewActivityPage(CreateActivity));
        }

        async void OnEditActivity(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new NewActivityPage(DummyUpdateActivity, activity));
        }
        async void OnInventory(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new InventoryView());
        }
        async void OnEncyclopedy(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new EncyclopedyCategoryView(null));
        }
        async void OnCP(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new CPListView());
        }

        async void OnActivitySelector(object sender, EventArgs args)
        {
            ActivitySelectionPage page = new ActivitySelectionPage(_displayName);
            await Navigation.PushAsync(page);


            void _displayName(LarpActivity activity)
            {

                if (activity != null)
                    _ = DisplayAlert("Activity", activity.name, "OK");
                else
                    _ = DisplayAlert("Problem", "Something went wrong and no activity is present.", "BUMMER");


            }
        }
        void OnResetDatabase(object sender, EventArgs args)
        {
            SQLConnectionWrapper.ResetDatabase();
        }
        async void OnHideExample(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new HideExamplePage());
        }


        async void OnDropdownOverExample(object sender, EventArgs args)
		{
            await Navigation.PushAsync(new DropdownMenuOverTestPage());
		}


        async void OnDropdownAboveExample(object sender, EventArgs args)
		{
            await Navigation.PushAsync(new DropdownMenuAboveTestPage());
		}


        async void OnImageExample(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new ExampleImagePage());
        }

        void SwitchToAdmin(object sender, EventArgs args)
		{
            int id = 0;
            var admin = DatabaseHolder<CP, CPStorage>.Instance.rememberedList.getByID(id);
            if (admin == null)
            {
                admin = new CP(id, "The Great and Powerful One", "admin", new EventList<string>(), "", "", "", "");
                DatabaseHolder<CP, CPStorage>.Instance.rememberedList.add(admin);

            }
            if (!admin.permissions.Contains(CP.PermissionType.SetPermission))
                admin.permissions.Add(CP.PermissionType.SetPermission);
            if (!admin.permissions.Contains(CP.PermissionType.ChangeEncyclopedy))
                admin.permissions.Add(CP.PermissionType.ChangeEncyclopedy);
            if (!admin.permissions.Contains(CP.PermissionType.ManageInventory))
                admin.permissions.Add(CP.PermissionType.ManageInventory);
            if (!admin.permissions.Contains(CP.PermissionType.ChangeActivity))
                admin.permissions.Add(CP.PermissionType.ChangeActivity);
            if (!admin.permissions.Contains(CP.PermissionType.ChangeCP))
                admin.permissions.Add(CP.PermissionType.ChangeCP);
            if (!admin.permissions.Contains(CP.PermissionType.EditMap))
                admin.permissions.Add(CP.PermissionType.EditMap);
            if (!admin.permissions.Contains(CP.PermissionType.EditGraph))
                admin.permissions.Add(CP.PermissionType.EditGraph);

            LocalStorage.cpID = id;

            DisplayAlert("Current user:", admin.name, "ok");
        }

        void SwitchToUser(object sender, EventArgs args)
        {
            int id = 117;
            var user = DatabaseHolder<CP, CPStorage>.Instance.rememberedList.getByID(id);
            if (user == null)
            {
                user = new CP(id, "John 117", "john117", new EventList<string>(), "", "", "", "");
                DatabaseHolder<CP, CPStorage>.Instance.rememberedList.add(user);

            }
            if (user.permissions.Contains(CP.PermissionType.ChangeEncyclopedy))
                user.permissions.Remove(CP.PermissionType.ChangeEncyclopedy);
            if (user.permissions.Contains(CP.PermissionType.ManageInventory))
                user.permissions.Remove(CP.PermissionType.ManageInventory);
            if (user.permissions.Contains(CP.PermissionType.ChangeActivity))
                user.permissions.Remove(CP.PermissionType.ChangeActivity);
            if (user.permissions.Contains(CP.PermissionType.ChangeCP))
                user.permissions.Remove(CP.PermissionType.ChangeCP);
            if (user.permissions.Contains(CP.PermissionType.EditMap))
                user.permissions.Remove(CP.PermissionType.EditMap);

            LocalStorage.cpID = id;

            DisplayAlert("Current user:", user.name, "ok");
        }
        async void OnLarpEvent(object sender, EventArgs args)
        {
            await Navigation.PushAsync(new LarpEventView());
        }

        async void OnIconSelection(object sender, EventArgs args)
        {
            string icon = await IconSelectionPage.ShowIconSelectionPage(Navigation,
                new string[] {
                "LAMA.Resources.Icons.accept_1.png",
                "LAMA.Resources.Icons.accept_cr.png",
                "LAMA.Resources.Icons.add_sq.png",
                "LAMA.Resources.Icons.aim_1.png",
                "LAMA.Resources.Icons.arrow_cr.png",
                "LAMA.Resources.Icons.bag_3_open.png",
                "LAMA.Resources.Icons.box.png",
                "LAMA.Resources.Icons.calendar.png",
                });
        }


        void onAddPermissionsPermission(object sender, EventArgs args)
        {
            var me = DatabaseHolder<CP, CPStorage>.Instance.rememberedList.getByID(666);
            if (me.permissions.Contains(CP.PermissionType.SetPermission))
                me.permissions.Add(CP.PermissionType.SetPermission);
        }
        void onRemovePermissionspermission(object sender, EventArgs args)
        {
            var me = DatabaseHolder<CP, CPStorage>.Instance.rememberedList.getByID(666);
            if (me.permissions.Contains(CP.PermissionType.SetPermission))
                me.permissions.Remove(CP.PermissionType.SetPermission);
        }


        private void CreateActivity(LarpActivityDTO larpActivity)
        {
            larpActivity.ID = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.nextID();
            LarpActivity newActivity = larpActivity.CreateLarpActivity();

            DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.add(newActivity);
            activity = newActivity;
        }

        private void DummyUpdateActivity(LarpActivityDTO larpActivity)
        {
            activity = larpActivity.CreateLarpActivity();
        }

        protected async override void OnAppearing()
        {
            //WPF doesn't have permissions
            if (Device.RuntimePlatform != Device.WPF)
            {
                var permission = await Permissions.RequestAsync<Permissions.LocationAlways>();

                if (permission == PermissionStatus.Denied)
                {
                    // TODO Let the user know they need to accept
                    return;
                }
            }
            if (Device.RuntimePlatform == Device.Android)
            {
                if (!Preferences.Get("LocationServiceRunning", false))
                    StartService();
                else
                    StopService();
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                if (CrossGeolocator.Current.IsListening)
                {
                    await CrossGeolocator.Current.StopListeningAsync();
                    CrossGeolocator.Current.PositionChanged -= IOSPositionChanged;
                    return;
                }

                await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(30), 10);
                CrossGeolocator.Current.PositionChanged += IOSPositionChanged;


                DependencyService.Get<INotificationManager>().SendNotification("Location",
                    "We are tracking your location to show others where you are.");
            }

            base.OnAppearing();
        }

        private void IOSPositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            var cpList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;
            cpList[LocalStorage.cpID].location = new Pair<double, double>(e.Position.Longitude, e.Position.Latitude);
        }

        private void StartService()
        {
            var startServiceMessage = new StartServiceMessage();
            MessagingCenter.Send(startServiceMessage, "ServiceStarted");
            Preferences.Set("LocationServiceRunning", true);
        }

        private void StopService()
        {
            var stopServiceMessage = new StopServiceMessage();
            MessagingCenter.Send(stopServiceMessage, "ServiceStopped");
            Preferences.Set("LocationServiceRunning", false);
        }

    }
}