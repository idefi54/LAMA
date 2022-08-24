using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static LAMA.Models.LarpActivity;

namespace LAMA.Models.DTO
{
    internal class LarpActivityDTO
    {
        public int ID {get; set;}
        public string name {get; set;}
        public string description {get; set;}
        public string preparationNeeded {get; set;}
        public EventType eventType {get; set;}
        public List<int> prerequisiteIDs {get; set;}
        public Time duration {get; set;}
        public int day {get; set;}
        public Time start {get; set;}
        public Pair<double, double> place {get; set;}
        public Status status {get; set;}
        public List<Pair<int, int>> requiredItems {get; set;}
        public List<Pair<string, int>> roles {get; set;}
        public List<Pair<int, string>> registrationByRole {get; set;}

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
            place = larpActivity.place;
            status = larpActivity.status;
            requiredItems = larpActivity.requiredItems.Select(x => x).ToList();
            roles = larpActivity.roles.Select(x=>x).ToList();
            registrationByRole = larpActivity.registrationByRole.Select(x => x).ToList();
        }

        public LarpActivity CreateLarpActivity()
        {
            return new LarpActivity(ID, name, description, preparationNeeded, eventType,
                new EventList<int>(prerequisiteIDs),
                duration, day, start, place, status,
                new EventList<Pair<int, int>>(requiredItems),
                new EventList<Pair<string, int>>(roles),
                new EventList<Pair<int, string>>(registrationByRole));
        }
    }
}


