using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    public class EncyclopedyCategory: Serializable
    {
        long _ID;
        string _name;
        string _description;
        EventList<long> _childCategories = new EventList<long>();
        EventList<long> _records = new EventList<long>();




        public long ID { get { return _ID; } }
        public string Name 
        { 
            get { return _name; }
            set 
            {
                _name = value;
                updateValue(1, value);
            }
        }
        public string Description
        {
            get { return _description; }
            set 
            { 
                _description = value;
                updateValue(2, value);
            }
        }
        public EventList<long> ChildCategories { get { return _childCategories; } }
        void onCategoryChanged()
        {
            updateValue(3, _childCategories.ToString());
        }
        public EventList<long> Records { get { return _records; } }
        void onRecordsChange()
        {
            updateValue(4, _records.ToString());
        }



        RememberedList<EncyclopedyCategory, EncyclopedyCategoryStorage> list = null;
        public void addedInto(object holder)
        {
            list = holder as RememberedList<EncyclopedyCategory, EncyclopedyCategoryStorage>;
        }
        public void removed()
        {
            list = null;
        }
        void updateValue(int index, string newVal)
        {
            list?.sqlConnection.changeData(index, newVal, this);
        }


        void initListeners()
        {
            _records.dataChanged += onRecordsChange;
            _childCategories.dataChanged += onCategoryChanged;
        }

        public EncyclopedyCategory()
        {
            initListeners();
        }
        public EncyclopedyCategory(long ID, string name, string description)
        {
            _ID = ID;
            _name = name;
            _description = description;
            initListeners();
        }

        public event EventHandler<int> IGotUpdated;
        public void InvokeIGotUpdated(int index)
        {
            IGotUpdated?.Invoke(this, index);
        }


        static string[] attributes = { "ID", "Name", "Description", "ChildCategories", "Records"};

        public void setAttribute(int i, string value)
        {
            switch(i)
            {
                case 0:
                    _ID = Helpers.readLong(value);
                    break;
                case 1: 
                    _name = value;
                    break;
                case 2: 
                    _description = value;
                    break;
                case 3:
                    _childCategories = Helpers.readLongField(value);
                    _childCategories.dataChanged += onCategoryChanged;
                    break;
                case 4: 
                    _records = Helpers.readLongField(value);
                    _records.dataChanged += onRecordsChange;
                    break;
            
            }
        }

        public string[] getAttributeNames()
        {
            return attributes;
        }

        public string getAttribute(int i)
        {
            switch(i)
            {
                case 0:
                    return _ID.ToString();
                case 1: return _name;
                case 2: return _description;
                case 3: return _childCategories.ToString();
                case 4: return _records.ToString();
            }
            throw new Exception("wrong index");
        }

        public string[] getAttributes()
        {
            string[] output = new string[attributes.Length];
            for (int i = 0; i < attributes.Length; ++i)
            {
                output[i] = getAttribute(i);
            }
            return output;
        }
        public int numOfAttributes()
        {
            return attributes.Length;
        }
        public long getID()
        { return _ID; }

        public void buildFromStrings(string[] input)
        {
            for (int i = 0; i < input.Length; ++i)
            {
                setAttribute(i, input[i]);
            }
        }
        public int getTypeID()
        {
            return 5;
        }

    }
}
