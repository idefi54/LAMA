using LAMA.Communicator;
using LAMA.Models;
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
        string name;
        public string Name { get { return name; } private set{ SetProperty(ref name, value);} }
        string description;
        public string Description { get { return description; } private set { SetProperty(ref description, value); } }

        public Xamarin.Forms.Command Save;
        public Xamarin.Forms.Command Edit;
        public Xamarin.Forms.Command Cancel;
        public Xamarin.Forms.Command Delete;
        public Xamarin.Forms.Command AddChildCategory;
        public Xamarin.Forms.Command AddRecord;

        List<EncyclopedyCategory> parentlessCategories = new List<EncyclopedyCategory>();
        List<EncyclopedyRecord> parentlessRecords = new List<EncyclopedyRecord>();

        public EncyclopedyCategoryViewModel(EncyclopedyCategory category)
        {
            this.category = category;
            Save = new Xamarin.Forms.Command(onSave);
            Edit = new Xamarin.Forms.Command(onEdit);
            Cancel = new Xamarin.Forms.Command(onCancel);
            Delete = new Xamarin.Forms.Command(onDelete);

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
            foreach (var id in notTakenRecordIDs)
            {
                parentlessRecords.Add(recordList.getByID(id));
            }


            if (category == null)
            {
                foreach(var a in parentlessCategories)
                {
                    Categories.Add(new EncyclopedyCategoryViewModel(a));
                }
                foreach(var a in parentlessRecords)
                {
                    Records.Add(new EncyclopedyRecordViewModel(a));
                }
            }
            else
            {
                for (int i = 0; i < category.ChildCategories.Count; ++i)
                {
                    Categories.Add(new EncyclopedyCategoryViewModel(categoryList.getByID(category.ChildCategories[i])));
                }
                for (int i = 0; i < category.Records.Count; ++i)
                {
                    Records.Add(new EncyclopedyRecordViewModel(recordList.getByID(category.Records[i])));
                }
            }

        }

        void onSave()
        {
            category.Name = name;
            category.Description = description;
        }
        void onEdit()
        {
            throw new NotImplementedException();
        }
        void onCancel()
        {
            throw new NotImplementedException();
        }
        void onDelete()
        {
            var list = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;
            list.removeByID(category.ID);
            throw new NotImplementedException("remove also the ID from other categories");
        }

    }
}
