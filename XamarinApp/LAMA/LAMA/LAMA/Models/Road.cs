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
        EventList<double> color = new EventList<double>();
        public EventList<double> Color { get { return color; } }
        void onColorUpdated()
        {
            updateValue(2, color.ToString());
        }

        public Road()
        {
            coordinates.dataChanged += onCoordinatesUpdated;
        }


        RememberedList<CP, CPStorage> list = null;
        public void addedInto(object holder)
        {
            list = holder as RememberedList<CP, CPStorage>;
        }
        public void removed()
        {
            list = null;
        }
        void updateValue(int index, string newVal)
        {
            list?.sqlConnection.changeData(index, newVal, this);
        }


        static string[] attributes = new string[] { "ID", "name", "nick", "roles", "phone", "facebook",
            "discord", "location", "notes", "permissions" };

        public long getID()
        {
            return _ID;
        }
        public int numOfAttributes()
        {
            return attributes.Length;
        }
        public string[] getAttributeNames()
        {
            return attributes;
        }
        public void setAttribute(int index, string value)
        {
            switch (index)
            {
                case 0:
                    _ID = Helpers.readLong(value);
                    break;
                case 1:
                    _name = value;
                    break;
                case 2:
                    _nick = value;
                    break;
                case 3:
                    _roles = Helpers.readStringField(value);
                    _roles.dataChanged += onRolesChange;
                    break;
                case 4:
                    _phone = value;
                    break;
                case 5:
                    _facebook = value;
                    break;
                case 6:
                    _discord = value;
                    break;
                case 7:
                    _location = Helpers.readDoublePair(value);
                    break;
                case 8:
                    _notes = value;
                    break;
                case 9:
                    var temp = Helpers.readIntField(value);
                    _permissions = new EventList<PermissionType>();
                    foreach (var a in temp)
                    {
                        _permissions.Add((PermissionType)a);
                    }
                    _permissions.dataChanged += onPermissionsChange;
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
                case 0: return _ID.ToString();
                case 1: return _name;
                case 2: return _nick;
                case 3: return _roles.ToString();
                case 4: return _phone;
                case 5: return _facebook;
                case 6: return _discord;
                case 7: return _location.ToString();
                case 8: return _notes;
                case 9: return Helpers.EnumEventListToString(_permissions);
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
            return 1;
        }

        public event EventHandler<int> IGotUpdated;
        public void InvokeIGotUpdated(int index)
        {
            IGotUpdated?.Invoke(this, index);
        }



    }
}
