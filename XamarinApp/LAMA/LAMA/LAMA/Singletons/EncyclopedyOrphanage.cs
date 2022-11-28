using LAMA.Models;
using Mapsui.Providers.Wfs.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace LAMA.Singletons
{
    public class EncyclopedyOrphanage
    {
        private static EncyclopedyOrphanage instance = null;

        public static ObservableCollection<EncyclopedyCategory> ParentlessCategories
        {
            get
            {
                if(instance == null)
                    instance = new EncyclopedyOrphanage();
                return instance.parentlessCategories;
            }
        }
        public static ObservableCollection<EncyclopedyRecord> ParentlessRecords
        {
            get
            {
                if(instance==null)
                    instance = new EncyclopedyOrphanage();
                return instance.parentlessRecords;
            }
        }
        private ObservableCollection<EncyclopedyCategory> parentlessCategories;
        private ObservableCollection<EncyclopedyRecord> parentlessRecords;

        public EncyclopedyOrphanage()
        {
            this.parentlessCategories = new ObservableCollection<EncyclopedyCategory>();
            this.parentlessRecords = new ObservableCollection<EncyclopedyRecord>();

            setParentless();
            // every single category has only one parent - parent gets removed-> kids go into orphanage
            // so on remove to add its kids

            SQLEvents.created += OnCreated;
            SQLEvents.dataDeleted += OnDeleted;
            SQLEvents.dataChanged += OnChange;


        }


        private void OnChange(Serializable ser, int index)
        {
            // i don't know the previous value, only the one now 
            // so if something should be added into parentless i have to do the whole search anyways because i don't know what was removed from the list within an instance of a record
            if(ser.GetType() == typeof(EncyclopedyCategory) && (index == 3 ||index == 4))
            {
                setParentless();
            }
        }

        private void OnDeleted(Serializable ser)
        {
            var categoryList = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;
            var recordList = DatabaseHolder<EncyclopedyRecord, EncyclopedyRecordStorage>.Instance.rememberedList;

            if( ser.GetType() == typeof(EncyclopedyCategory))
            {
                EncyclopedyCategory deleted = (EncyclopedyCategory)ser;
                foreach (var a in deleted.ChildCategories)
                    parentlessCategories.Add(categoryList.getByID(a));
                foreach (var a in deleted.Records)
                    ParentlessRecords.Add(recordList.getByID(a));
                for (int i = 0; i < parentlessCategories.Count; ++i)
                {
                    if (parentlessCategories[i].ID == deleted.ID)
                    {
                        parentlessCategories.RemoveAt(i);
                        break;
                    }
                }
            }
            else if(ser.GetType() == typeof(EncyclopedyRecord))
            {
                var deleted = (EncyclopedyRecord)ser;
                for (int i = 0; i < ParentlessRecords.Count; ++i) 
                {
                    if (ParentlessRecords[i].ID == deleted.ID)
                    {
                        ParentlessRecords.RemoveAt(i);
                        break;
                    }
                }

            }
            return;
        }



        private void removeKidsFromOrphanage(EncyclopedyCategory who)
        {
            foreach(var a in who.ChildCategories)
            {
                for (int i = 0; i < parentlessCategories.Count; ++i)
                {
                    if (a == parentlessCategories[i].ID)
                    {
                        parentlessCategories.RemoveAt(i);
                        --i;
                    }
                }
            }
            foreach (var a in who.Records)
            {
                for (int i = 0; i < parentlessRecords.Count; ++i)
                {
                    if (parentlessRecords[i].ID == a)
                    {
                        parentlessRecords.RemoveAt(i);
                        --i;
                    }
                }
            }
        }

        private void OnCreated(Serializable ser)
        {
            var categoryList = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList;
            if(ser.GetType() == typeof(EncyclopedyCategory))
            {
                var added = (EncyclopedyCategory)ser;
                bool isSomewhere = false;
                for (int i = 0; i < categoryList.Count; ++i) 
                {
                    for (int j = 0; j < categoryList[i].ChildCategories.Count; ++j)
                    {
                        if (categoryList[i].ChildCategories[j] == added.ID)
                        {
                            isSomewhere = true;
                            break;
                        }
                    }
                    if (isSomewhere)
                        break;
                }
                if (!isSomewhere)
                    parentlessCategories.Add(added);
                removeKidsFromOrphanage(added);
            }
            else if (ser.GetType() == typeof(EncyclopedyRecord))
            {
                EncyclopedyRecord added = (EncyclopedyRecord)ser;
                bool isSomewhere = false;
                for (int i = 0; i< categoryList.Count; ++i)
                {
                    EncyclopedyCategory category = categoryList[i];
                    foreach(var id in category.Records)
                    {
                        if(id == added.ID)
                        {
                            isSomewhere = true;
                            break;
                        }
                    }
                    if (isSomewhere)
                        break;
                }
                if(!isSomewhere)
                    parentlessRecords.Add(added);
            }
            return;
        }


        private void setParentless()
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
    }
}
