using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
namespace LAMA.ViewModels
{
    internal class InventoryItemDetailViewModel:BaseViewModel
    {

        InventoryItem _item;

        string _name;
        string _description;
        int _numBorrowed;
        int _numFree;
        string _borrowedBy;

        int _borrowName;
        int _howManyBorrow;
        int _returnName;
        int _returnNum;

        List<string> _CPNames = new List<string>();
        Dictionary<long, string> CPs = new Dictionary<long, string>();
        Dictionary<string, long> CPIDs = new Dictionary<string, long>();

        public string Name { get { return _name; } private set { SetProperty(ref _name, value); } }
        public string Description { get { return _description; } private set { SetProperty(ref _description, value); } }
        public string NumBorrowed { get { return _numBorrowed.ToString(); } private set { SetProperty(ref _numBorrowed, Helpers.readInt( value)); } }
        public string NumFree { get { return _numFree.ToString(); } private set { SetProperty(ref _numFree, Helpers.readInt(value)); } }
        public string BorrowedBy { get { return _borrowedBy; } private set { SetProperty(ref _borrowedBy, value); } }
        public bool ManageInventory { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageInventory); } set { } }
        public Command DetailedBorrowCommand { get; private set; }
        public Command DetailedReturnCommand { get; private set; }
        public Command DeleteCommand { get; private set; }

        public List<string> CPNames { get { return _CPNames; } set { SetProperty(ref _CPNames, value); } }
        public int BorrowerSelected { get { return _borrowName; } set { SetProperty(ref _borrowName, value); } }
        public string HowManyBorrow { get { return _howManyBorrow.ToString(); } set { SetProperty(ref _howManyBorrow, Helpers.readInt(value)); } }
        public int ReturnerSelected { get { return _returnName; } set { SetProperty(ref _returnName, value); } }
        public string HowManyReturn { get { return _returnNum.ToString(); } set { SetProperty(ref _returnNum, Helpers.readInt(value)); } }
        //Name, Description, NumBorrowed, NumFree, BorrowedBy

        //DetailedBorrowCommand, DetailedReturnCommand
        //BorrowerName, HowManyBorrow
        //ReturnerName, HowManyReturn

        INavigation Navigation;
        public InventoryItemDetailViewModel(INavigation navigation, InventoryItem item)
        {
            _item = item;
            Navigation = navigation;


            _name = item.name;
            _description = item.description;
            _numBorrowed = item.taken;
            _numFree = item.free;

            writeBorrowedBy();
            
            DetailedBorrowCommand = new Command(OnBorrow);
            DetailedReturnCommand = new Command(OnReturn);
            DeleteCommand = new Command(onDelete);
            var CPList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;

            for (int i =0; i< CPList.Count; ++i)
            {
                var CP = CPList[i];
                CPs.Add(CP.ID, CP.nick);
                CPIDs.Add(CP.nick, CP.ID);
                CPNames.Add(CP.nick);
            }
        }
        public void OnBorrow()
        {
            if (!CheckExistence().Result)
                return;

            if (_howManyBorrow < 1)
                return;

            _item.Borrow(_howManyBorrow, CPIDs[CPNames[_borrowName]]);
            NumBorrowed = _item.taken.ToString();
            NumFree = _item.free.ToString();

            SetProperty(ref _howManyBorrow, 0);
            SetProperty(ref _borrowName, 0);
            writeBorrowedBy();
        }
        public void OnReturn()
        {
            if (!CheckExistence().Result)
                return;

            if (_returnNum < 1)
                return;

            _item.Return(_returnNum, CPIDs[CPNames[ _returnName]]);
            NumBorrowed = _item.taken.ToString();
            NumFree = _item.free.ToString();

            SetProperty(ref _returnNum, 0);
            SetProperty(ref _returnName, 0);
            writeBorrowedBy();
        }

        private void writeBorrowedBy()
        {
            SetProperty(ref _numBorrowed, _item.taken);
            SetProperty(ref _numFree, _item.free);


            var CPList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;
            StringBuilder output = new StringBuilder();
            foreach(var a in this._item.takenBy)
            {
                output.Append(CPList.getByID(a.first).nick);
                output.Append(": ");
                output.Append(a.second.ToString());
                output.Append(", ");
            }


            BorrowedBy = output.ToString();
        }
        async void onDelete()
        {
            if (!CheckExistence().Result)
                return;

            DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList.removeByID(_item.ID);
            await Navigation.PopAsync();
        }

        private async Task<bool> CheckExistence()
        {
            bool itemDeleted = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList.getByID(_item.ID) == default(InventoryItem);

            if (itemDeleted)
            {
                await DependencyService.Get<Services.IMessageService>()
                    .ShowAlertAsync(
                        "Vypadá to, že se snažíte pracovat s předmětem, který mezitím byl smazán. Nyní budete navráceni zpět do seznamu předmětů."),
                        "Předmět neexistuje");
                    await Navigation.PopAsync();
                IsBusy = false;
                return false;
            }
            return true;
        }
    }
}
