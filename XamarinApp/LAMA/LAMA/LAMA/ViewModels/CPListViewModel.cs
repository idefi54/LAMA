using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;
namespace LAMA.ViewModels
{
    internal class CPListViewModel : BaseViewModel
    {
        INavigation navigation;


        public Command AddCPCommand { get; private set; }
        public ObservableCollection<CPViewModel> CPList{ get; private set; }
        Dictionary<long, CPViewModel> IDToViewModel = new Dictionary<long, CPViewModel>();
        public Command<object> OpenDetailCommand { get; private set; }

        public CPListViewModel(INavigation navigation)
        {
            this.navigation = navigation;
            AddCPCommand = new Command(OnMakeCP);
            OpenDetailCommand = new Command<object>(OnOpenDetail);
            CPList = new ObservableCollection<CPViewModel>();
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
        void OnMakeCP()
        {
            navigation.PushAsync(new )
        }
        void OnOpenDetail(object obj)
        {

        }
    }
}
