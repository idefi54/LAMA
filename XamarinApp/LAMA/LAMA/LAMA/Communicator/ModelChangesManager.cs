using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace LAMA.Communicator
{
    class ModelChangesManager
    {
        private Communicator communicator;
        private RememberedStringDictionary<Command, CommandStorage> objectsCache;
        private RememberedStringDictionary<TimeValue, TimeValueStorage> attributesCache;
        private bool server;
        private bool noCommandSending;

        public ModelChangesManager(Communicator initCommunicator, RememberedStringDictionary<Command, CommandStorage> objectsCache, RememberedStringDictionary<TimeValue, TimeValueStorage> attributesCache, bool server = false, bool noCommandSending = false)
        {
            this.server = server;
            this.noCommandSending = noCommandSending;
            this.objectsCache = objectsCache;
            this.attributesCache = attributesCache;
            communicator = initCommunicator;
        }

        public void ProcessCommand(string command, Socket current)
        {
            string[] messageParts = command.Split(';');
            if (!server)
            {
                if (messageParts[1] == "DataUpdated")
                {
                    if (!server) communicator.LastUpdate = Int64.Parse(messageParts[0]);
                    DataUpdated(messageParts[2], Int32.Parse(messageParts[3]), Int32.Parse(messageParts[4]), messageParts[5], Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1), current);
                }
                if (messageParts[1] == "ItemCreated")
                {
                    if (!server) communicator.LastUpdate = Int64.Parse(messageParts[0]);
                    ItemCreated(messageParts[2], messageParts[3], Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1), current);
                }
                if (messageParts[1] == "ItemDeleted")
                {
                    if (!server) communicator.LastUpdate = Int64.Parse(messageParts[0]);
                    ItemDeleted(messageParts[2], Int32.Parse(messageParts[3]), Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1));
                }
                if (!server && (messageParts[1] == "Rollback"))
                {
                    if (messageParts[2] == "DataUpdated")
                    {
                        communicator.LastUpdate = Int64.Parse(messageParts[0]);
                        RollbackDataUpdated(messageParts[3], Int32.Parse(messageParts[4]), Int32.Parse(messageParts[5]), messageParts[6], Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1));
                    }
                    if (messageParts[2] == "ItemCreated")
                    {
                        communicator.LastUpdate = Int64.Parse(messageParts[0]);
                        RollbackItemCreated(messageParts[3], messageParts[4], Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1));
                    }
                }
            }
        }

        public void OnDataUpdated(Serializable changed, int attributeIndex)
        {
            int objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string attributeID = objectType + ";" + objectID + ";" + attributeIndex;

            communicator.Logger.LogWrite($"OnDataUpdated: {changed.getAttribute(attributeIndex)}");


            long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (!attributesCache.containsKey(attributeID))
            {
                attributesCache.add(new TimeValue(updateTime, changed.getAttribute(attributeIndex), attributeID));
            }
            else
            {
                attributesCache.getByKey(attributeID).value = changed.getAttribute(attributeIndex);
                attributesCache.getByKey(attributeID).time = updateTime;
            }
            string command = "DataUpdated" + ";" + objectType + ";" + objectID + ";" + attributeID + ";" + changed.getAttribute(attributeIndex);
            if (!noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectType + ";" + objectID));
        }

        public void DataUpdated(string objectType, int objectID, int indexAttribute, string value, long updateTime, string command, Socket current)
        {
            string attributeID = objectType + ";" + objectID + ";" + indexAttribute;
            communicator.Logger.LogWrite($"DataUpdated: {command}, {attributeID}, {value}, {updateTime}");
            if (attributesCache.containsKey(attributeID) && attributesCache.getByKey(attributeID).time <= updateTime)
            {
                attributesCache.getByKey(attributeID).value = value;
                attributesCache.getByKey(attributeID).time = updateTime;

                if (objectType == "LAMA.Models.LarpActivity")
                {
                    DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "LAMA.Models.CP")
                {
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "LAMA.Models.InventoryItem")
                {
                    DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }
                if (server)
                {
                    // Notify every client
                    if (!noCommandSending) communicator.SendCommand(new Command(command, updateTime, attributeID));
                }
            }
            else if (attributesCache.containsKey(attributeID) && server)
            {
                //Rollback
                string rollbackCommand = "Rollback;";
                rollbackCommand = "DataUpdated" + ";" + objectType + ";" + objectID + ";" + attributeID + ";" + attributesCache.getByKey(attributeID).value;
                try
                {
                    if (!noCommandSending) current.Send((new Command(rollbackCommand, attributesCache.getByKey(attributeID).time, attributeID)).Encode());
                }
                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                {
                    return;
                }
            }
        }

        public void RollbackDataUpdated(string objectType, int objectID, int indexAttribute, string value, long updateTime, string command)
        {
            string attributeID = objectType + ";" + objectID + ";" + indexAttribute;
            communicator.Logger.LogWrite($"RollbackDataUpdated: {command}, {attributeID}, {value}, {updateTime}");
            if (attributesCache.containsKey(attributeID))
            {
                attributesCache.getByKey(attributeID).value = value;
                attributesCache.getByKey(attributeID).time = updateTime;

                if (objectType == "LAMA.Models.LarpActivity")
                {
                    DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "LAMA.Models.CP")
                {
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "LAMA.Models.InventoryItem")
                {
                    DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }
            }
        }

        public void OnItemCreated(Serializable changed)
        {
            int objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string objectCacheID = objectType + ";" + objectID;

            long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            string[] attributes = changed.getAttributes();
            string command = "ItemCreated" + ";" + objectType + ";" + String.Join(",", attributes);

            communicator.Logger.LogWrite($"OnItemCreated: {command}");
            if (!objectsCache.containsKey(objectCacheID))
            {
                objectsCache.add(new Command(command, updateTime, objectCacheID));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.add(new TimeValue(updateTime, attributes[i], objectType + ";" + objectID + ";" + i));
                }
                if (!noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectCacheID));
            }
        }


        public void ItemCreated(string objectType, string serializedObject, long updateTime, string command, Socket current)
        {
            communicator.Logger.LogWrite($"ItemCreated: {command}, {objectType}");
            if (objectType == "LAMA.Models.LarpActivity")
            {
                Models.LarpActivity activity = new Models.LarpActivity();
                string[] attributtes = serializedObject.Split(',');
                activity.buildFromStrings(attributtes);
                string objectID = objectType + ";" + activity.getID();

                if (!objectsCache.containsKey(objectID))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.add(activity, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    if (server && !noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }

            if (objectType == "LAMA.Models.CP")
            {
                Models.CP cp = new Models.CP();
                string[] attributtes = serializedObject.Split(',');
                cp.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cp.getID();

                if (!objectsCache.containsKey(objectID))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.add(cp, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    if (server && !noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }

            if (objectType == "LAMA.Models.InventoryItem")
            {
                Models.InventoryItem ii = new Models.InventoryItem();
                string[] attributtes = serializedObject.Split(',');
                ii.buildFromStrings(attributtes);
                string objectID = objectType + ";" + ii.getID();

                if (!objectsCache.containsKey(objectID))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.add(ii, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    if (server && !noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }
        }

        private void ItemCreatedSendRollback(string objectID, Socket current)
        {
            communicator.Logger.LogWrite($"ItemCreatedSendRollback: {objectID}");
            string rollbackCommand = "Rollback;";
            rollbackCommand += objectsCache.getByKey(objectID).command;
            try
            {
                if (!noCommandSending) current.Send((new Command(rollbackCommand, objectsCache.getByKey(objectID).time, objectID)).Encode());
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                return;
            }
        }

        public void RollbackItemCreated(string objectType, string serializedObject, long updateTime, string command)
        {
            communicator.Logger.LogWrite($"RollbackItemCreated: {command}, {objectType}");
            if (objectType == "LAMA.Models.LarpActivity")
            {
                Models.LarpActivity activity = new Models.LarpActivity();
                string[] attributtes = serializedObject.Split(',');
                activity.buildFromStrings(attributtes);
                string objectID = objectType + ";" + activity.getID();
                int activityID = activity.getID();

                if (objectsCache.containsKey(objectID))
                {

                    activity = DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.getByID(activityID);
                    objectsCache.getByKey(objectID).command = command;
                    objectsCache.getByKey(objectID).time = updateTime;
                    activity.buildFromStrings(attributtes);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.getByKey(objectID + ";" + i).value = attributtes[i];
                        attributesCache.getByKey(objectID + ";" + i).time = updateTime;
                    }
                }
            }

            if (objectType == "LAMA.Models.CP")
            {
                Models.CP cp = new Models.CP();
                string[] attributtes = serializedObject.Split(',');
                cp.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cp.getID();

                int cpID = cp.getID();

                if (objectsCache.containsKey(objectID))
                {

                    cp = DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.getByID(cpID);
                    objectsCache.getByKey(objectID).command = command;
                    objectsCache.getByKey(objectID).time = updateTime;
                    cp.buildFromStrings(attributtes);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.getByKey(objectID + ";" + i).value = attributtes[i];
                        attributesCache.getByKey(objectID + ";" + i).time = updateTime;
                    }
                }
            }

            if (objectType == "LAMA.Models.InventoryItem")
            {
                Models.InventoryItem ii = new Models.InventoryItem();
                string[] attributtes = serializedObject.Split(',');
                ii.buildFromStrings(attributtes);
                string objectID = objectType + ";" + ii.getID();
                int itemID = ii.getID();

                if (objectsCache.containsKey(objectID))
                {

                    ii = DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.getByID(itemID);
                    objectsCache.getByKey(objectID).command = command;
                    objectsCache.getByKey(objectID).time = updateTime;
                    ii.buildFromStrings(attributtes);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.getByKey(objectID + ";" + i).value = attributtes[i];
                        attributesCache.getByKey(objectID + ";" + i).time = updateTime;
                    }
                }
            }
        }

        public void OnItemDeleted(Serializable changed)
        {
            int objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string objectCacheID = objectType + ";" + objectID;

            communicator.Logger.LogWrite($"OnItemDeleted: {objectCacheID}");

            long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            string[] attributes = changed.getAttributes();
            string command = "ItemDeleted" + ";" + objectType + ";" + objectID;

            if (objectsCache.containsKey(objectCacheID))
            {
                objectsCache.getByKey(objectCacheID).command = command;
                objectsCache.getByKey(objectCacheID).time = updateTime;
                if (!noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectCacheID));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.removeByKey(objectID + ";" + i);
                }
            }
        }


        public void ItemDeleted(string objectType, int objectID, long updateTime, string command)
        {
            communicator.Logger.LogWrite($"OnItemDeleted: {command}, {objectType}, {objectID}, {updateTime}");
            string objectCacheID = objectType + ";" + objectID;
            if (objectsCache.containsKey(objectCacheID))
            {
                int nAttributes = 0;
                Models.LarpActivity removedActivity;
                if (objectType == "LAMA.Models.LarpActivity" && (removedActivity = DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedActivity.numOfAttributes();
                    DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.removeByID(objectID);
                }
                Models.CP removedCP;
                if (objectType == "LAMA.Models.CP" && (removedCP = DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedCP.numOfAttributes();
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.removeByID(objectID);
                }
                Models.InventoryItem removedInventoryItem;
                if (objectType == "LAMA.Models.InventoryItem" && (removedInventoryItem = DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedInventoryItem.numOfAttributes();
                    DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.removeByID(objectID);
                }
                for (int i = 0; i < nAttributes; i++)
                {
                    attributesCache.removeByKey(objectID + ";" + i);
                }
                objectsCache.getByKey(objectCacheID).command = command;
                objectsCache.getByKey(objectCacheID).time = updateTime;

                if (server)
                {
                    if (!noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectCacheID));
                }
            }
        }
    }
}
