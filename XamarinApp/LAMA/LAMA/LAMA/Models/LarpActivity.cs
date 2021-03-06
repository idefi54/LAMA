using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA.Models
{
    public class LarpActivity : Serializable
    {
        public enum EventType { normal, preparation };
        public enum Status { awaitingPrerequisites, readyToLaunch, launched, inProgress, completed };

        int _ID;
        public int ID { get { return _ID; } }

        string _name;
        public string name
        {
            get { return _name; }
            set
            {
                name = value;
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
        string _preparationNeeded;
        public string preparationNeeded
        {
            get { return _preparationNeeded; }
            set
            {
                _preparationNeeded = value;
                updateValue(3, value);
            }
        }
        EventType _eventType;
        public EventType eventType
        {
            get { return _eventType; }
            set
            {
                _eventType = value;
                updateValue(4, value.ToString());
            }
        }

        EventList<int> _prerequisiteIDs = new EventList<int>();
        public EventList<int> prerequisiteIDs
        {
            get { return _prerequisiteIDs; }
        }
        void onPrerequisiteChange() { updateValue(5, _prerequisiteIDs.ToString()); }



        Time _duration = new Time();
        public Time duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                _duration.dataChanged += durationChanged;
                updateValue(6, value.ToString());
            }
        }
        void durationChanged() { updateValue(6, _duration.ToString()); }
        int _day;
        public int day
        {
            get { return _day; }
            set
            {
                _day = value;
                updateValue(7, value.ToString());
            }
        }

        Time _start = new Time();
        public Time start
        {
            get { return _start; }
            set
            {
                _start = value;
                start.dataChanged += startChanged;
                updateValue(8, _start.ToString());
            }
        }
        void startChanged() { updateValue(8, _start.ToString()); }
        Pair<double, double> _place;
        public Pair<double, double> place
        {
            get { return _place; }
            set
            {
                _place = value;
                updateValue(9, _place.ToString());
            }
        }

        Status _status;
        public Status status
        {
            get { return _status; }
            set 
            { 
                _status = value;
                updateValue(10, value.ToString());
            }
        }

        EventList<Pair<int, int>> _requiredItems = new EventList<Pair<int, int>>();
        public EventList<Pair<int, int>> requiredItems
        {
            get { return _requiredItems;}
        }
        void requiredItemsChange() { updateValue(11, _requiredItems.ToString()); }

        // name of role and how many people are needed for it
        EventList<Pair<string, int>> _roles = new EventList<Pair<string, int>>();
        public EventList<Pair<string, int>> roles { get { return _roles; } }
        void rolesChange() { updateValue(12, _roles.ToString()); }
        // person ID, to what role they are registered
        EventList<Pair<int, string>> _registrationByRole = new EventList<Pair<int, string>>();
        public EventList<Pair<int, string>> registrationByRole { get { return _registrationByRole; } }
        void registrationByRoleChanged() { updateValue(13, _registrationByRole.ToString()); }


        void initChangeListeners()
        {
            _prerequisiteIDs.dataChanged += onPrerequisiteChange;
            _duration.dataChanged += durationChanged;
            _start.dataChanged += startChanged;
            _requiredItems.dataChanged += requiredItemsChange;
            _roles.dataChanged += rolesChange;
            _registrationByRole.dataChanged += registrationByRoleChanged;
        }
        public LarpActivity()
        {
            initChangeListeners();
        }
        public LarpActivity(int ID, string name, string description, string preparation, EventType eventType, EventList<int> prerequisiteIDs,
            Time duration, int day, Time start, Pair<double, double> place, Status status, EventList<Pair<int, int>>requiredItems, 
            EventList<Pair<string, int>> roles, EventList<Pair<int, string>> registrations)
        {
            _ID = ID;
            _name = name;
            _description = description;
            _preparationNeeded = preparation;
            _eventType = eventType;
            _prerequisiteIDs = prerequisiteIDs;
            _duration = duration;
            _day = day;
            _start = start;
            _place = place;
            _status = status;
            _requiredItems = requiredItems;
            _roles = roles;
            _registrationByRole = registrations;

            initChangeListeners();
        }






        //helper function to update values inside SQL
        void updateValue(int index, string newVal)
        {
            var list = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList;
            list.sqlConnection.changeData(index, newVal, this);
        }

        // interface Serializable implementation
        public long getID()
        {
            return _ID;
        }
        public int numOfAttributes()
        {
            return attributeNames.Length;
        }
        
        static string[] attributeNames = { "ID", "name", "description", "preparationNeeded", "eventType", "prerequisiteIDs",
            "duration", "day", "start", "place", "status", "requiredItems", "roles", "registrationByRole" };
        public string[] getAttributeNames()
        {
            return attributeNames;
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

        public string getAttribute(int index)
        {
            switch(index)
            {
                case 0:
                    return _ID.ToString();
                case 1:
                    return _name;
                case 2:
                    return _description;
                case 3:
                    return _preparationNeeded.ToString();
                case 4:
                    return _eventType.ToString();
                case 5:
                    return _prerequisiteIDs.ToString();
                case 6:
                    return _duration.ToString();
                case 7: 
                    return _day.ToString();
                case 8: 
                    return _start.ToString();
                case 9: 
                    return _place.ToString();
                case 10:
                    return _status.ToString();
                case 11:
                    return _requiredItems.ToString();
                case 12:
                    return _roles.ToString();
                case 13:
                    return _registrationByRole.ToString();
            }
            throw new Exception("wrong index selected");
        }
        public void setAttribute(int i, string value)
        {

            switch(i)
            {
                case 0:
                    _ID = Helpers.readInt(value);
                    break;
                case 1:
                    _name = value;
                    break;
                case 2:
                    _description = value;
                    break;
                case 3:
                    _preparationNeeded = value;
                    break;
                case 4:
                    _eventType = (EventType)Helpers.findIndex( Enum.GetNames(typeof(EventType)), value);
                    break;
                case 5:
                    _prerequisiteIDs = Helpers.readIntField(value);
                    _prerequisiteIDs.dataChanged += onPrerequisiteChange;
                    break;
                case 6:
                    _duration.setRawMinutes( Helpers.readInt(value), false);
                    break;
                case 7:
                    _day = Helpers.readInt(value);
                    break;
                case 8:
                    _start.setRawMinutes(Helpers.readInt(value), false);
                    break;
                case 9:
                    _place = Helpers.readDoublePair(value);
                    break;
                case 10:
                    _status = (Status)Helpers.findIndex(Enum.GetNames(typeof(Status)), value);
                    break;
                case 11:
                    _requiredItems = Helpers.readIntPairField(value);
                    _requiredItems.dataChanged += requiredItemsChange;
                    break;
                case 12:
                    _roles = Helpers.readStringIntPairField(value);
                    _roles.dataChanged += rolesChange;
                    break;
                case 13:
                    _registrationByRole = Helpers.readIntStringPairField(value);
                    _registrationByRole.dataChanged += registrationByRoleChanged;
                    break;

            }
        }

        private void _requiredItems_dataChanged()
        {
            throw new NotImplementedException();
        }

        private void _prerequisiteIDs_dataChanged()
        {
            throw new NotImplementedException();
        }

        public void buildFromStrings(string[] input)
        {
            for (int i = 0; i < input.Length; ++i) 
            {
                setAttribute(i, input[i]);
            }
        }
        public const int typeID = 3;
        public int getTypeID()
        {
            return 3;
        }

    }
}
