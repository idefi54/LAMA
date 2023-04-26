using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static LAMA.Models.LarpActivity;

namespace LAMA.Models.DTO
{
    public class LarpActivityDTO
    {
        public long ID {get; set;}
        public string name {get; set;}
        public string description {get; set;}
        public string preparationNeeded {get; set;}
        public EventType eventType {get; set;}
        public List<long> prerequisiteIDs {get; set;}
        public long duration {get; set;}
        public int day {get; set;}
        public long start {get; set;}
        public double GraphY { get; set; }
        public Pair<double, double> place {get; set;}
        public Status status {get; set;}
        public List<Pair<long, int>> requiredItems {get; set;}
        public List<Pair<string, int>> roles {get; set;}
        public List<Pair<long, string>> registrationByRole {get; set;}

        public LarpActivityDTO(LarpActivity larpActivity)
        {
            ID = larpActivity.ID;
            name = larpActivity.name;
            description = larpActivity.description;
            preparationNeeded = larpActivity.preparationNeeded;
            eventType = larpActivity.eventType;
            prerequisiteIDs = larpActivity.prerequisiteIDs.Select(x => x).ToList();
            duration = larpActivity.duration;
            day = larpActivity.day;
            start = larpActivity.start;
            GraphY = larpActivity.GraphY;
            place = larpActivity.place;
            status = larpActivity.status;
            requiredItems = larpActivity.requiredItems.Select(x => x).ToList();
            roles = larpActivity.roles.Select(x=>x).ToList();
            registrationByRole = larpActivity.registrationByRole.Select(x => x).ToList();
        }

        public LarpActivity CreateLarpActivity()
        {
            var larpActivity = new LarpActivity(ID, name, description, preparationNeeded, eventType,
                new EventList<long>(prerequisiteIDs),
                duration, day, start, place, status,
                new EventList<Pair<long, int>>(requiredItems),
                new EventList<Pair<string, int>>(roles),
                new EventList<Pair<long, string>>(registrationByRole));

            // Because it's not in the constructor...
            larpActivity.GraphY = GraphY;

            return larpActivity;
        }
    }
}


