using LAMA.Communicator;
using LAMA.Models;
using LAMA.Singletons;
using LAMA.Views;
using Mapsui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Text;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace LAMA.ViewModels
{
    public class EncyclopedyCategoryViewModel: BaseViewModel, INotifyPropertyChanged
    {
        public EncyclopedyCategory category { get; private set; }


        public TrulyObservableCollection<EncyclopedyCategoryViewModel> Categories { get; private set; }
        public TrulyObservableCollection<EncyclopedyRecordViewModel> Records { get; private set; }
        string name = "";
        public string Name { get { return name; } set{ SetProperty(ref name, value);} }
        string description = "";
        public string Description { get { return description; } set { SetProperty(ref description, value); } }
        public bool CanChangeEncyclopedy { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeEncyclopedy); } }
        public bool CanCreate { get { return CanChangeEncyclopedy && category == null; } }
        public bool CanEdit { get { return CanChangeEncyclopedy && category != null; } }
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
                foreach(var a in EncyclopedyOrphanage.ParentlessCategories)
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
                
                foreach(var a in EncyclopedyOrphanage.ParentlessRecords)
                {
                    _recordNames.Add(a.Name);
                }
                return _recordNames;
            } 
        }
        public int SelectedCategoryIndex { get { return _SelectedCategoryIndex; } set { SetProperty(ref _SelectedCategoryIndex, value); } }
        public int SelectedRecordIndex { get { return _SelectedRecordIndex; } set { SetProperty(ref _SelectedRecordIndex, value); } }


        public event PropertyChangedEventHandler PropertyChanged;

        public EncyclopedyCategoryViewModel(EncyclopedyCategory category, INavigation navigation)
        {
            this.category = category;
            this.Navigation = navigation;
            Categories = new TrulyObservableCollection<EncyclopedyCategoryViewModel>();
            Records = new TrulyObservableCollection<EncyclopedyRecordViewModel>();


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
                category.ChildCategories.dataChanged += OnMyCategoriesChanged;
                category.Records.dataChanged += OnMyRecordsChanged;
            }

           
            if (category == null)
            {
                EncyclopedyOrphanage.ParentlessCategories.CollectionChanged += OnCategoriesChanged;
                EncyclopedyOrphanage.ParentlessRecords.CollectionChanged += OnRecordsChanged;

                Categories = new TrulyObservableCollection<EncyclopedyCategoryViewModel>();
                foreach(var a in EncyclopedyOrphanage.ParentlessCategories)
                {
                    Categories.Add(new EncyclopedyCategoryViewModel(a, navigation));
                }
                Records = new TrulyObservableCollection<EncyclopedyRecordViewModel>();
                foreach(var a in EncyclopedyOrphanage.ParentlessRecords)
                {
                    Records.Add(new EncyclopedyRecordViewModel(a, navigation));
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

            if(category != null)
                category.IGotUpdated += onUpdated;

        }

        void onUpdated(object sender, int index)
        {
            string propName = string.Empty;

            switch(index)
            {
                case 1:
                    propName = nameof(Name);
                    break;
                case 2:
                    propName = nameof(Description);
                    break;
                default:
                    return;

            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }



        void OnMyCategoriesChanged()
        {
            var categoryList = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;

            Categories.Clear();
            foreach (var a in category.ChildCategories)
            {
                Categories.Add(new EncyclopedyCategoryViewModel(categoryList.getByID(a), Navigation));
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Categories)));
        }
        void OnMyRecordsChanged()
        {
            var recordList = DatabaseHolder<EncyclopedyRecord, EncyclopedyRecordStorage>.Instance.rememberedList;
            Records.Clear();
            foreach(var a in category.Records)
            {
                Records.Add(new EncyclopedyRecordViewModel(recordList.getByID(a), Navigation));
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Records)));
        }
        void OnCategoriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Categories.Clear();
            foreach (var a in EncyclopedyOrphanage.ParentlessCategories)
            {
                Categories.Add(new EncyclopedyCategoryViewModel(a, Navigation));
            }
        }
        void OnRecordsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Records.Clear();
            foreach (var a in EncyclopedyOrphanage.ParentlessRecords)
            {
                Records.Add(new EncyclopedyRecordViewModel(a, Navigation));
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
            bool categoryNameValid = InputChecking.CheckInput(name, "Jméno Kategorie", 50);
            if (!categoryNameValid) return;
            bool categoryDescriptionValid = InputChecking.CheckInput(description, "Popis Kategorie", 1000);
            if (!categoryDescriptionValid) return;
            category.Name = name;
            category.Description = description;

            await Navigation.PopAsync();
        }
        async void onCreate()
        {
            bool categoryNameValid = InputChecking.CheckInput(Name, "Jméno Kategorie", 50);
            if (!categoryNameValid) return;
            bool categoryDescriptionValid = InputChecking.CheckInput(Description, "Popis Kategorie", 1000);
            if (!categoryDescriptionValid) return;
            var list = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;
            var newCategory = new EncyclopedyCategory(list.nextID(), Name, Description);
            list.add(newCategory);
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
        }
        async void onAddChildRecord()
        {
            if (SelectedRecordIndex < 0)
                return;
            this.category.Records.Add(EncyclopedyOrphanage.ParentlessRecords[SelectedRecordIndex].ID);
            _recordNames.RemoveAt(SelectedRecordIndex);
        }
    }
}
