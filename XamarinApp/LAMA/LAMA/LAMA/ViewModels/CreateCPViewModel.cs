using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class CreateCPViewModel : BaseViewModel
    {
        INavigation navigation;

        public Command CancelCommand { get; private set; }
        public Command SaveCommand { get; private set; }

        string _name, _nick, _roles, _phone, _facebook, _discord, _notes;

        public string Name { get { return _name; } set { SetProperty(ref _name, value); } } 
        public string Roles { get { return _roles; } set { SetProperty(ref _roles, value); } }
        public string Nick { get { return _nick; } set { SetProperty(ref _nick, value); } }
        public string Phone { get { return _phone; } set { SetProperty(ref _phone, value);} }
        public string Facebook { get { return _facebook; } set { SetProperty(ref _facebook, value); } }
        public string Discord { get { return _discord; } set { SetProperty(ref _discord, value); } }
        public string Notes { get { return _notes; } set { SetProperty(ref _notes, value); } }


        public CreateCPViewModel(INavigation navigation)
        {
            this.navigation = navigation;

            CancelCommand = new Command(OnCancel);
            SaveCommand = new Command(OnSave);
        }

        void OnCancel()
        {
            navigation.PopAsync();
        }
        void OnSave()
        {
            var list = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;
            var toAdd = new CP(list.nextID(), _name, _nick, Helpers.readStringField(_roles), Helpers.readInt(_phone), _facebook, _discord, _notes);
            list.add(toAdd);
            navigation.PopAsync();
        }

    }
}
