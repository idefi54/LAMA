using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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

        List<string> _CPNames;
        Dictionary<int, string> CPs = new Dictionary<int, string>();
        Dictionary<string, int> CPIDs = new Dictionary<string, int>();

        public string Name { get { return _name; } }
        public string Description { get { return _description; } }
        public string NumBorrowed { get { return _numBorrowed.ToString(); } }
        public string NumFree { get { return _numFree.ToString(); } }
        public string BorrowedBy { get { return _borrowedBy; } }

        public Command DetailedBorrowCommand;
        public Command DetailedReturnCommand;

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
            _borrowedBy = item.takenBy.ToString();

            DetailedBorrowCommand = new Command(OnBorrow);
            DetailedReturnCommand = new Command(OnReturn);

            var CPList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;

            for (int i =0; i< CPList.Count; ++i)
            {
                var CP = CPList[i];
                CPs.Add(CP.ID, CP.name);
                CPIDs.Add(CP.name, CP.ID);
                CPNames.Add(CP.name);
            }
        }
        public async void OnBorrow()
        {
            if (_howManyBorrow < 1)
                return;

            _item.Borrow(_howManyBorrow, CPIDs[CPNames[_borrowName]]);
            _howManyBorrow = 0;
            _borrowName = 0;
        }
        public async void OnReturn()
        {
            _item.Return(_returnNum, CPIDs[CPNames[ _returnName]]);
            _returnNum = 0;
            _returnName = 0;
        }

    }
}
