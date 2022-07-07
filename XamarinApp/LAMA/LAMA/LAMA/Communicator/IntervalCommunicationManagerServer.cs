using System;
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
            Pair<int, int> interval = new Pair<int, int>(0, 0);
            if (type == "LAMA.Models.LarpActivity")
            {
                interval = database.IntervalManager<Models.LarpActivity>.GiveNewInterval(0);
            }
            else if (type == "LAMA.Models.CP")
            {
                interval = database.IntervalManager<Models.CP>.GiveNewInterval(0);
            }
            else if (type == "LAMA.Models.InventoryItem")
            {
                interval = database.IntervalManager<Models.InventoryItem>.GiveNewInterval(0);
            }
            string command = "Interval" + ";" + "Add" + ";" + type + ";" + interval.first + ";" + interval.second;
            communicator.SendCommand(new Command(command, "None"));
        }

        public void IntervalsUpdate(string intervalCommand, string objectType, int lowerLimit, int upperLimit, int id, string command)
        {
            if (intervalCommand == "Request")
            {
                if (objectType == "LAMA.Models.LarpActivity")
                {
                    Pair<int, int> interval = database.IntervalManager<Models.LarpActivity>.GiveNewInterval(id);
                    string commandToSend = "Interval" + ";" + "Add" + ";" + "LarpActivity" + ";" + interval.first + ";" + interval.second + ";" + id;
                    communicator.SendCommand(new Command(commandToSend, "None"));
                }
                else if (objectType == "LAMA.Models.CP")
                {
                    Pair<int, int> interval = database.IntervalManager<Models.CP>.GiveNewInterval(id);
                    string commandToSend = "Interval" + ";" + "Add" + ";" + "CP" + ";" + interval.first + ";" + interval.second + ";" + id;
                    communicator.SendCommand(new Command(commandToSend, "None"));
                }
                else if (objectType == "LAMA.Models.InventoryItem")
                {
                    Pair<int, int> interval = database.IntervalManager<Models.InventoryItem>.GiveNewInterval(id);
                    string commandToSend = "Interval" + ";" + "Add" + ";" + "InventoryItem" + ";" + interval.first + ";" + interval.second + ";" + id;
                    communicator.SendCommand(new Command(commandToSend, "None"));
                }
            }
        }
    }
}
