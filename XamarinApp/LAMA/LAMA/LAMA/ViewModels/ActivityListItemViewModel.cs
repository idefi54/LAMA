using LAMA.Models;
using LAMA.Models.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LAMA.ViewModels
{
    public class ActivityListItemViewModel : BaseViewModel
    {

        LarpActivity _larpActivity;

        public LarpActivity LarpActivity => _larpActivity;

        public string Name => _larpActivity == null ? "" : _larpActivity.name + " " + (_larpActivity.eventType == Models.LarpActivity.EventType.normal ? "(N)" : "(P)");

        public string Detail => _larpActivity == null ? "" : "začíná za " + TimeFormat(_larpActivity.start.hours, _larpActivity.start.minutes);


        private bool _showDeleteButton;
        public bool ShowDeleteButton { get { return _showDeleteButton; } set { SetProperty(ref _showDeleteButton, value, nameof(ShowDeleteButton)); } }

        public void SetName(string name)
		{
            if (_larpActivity == null)
                return;

            _larpActivity.name = name;

            OnPropertyChanged(nameof(Name));
		}


        public ActivityListItemViewModel(LarpActivity activity)
        {
            _larpActivity = activity;
            ShowDeleteButton = false;
        }

        string TimeFormat(int hours, int minutes)
        {
            int fullMinutes = hours * 60 + minutes;

            int nowMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;

            int resultMinutes = fullMinutes - nowMinutes;

            string result = "";
            int resultHours = resultMinutes / 60;
            if (resultHours > 0)
                result = resultHours.ToString() + "h ";

            result += (resultMinutes - resultHours * 60) + "m";

            return result;

            //return hours + ":" + minutes;
        }

        internal void ResetDisplay()
        {
            ShowDeleteButton = false;
        }

        internal void UpdateActivity(LarpActivity activity)
        {
            SetProperty(ref _larpActivity, activity);

            OnPropertyChanged(nameof(Name));

            OnPropertyChanged(nameof(Detail));
        }
    }
}
