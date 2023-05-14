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
        public enum Status { awaitingPrerequisites, readyToLaunch, launched, inProgress, completed, cancelled };

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

        EventList<long> _prerequisiteIDs = new EventList<long>();
        public EventList<long> prerequisiteIDs
        {
            get { return _prerequisiteIDs; }
        }
        void onPrerequisiteChange() { updateValue(5, _prerequisiteIDs.ToString()); }



        long _duration = 0;
        public long duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
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

        long _start = 0;
        public long start
        {
            get { return _start; }
            set
            {
                _start = value;
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

        EventList<Pair<long, int>> _requiredItems = new EventList<Pair<long, int>>();
        public EventList<Pair<long, int>> requiredItems
        {
            get { return _requiredItems;}
        }
        void requiredItemsChange() { updateValue(11, _requiredItems.ToString()); }

        // name of role and how many people are needed for it
        EventList<Pair<string, int>> _roles = new EventList<Pair<string, int>>();
        public EventList<Pair<string, int>> roles { get { return _roles; } }
        void rolesChange() { updateValue(12, _roles.ToString()); }
        // person ID, to what role they are registered
        EventList<Pair<long, string>> _registrationByRole = new EventList<Pair<long, string>>();
        public EventList<Pair<long, string>> registrationByRole { get { return _registrationByRole; } }
        void registrationByRoleChanged() { updateValue(13, _registrationByRole.ToString()); }

        double _graphY = -1;
        public double GraphY
        {
            get { return _graphY; }
            set
            {
                _graphY = value;
                updateValue(14, _graphY.ToString());
            }
        }

        int _iconIndex;
        public int IconIndex
        {
            get { return _iconIndex; }
            set
            {
                _iconIndex = value;
                updateValue(15, _iconIndex.ToString());
            }
        }

        void initChangeListeners()
        {
            _prerequisiteIDs.dataChanged += onPrerequisiteChange;
            _requiredItems.dataChanged += requiredItemsChange;
            _roles.dataChanged += rolesChange;
            _registrationByRole.dataChanged += registrationByRoleChanged;
        }
        public LarpActivity()
        {
            initChangeListeners();
        }
        public LarpActivity(long ID, string name, string description, string preparation, EventType eventType, EventList<long> prerequisiteIDs,
            long duration, int day, long start, Pair<double, double> place, Status status, EventList<Pair<long, int>>requiredItems, 
            EventList<Pair<string, int>> roles, EventList<Pair<long, string>> registrations, int iconIndex = 0)
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
            _iconIndex = iconIndex;

            initChangeListeners();
        }

        public void UpdateWhole(string name, string description, string preparation, EventType eventType, long duration, int day,
            long start, Pair<double, double> place, Status status, int icon)
        {
            if (name != _name)
                this.name = name;
            if(_description!=description)
                this.description = description;
            if(_preparationNeeded != preparation)
                this.preparationNeeded = preparation;
            if(_eventType != eventType)
                this.eventType = eventType;
            if(duration!=_duration)
                this.duration = duration;
            if (day != _day)
                this.day = day;
            if (start != _start)
                this.start = start;
            if (!place.Equals(_place))
                this.place = place;
            if (status != _status)
                this.status = status;
            if (icon != _iconIndex)
                this.IconIndex = icon;
        }

        public void UpdatePrerequisiteIDs(List<long> newPrerequisites)
        {
            for(int i = prerequisiteIDs.Count - 1; i >= 0; i--)
            {
                if(!newPrerequisites.Contains(prerequisiteIDs[i]))
                    prerequisiteIDs.RemoveAt(i);
            }

            foreach(long id in newPrerequisites)
            {
                if (!prerequisiteIDs.Contains(id))
                    prerequisiteIDs.Add(id);
            }
        }

        public void UpdateRoles(List<Pair<string,int>> newRoles)
        {
            for (int i = roles.Count - 1; i >= 0; i--)
            {
                var machRoles = newRoles.Where((x) => { return x.first == roles[i].first; });
                if (machRoles.Count() == 0)
				{
                    CompletelyRemoveRole(i);
				}
                else
				{
                    roles[i] = machRoles.First();
				}
            }

            foreach (Pair<string,int> id in newRoles)
            {
                if (roles.Where((x) => { return x.first == id.first; }).Count() == 0)
                    roles.Add(id);
            }
        }

        public void CompletelyRemoveRole(string name)
		{
            for (int i = roles.Count - 1; i >= 0; i--)
			{
                if (roles[i].first == name)
				{
                    CompletelyRemoveRole(i);
                    return;
				}
			}
		}

        public void CompletelyRemoveRole(int index)
		{
            if (index >= roles.Count || index < 0)
                return;

            Pair<string, int> roleToRemove = roles[index];
            registrationByRole.RemoveAll(x => x.second == roleToRemove.first);
            roles.RemoveAt(index);

            roles.RemoveAll(x => x.first == roleToRemove.first);
        }

        public void UpdateItems(List<Pair<long,int>> newItems)
        {
            for (int i = requiredItems.Count - 1; i >= 0; i--)
            {
                var machItems = newItems.Where((x) => { return x.first == requiredItems[i].first; });
                if (machItems.Count() == 0)
				{
                    CompletelyRemoveItem(i);
				}
                else
				{
                    requiredItems[i] = machItems.First();
				}
            }

            foreach (Pair<long,int> id in newItems)
            {
                if (requiredItems.Where((x) => { return x.first == id.first; }).Count() == 0)
                    requiredItems.Add(id);
            }
        }

        public void CompletelyRemoveItem(int index)
        {
            if (index >= requiredItems.Count || index < 0)
                return;

            Pair<long, int> itemToRemove = requiredItems[index];
            requiredItems.RemoveAt(index);

            requiredItems.RemoveAll(x => x.first == itemToRemove.first);
        }


        RememberedList<LarpActivity, LarpActivityStorage> list = null;
        public void removed()
        {
            list = null;
        }
        public void addedInto(object holder)
        {
            list = holder as RememberedList<LarpActivity, LarpActivityStorage>;
        }

        //helper function to update values inside SQL
        void updateValue(int index, string newVal)
        {
            
            list?.sqlConnection.changeData(index, newVal, this);
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
            "duration", "day", "start", "place", "status", "requiredItems", "roles", "registrationByRole", "GraphY" };
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
                    return _preparationNeeded != null?_preparationNeeded.ToString():"";
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
                case 14:
                    return _graphY.ToString();
                case 15:
                    return _iconIndex.ToString();
            }
            throw new Exception("wrong index selected");
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
                    _description = value;
                    break;
                case 3:
                    _preparationNeeded = value;
                    break;
                case 4:
                    _eventType = (EventType)Helpers.findIndex( Enum.GetNames(typeof(EventType)), value);
                    break;
                case 5:
                    _prerequisiteIDs = Helpers.readLongField(value);
                    _prerequisiteIDs.dataChanged += onPrerequisiteChange;
                    break;
                case 6:
                    _duration = Helpers.readLong(value);
                    break;
                case 7:
                    _day = Helpers.readInt(value);
                    break;
                case 8:
                    _start = Helpers.readLong(value);
                    break;
                case 9:
                    _place = Helpers.readDoublePair(value);
                    break;
                case 10:
                    _status = (Status)Helpers.findIndex(Enum.GetNames(typeof(Status)), value);
                    break;
                case 11:
                    _requiredItems = Helpers.readLongIntPairField(value);
                    _requiredItems.dataChanged += requiredItemsChange;
                    break;
                case 12:
                    _roles = Helpers.readStringIntPairField(value);
                    _roles.dataChanged += rolesChange;
                    break;
                case 13:
                    _registrationByRole = Helpers.readLongStringPairField(value);
                    _registrationByRole.dataChanged += registrationByRoleChanged;
                    break;
                case 14:
                    int j = 0;
                    _graphY = Helpers.readDouble(value, ref j);
                    break;
                case 15:
                    _iconIndex = Helpers.readInt(value);
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
        public const int typeID = 3;
        public int getTypeID()
        {
            return 3;
        }


        public event EventHandler<int> IGotUpdated;
        public void InvokeIGotUpdated(int index)
        {
            IGotUpdated?.Invoke(this, index);
        }
    }
}
