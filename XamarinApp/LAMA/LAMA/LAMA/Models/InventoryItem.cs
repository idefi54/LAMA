using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    public class InventoryItem : Serializable
    {
        long _ID;
        public long ID { get { return _ID; } }

        string _name;
        public string name
        {
            get { return _name; }
            set
            {
                _name = value;
                updateValue(1, value);
            }
        }
        string _description;
        public string description
        {
            get { return _description; }
            set
            {
                _description = value;
                updateValue(2, value);
            }
        }
        int _taken = 0;
        public int taken
        {
            get { return _taken; }
            set
            {
                _taken = value;
                updateValue(3, value.ToString());
            }
        }

        int _free = 0;
        public int free
        {
            get { return _free; }
            set { updateValue(4, value.ToString()); }
        }

        EventList<Pair<long,int>> _takenBy = new EventList<Pair<long, int>>();
        public EventList<Pair<long, int>> takenBy { get { return _takenBy; } }
        void onTakenUpdate()
        {
            updateValue(5, _takenBy.ToString());
        }




        RememberedList<InventoryItem, InventoryItemStorage> list = null;
        public void removed()
        {
            list = null;
        }
        public void addedInto(object holder)
        {
            list = holder as RememberedList<InventoryItem, InventoryItemStorage>;
        }

        void updateValue(int i, string data)
        {
            list?.sqlConnection.changeData(i, data, this);
        }

        public InventoryItem()
        {
            _takenBy.dataChanged += onTakenUpdate;
        }

        public InventoryItem(long ID, string name, string description, int taken, int free)
        {
            _takenBy.dataChanged += onTakenUpdate;
            _ID = ID;
            _name = name;
            _description = description;
            _taken = taken;
            _free = free;
        }
        public void updateWhole(string name, string description, int taken, int free)
        {
            if (name != _name)
                this.name = name;
            if (description != _description)
                this.description = description;
            if(taken != _taken)
                this.taken = taken;
            if(free != _free)
                this.free = free;
        }



        static string[] attributeNames = new string[] { "ID", "name", "description", "taken", "free", "takenBy" };
        public long getID()
        {
            return _ID;
        }
        public void setAttribute(int i, string s)
        {
            switch (i)
            {
                case 0:
                    _ID = Helpers.readInt(s);
                    break;
                case 1:
                    _name = s;
                    break;
                case 2:
                    _description = s;
                    break;
                case 3:
                    _taken = Helpers.readInt(s);
                    break;
                case 4:
                    _free = Helpers.readInt(s);
                    break;
                case 5:
                    _takenBy = Helpers.readLongIntPairField(s);
                    _takenBy.dataChanged += onTakenUpdate;
                    break;
            }
        }
        public string getAttribute(int i)
        {
            switch (i)
            {
                case 0: return _ID.ToString();
                case 1: return _name;
                case 2: return _description;
                case 3: return _taken.ToString();
                case 4: return _free.ToString();
                case 5: return _takenBy.ToString();
            }
            throw new Exception("wrong index");
        }
        public string[] getAttributeNames()
        {
            return attributeNames;
        }
        public string[] getAttributes()
        {
            List<string> output = new List<string>();
            for (int i = 0; i < attributeNames.Length; ++i)
            {
                output.Add(getAttribute(i));
            }
            return output.ToArray();
        }

        public int numOfAttributes()
        {
            return attributeNames.Length;
        }
        public void buildFromStrings(string[] input)
        {
            for (int i = 0; i < input.Length; ++i)
            {
                setAttribute(i, input[i]);
            }
        }
        public const int typeID = 2;
        public int getTypeID()
        {
            return 2;
        }





        public void Borrow(int howMany)
        {
            if (howMany <= free)
            {
                free -= howMany;
                taken += howMany;
            }
        }

        private int findBorrowerIndex(long borrower)
        {
            for (int i = 0; i < takenBy.Count; ++i) 
            {
                if(takenBy[i].first == borrower)
                    return i;
            }
            return -1;
        }

        public void Borrow(int howMany, long who)
        {
            if (howMany <= free)
            {
                free -= howMany;
                taken += howMany;
                int index = findBorrowerIndex(who);
                if (index == -1)
                {
                    takenBy.Add(new Pair<long, int>(who, howMany));
                }
                else
                {
                    takenBy[index] = new Pair<long, int>(who, takenBy[index].second + howMany);
                }
            }
        }
        public void Return (int howMany)
        {
            if(howMany <= taken)
            {       
                taken -= howMany;
                free += howMany;
            }
        }
        public void Return (int howMany, long who)
        {
            int index = findBorrowerIndex(who);
            if (howMany <= taken && index != -1 && takenBy[index].second >= howMany) 
            {
                free += howMany;
                taken -= howMany;

                if (takenBy[index].second == howMany)
                    takenBy.RemoveAt(index);
                else
                    takenBy[index] = new Pair<long, int>(who, takenBy[index].second - howMany);
            }
        }
    }
}
