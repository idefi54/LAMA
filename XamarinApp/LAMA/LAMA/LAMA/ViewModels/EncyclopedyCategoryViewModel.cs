﻿using LAMA.Communicator;
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


        public ObservableCollection<EncyclopedyCategoryViewModel> Categories { get; private set; }
        public ObservableCollection<EncyclopedyRecordViewModel> Records { get; private set; }
        string name = "";
        public string Name { get { return name; } set{ SetProperty(ref name, value);} }
        string description = "";
        public string Description { get { return description; } set { SetProperty(ref description, value); } }

        public Command<object> OpenRecordDetailsCommand { get; private set; }
        public Command<object> OpenCategoryDetailsCommand { get; private set; }

        public Xamarin.Forms.Command Save { get; private set; }
        public Xamarin.Forms.Command Create { get; private set; }
        public Xamarin.Forms.Command Edit { get; private set; }
        public Xamarin.Forms.Command Cancel { get; private set; }
        public Xamarin.Forms.Command Delete { get; private set; }
        public Xamarin.Forms.Command AddCategoryCommand { get; private set; }
        public Xamarin.Forms.Command AddRecordCommand { get; private set; }
        public Xamarin.Forms.Command AddChildCategoryCommand { get; private set; }
        public Xamarin.Forms.Command AddChildRecordCommand { get; private set; }


        static ObservableCollection<EncyclopedyCategory> parentlessCategories = new ObservableCollection<EncyclopedyCategory>();
        static ObservableCollection<EncyclopedyRecord> parentlessRecords = new ObservableCollection<EncyclopedyRecord>();

        INavigation Navigation;


        
        int _SelectedCategoryIndex;
        int _SelectedRecordIndex;

        List<long> AddableCategoryIndexes = new List<long>();
        ObservableCollection<string> _categoryNames = new ObservableCollection<string>();
        public ObservableCollection<string> CategoryNames
        {
            get
            {
                _categoryNames.Clear();
                AddableCategoryIndexes = new List<long>();
                foreach(var a in parentlessCategories)
                {
                    if (category.ID == a.ID || isMyParent(a))
                        continue;
                    _categoryNames.Add(a.Name);
                    AddableCategoryIndexes.Add(a.ID);
                }
                return _categoryNames;
            }
        }
        ObservableCollection<string> _recordNames = new ObservableCollection<string>();
        public ObservableCollection<string> RecordNames { 
            get
            {
                _recordNames.Clear();
                
                foreach(var a in parentlessRecords)
                {
                    _recordNames.Add(a.Name);
                }
                return _recordNames;
            } 
        }
        public int SelectedCategoryIndex { get { return _SelectedCategoryIndex; } set { SetProperty(ref _SelectedCategoryIndex, value); } }
        public int SelectedRecordIndex { get { return _SelectedRecordIndex; } set { SetProperty(ref _SelectedRecordIndex, value); } }

        public EncyclopedyCategoryViewModel(EncyclopedyCategory category, INavigation navigation)
        {
            this.category = category;
            this.Navigation = navigation;
            Categories = new ObservableCollection<EncyclopedyCategoryViewModel>();
            Records = new ObservableCollection<EncyclopedyRecordViewModel>();


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


            var categoryList = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;
            var recordList = DatabaseHolder<EncyclopedyRecord, EncyclopedyRecordStorage>.Instance.rememberedList;

            if (category != null)
            {
                name = category.Name;
                description = category.Description;
            }

            setParentless();

            if (category == null)
            {
                for (int i = 0; i < parentlessCategories.Count; ++i) 
                {
                    Categories.Add(new EncyclopedyCategoryViewModel(parentlessCategories[i], Navigation));
                }
                for (int i = 0; i< parentlessRecords.Count; ++i)
                {
                    Records.Add(new EncyclopedyRecordViewModel(parentlessRecords[i], Navigation));
                }
            }
            else
            {
                for (int i = 0; i < category.ChildCategories.Count; ++i)
                {
                    if (category.ChildCategories[i] == this.category.ID)
                    {
                        category.ChildCategories.RemoveAt(i);
                        continue;
                    }

                    Categories.Add(new EncyclopedyCategoryViewModel(categoryList.getByID(category.ChildCategories[i]), Navigation));
                }
                for (int i = 0; i < category.Records.Count; ++i)
                {
                    Records.Add(new EncyclopedyRecordViewModel(recordList.getByID(category.Records[i]), Navigation));
                }
            }

        }
        public static void setParentless()
        {
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

        }


        bool isMyParent(EncyclopedyCategory root)
        {
            var categoryList = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;

            Queue<EncyclopedyCategory> frontier = new Queue<EncyclopedyCategory>();

            frontier.Enqueue(root);

            while(frontier.Count > 0)
            {
                EncyclopedyCategory current = frontier.Dequeue();
                if (current.ID == category.ID)
                    return true;
                for (int i = 0; i < current.ChildCategories.Count; ++i)
                {
                    frontier.Enqueue(categoryList.getByID(current.ChildCategories[i]));
                }
            }
            return false;

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
                for (int j = 0; j < categoryList[i].ChildCategories.Count; ++j)
                {
                    if (categoryList[i].ChildCategories[j] == category.ID)
                        categoryList[i].ChildCategories.RemoveAt(j);
                }
                    

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
            if (SelectedCategoryIndex < 0)
                return;
            this.category.ChildCategories.Add(AddableCategoryIndexes[SelectedCategoryIndex]);
            AddableCategoryIndexes.RemoveAt(SelectedRecordIndex);
            _categoryNames.RemoveAt(SelectedRecordIndex);
            setParentless();

        }
        async void onAddChildRecord()
        {
            if (SelectedRecordIndex < 0)
                return;
            this.category.Records.Add(parentlessRecords[SelectedRecordIndex].ID);
            _recordNames.RemoveAt(SelectedRecordIndex);
            setParentless();
        }
    }
}