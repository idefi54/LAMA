using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
	public class DropdownMenuTestViewModel : BaseViewModel
	{
		private string _dropdownName;
		public string DropdownName
		{
			get { return _dropdownName; } 
			set 
			{
				SetProperty(ref _dropdownName, value, nameof(DropdownName));
				OnPropertyChanged(nameof(String1));
				OnPropertyChanged(nameof(String2));
				OnPropertyChanged(nameof(String3));
				OnPropertyChanged(nameof(String4));
			}
		}

		private bool _dropdownCheck;
		public bool DropdownCheck 
		{ 
			get { return _dropdownCheck; } 
			set 
			{
				SetProperty(ref _dropdownCheck, value, nameof(DropdownCheck));
				OnPropertyChanged(nameof(String1));
				OnPropertyChanged(nameof(String2));
				OnPropertyChanged(nameof(String3));
				OnPropertyChanged(nameof(String4));
			}
		}



		public string String1 => (_dropdownCheck ? DropdownName : "String1");
		public string String2 => (_dropdownCheck ? DropdownName + DropdownName : "String2");
		public string String3 => (_dropdownCheck ? DropdownName + "3" : "String3");
		public string String4 => (_dropdownCheck ? "Test" : "String4");

		private string _string5 = "TestCounter: ";
		public string String5 { get { return _string5; } set { SetProperty(ref _string5, value, nameof(String5)); } }

		public Command SwitchShowDropdownCommand { get; private set; }
		public Command TestCommand { get; private set; }

		private bool _showDropdown = true;
		public bool ShowDropdown { get { return _showDropdown; } set { SetProperty(ref _showDropdown, value, nameof(ShowDropdown)); } }

		public DropdownMenuTestViewModel()
		{
			SwitchShowDropdownCommand = new Command(SwichShowDropdown);
			TestCommand = new Command(() => String5 += "|");
		}

		private void SwichShowDropdown(object obj)
		{
			ShowDropdown = !ShowDropdown;
		}
	}
}
