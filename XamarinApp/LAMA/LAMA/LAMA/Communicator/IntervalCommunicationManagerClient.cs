using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    class IntervalCommunicationManagerClient
    {
        private ClientCommunicator communicator;

        public IntervalCommunicationManagerClient(ClientCommunicator communicator)
        {
            this.communicator = communicator;
        }
        /*
        public void OnIntervalRequestCP()
        {
            OnIntervalRequest("LAMA.Models.CP");
        }

        public void OnIntervalRequestInventoryItem()
        {
            OnIntervalRequest("LAMA.Models.InventoryItem");
        }
        */
        public void OnIntervalRequestCP()
        {
            string command = "Interval" + ";" + "Request" + ";" + "LAMA.Models.CP" + ";" + 0 + ";" + 0 + ";" + communicator.id;
            communicator.SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeSeconds(), "None"));
        }
        public void OnIntervalRequestInventoryItem()
        {
            string command = "Interval" + ";" + "Request" + ";" + "LAMA.Models.InventoryItem" + ";" + 0 + ";" + 0 + ";" + communicator.id;
            communicator.SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeSeconds(), "None"));
        }

        public void OnIntervalRequestLarpActivity()
        {
            string command = "Interval" + ";" + "Request" + ";" + "LAMA.Models.LarpActivity" + ";" + 0 + ";" + 0 + ";" + communicator.id;
            communicator.SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeSeconds(), "None"));
        }

        public void OnIntervalRequest<T>(T type)
        {
            string command = "Interval" + ";" + "Request" + ";" + typeof(T).FullName + ";" + 0 + ";" + 0 + ";" + communicator.id;
            communicator.SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeSeconds(), "None"));
        }

        public void IntervalsUpdate(string intervalCommand, string objectType, int lowerLimit, int upperLimit, int id, string command)
        {
            communicator.logger.LogWrite($"{command}, {intervalCommand}, {objectType}, {lowerLimit}, {upperLimit}, {id}");
            if (intervalCommand == "Add" && communicator.id == id)
            {
                if (objectType == "LAMA.Models.LarpActivity")
                {
                    
                    DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.NewIntervalReceived( new Database.Interval(lowerLimit, upperLimit, id));
                }

                if (objectType == "LAMA.Models.CP")
                {
                    
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.NewIntervalReceived(new Database.Interval(lowerLimit, upperLimit, id));
                }

                if (objectType == "LAMA.Models.InventoryItem")
                {
                    
                    DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.NewIntervalReceived(new Database.Interval(lowerLimit, upperLimit, id));
                }
            }
        }
    }
}
