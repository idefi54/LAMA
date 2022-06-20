using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace LAMA.Communicator
{
    class ModelChangesManager
    {
        private Communicator communicator;
        private RememberedStringDictionary<Command> objectsCache;
        private RememberedStringDictionary<TimeValue> attributesCache;
        private bool server;

        public ModelChangesManager(Communicator initCommunicator, RememberedStringDictionary<Command> objectsCache, RememberedStringDictionary<TimeValue> attributesCache, bool server = false)
        {
            this.server = server;
            this.objectsCache = objectsCache;
            this.attributesCache = attributesCache;
            communicator = initCommunicator;
        }

        public void OnDataUpdated(Serializable changed, int attributeIndex)
        {
            int objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string attributeID = objectType + ";" + objectID + ";" + attributeIndex;

            long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            attributesCache.getByKey(attributeID).value = changed.getAttribute(attributeIndex);
            attributesCache.getByKey(attributeID).time = updateTime;
            string command = "DataUpdated" + ";" + objectType + ";" + objectID + ";" + attributeID + ";" + changed.getAttribute(attributeIndex);
            communicator.SendCommand(new Command(command, updateTime, objectType + ";" + objectID));
        }

        public void DataUpdated(string objectType, int objectID, int indexAttribute, string value, long updateTime, string command, Socket current)
        {
            string attributeID = objectType + ";" + objectID + ";" + indexAttribute;
            if (attributesCache.containsKey(attributeID) && attributesCache.getByKey(attributeID).time <= updateTime)
            {
                attributesCache.getByKey(attributeID).value = value;
                attributesCache.getByKey(attributeID).time = updateTime;

                if (objectType == "LarpActivity")
                {
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "CP")
                {
                    DatabaseHolder<Models.CP>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "InventoryItem")
                {
                    DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }
                if (server)
                {
                    // Notify every client
                    communicator.SendCommand(new Command(command, updateTime, attributeID));
                }
            }
            else if (attributesCache.containsKey(attributeID) && server)
            {
                //Rollback
                string rollbackCommand = "Rollback;";
                rollbackCommand = "DataUpdated" + ";" + objectType + ";" + objectID + ";" + attributeID + ";" + attributesCache.getByKey(attributeID).value;
                try
                {
                    current.Send((new Command(rollbackCommand, attributesCache.getByKey(attributeID).time, attributeID)).Encode());
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
            if (attributesCache.containsKey(attributeID))
            {
                attributesCache.getByKey(attributeID).value = value;
                attributesCache.getByKey(attributeID).time = updateTime;

                if (objectType == "LarpActivity")
                {
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "CP")
                {
                    DatabaseHolder<Models.CP>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "InventoryItem")
                {
                    DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
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

            if (!objectsCache.containsKey(objectCacheID))
            {
                objectsCache.add(new Command(command, updateTime, objectCacheID));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.add(new TimeValue(updateTime, attributes[i], objectID + ";" + i));
                }
                communicator.SendCommand(new Command(command, updateTime, objectCacheID));
            }
        }


        public void ItemCreated(string objectType, string serializedObject, long updateTime, string command, Socket current)
        {
            if (objectType == "LarpActivity")
            {
                Models.LarpActivity activity = new Models.LarpActivity();
                string[] attributtes = serializedObject.Split(',');
                activity.buildFromStrings(attributtes);
                string objectID = objectType + ";" + activity.getID();

                if (!objectsCache.containsKey(objectID))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.add(activity, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    if (server) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }

            if (objectType == "CP")
            {
                Models.CP cp = new Models.CP();
                string[] attributtes = serializedObject.Split(',');
                cp.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cp.getID();

                if (!objectsCache.containsKey(objectID))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    DatabaseHolder<Models.CP>.Instance.rememberedList.add(cp, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    if (server) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }

            if (objectType == "InventoryItem")
            {
                Models.InventoryItem ii = new Models.InventoryItem();
                string[] attributtes = serializedObject.Split(',');
                ii.buildFromStrings(attributtes);
                string objectID = objectType + ";" + ii.getID();

                if (!objectsCache.containsKey(objectID))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.add(ii, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    if (server) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }
        }

        private void ItemCreatedSendRollback(string objectID, Socket current)
        {
            string rollbackCommand = "Rollback;";
            rollbackCommand += objectsCache.getByKey(objectID).command;
            try
            {
                current.Send((new Command(rollbackCommand, objectsCache.getByKey(objectID).time, objectID)).Encode());
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                return;
            }
        }

        public void RollbackItemCreated(string objectType, string serializedObject, long updateTime, string command)
        {
            if (objectType == "LarpActivity")
            {
                Models.LarpActivity activity = new Models.LarpActivity();
                string[] attributtes = serializedObject.Split(',');
                activity.buildFromStrings(attributtes);
                string objectID = objectType + ";" + activity.getID();
                int activityID = activity.getID();

                if (objectsCache.containsKey(objectID))
                {

                    activity = DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.getByID(activityID);
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

            if (objectType == "CP")
            {
                Models.CP cp = new Models.CP();
                string[] attributtes = serializedObject.Split(',');
                cp.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cp.getID();

                int cpID = cp.getID();

                if (objectsCache.containsKey(objectID))
                {

                    cp = DatabaseHolder<Models.CP>.Instance.rememberedList.getByID(cpID);
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

            if (objectType == "InventoryItem")
            {
                Models.InventoryItem ii = new Models.InventoryItem();
                string[] attributtes = serializedObject.Split(',');
                ii.buildFromStrings(attributtes);
                string objectID = objectType + ";" + ii.getID();
                int itemID = ii.getID();

                if (objectsCache.containsKey(objectID))
                {

                    ii = DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.getByID(itemID);
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

            long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            string[] attributes = changed.getAttributes();
            string command = "ItemDeleted" + ";" + objectType + ";" + objectID;

            if (objectsCache.containsKey(objectCacheID))
            {
                objectsCache.getByKey(objectCacheID).command = command;
                objectsCache.getByKey(objectCacheID).time = updateTime;
                communicator.SendCommand(new Command(command, updateTime, objectCacheID));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.removeByKey(objectID + ";" + i);
                }
            }
        }


        public void ItemDeleted(string objectType, int objectID, long updateTime, string command)
        {
            string objectCacheID = objectType + ";" + objectID;
            if (objectsCache.containsKey(objectCacheID))
            {
                int nAttributes = 0;
                Models.LarpActivity removedActivity;
                if (objectType == "LarpActivity" && (removedActivity = DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedActivity.numOfAttributes();
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.removeByID(objectID);
                }
                Models.CP removedCP;
                if (objectType == "CP" && (removedCP = DatabaseHolder<Models.CP>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedCP.numOfAttributes();
                    DatabaseHolder<Models.CP>.Instance.rememberedList.removeByID(objectID);
                }
                Models.InventoryItem removedInventoryItem;
                if (objectType == "InventoryItem" && (removedInventoryItem = DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedInventoryItem.numOfAttributes();
                    DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.removeByID(objectID);
                }
                for (int i = 0; i < nAttributes; i++)
                {
                    attributesCache.removeByKey(objectID + ";" + i);
                }
                objectsCache.getByKey(objectCacheID).command = command;
                objectsCache.getByKey(objectCacheID).time = updateTime;

                if (server)
                {
                    communicator.SendCommand(new Command(command, updateTime, objectCacheID));
                }
            }
        }
    }
}
