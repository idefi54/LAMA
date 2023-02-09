using LAMA.Extensions;
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

        public string Name => _larpActivity == null ? "" : _larpActivity.name + " " + _larpActivity.eventType.ToShortString();

        public string Detail => _larpActivity == null ? "" : TimeFormat(_larpActivity.start);


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

        string TimeFormat(long unixStart)
        {

            long nowSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();

            long resultSeconds = (unixStart/1000) - nowSeconds;

            string result = resultSeconds >= 0 ? "začíná za " : "začalo před ";
            resultSeconds = Math.Abs(resultSeconds);

            long resultMinutes = (resultSeconds / 60) % 60;
            long resultHours = (resultSeconds / 3600) % 24;
            long resultDays = resultSeconds / 86400;

            if (resultDays > 0)
                result += resultDays.ToString() + "d ";

            if (resultHours > 0)
                result += resultHours.ToString() + "h ";

            //if (resultMinutes > 0)
            result += resultMinutes.ToString() + "m ";


            //result += (resultSeconds % 60) + "s";

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
