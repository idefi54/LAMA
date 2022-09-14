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
            communicator.logger.LogWrite($"OnIntervalRequestCP");
            string command = "Interval" + ";" + "Request" + ";" + "LAMA.Models.CP" + ";" + 0 + ";" + 0 + ";" + LocalStorage.clientID;
            communicator.SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeSeconds(), "None"));
        }
        public void OnIntervalRequestInventoryItem()
        {
            communicator.logger.LogWrite($"OnIntervalRequestInventoryItem");
            string command = "Interval" + ";" + "Request" + ";" + "LAMA.Models.InventoryItem" + ";" + 0 + ";" + 0 + ";" + LocalStorage.clientID;
            communicator.SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeSeconds(), "None"));
        }

        public void OnIntervalRequestLarpActivity()
        {
            communicator.logger.LogWrite($"OnIntervalRequestLarpActivity");
            string command = "Interval" + ";" + "Request" + ";" + "LAMA.Models.LarpActivity" + ";" + 0 + ";" + 0 + ";" + LocalStorage.clientID;
            communicator.SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeSeconds(), "None"));
        }

        public void OnIntervalRequestChatMessage()
        {
            communicator.logger.LogWrite($"OnIntervalRequestChatMessage");
            string command = "Interval" + ";" + "Request" + ";" + "LAMA.Models.ChatMessage" + ";" + 0 + ";" + 0 + ";" + LocalStorage.clientID;
            communicator.SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeSeconds(), "None"));
        }

        public void OnIntervalRequest<T>(T type)
        {
            communicator.logger.LogWrite($"OnIntervalRequest<T>");
            string command = "Interval" + ";" + "Request" + ";" + typeof(T).FullName + ";" + 0 + ";" + 0 + ";" + LocalStorage.clientID;
            communicator.SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeSeconds(), "None"));
        }

        public void IntervalsUpdate(string intervalCommand, string objectType, int lowerLimit, int upperLimit, int id, string command)
        {
            communicator.logger.LogWrite($"{command}, {intervalCommand}, {objectType}, {lowerLimit}, {upperLimit}, {id}");
            if (intervalCommand == "Add" && +LocalStorage.clientID == id)
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

                if (objectType == "LAMA.Models.ChatMessage")
                {

                    DatabaseHolder<Models.ChatMessage, Models.ChatMessageStorage>.Instance.rememberedList.NewIntervalReceived(new Database.Interval(lowerLimit, upperLimit, id));
                }
            }
        }
    }
}
