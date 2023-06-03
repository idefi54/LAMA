using LAMA.Models;
using LAMA.Services;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
namespace LAMA.ViewModels
{
    internal class InventoryItemDetailViewModel : BaseViewModel
    {

        InventoryItem _item;

        string _name;
        string _description;
        int _numBorrowed;
        int _numFree;
        string _borrowedBy;

        int _howManyChange;
        string _CPName;

        public string Name { get { return _name; } private set { SetProperty(ref _name, value); } }
        public string Description { get { return _description; } private set { SetProperty(ref _description, value); } }
        public string NumBorrowed { get { return _numBorrowed.ToString(); } private set { SetProperty(ref _numBorrowed, Helpers.readInt( value)); } }
        public string NumFree { get { return _numFree.ToString(); } private set { SetProperty(ref _numFree, Helpers.readInt(value)); } }
        public string BorrowedBy { get { return _borrowedBy; } private set { SetProperty(ref _borrowedBy, value); } }

        private bool _manageInventory;
        public bool ManageInventory { get => _manageInventory; set => SetProperty(ref _manageInventory, value); }
        public Command DetailedBorrowCommand { get; private set; }
        public Command DetailedReturnCommand { get; private set; }
        public Command DetailedDeleteCommand { get; private set; }
        public Command SelectCP { get; private set; }
        public Command DeleteCommand { get; private set; }
        public Command EditCommand { get; private set; }
        public string CPName { get { return _CPName; } set { SetProperty(ref _CPName, value); } }
        private CP selectedCP;

        public string HowManyChange { get { return _howManyChange.ToString(); } set { SetProperty(ref _howManyChange, Helpers.readInt(value)); } }
        

        INavigation _navigation;
        IMessageService _messageService;
        public InventoryItemDetailViewModel(INavigation navigation, InventoryItem item)
        {
            _item = item;
            _navigation = navigation;
            _messageService = DependencyService.Get<IMessageService>();

            _name = item.name;
            _description = item.description;
            _numBorrowed = item.taken;
            _numFree = item.free;

            writeBorrowedBy();
            
            DetailedBorrowCommand = new Command(OnBorrow);
            DetailedReturnCommand = new Command(OnReturn);
            DetailedDeleteCommand = new Command(OnDeleteBorrow);
            SelectCP = new Command(OnSelectCP);
            DeleteCommand = new Command(onDelete);
            EditCommand = new Command(OnEdit);

            _item.IGotUpdated += onItemUpdated;

            SetProperty(ref _howManyChange, 0);
            selectedCP = DatabaseHolder<CP, CPStorage>.Instance.rememberedList[0];
            CPName = selectedCP.name;
            ManageInventory = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageInventory);
            SQLEvents.dataChanged += (Serializable changed, int changedAttributeIndex) =>
            {
                if (changed is CP cp && cp.ID == LocalStorage.cpID && changedAttributeIndex == 9)
                    ManageInventory = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageInventory);
            };
        }

        private void onItemUpdated(object sender, int i)
        {
            switch(i)
            {
                default:
                    return;
                case 1:
                    Name = _item.name;
                    break;
                case 2:
                    Description = _item.description;
                    break;
                case 3:
                    NumBorrowed = _item.taken.ToString();
                    break;
                case 4: 
                    NumFree = _item.free.ToString();
                    break;
                case 5:
                    writeBorrowedBy();
                    break;
            }

        }

        private async void OnEdit()
        {
            await _navigation.PushAsync(new InventoryItemDetailEditView(_item));
        }
        private async void OnSelectCP(object obj)
        {
            CPSelectionPage page = new CPSelectionPage(_displayName);
            await _navigation.PushAsync(page);


            void _displayName(CP cp)
            {
                if (cp != null)
                {
                    selectedCP = cp;
                    CPName = selectedCP.name;
                }
            }
        }

        public async void OnBorrow()
        {
            if (!CheckExistence().Result)
                return;

            if (_howManyChange < 1)
                return;

            if (selectedCP == null)
            {
                await _messageService.ShowAlertAsync("Nebylo vybráno žádné CP.", "Nečekaná chyba");
                return;
            }

            _item.Borrow(_howManyChange, selectedCP.ID);
            NumBorrowed = _item.taken.ToString();
            NumFree = _item.free.ToString();

            HowManyChange = "0";
            writeBorrowedBy();
        }

        public async void OnReturn()
        {
            if (!CheckExistence().Result)
                return;

            if (_howManyChange < 1)
                return;

            if (selectedCP == null)
            {
                await _messageService.ShowAlertAsync("Nebylo vybráno žádné CP.", "Nečekaná chyba");
                return;
            }

            _item.Return(_howManyChange, selectedCP.ID);
            NumBorrowed = _item.taken.ToString();
            NumFree = _item.free.ToString();

            HowManyChange = "0";
            writeBorrowedBy();
        }
        async void OnDeleteBorrow()
        {
            if (_howManyChange < 1)
                return;

            if (selectedCP == null)
            {
                await _messageService.ShowAlertAsync("Nebylo vybráno žádné CP.", "Nečekaná chyba");
                return;
            }
            _item.AlreadyReturned(_howManyChange, selectedCP.ID);
            writeBorrowedBy();
            HowManyChange="0";
        }
        private void writeBorrowedBy()
        {
            SetProperty(ref _numBorrowed, _item.taken);
            SetProperty(ref _numFree, _item.free);

            var CPList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;
            StringBuilder output = new StringBuilder();
            bool first = true;
            foreach(var a in this._item.takenBy)
            {
                if (!first)
                    output.Append(", ");
                else
                    first = false;

                output.Append(CPList.getByID(a.first).nick);
                output.Append(": ");
                output.Append(a.second.ToString());
                
            }

            BorrowedBy = output.ToString();
        }
        async void onDelete()
        {
            if (!CheckExistence().Result)
                return;

            DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList.removeByID(_item.ID);
            await _navigation.PopAsync();
        }

        private async Task<bool> CheckExistence()
        {
            bool itemDeleted = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList.getByID(_item.ID) == default(InventoryItem);

            if (itemDeleted)
            {
                await DependencyService.Get<Services.IMessageService>()
                    .ShowAlertAsync(
                        "Vypadá to, že se snažíte pracovat s předmětem, který mezitím byl smazán. Nyní budete navráceni zpět do seznamu předmětů.",
                        "Předmět neexistuje");
                await _navigation.PopAsync();
                IsBusy = false;
                return false;
            }
            return true;
        }
    }
}
