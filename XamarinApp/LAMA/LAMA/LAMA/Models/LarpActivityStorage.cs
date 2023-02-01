using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace LAMA.Models
{
    internal class LarpActivityStorage : Database.StorageInterface
    {
        //{ "ID", "name", "description", "preparationNeeded", "eventType", "prerequisiteIDs",
            //"duration", "day", "start", "place", "status", "requiredItems", "roles", "registrationByRole" };
        [PrimaryKey]
        public long ID { get; set; }

        public string name { get; set; }
        public string description { get; set; }
        public string preparationNeeded { get; set; }
        public string eventType { get; set; }
        public string prerequisiteIDs { get; set; }
        public string duration { get; set; }
        public string day { get; set; }
        public string start { get; set; }
        public string place { get; set; }
        public string status { get; set; }
        public string requiredItems { get; set; }
        public string roles { get; set; }
        public string registrationByRole { get; set; }
        public long lastChange { get; set; }
        public string graphY { get; set; }
        public void makeFromStrings(string[] input)
        {
            ID = Helpers.readLong(input[0]);
            name = input[1];
            description = input[2];
            preparationNeeded = input[3];
            eventType = input[4];
            prerequisiteIDs = input[5];
            duration = input[6];
            day = input[7];
            start = input[8];
            place = input[9];
            status = input[10];
            requiredItems = input[11];
            roles = input[12];
            registrationByRole = input[13];
            graphY = input[14];
        }
        public string[] getStrings()
        {
            return new string[] { ID.ToString(), name, description, preparationNeeded, eventType, prerequisiteIDs, duration, day, start, place,
            status, requiredItems, roles, registrationByRole, graphY};
        }
        public long getID()
        { return ID; }
    }
}
