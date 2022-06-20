using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    public class InventoryItem : Serializable
    {
        int _ID;
        public int ID { get { return _ID; } }

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

        EventList<int> _takenBy = new EventList<int>();
        public EventList<int> takenBy { get { return _takenBy; } }
        void onTakenUpdate()
        {
            updateValue(5, _takenBy.ToString());
        }






        void updateValue(int i, string data)
        {
            var list = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList;
            list.sqlConnection.changeData(i, data, this);
        }

        public InventoryItem()
        {
            _takenBy.dataChanged += onTakenUpdate;
        }

        public InventoryItem(int ID, string name, string description, int taken, int free)
        {
            _takenBy.dataChanged += onTakenUpdate;
            _ID = ID; 
            _name = name;
            _description = description;
            _taken = taken;
            _free = free;
        }



        static string[] attributeNames = new string[] { "ID", "name", "description", "taken", "free", "takenBy" };
        public int getID()
        {
            return _ID;
        }
        public void setAttribute(int i, string s)
        {
            switch(i)
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
                    _takenBy = Helpers.readIntField(s);
                    _takenBy.dataChanged += onTakenUpdate;
                    break;
            }
        }
        public string getAttribute(int i)
        {
            switch(i)
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


    }
}
