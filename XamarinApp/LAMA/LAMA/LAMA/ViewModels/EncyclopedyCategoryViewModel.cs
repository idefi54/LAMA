using LAMA.Communicator;
using LAMA.Models;
using LAMA.Views;
using Mapsui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class EncyclopedyCategoryViewModel: BaseViewModel
    {
        public EncyclopedyCategory category { get; private set; }


        public ObservableCollection<EncyclopedyCategoryViewModel> Categories { get; }
        public ObservableCollection<EncyclopedyRecordViewModel> Records { get; }
        string name="";
        public string Name { get { return name; } private set{ SetProperty(ref name, value);} }
        string description="";
        public string Description { get { return description; } private set { SetProperty(ref description, value); } }

        public Command<object> OpenRecordDetailsCommand;
        public Command<object> OpenCategoryDetailsCommand;

        public Xamarin.Forms.Command Save;
        public Xamarin.Forms.Command Create;
        public Xamarin.Forms.Command Edit;
        public Xamarin.Forms.Command Cancel;
        public Xamarin.Forms.Command Delete;
        public Xamarin.Forms.Command AddCategoryCommand;
        public Xamarin.Forms.Command AddRecordCommand;
        public Xamarin.Forms.Command AddChildCategoryCommand;
        public Xamarin.Forms.Command AddChildRecordCommand;
        

        static ObservableCollection<EncyclopedyCategory> parentlessCategories = new ObservableCollection<EncyclopedyCategory>();
        static ObservableCollection<EncyclopedyRecord> parentlessRecords = new ObservableCollection<EncyclopedyRecord>();

        INavigation Navigation;


        
        int _SelectedCategoryIndex;
        int _SelectedRecordIndex;

        public List<string> CategoryNames
        {
            get
            {
                List<string> output = new List<string>();
                foreach(var a in parentlessCategories)
                {
                    output.Add(a.Name);
                }
                return output;
            }
        }
        public List<string> RecordNames { 
            get
            {
                List<string> output = new List<string>();
                foreach(var a in parentlessRecords)
                {
                    output.Add(a.Name);
                }
                return output;
            } 
        }
        public int SelectedCategoryIndex { get { return _SelectedCategoryIndex; } set { SetProperty(ref _SelectedCategoryIndex, value); } }
        public int SelectedRecordIndex { get { return _SelectedRecordIndex; } set { SetProperty(ref _SelectedRecordIndex, value); } }

        public EncyclopedyCategoryViewModel(EncyclopedyCategory category, INavigation navigation)
        {
            this.category = category;
            this.Navigation = navigation;

            OpenRecordDetailsCommand = new Command<object>(onOpenRecord);
            OpenCategoryDetailsCommand = new Command<object>(onOpenCategory);

            Save = new Xamarin.Forms.Command(onSave);
            Edit = new Xamarin.Forms.Command(onEdit);
            Cancel = new Xamarin.Forms.Command(onCancel);
            Delete = new Xamarin.Forms.Command(onDelete);
            Create = new Xamarin.Forms.Command(onCreate);
            AddCategoryCommand = new Xamarin.Forms.Command(onAddCategory);
            AddRecordCommand = new Xamarin.Forms.Command(onAddRecord);
            AddChildCategoryCommand = new Xamarin.Forms.Command(onAddChildCategory);
            AddChildRecordCommand = new Xamarin.Forms.Command(onAddChildRecord);
            

            HashSet<long> notTakenCategoryIDs = new HashSet<long>();
            var categoryList = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;
            for (int i = 0; i < categoryList.Count; ++i)
            {
                notTakenCategoryIDs.Add(categoryList[i].ID);
            }
            for (int i = 0; i < categoryList.Count; ++i)
            {
                for (int j = 0; j < categoryList[i].ChildCategories.Count; ++j)
                {
                    notTakenCategoryIDs.Remove(categoryList[i].ChildCategories[j]);
                }

            }
            parentlessCategories.Clear();
            foreach (var id in notTakenCategoryIDs)
            {
                parentlessCategories.Add(categoryList.getByID(id));
            }



            HashSet<long> notTakenRecordIDs = new HashSet<long>();
            var recordList = DatabaseHolder<EncyclopedyRecord, EncyclopedyRecordStorage>.Instance.rememberedList;
            for (int i = 0; i < recordList.Count; ++i)
            {
                notTakenRecordIDs.Add(recordList[i].ID);
            }
            for (int i = 0; i < categoryList.Count; ++i)
            {
                for (int j = 0; j < categoryList[i].Records.Count; ++j)
                {
                    notTakenRecordIDs.Remove(categoryList[i].Records[j]);
                }

            }
            parentlessRecords.Clear();
            foreach (var id in notTakenRecordIDs)
            {
                parentlessRecords.Add(recordList.getByID(id));
            }


            if (category == null)
            {
                foreach (var a in parentlessCategories)
                {
                    Categories.Add(new EncyclopedyCategoryViewModel(a, Navigation));
                }
                foreach (var a in parentlessRecords)
                {
                    Records.Add(new EncyclopedyRecordViewModel(a, Navigation));
                }
            }
            else
            {
                for (int i = 0; i < category.ChildCategories.Count; ++i)
                {
                    Categories.Add(new EncyclopedyCategoryViewModel(categoryList.getByID(category.ChildCategories[i]), Navigation));
                }
                for (int i = 0; i < category.Records.Count; ++i)
                {
                    Records.Add(new EncyclopedyRecordViewModel(recordList.getByID(category.Records[i]), Navigation));
                }
            }

        }

        async void onSave()
        {
            category.Name = name;
            category.Description = description;
            category.ChildCategories.Clear();
            category.Records.Clear();
            foreach(var a in Categories)
            {
                category.ChildCategories.Add(a.category.ID);
            }
            foreach(var a in Records)
            {
                category.Records.Add(a.record.getID());
            }



            await Navigation.PopAsync();
        }
        async void onCreate()
        {
            var list = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;
            var newCategory = new EncyclopedyCategory(list.nextID(), Name, Description);
            list.add(newCategory);
            parentlessCategories.Add(newCategory);
            await Navigation.PopAsync();
        }
        async void onEdit()
        {
            if (category != null) 
                await Navigation.PushAsync(new EncyclopedyCategoryEditView(category));
        }
        async void onCancel()
        {
            await Navigation.PopAsync();
        }
        void onDelete()
        {
            var list = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;
            list.removeByID(category.ID);
            
            var categoryList = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;
            for (int i = 0; i < categoryList.Count; ++i)
            {
                categoryList[i].ChildCategories.Remove(category.ID);
            }
            Navigation.PopAsync();
        }
        async void onOpenRecord(object obj)
        {
            if (obj.GetType() != typeof(EncyclopedyRecordViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(EncyclopedyRecordViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }
            var viewModel = (EncyclopedyRecordViewModel)obj;
            await Navigation.PushAsync(new EncyclopedyRecordView(viewModel.record));
        }
        async void onOpenCategory(object obj)
        {

            if (obj.GetType() != typeof(EncyclopedyCategoryViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(EncyclopedyCategoryViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }
            var viewModel = (EncyclopedyCategoryViewModel)obj;
            await Navigation.PushAsync(new EncyclopedyCategoryView(viewModel.category));
        }

        async void onAddCategory()
        {
            await Navigation.PushAsync(new CreateEncyclopedyCategoryView());
        }
        async void onAddRecord()
        {
            await Navigation.PushAsync(new CreateEncyclopedyRecordView());
        }

        async void onAddChildCategory()
        {
            this.category.ChildCategories.Add(parentlessCategories[SelectedCategoryIndex].ID);
            parentlessCategories.RemoveAt(SelectedRecordIndex);

        }
        async void onAddChildRecord()
        {
            this.category.Records.Add(parentlessRecords[SelectedRecordIndex].ID);
            parentlessRecords.RemoveAt(SelectedRecordIndex);
        }
    }
}
