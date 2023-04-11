using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    public class EncyclopedyRecord: Serializable
    {
        long _ID;

        string _name;
        string _tldr;
        string _fullText;

        public long ID { get { return _ID; } }
        public string Name { get { return _name; }
            set
            {
                _name = value;
                updateValue(1, value);
            }
        }
        public string TLDR { get { return _tldr; }
            set 
            {
                _tldr = value;
                updateValue(2, value);
            }
        }
        public string FullText { get { return _fullText; }
            set 
            {
                _fullText = value;
                updateValue(3, value);
            }
        }

        public EncyclopedyRecord()
        {

        }
        public EncyclopedyRecord(long ID, string name, string tldr, string fullText)
        {
            this._ID = ID;
            this._name = name;
            this._tldr = tldr;
            this._fullText = fullText;
        }

        RememberedList<EncyclopedyRecord, EncyclopedyRecordStorage> list = null;
        public void addedInto(object holder)
        {
            list = holder as RememberedList<EncyclopedyRecord, EncyclopedyRecordStorage>;
        }
        public void removed()
        {
            list = null;
        }
        void updateValue(int index, string newVal)
        {
            list?.sqlConnection.changeData(index, newVal, this);
        }


        public event EventHandler<int> IGotUpdated;
        public void InvokeIGotUpdated(int index)
        {
            IGotUpdated?.Invoke(this, index);
        }


        static string[] attributes = { "ID", "Name", "TLDR", "FullText" };

        public int getTypeID()
        {
            return 6;
        }

        public void setAttributeDatabase(int index, string value)
        {
            setAttribute(index, value);
            updateValue(index, value);
        }

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
                    _tldr = value;
                    break;
                case 3:
                    _fullText = value;
                    break;
                default:
                    throw new Exception("wrongIndex");
            }
            
        }
        public string getAttribute(int i)
        {
            switch(i)
            {
                case 0:
                    return ID.ToString();
                case 1: return _name;
                case 2: return _tldr;
                case 3: return _fullText;
            }
            throw new Exception("wrong index");
        }
        public string[] getAttributeNames()
        {
            return attributes;
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
        {
            return _ID;
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
