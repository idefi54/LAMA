using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace LAMA.ViewModels
{
    public class EncyclopedyRecordViewModel:BaseViewModel, INotifyPropertyChanged
    {
        INavigation Navigation;
        public EncyclopedyRecord record { get; private set; }

        string _Name = "";
        public string Name { get { return _Name; } set { SetProperty(ref _Name, value); } }
        string _Text = "";
        public string Text { get { return _Text; }  set { SetProperty(ref _Text, value); } }
        string _TLDR = "";
        public string TLDR { get { return _TLDR; } set { SetProperty(ref _TLDR, value); } }
        private bool _canChangeEncyclopedy;
        public bool CanChangeEncyclopedy { get => _canChangeEncyclopedy; set => SetProperty(ref _canChangeEncyclopedy, value); }

        public Xamarin.Forms.Command AddRecordCommand { get; private set; }
        public Xamarin.Forms.Command Edit { get; private set; }
        public Xamarin.Forms.Command Return { get; private set; }
        public Xamarin.Forms.Command Create { get; private set; }
        public Xamarin.Forms.Command Save { get; private set; }

        EncyclopedyCategory parent;
        public EncyclopedyRecordViewModel(EncyclopedyRecord record, INavigation navigation, EncyclopedyCategory parent = null)
        {
            this.parent = parent;
            Navigation = navigation;
            this.record = record;
            if (record != null)
            {
                _Name = record.Name;
                _Text = record.FullText;
                _TLDR = record.TLDR;
                record.IGotUpdated += onUpdated;
            }
            AddRecordCommand = new Command(onCreate);
            Edit = new Command(onEdit);
            Return = new Command(onReturn);
            Create = new Command(onCreateSave);
            Save = new Command(onSave);
            CanChangeEncyclopedy = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeEncyclopedy);
            SQLEvents.dataChanged += SQLEvents_dataChanged;
        }

        private void SQLEvents_dataChanged(Serializable changed, int changedAttributeIndex)
        {
            if (changed is CP cp && cp.ID == LocalStorage.cpID && changedAttributeIndex == 9)
                CanChangeEncyclopedy = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeEncyclopedy);
        }

        void onUpdated(object sender, int index)
        {
            var record = sender as EncyclopedyRecord;
            switch(index)
            {
                default: 
                    return;
                case 1:
                    Name = record.Name;
                    break;
                case 2:
                    TLDR = record.TLDR;
                    break;
                case 3:
                    Text = record.FullText;
                    break;
            }
        }
        void onCreate()
        {
            EncyclopedyCategory recordParent = null;
            if (record != null)
            {
                var list = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].Records.Find(o => o == record.ID) != 0)
                    {
                        recordParent = list[i];
                        break;
                    }
                }
            }
            Navigation.PushAsync(new CreateEncyclopedyRecordView(recordParent));
        }
        void onEdit()
        {
            Navigation.PushAsync(new EncyclopedyRecordEditView(record));
        }
        async void onReturn()
        {
            await Navigation.PopAsync();
        }
        async void onCreateSave()
        {
            bool textValid = InputChecking.CheckInput(_Text, "Plný Text", 5000);
            if (!textValid) return;
            bool nameValid = InputChecking.CheckInput(_Name, "Název Stránky", 100);
            if (!nameValid) return;
            bool TLDRValid = InputChecking.CheckInput(_TLDR, "Shrnutí", 500, true);
            if (!TLDRValid) return;

            var list = DatabaseHolder<EncyclopedyRecord, EncyclopedyRecordStorage>.Instance.rememberedList;
            var newRecord = new EncyclopedyRecord(list.nextID(), _Name, _TLDR, _Text);
            list.add(newRecord);
            if (parent != null)
                parent.Records.Add(newRecord.ID);
            await Navigation.PopAsync();
        }
        async void onSave()
        {
            bool textValid = InputChecking.CheckInput(record.FullText, "Plný Text", 5000);
            if (!textValid) return;
            bool nameValid = InputChecking.CheckInput(record.Name, "Název Stránky", 100);
            if (!nameValid) return;
            bool TLDRValid = InputChecking.CheckInput(record.TLDR, "Shrnutí", 500, true);
            if (!TLDRValid) return;

            record.FullText = _Text;
            record.Name = _Name;
            record.TLDR = _TLDR;
            await Navigation.PopAsync();
        }
    }
}
