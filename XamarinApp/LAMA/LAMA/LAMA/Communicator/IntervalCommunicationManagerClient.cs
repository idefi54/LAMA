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
            string command = "Interval" + ";" + "Request" + ";" + type + ";" + 0 + ";" + 0 + ";" + communicator.id;
            communicator.SendCommand(new Command(command, "None"));
        }

        public void IntervalsUpdate(string intervalCommand, string objectType, int lowerLimit, int upperLimit, int id, string command)
        {
            if (intervalCommand == "Add" && communicator.id == id)
            {
                if (objectType == "LAMA.Models.LarpActivity")
                {
                    
                    DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.NewIntervalReceived( new Database.Interval(lowerLimit, upperLimit, ProsimDoplnitownerID));
                }

                if (objectType == "LAMA.Models.CP")
                {
                    
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.NewIntervalReceived(new Database.Interval(lowerLimit, upperLimit, ProsimDoplnitownerID));
                }

                if (objectType == "LAMA.Models.InventoryItem")
                {
                    
                    DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.NewIntervalReceived(new Database.Interval(lowerLimit, upperLimit, ProsimDoplnitownerID));
                }
            }
        }
    }
}
