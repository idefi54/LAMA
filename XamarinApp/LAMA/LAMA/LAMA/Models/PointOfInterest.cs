using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using static LAMA.Models.CP;
using static Xamarin.Essentials.Permissions;
using Xamarin.Essentials;

namespace LAMA.Models
{
    public class PointOfInterest:Serializable
    {
        long id;
        public long ID { get { return id; } set { id = value; updateValue(0, value.ToString()); } }

        Pair<double, double> coordinates;
        public Pair<double, double> Coordinates { get { return coordinates; } set { coordinates = value; updateValue(1, value.ToString()); } }

        int icon;
        public int Icon { get { return icon; } set { icon = value; updateValue(2, value.ToString()); } }
        string name;
        public string Name { get { return name; } set { name = value; updateValue(3, value); } }
        public string description;
        public string Description { get { return description; } set { description = value; updateValue(4, value); } }

        RememberedList<PointOfInterest, PointOfInterestStorage> list = null;

        void updateValue(int index, string newVal)
        {
            list?.sqlConnection.changeData(index, newVal, this);
        }

        public void addedInto(object holder)
        {
            list = holder as RememberedList<PointOfInterest, PointOfInterestStorage>;
        }
        public void removed()
        {
            list = null;
        }

        static string[] attributes = new string[] { "ID", "coordinates", "icon", "name", "description" };
        public long getID()
        {
            return ID;
        }
        public int numOfAttributes()
        {
            return attributes.Length;
        }
        public string[] getAttributeNames()
        {
            return attributes;
        }

        public void setAttributeDatabase(int index, string value)
        {
            setAttribute(index, value);
            updateValue(index, value);
        }

        public void setAttribute(int index, string value)
        {
            switch (index)
            {
                case 0:
                    id = Helpers.readLong(value);
                    break;
                case 1:
                    coordinates = Helpers.readDoublePair(value);
                    break;
                case 2:
                    icon = Helpers.readInt(value);
                    break;
                case 3:
                    name = value;
                    break;
                case 4:
                    description = value;
                    break;
                }
        }

        public void buildFromStrings(string[] input)
        {
            for (int i = 0; i < input.Length; ++i)
            {
                setAttribute(i, input[i]);
            }
        }


        public string getAttribute(int i)
        {
            switch (i)
            {
                case 0: return id.ToString();
                case 1: return coordinates.ToString();
                case 2: return icon.ToString();
                case 3 : return name;
                case 4 : return description;
            }
            throw new Exception("wrong index called");
        }
        public string[] getAttributes()
        {
            List<string> output = new List<string>();
            for (int i = 0; i < numOfAttributes(); ++i)
            {
                output.Add(getAttribute(i));
            }
            return output.ToArray();
        }

        public int getTypeID()
        {
            return 7;
        }

        public event EventHandler<int> IGotUpdated;
        public void InvokeIGotUpdated(int index)
        {
            IGotUpdated?.Invoke(this, index);
        }

        public PointOfInterest()
        {

        }
        public PointOfInterest(long ID, Pair<double, double> coordinates, int icon, string name, string description)
        {
            id = ID;
            this.coordinates = coordinates;
            this.icon = icon;
            this.name = name;
            this.description = description;
        }

    }
}
