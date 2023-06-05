using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using LAMA.Models;
using Newtonsoft.Json;
using LAMA.Views;
using System.Collections.ObjectModel;
using LAMA.Services;
using System.Linq;

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

        string password;
        public string Password { get { return password; } set { SetProperty(ref password, value); } }

        string roles;
        public string Roles { get { return roles; } set { SetProperty(ref roles, value); } }

        ObservableCollection<CPRoleItemViewModel> roleList;
        public ObservableCollection<CPRoleItemViewModel> RoleList { get { return roleList; } set { SetProperty(ref roleList, value); } }

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

        private bool _canDeleteCP = false;
        public bool CanDeleteCP { get => _canDeleteCP; set => SetProperty(ref _canDeleteCP, value); }

        private bool _canEditDetails = false;
        public bool CanEditDetails { get => _canEditDetails; set => SetProperty(ref _canEditDetails, value); }

        private bool _canEdit = false;
        public bool CanEdit { get => _canEdit; set => SetProperty(ref _canEdit, value); }

        private bool _canChangePermissions = false;
        public bool CanChangePermissions { get => _canChangePermissions; set => SetProperty(ref _canChangePermissions, value); }

        bool _CanArchiveCP = false;
        public bool CanArchiveCP { get { return _CanArchiveCP; } set { SetProperty(ref _CanArchiveCP, value); } }

        bool _CanUnarchiveCP = false;
        public bool CanUnarchiveCP { get { return _CanUnarchiveCP; } set { SetProperty(ref _CanUnarchiveCP, value); } }

        public Command SaveCommand { get; private set; }
        public Command EditCommand { get; private set; }
        //public Command AddPermissionCommand { get; private set; }
        //public Command RemovePermissionCommand { get; private set; }
        public Command Archive { get; }
        public Command Unarchive { get; }
        public Command AddRole { get; }
        public Command DeleteCommand { get; private set; }
        public Command CancelCommand { get; private set; }
        public ObservableCollection<string> AddablePermissions { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> CurrentPermissions { get; } = new ObservableCollection<string>();
        //int permissionToAdd;
        //public int PermissionToAdd { get { return permissionToAdd; }set { SetProperty(ref permissionToAdd, value); } }
        //int permissionToRemove;
        //public int PermissionToRemove { get { return permissionToRemove; } set { SetProperty(ref permissionToRemove, value); } }

        //Dictionary<string, CP.PermissionType> namesToPermissions = new Dictionary<string, CP.PermissionType>();

        ObservableCollection<PermissionViewModel> _permissionList = new ObservableCollection<PermissionViewModel>();
        public ObservableCollection<PermissionViewModel> PermissionList { get { return _permissionList; } }

        private IMessageService messageService;

        public CPDetailsViewModel(INavigation navigation, CP cp)
        {
            messageService = DependencyService.Get<IMessageService>();

            SetProperty(ref _CanArchiveCP, LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && !cp.IsArchived);
            SetProperty(ref _CanUnarchiveCP, LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && cp.IsArchived);


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
            
            roleList = new ObservableCollection<CPRoleItemViewModel>(cp.roles.Select(x => new CPRoleItemViewModel(x, RemoveRoleItem, cp.ID)));
            
            CanDeleteCP = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP);
            CanEditDetails = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) || cp.ID == LocalStorage.cp.ID;
            CanEdit = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) || LocalStorage.cp.permissions.Contains(CP.PermissionType.SetPermission) || cp.ID == LocalStorage.cp.ID;
            CanChangePermissions = LocalStorage.cp.permissions.Contains(CP.PermissionType.SetPermission) || LocalStorage.cpID == 0;
            CanArchiveCP = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && !cp.IsArchived;
            CanUnarchiveCP = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && cp.IsArchived;

            foreach (CP.PermissionType perm in Enum.GetValues(typeof(CP.PermissionType)))
            {
                PermissionList.Add(new PermissionViewModel(CP.permissionsCzech[perm], cp.permissions.Contains(perm), 
                    () => CanChangePermissions && 
                    //also don't allow to change my own permission to change permissions
                    !(cp.ID==LocalStorage.cpID && perm == CP.PermissionType.SetPermission), 
                    perm));
            }

            //figureOutPermissions();

            SQLEvents.dataChanged += gotChanged;
            Archive = new Command(onArchive);
            Unarchive = new Command(onUnarchive);
            CancelCommand = new Command(onCancel);
            AddRole = new Command(AddRoleItem);
        }

        private void AddRoleItem()
		{
            RoleList.Add(new CPRoleItemViewModel("role", RemoveRoleItem, cp.ID));
		}

        private void RemoveRoleItem(CPRoleItemViewModel roleItem)
		{
            if(RoleList.Contains(roleItem))
                RoleList.Remove(roleItem);
		}

        private void onCancel()
        {
            navigation.PopAsync();
        }

        public async void onArchive()
        {
            bool result = await messageService.ShowConfirmationAsync("Opravdu chcete archivovat toto CP? Nikdo kromě lidí s pravomocí archivovat nebude schopen CP vidět a nebude možné se do daného CP přihlásit.", "Opravdu archivovat?");
            if (!result)
                return;

            cp.IsArchived = true;
            //SetProperty(ref _CanArchiveCP, LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && !cp.IsArchived);
            //SetProperty(ref _CanUnarchiveCP, LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && cp.IsArchived);
            CanArchiveCP = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && !cp.IsArchived;
            CanUnarchiveCP = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && cp.IsArchived;
        }
        public void onUnarchive()
        {
            cp.IsArchived = false;
            CanArchiveCP = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && !cp.IsArchived;
            CanUnarchiveCP = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && cp.IsArchived;
        }

        void OnSave()
        {
            bool cpNameValid = InputChecking.CheckInput(name, "Jméno CP", 50);
            if (!cpNameValid) return;
            bool cpNickValid = InputChecking.CheckInput(nick, "Přezdívka CP", 50);
            if (!cpNickValid) return;
			for (int i = 0; i < RoleList.Count; i++)
            {
				CPRoleItemViewModel roleItem = RoleList[i];
				bool cpRolesValid = InputChecking.CheckInput(roleItem.RoleName, (i + 1) + ". Role", 50, false);
                if (!cpRolesValid) return;
            }
            bool cpPhoneValid = InputChecking.CheckInput(phone, "Telefon", 20, true);
            if (!cpPhoneValid) return;
            bool cpFacebookValid = InputChecking.CheckInput(facebook, "Facebook", 100, true);
            if (!cpFacebookValid) return;
            bool cpDiscordValid = InputChecking.CheckInput(discord, "Discord", 100, true);
            if (!cpDiscordValid) return;
            bool cpNotesValid = InputChecking.CheckInput(_notes, "Role", 1000, true);
            if (!cpNotesValid) return;

            if (cp.name != name)
                cp.name = name;
            if (cp.nick != nick)
                cp.nick = nick;
            cp.roles.Update(RoleList.Select(x => x.RoleName).ToList());
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

        void gotChanged(Serializable obj, int index)
        {
            var cp = obj as CP;
            if (cp == null) return;

            if (cp.ID == this.cp.ID)
                switch (index)
                {
                    case 1: Name = cp.name; break;
                    case 2: Nick = cp.nick; break;
                    case 3: Roles = cp.roles.ToReadableString(); break;
                    case 4: Phone = cp.phone; break;
                    case 5: Facebook = cp.facebook; break;
                    case 6: Discord = cp.discord; break;
                    case 8: Notes = cp.notes; break;
                    case 9: Permissions = cp.PermissionsToReadableString(); break;
                }

            if (cp.ID == LocalStorage.cpID)
            {
                CanDeleteCP = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP);
                CanEditDetails = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) || this.cp.ID == LocalStorage.cp.ID;
                CanEdit = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) || LocalStorage.cp.permissions.Contains(CP.PermissionType.SetPermission) || this.cp.ID == LocalStorage.cp.ID;
                CanChangePermissions = LocalStorage.cp.permissions.Contains(CP.PermissionType.SetPermission) || LocalStorage.cpID == 0;
                CanArchiveCP = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && !cp.IsArchived;
                CanUnarchiveCP = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) && cp.IsArchived;
            }
        }
     }
}
