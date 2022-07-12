﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    internal class IntervalCommunicationManagerServer
    {
        private ServerCommunicator communicator;

        public IntervalCommunicationManagerServer(ServerCommunicator communicator)
        {
            this.communicator = communicator;
        }
        public void OnIntervalRequestCP()
        {
            OnIntervalRequest("LAMA.Models.CP");
        }

        public void OnIntervalRequestInventoryItem()
        {
            OnIntervalRequest("LAMA.Models.InventoryItem");
        }

        public void OnIntervalRequestLarpActivity()
        {
            OnIntervalRequest("LAMA.Models.LarpActivity");
        }

        private void OnIntervalRequest(string type)
        {
            LAMA.Database.Interval interval = new Database.Interval();
            if (type == "LAMA.Models.LarpActivity")
            {
                interval = Database.IntervalManager<Models.LarpActivity>.GiveNewInterval(0);
            }
            else if (type == "LAMA.Models.CP")
            {
                interval = Database.IntervalManager<Models.CP>.GiveNewInterval(0);
            }
            else if (type == "LAMA.Models.InventoryItem")
            {
                interval = Database.IntervalManager<Models.InventoryItem>.GiveNewInterval(0);
            }
            string command = "Interval" + ";" + "Add" + ";" + type + ";" + interval.start + ";" + interval.end;
            communicator.SendCommand(new Command(command, "None"));
        }

        public void IntervalsUpdate(string intervalCommand, string objectType, int lowerLimit, int upperLimit, int id, string command)
        {
            if (intervalCommand == "Request")
            {
                if (objectType == "LAMA.Models.LarpActivity")
                {
                    var interval = Database.IntervalManager<Models.LarpActivity>.GiveNewInterval(id);
                    string commandToSend = "Interval" + ";" + "Add" + ";" + "LarpActivity" + ";" + interval.start + ";" + interval.end + ";" + id;
                    communicator.SendCommand(new Command(commandToSend, "None"));
                }
                else if (objectType == "LAMA.Models.CP")
                {
                    var interval = Database.IntervalManager<Models.CP>.GiveNewInterval(id);
                    string commandToSend = "Interval" + ";" + "Add" + ";" + "CP" + ";" + interval.start + ";" + interval.end + ";" + id;
                    communicator.SendCommand(new Command(commandToSend, "None"));
                }
                else if (objectType == "LAMA.Models.InventoryItem")
                {
                    var interval = Database.IntervalManager<Models.InventoryItem>.GiveNewInterval(id);
                    string commandToSend = "Interval" + ";" + "Add" + ";" + "InventoryItem" + ";" + interval.start + ";" + interval.end + ";" + id;
                    communicator.SendCommand(new Command(commandToSend, "None"));
                }
            }
        }
    }
}
