using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using LAMA.Models;
using Newtonsoft.Json;
using LAMA.Views;
using System.Collections.ObjectModel;

namespace LAMA.ViewModels
{
    internal class CPDetailsViewModel : BaseViewModel
    {
        INavigation navigation;
        CP cp;

        string name= "";
        public string Name { get { return name; } set { SetProperty(ref name, value); } }
        string nick;
        public string Nick { get { return nick; } set { SetProperty(ref nick, value); } }
        string roles;
        public string Roles { get { return roles; } set { SetProperty(ref roles, value); } }
        string phone;
        public string Phone 
        { 
            get { return phone; }
            set { SetProperty(ref phone, value); }
        }
        string facebook;
        public string Facebook { get { return facebook; } set { SetProperty(ref facebook, value); } }
        string discord;
        public string Discord { get { return discord; } set { SetProperty(ref discord, value); } }
        string _notes;
        public string Notes { get { return _notes; } set { SetProperty(ref _notes, value); } }
        string permissions;
        public string Permissions { get{ return permissions; } set { SetProperty(ref permissions, value); } }
        public bool CanChangeCPs { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) || cp.ID == LocalStorage.cpID; } set { } }
        public bool CanOpenDetails { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) || LocalStorage.cp.permissions.Contains(CP.PermissionType.SetPermission) || cp.ID== LocalStorage.cpID; } set { } }
        public bool CanChangePermissions { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.SetPermission) || LocalStorage.cpID==0; } set { } }
        public Command SaveCommand { get; private set; }
        public Command EditCommand { get; private set; }
        //public Command AddPermissionCommand { get; private set; }
        //public Command RemovePermissionCommand { get; private set; }
        public Command DeleteCommand { get; private set; }
        public ObservableCollection<string> AddablePermissions { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> CurrentPermissions { get; } = new ObservableCollection<string>();
        //int permissionToAdd;
        //public int PermissionToAdd { get { return permissionToAdd; }set { SetProperty(ref permissionToAdd, value); } }
        //int permissionToRemove;
        //public int PermissionToRemove { get { return permissionToRemove; } set { SetProperty(ref permissionToRemove, value); } }

        //Dictionary<string, CP.PermissionType> namesToPermissions = new Dictionary<string, CP.PermissionType>();

        ObservableCollection<PermissionViewModel> _permissionList = new ObservableCollection<PermissionViewModel>();
        public ObservableCollection<PermissionViewModel> PermissionList { get { return _permissionList; } }

        public CPDetailsViewModel(INavigation navigation, CP cp)
        {
            this.navigation = navigation;
            this.cp = cp;
            _notes = cp.notes;
            SaveCommand = new Command(OnSave);
            EditCommand = new Command(OnEdit);
            DeleteCommand = new Command(OnDelete);
            name = cp.name;
            nick = cp.nick;
            roles = cp.roles.ToReadableString();
            phone = cp.phone;
            facebook = cp.facebook;
            discord = cp.discord;
            _notes = cp.notes;
            permissions = cp.permissions.ToReadableString();
            

            foreach (CP.PermissionType perm in Enum.GetValues(typeof(CP.PermissionType)))
            {
                PermissionList.Add(new PermissionViewModel(perm.ToString(), cp.permissions.Contains(perm), 
                    CanChangePermissions && 
                    //also don't allow to change my own permission to change permissions
                    !(cp.ID==LocalStorage.cpID && perm == CP.PermissionType.SetPermission), 
                    perm));
            }

            //figureOutPermissions();


        }
        void OnSave()
        {
            if (cp.name != name)
                cp.name = name;
            if (cp.nick != nick)
                cp.nick = nick;
            cp.roles.Update(Helpers.readStringField(roles));
            if (cp.phone != phone)
                cp.phone = phone;
            if (cp.facebook != facebook)
                cp.facebook = facebook;
            if (cp.discord != discord)
                cp.discord = discord;
            if (cp.notes != _notes)
                cp.notes = _notes;

            if(CanChangePermissions)
                foreach(var a in PermissionList)
                {
                    if(a.Checked && !cp.permissions.Contains(a.type))
                        cp.permissions.Add(a.type);
                    else if (!a.Checked && cp.permissions.Contains(a.type))
                        cp.permissions.Remove(a.type);
                }
            navigation.PopAsync();
        }

        void OnEdit()
        {
            navigation.PushAsync(new CPDetailsEditView(cp));
        }
        /*
        void figureOutPermissions()
        {
            AddablePermissions.Clear();
            CurrentPermissions.Clear();
            HashSet<CP.PermissionType> taken = new HashSet<CP.PermissionType>();

            for(int i = 0; i< cp.permissions.Count;++i)
            {
                taken.Add(cp.permissions[i]);
            }

            foreach(CP.PermissionType perm in Enum.GetValues(typeof (CP.PermissionType)))
            {
                if(taken.Contains(perm))
                {
                    CurrentPermissions.Add(Enum.GetName(typeof(CP.PermissionType), perm));
                }
                else
                {
                    AddablePermissions.Add(Enum.GetName(typeof(CP.PermissionType), perm));
                }
            }
        }
        */
        /*
        void onAddPermission()
        {
            cp.permissions.Add(namesToPermissions[AddablePermissions[PermissionToAdd]]);
            figureOutPermissions();
            SetProperty(ref permissions, cp.permissions.ToReadableString());
            Permissions = cp.permissions.ToReadableString();
        }
        void onRemovePermission()
        {
            //do NOT remove your own permission to change permissions, that's usually a dumb idea and a potential soft lock of a whole larp
            if (cp == LocalStorage.cp && namesToPermissions[CurrentPermissions[permissionToRemove]]== CP.PermissionType.SetPermission)
                return;
            cp.permissions.Remove(namesToPermissions[CurrentPermissions[permissionToRemove]]);
            figureOutPermissions();
            SetProperty(ref permissions, cp.permissions.ToReadableString());
            Permissions = cp.permissions.ToReadableString();
        }
        */
        async void OnDelete()
        {
            // don't delete yourself
            // also do not delete the server
            if (LocalStorage.cpID == cp.ID || cp.ID==0)
                return;

            DatabaseHolder<CP, CPStorage>.Instance.rememberedList.removeByID(cp.ID);
            await navigation.PopAsync();
        }
     }
}
