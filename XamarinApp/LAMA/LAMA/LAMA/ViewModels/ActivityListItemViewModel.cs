using LAMA.Extensions;
using LAMA.Models;
using LAMA.Models.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ActivityListItemViewModel : BaseViewModel
    {

        LarpActivity _larpActivity;
        /// <summary>
        /// The activity it is displaying.
        /// </summary>
        public LarpActivity LarpActivity => _larpActivity;

        /// <summary>
        /// Name of the activity along with the designation if it’s preparation or normal activity.
        /// </summary>
        public string Name => _larpActivity == null ? "" : _larpActivity.name + " " + _larpActivity.eventType.ToShortString();

        /// <summary>
        /// Relative time to the start of the activity in readable format.
        /// </summary>
        public string Detail => _larpActivity == null ? "" : TimeFormat(_larpActivity.start);

        /// <summary>
        /// How many people are registered and what is the total number of role positions.
        /// </summary>
        public string Participation => _larpActivity == null ? "?/?" : ParticipationFormat();

        private bool _showDeleteButton;
        public bool ShowDeleteButton { get { return _showDeleteButton; } set { SetProperty(ref _showDeleteButton, value, nameof(ShowDeleteButton)); } }

        /// <summary>
        /// Color based on the status of the activity.
        /// </summary>
        public Color Color => _larpActivity.GetColor(0.2f);

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

            DateTime startDate = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(unixStart).ToLocalTime();

            TimeSpan difference = startDate - DateTime.Now;

            string result = difference >= TimeSpan.Zero ? "začíná za " : "začalo před ";

            TimeSpan duration = difference.Duration();

            long resultMinutes = duration.Minutes;
            long resultHours = duration.Hours;
            long resultDays = duration.Days;

            if (resultDays != 0)
                result += resultDays.ToString() + "d ";

            if (resultHours != 0)
                result += resultHours.ToString() + "h ";

            result += resultMinutes.ToString() + "m ";

            return $"{startDate.ToString("H:mm d/M/yyyy")} ({result})";
        }

        private string ParticipationFormat()
        {
            int sumRoles = _larpActivity.roles.Sum((role) => role.second);
            int sumRegistration = _larpActivity.registrationByRole.Count();
            return $"{sumRegistration}/{sumRoles}";
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

            OnPropertyChanged(nameof(Participation));

            OnPropertyChanged(nameof(Color));
        }
    }
}
