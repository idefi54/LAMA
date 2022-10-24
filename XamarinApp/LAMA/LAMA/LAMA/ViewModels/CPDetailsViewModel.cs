using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using LAMA.Models;

namespace LAMA.ViewModels
{
    internal class CPDetailsViewModel : BaseViewModel
    {
        INavigation navigation;
        CP cp;

        public string Name { get { return cp.name; } }
        public string Nick { get { return cp.nick; } }
        public string Roles { get { return cp.roles.ToString(); } }
        public string Phone 
        { 
            get 
            {
                return cp.phone;
            } 
        }
        public string Facebook { get { return cp.facebook; } }
        public string Discord { get { return cp.discord; } }
        string _notes;
        public string Notes { get { return _notes; } set { SetProperty(ref _notes, value); } }
        public Command SaveCommand { get;}
        public CPDetailsViewModel(INavigation navigation, CP cp)
        {
            this.navigation = navigation;
            this.cp = cp;
            _notes = cp.notes;
            SaveCommand = new Command(OnSave);
        }
        void OnSave()
        {
            cp.notes = _notes;
        }

        
     }
}
