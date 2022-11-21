using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class EncyclopedyRecordViewModel:BaseViewModel
    {
        INavigation Navigation;
        public EncyclopedyRecord record { get; private set; }

        string _Name = "";
        public string Name { get { return _Name; } set { SetProperty(ref _Name, value); } }
        string _Text = "";
        public string Text { get { return _Text; }  set { SetProperty(ref _Text, value); } }
        string _TLDR = "";
        public string TLDR { get { return _TLDR; } set { SetProperty(ref _TLDR, value); } }

        public Xamarin.Forms.Command AddRecordCommand { get; private set; }
        public Xamarin.Forms.Command Edit { get; private set; }
        public Xamarin.Forms.Command Return { get; private set; }
        public Xamarin.Forms.Command Create { get; private set; }
        public Xamarin.Forms.Command Save { get; private set; }
        public EncyclopedyRecordViewModel(EncyclopedyRecord record, INavigation navigation)
        {
            Navigation = navigation;
            this.record = record;
            if (record != null)
            {
                _Name = record.Name;
                _Text = record.FullText;
                _TLDR = record.TLDR;
            }
            AddRecordCommand = new Command(onCreate);
            Edit = new Command(onEdit);
            Return = new Command(onReturn);
            Create = new Command(onCreateSave);
            Save = new Command(onSave);
        }

        void onCreate()
        {
            Navigation.PushAsync(new CreateEncyclopedyRecordView());
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
            var list = DatabaseHolder<EncyclopedyRecord, EncyclopedyRecordStorage>.Instance.rememberedList;
            list.add(new EncyclopedyRecord(list.nextID(), _Name, _TLDR, _Text));
            await Navigation.PopAsync();
        }
        async void onSave()
        {
            record.FullText = _Text;
            record.Name = _Name;
            record.TLDR = _TLDR;
            await Navigation.PopAsync();
        }
    }
}
