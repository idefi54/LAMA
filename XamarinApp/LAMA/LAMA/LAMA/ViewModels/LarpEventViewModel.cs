﻿using LAMA.Singletons;
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
        string _startDay = string.Empty;
        public string StartDay { get { return _startDay; } set { SetProperty(ref _startDay, value); } }
        string _endDay = string.Empty;
        public string EndDay { get { return _endDay; } set { SetProperty(ref _endDay, value); } }

        public string Name { get; set; }
        public string ChatChannels { get; set; }
        public string NewChannel { get; set; }
        public bool CanChangeLarpEvent { get { return LocalStorage.cp.permissions.Contains(Models.CP.PermissionType.ManageEvent); } }
        public bool CanNotChangeLarpEvent { get { return !CanChangeLarpEvent; } }
        public Command SaveCommand { get; set; }
        public Command AddChannelCommand { get; set; }

        public Command SetStartDay { get; set; }
        public Command SetEndDay { get; set; }

        INavigation navigation;
        public LarpEventViewModel(INavigation navigation)
        {
            this.navigation = navigation;

            Name = LarpEvent.Name;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < LarpEvent.ChatChannels.Count; ++i)
            {
                if(i !=0)
                    builder.Append(Environment.NewLine);
                builder.Append(ChatChannels[i]);
            }
            SaveCommand = new Command(OnSave);
            AddChannelCommand = new Command(OnAddChannel);
            SetStartDay = new Command(OnSetStartDay);
            SetEndDay = new Command(OnSetEndDay);

            StartDay = LarpEvent.Days.first.DateTime.ToShortDateString();
            EndDay = LarpEvent.Days.second.DateTime.ToShortDateString();

        }
        void OnSave()
        {
            LarpEvent.Name = Name;
        }
        void OnAddChannel()
        {
            LarpEvent.ChatChannels.Add(NewChannel);
            NewChannel = string.Empty;
        }

        async void OnSetStartDay()
        {
            var time = await CalendarPage.ShowCalendarPage(navigation);
            LarpEvent.Days = new Pair<DateTimeOffset, DateTimeOffset>(time, LarpEvent.Days.second);
            StartDay = time.ToShortDateString();
        }
        async void OnSetEndDay()
        {
            var time = await CalendarPage.ShowCalendarPage(navigation);
            LarpEvent.Days = new Pair<DateTimeOffset, DateTimeOffset>(LarpEvent.Days.first, time);
            EndDay = time.ToShortDateString();
        }
    }
}