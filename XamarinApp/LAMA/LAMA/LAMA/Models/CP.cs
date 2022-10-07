using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA.Models
{
    public class CP : Serializable
    {
        public enum Status { ready, onBreak, onActivity};

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

        string _nick;
        public string nick
        {
            get { return _nick; }
            set
            {
                _nick = value;
                updateValue(2, value);
            }
        }
        EventList<string> _roles =  new EventList<string>();
        public EventList<string> roles
        {
            get { return _roles; }
        }
        void onRolesChange()
        {
            updateValue(3, _roles.ToString());
        }


        string _phone;
        public string phone
        {
            get { return _phone; }
            set { _phone = value; updateValue(4, value.ToString()); }
        }
        string _facebook;
        public string facebook { get { return _facebook; } set { _facebook = value; updateValue(5, value); } }
        string _discord;
        public string discord { get { return _discord; } set { _discord = value; updateValue(6, value); } }
        Pair<double, double> _location;
        public Pair<double, double> location { get { return _location; }  set { _location = value; updateValue(7, value.ToString()); } }
        string _notes;
        public string notes { get { return _notes; } set { _notes = value; updateValue(8, value); } }



        public CP()
        {
            _roles.dataChanged += onRolesChange;
        }
        public CP(long ID, string name, string nick, EventList<string> roles, string phone,
            string facebook, string discord, string notes)
        {
            _roles.dataChanged += onRolesChange;
            _ID = ID;
            _name = name;
            _nick = nick;
            _roles = roles;
            _phone = phone;
            _facebook = facebook;
            _discord = discord;
            _notes = notes;
        }
        public void updateWhole(string name, string nick, string phone, string facebook, string discord, string notes)
        {
            if (name != _name)
                this.name = name;
            if(_nick != nick)
                this.nick = nick;
            if(phone != _phone)
                this.phone = phone;
            if(discord != _discord)
                this.discord = discord;
            if (notes != _notes)
                this.notes = notes;
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
            "discord", "location", "notes" };

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
            switch(index)
            {
                case 0: _ID = Helpers.readInt(value);
                    break;
                case 1: _name = value;
                    break;
                case 2: _nick = value;
                    break;
                case 3: _roles = Helpers.readStringField(value);
                    _roles.dataChanged += onRolesChange;
                    break;
                case 4: _phone = value;
                    break;
                case 5: _facebook = value;
                    break;
                case 6: _discord = value;
                    break;
                case 7: _location = Helpers.readDoublePair(value);
                    break;
                case 8:_notes = value;
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
            switch(i)
            {
                case 0: return _ID.ToString();
                case 1: return _name;
                case 2: return _nick;
                case 3: return _roles.ToString();
                case 4: return _phone.ToString();
                case 5: return _facebook;
                case 6: return _discord;
                case 7: return _location.ToString();
                case 8: return _notes;
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
