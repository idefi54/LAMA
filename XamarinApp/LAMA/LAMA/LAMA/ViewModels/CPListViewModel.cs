using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;
namespace LAMA.ViewModels
{
    internal class CPListViewModel : BaseViewModel
    {
        INavigation navigation;


        public Command AddCPCommand { get; private set; }

        TrulyObservableCollection<CPViewModel> _CPList = new TrulyObservableCollection<CPViewModel>();
        public TrulyObservableCollection<CPViewModel> CPList{ get { return _CPList; } private set { SetProperty(ref _CPList, value); } }

        Dictionary<long, CPViewModel> IDToViewModel = new Dictionary<long, CPViewModel>();
        public Command<object> OpenDetailCommand { get; private set; }
        public bool CanAddCP { get {return LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP); } set { } }

        public string filterText { get; set; }



        public CPListViewModel(INavigation navigation)
        {
            this.navigation = navigation;
            AddCPCommand = new Command(OnMakeCP);
            OpenDetailCommand = new Command<object>(OnOpenDetail);
            CPList = new TrulyObservableCollection<CPViewModel>();
            LoadCPs();
        }

        void LoadCPs()
        {
            var list = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;
            CPList.Clear();
            for (int i = 0; i < list.Count; ++i) 
            { 
                var newOne = new CPViewModel(list[i]);
                CPList.Add(newOne);
                IDToViewModel.Add(list[i].ID, newOne);
            
            }
            SQLEvents.dataChanged += OnChange;
            SQLEvents.created += OnCreated;
            SQLEvents.dataDeleted += OnDeleted;
        }
        private void OnChange(Serializable changed, int index)
        {
            if (changed.GetType() != typeof(CP))
            {
                return;
            }
            //REFRESH DATA

        }
        private void OnCreated(Serializable made)
        {
            if (made.GetType() != typeof(CP))
            {
                return;
            }
            var item = (CP)made;
            CPList.Add(new CPViewModel(item));
            IDToViewModel.Add(item.ID, CPList[CPList.Count - 1]);

        }
        private void OnDeleted(Serializable deleted)
        {
            if (deleted.GetType() != typeof(CP))
            {
                return;
            }
            var item = (CP)deleted;

            CPList.Remove(IDToViewModel[item.ID]);
            IDToViewModel.Remove(item.ID);
        }
        async void OnMakeCP()
        {
            await navigation.PushAsync(new CreateCPView());
        }
        async void OnOpenDetail(object obj)
        {
            if(obj.GetType() != typeof(CPViewModel))
            {
                return;
            }
            var cpView = (CPViewModel)obj;
            var cp = cpView.cp;
            await navigation.PushAsync(new CPDetailsView(cp));
        }


        TrulyObservableCollection<CPViewModel> rememberForFilter = null;

        void OnFilter()
        {
            OnCancelFilter();

            rememberForFilter = CPList;

            TrulyObservableCollection<CPViewModel> newList = new TrulyObservableCollection<CPViewModel>();

            foreach(var cpView in CPList)
            {
                if(cpView.Name.ToLower().Contains(filterText.ToLower()) || cpView.Nick.ToLower().Contains(filterText.ToLower()))
                    newList.Add(cpView);
            }

            SetProperty(ref _CPList, newList);

        }
        void OnCancelFilter()
        {
            if(rememberForFilter != null)
                SetProperty(ref _CPList, rememberForFilter);
            rememberForFilter=null;
        }



        bool nameDescended = false;
        bool nickDescended = false;
        void OnOrderByName()
        {
            nameDescended = !nameDescended;
            order(nameDescended, new CompareByName());
        }
        void OnOrderByNick()
        {
            nickDescended = !nickDescended;
            order(nickDescended, new CompareByNick());
        }
        void order(bool ascending, IComparer<CPViewModel> comparer)
        {
            // just bubble sort because i wanna do it super simply and in place
            // and i am too lazy to do merge sort in place
            bool changed = true;
            while(changed)
            {
                changed = false;
                // one pass
                for (int i = 0; i < CPList.Count-1; ++i)
                {
                    if((ascending && comparer.Compare(CPList[i], CPList[i+1]) <0) || 
                        (!ascending && comparer.Compare(CPList[i], CPList[i + 1])>0))
                    {
                        //swap 
                        var temp = CPList[i];
                        CPList[i] = CPList[i + 1];
                        CPList[i + 1] = temp;
                        if(!changed)
                            changed = true;
                    }
                }
            }
        }
        
        class CompareByName : IComparer<CPViewModel>
        {
            public int Compare(CPViewModel x, CPViewModel y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }
        class CompareByNick : IComparer<CPViewModel>
        {
            public int Compare(CPViewModel x, CPViewModel y)
            {
                return x.Nick.CompareTo(y.Nick);
            }
        }

    }
}
