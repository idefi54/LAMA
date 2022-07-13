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

        public void OnIntervalRequestInventoryItem()
        {
            var interval = Database.IntervalManager<Models.InventoryItem>.GiveNewInterval(0);
            string command = "Interval" + ";" + "Add" + ";" + "LAMA.Models.InventoryItem" + ";" + interval.start + ";" + interval.end;
            communicator.SendCommand(new Command(command, "None"));
        }

        public void OnIntervalRequestLarpActivity()
        {
            var interval = Database.IntervalManager<Models.LarpActivity>.GiveNewInterval(0);
            string command = "Interval" + ";" + "Add" + ";" + "LAMA.Models.LarpActivity" + ";" + interval.start + ";" + interval.end;
            communicator.SendCommand(new Command(command, "None"));
        }

        public void OnIntervalRequestCP()
        {
            var interval = Database.IntervalManager<Models.CP>.GiveNewInterval(0);
            string command = "Interval" + ";" + "Add" + ";" + "LAMA.Models.CP" + ";" + interval.start + ";" + interval.end;
            communicator.SendCommand(new Command(command, "None"));
        }

        public void IntervalsUpdate(string intervalCommand, string objectType, int lowerLimit, int upperLimit, int id, string command)
        {
            communicator.logger.LogWrite($"{command}, {intervalCommand}, {objectType}, {lowerLimit}, {upperLimit}, {id}");
            if (intervalCommand == "Request")
            {
                if (objectType == "LAMA.Models.LarpActivity")
                {
                    var interval = Database.IntervalManager<Models.LarpActivity>.GiveNewInterval(id);
                    string commandToSend = "Interval" + ";" + "Add" + ";" + "LAMA.Models.LarpActivity" + ";" + interval.start + ";" + interval.end + ";" + id;
                    communicator.SendCommand(new Command(commandToSend, "None"));
                }
                else if (objectType == "LAMA.Models.CP")
                {
                    var interval = Database.IntervalManager<Models.CP>.GiveNewInterval(id);
                    string commandToSend = "Interval" + ";" + "Add" + ";" + "LAMA.Models.CP" + ";" + interval.start + ";" + interval.end + ";" + id;
                    communicator.SendCommand(new Command(commandToSend, "None"));
                }
                else if (objectType == "LAMA.Models.InventoryItem")
                {
                    var interval = Database.IntervalManager<Models.InventoryItem>.GiveNewInterval(id);
                    string commandToSend = "Interval" + ";" + "Add" + ";" + "LAMA.Models.InventoryItem" + ";" + interval.start + ";" + interval.end + ";" + id;
                    communicator.SendCommand(new Command(commandToSend, "None"));
                }
            }
        }
    }
}
