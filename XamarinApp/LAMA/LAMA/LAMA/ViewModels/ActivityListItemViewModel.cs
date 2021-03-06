using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    public class ActivityListItemViewModel : BaseViewModel
    {

        LarpActivity _larpActivity;

        public LarpActivity LarpActivity => _larpActivity;

        public string Name => _larpActivity == null ? "" : _larpActivity.name + " " + (_larpActivity.eventType == LarpActivity.EventType.normal ? "(N)" : "(P)");

        public string Detail => _larpActivity == null ? "" : "začíná za " + TimeFormat(_larpActivity.start.hours, _larpActivity.start.minutes);


        private bool _showDeleteButton;
        public bool ShowDeleteButton { get { return _showDeleteButton; } set { SetProperty(ref _showDeleteButton, value, nameof(ShowDeleteButton)); } }


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
    }
}
