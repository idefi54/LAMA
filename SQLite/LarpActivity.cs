using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteTest
{
    class LarpActivity : Serializable
    {
        public enum EventType { normal, preparation};
        public enum Status { awaitingPrerequisites, readyToLaunch, launched, inProgress, completed };
        public LarpActivity() { }

        int ID;
        string name;
        string description;
        string preparationNeeded;
        EventType eventType;

        List<int> prerequisiteIDs = new List<int>();
        List<int> registeredPeople = new List<int>();


        Time duration;
        int day;
        Time start;

        //TODO 
        // required items

        (double, double) place;

        Status status;


        public int numOfAttributes()
        {
            return 12;
        }

        public string[] getAttributeNames()
        {
            return new string[] { "ID", "name", "description", "preparationNeeded", "eventType", "prerequisiteIDs",
                "registeredPeople", "duration", "day", "start", "place", "status"};
        }
        public string[] getAttributes()
        {
            return new string[] { ID.ToString(), name, description, preparationNeeded.ToString(), eventType.ToString(), prerequisiteIDs.ToString(),
            registeredPeople.ToString(), duration.ToString(), day.ToString(), start.ToString(), place.ToString(), status.ToString()};
        }
        public void setAttribute(int i, string value)
        {

            switch(i)
            {
                case 0:
                    ID = Helpers.readInt(value);
                    break;
                case 1:
                    name = value;
                    break;
                case 2:
                    description = value;
                    break;
                case 3:
                    preparationNeeded = value;
                    break;
                case 4:
                    eventType = (EventType)Helpers.findIndex( Enum.GetNames<EventType>(), value);
                    break;
                case 5:
                    prerequisiteIDs = Helpers.readIntField(value);
                    break;
                case 6:
                    registeredPeople = Helpers.readIntField(value);
                    break;
                case 7:
                    duration.setRawMinutes( Helpers.readInt(value));
                    break;
                case 8:
                    day = Helpers.readInt(value);
                    break;
                case 9:
                    start.setRawMinutes(Helpers.readInt(value));
                    break;
                case 10:
                    place = Helpers.readDoublePair(value);
                    break;
                case 11:
                    status = (Status)Helpers.findIndex(Enum.GetNames<Status>(), value);
                    break;

            }
        }



    }
}
