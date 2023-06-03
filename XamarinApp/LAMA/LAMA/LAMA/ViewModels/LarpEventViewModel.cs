using LAMA.Models;
using LAMA.Singletons;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class LarpEventViewModel:BaseViewModel
    {
        DateTime _startDay;
        private string _startDayString;
        public string StartDay { get { return _startDay.ToShortDateString(); } set { SetProperty(ref _startDayString, value); } }

        DateTime _endDay;
        private string _endDayString;
        public string EndDay { get { return _endDay.ToShortDateString(); } set { SetProperty(ref _endDayString, value); } }

        string _name = string.Empty;
        public string Name { get { return _name; } set { SetProperty(ref _name, value);} }

        string _serverName = string.Empty;
        public string ServerName { get { return _serverName; } set { SetProperty(ref _serverName, value); } }

        public string NewChannel { get; set; }

        private bool _canChangeLarpEvent;
        public bool CanChangeLarpEvent { get => _canChangeLarpEvent; set => SetProperty(ref _canChangeLarpEvent, value); }
        public bool CanNotChangeLarpEvent { get { return !CanChangeLarpEvent; } }
        public Command BackCommand { get; set; }

        public Command SetStartDay { get; set; }
        public Command SetEndDay { get; set; }
        public Command SaveChanges { get; set; }

        INavigation navigation;
        public LarpEventViewModel(INavigation navigation)
        {
            this.navigation = navigation;
            CanChangeLarpEvent = LocalStorage.cp.permissions.Contains(Models.CP.PermissionType.ManageEvent);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < LarpEvent.ChatChannels.Count; ++i)
            {
                if(i !=0)
                    builder.Append(Environment.NewLine);
                builder.Append(LarpEvent.ChatChannels[i]);
            }

            SetStartDay = new Command(OnSetStartDay);
            SetEndDay = new Command(OnSetEndDay);
            BackCommand = new Command(onBack);
            SaveChanges = new Command(OnSaveChanges);

            ServerName = LocalStorage.serverName;
            Name = LarpEvent.Name;
            SetStart(LarpEvent.Days.first);
            SetEnd(LarpEvent.Days.second);
            SQLEvents.dataChanged += (Serializable changed, int changedAttributeIndex) =>
            {
                if (changed is CP cp && cp.ID == LocalStorage.cpID && changedAttributeIndex == 9)
                    CanChangeLarpEvent = LocalStorage.cp.permissions.Contains(Models.CP.PermissionType.ManageEvent);
            };
        }

        async void onBack()
        {
            await navigation.PopAsync();
        }

        async void OnSetStartDay()
        {
            var start = await CalendarPage.ShowCalendarPage(navigation);
            if (start > _endDay)
            {
                await App.Current.MainPage.DisplayAlert("Chyba", "Začátek musí být po konci!", "OK");
                return;
            }; 
            
            SetStart(start);
        }
        async void OnSetEndDay()
        {
            var end = await CalendarPage.ShowCalendarPage(navigation);
            if (end < _startDay)
            {
                await App.Current.MainPage.DisplayAlert("Chyba", "Konec musí být před začátkem!", "OK");
                return;
            };

            SetEnd(end);
        }

        void OnSaveChanges()
        {
            LarpEvent.Name = Name;
            LarpEvent.Days = new Pair<DateTime, DateTime>(_startDay, _endDay);
        }

        private void SetStart(DateTime time)
        {
            _startDay = time;
            StartDay = time.ToShortDateString();
        }
        private void SetEnd(DateTime time)
        {
            _endDay = time;
            EndDay = time.ToShortDateString();
        }

    }
}
