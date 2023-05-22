using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
	public class CPSelectionViewModel : BaseViewModel
	{
		private CPSelectItemViewModel _selecedCP;
		public CPSelectItemViewModel SelectedCP
		{
			get
			{
				return _selecedCP;
			}
			set
			{
				_selecedCP = value;
				if (_selecedCP != null && _selecedCP.cp != null)
				{
					_callback(_selecedCP.cp);
					Close();
				}
			}
		}


		private TrulyObservableCollection<CPSelectItemViewModel> sourceCPList;
		public TrulyObservableCollection<CPSelectItemViewModel> FilteredCPList { get; }

		private Action<CP> _callback;

		public CPSelectorFilterViewModel FilterViewModel { get; }

		public CPSelectionViewModel(Action<CP> callback)
		{
			_callback = callback;

			sourceCPList = new TrulyObservableCollection<CPSelectItemViewModel>();
			FilteredCPList = new TrulyObservableCollection<CPSelectItemViewModel>();

			FilterViewModel = new CPSelectorFilterViewModel(sourceCPList, FilteredCPList);

			for (int i = 0; i < DatabaseHolder<CP, CPStorage>.Instance.rememberedList.Count; i++)
			{
				var item = new CPSelectItemViewModel(DatabaseHolder<CP, CPStorage>.Instance.rememberedList[i]);
				sourceCPList.Add(item);
				FilteredCPList.Add(item);
			}
		}

		private async void Close()
		{
			if (Device.RuntimePlatform == Device.WPF)
			{
				await App.Current.MainPage.Navigation.PopAsync();
			}
			else
			{
				await Shell.Current.GoToAsync("..");
			}
		}

	}
}
