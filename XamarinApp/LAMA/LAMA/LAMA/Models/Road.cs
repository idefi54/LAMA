using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using static LAMA.Models.CP;
using static Xamarin.Essentials.Permissions;
using Xamarin.Essentials;

namespace LAMA.Models
{
    public class Road:Serializable
    {
        long id;
        public long ID { get { return id; } set { id = value; updateValue(0, id.ToString()); } }
        EventList<Pair<double, double>> coordinates = new EventList<Pair<double, double>>();
        public EventList<Pair<double, double>> Coordinates { get { return coordinates; }}
        void onCoordinatesUpdated()
        {
            updateValue(1, coordinates.ToString());
        }
        EventList<double> color = new EventList<double> { 1.0, 0, 0, 1.0};
        public EventList<double> Color { get { return color; } }
        void onColorUpdated()
        {
            updateValue(2, color.ToString());
        }

        public Road()
        {
            coordinates.dataChanged += onCoordinatesUpdated;
            color.dataChanged += onColorUpdated;
        }
        public Road(long id)
        {
            this.id = id;
            coordinates.dataChanged += onCoordinatesUpdated;
            color.dataChanged += onColorUpdated;
        }
        double thickness;
        public double Thickness { get { return thickness; } set { thickness = value; updateValue(3,thickness.ToString()); } }


        RememberedList<Road, RoadStorage> list = null;
        public void addedInto(object holder)
        {
            list = holder as RememberedList<Road, RoadStorage>;
        }
        public void removed()
        {
            list = null;
        }
        void updateValue(int index, string newVal)
        {
            list?.sqlConnection.changeData(index, newVal, this);
        }


        static string[] attributes = new string[] { "ID", "coordinates", "color", "thickness"};

        public long getID()
        {
            return id;
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
                    coordinates = Helpers.readDoublePairField(value);
                    coordinates.dataChanged += onCoordinatesUpdated;
                    break;
                case 2:
                    color = Helpers.readDoubleField(value);
                    color.dataChanged += onColorUpdated;
                    break;
                case 3:
                    int i = 0;
                    thickness = Helpers.readDouble(value, ref i);
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
                case 2: return color.ToString();
                case 3: return thickness.ToString();
            }
            throw new Exception("wring index called");
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
            return 8;
        }

        public event EventHandler<int> IGotUpdated;
        public void InvokeIGotUpdated(int index)
        {
            IGotUpdated?.Invoke(this, index);
        }



    }
}
