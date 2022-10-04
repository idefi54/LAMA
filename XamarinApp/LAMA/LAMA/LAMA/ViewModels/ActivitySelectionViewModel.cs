using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class ActivitySelectionViewModel
    {
        public LarpActivity Activity;

        private Action<LarpActivity> _callback;

        public ObservableCollection<ActivityListItemViewModel> LarpActivityListItems { get; }

        private ActivityListItemViewModel _selectedItem;
        public ActivityListItemViewModel SelectedItem { get { return _selectedItem; } set
            {
                if (value == null)
                    return;
                _selectedItem = value;
                Activity = _selectedItem.LarpActivity;
                _callback(Activity);
                Close();
            }
        }


        public ActivitySelectionViewModel(Action<LarpActivity> callback)
		{
            _callback = callback;
            Activity = null;
            LarpActivityListItems = new ObservableCollection<ActivityListItemViewModel>();

            for (int i = 0; i < DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.Count; i++)
            {
                LarpActivityListItems.Add(new ActivityListItemViewModel(DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList[i]));
            }
		}

        private async void Close()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
