using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using LAMA.Singletons;
using SQLite;
using System.Linq;

namespace LAMA.Communicator
{
    class ModelChangesManager
    {
        private Communicator communicator;
        private RememberedStringDictionary<Command, CommandStorage> objectsCache;
        private RememberedStringDictionary<TimeValue, TimeValueStorage> attributesCache;
        private bool server;
        private bool testing;
        private string objectIgnoreCreation = "";
        private string objectIgnoreDeletion = "";

        /// <summary>
        /// Create new model changes manager, used to change application status based on received messages
        /// </summary>
        /// <param name="initCommunicator">Communicator that uses this manager</param>
        /// <param name="objectsCache"></param>
        /// <param name="attributesCache"></param>
        /// <param name="server">Is this server or client</param>
        /// <param name="testing">Is this manager used for testing, or in the application itself</param>
        public ModelChangesManager(Communicator initCommunicator, RememberedStringDictionary<Command, CommandStorage> objectsCache, RememberedStringDictionary<TimeValue, TimeValueStorage> attributesCache, bool server = false, bool testing = false)
        {
            this.server = server;
            this.testing = testing;
            this.objectsCache = objectsCache;
            this.attributesCache = attributesCache;
            communicator = initCommunicator;
        }

        /// <summary>
        /// Process a received message
        /// </summary>
        /// <param name="command">Received message</param>
        /// <param name="current"></param>
        public void ProcessCommand(string command, Socket current)
        {
            string[] messageParts = command.Split(';');
            if (!server)
            {
                //Some attribute got updated
                if (messageParts[1] == "DataUpdated")
                {
                    if (!server && !testing) communicator.LastUpdate = Int64.Parse(messageParts[0]);
                    DataUpdated(messageParts[2], Int32.Parse(messageParts[3]), Int32.Parse(messageParts[4]), messageParts[5], Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1), current);
                }
                if (messageParts[1] == "ItemCreated")
                {
                    if (!server && !testing) communicator.LastUpdate = Int64.Parse(messageParts[0]);
                    ItemCreated(messageParts[2], messageParts[3], Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1), current);
                }
                if (messageParts[1] == "ItemDeleted")
                {
                    if (!server && !testing) communicator.LastUpdate = Int64.Parse(messageParts[0]);
                    ItemDeleted(messageParts[2], Int32.Parse(messageParts[3]), Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1));
                }
                if (messageParts[1] == "CPLocations")
                {
                    if (!server && !testing) communicator.LastUpdate = Int64.Parse(messageParts[0]);
                    CPLocationsUpdated(messageParts.Skip(2).ToArray(), Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1));
                }
                //Rollback effect of some previous message
                if (!server && (messageParts[1] == "Rollback"))
                {
                    if (messageParts[2] == "DataUpdated")
                    {
                        if (!testing) communicator.LastUpdate = Int64.Parse(messageParts[0]);
                        RollbackDataUpdated(messageParts[3], Int32.Parse(messageParts[4]), Int32.Parse(messageParts[5]), messageParts[6], Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1));
                    }
                    if (messageParts[2] == "ItemCreated")
                    {
                        if (!testing) communicator.LastUpdate = Int64.Parse(messageParts[0]);
                        RollbackItemCreated(messageParts[3], messageParts[4], Int64.Parse(messageParts[0]), command.Substring(command.IndexOf(';') + 1));
                    }
                }
            }
        }

        public void CPLocationsUpdated(string[] locations, long updateTime, string command)
        {
            int i = 0;
            while (i < locations.Length)
            {
                long cpID = Int64.Parse(locations[i]);
                string location = locations[i+1];
                int locationIndex = 7;
                DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.getByID(cpID).setAttribute(locationIndex, location);
                i += 2;
            }
        }

        public void SendCPLocations()
        {
            Debug.WriteLine($"Server Location: {DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.getByID(0).getAttribute(7)}");
            List<string> cpStrings = new List<string>();
            for (int i = 0; i < DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.Count; i++)
            {
                Models.CP cp = DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i];
                cpStrings.Add(cp.getID() + ";" + cp.getAttribute(7));
            }
            string cpStringTogether = string.Join(";", cpStrings);
            string finalString = "CPLocations;" + cpStringTogether;
            communicator.SendCommand(new Command(finalString, DateTimeOffset.Now.ToUnixTimeMilliseconds(), "LAMA.Models.CP;Positions"));
        }

        /// <summary>
        /// Data updated event was triggered, communicator may have to send a message
        /// </summary>
        /// <param name="changed">Item whose attribute was changed</param>
        /// <param name="attributeIndex">Index of the modified attribute</param>
        public void OnDataUpdated(Serializable changed, int attributeIndex)
        {
            long objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string attributeID = objectType + ";" + objectID + ";" + attributeIndex;

            Debug.WriteLine($"OnDataUpdated: {changed.getAttribute(attributeIndex)}");
            communicator.Logger.LogWrite($"OnDataUpdated: {changed.getAttribute(attributeIndex)}");


            long updateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (!attributesCache.containsKey(attributeID))
            {
                attributesCache.add(new TimeValue(updateTime, changed.getAttribute(attributeIndex), attributeID));
            }
            else
            {
                attributesCache.getByKey(attributeID).value = changed.getAttribute(attributeIndex);
                attributesCache.getByKey(attributeID).time = updateTime;
            }
            string command = "DataUpdated" + ";" + objectType + ";" + objectID + ";" + attributeIndex + ";" + changed.getAttribute(attributeIndex);
            if (!testing && !(objectType == "LAMA.Models.CP" &&
                    changed.getAttributes()[attributeIndex] == "location"))
                communicator.SendCommand(new Command(command, updateTime, objectType + ";" + objectID));
        }

        /// <summary>
        /// Message received describing attribute change
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="objectID"></param>
        /// <param name="indexAttribute"></param>
        /// <param name="value"></param>
        /// <param name="updateTime"></param>
        /// <param name="command"></param>
        /// <param name="current"></param>
        public void DataUpdated(string objectType, long objectID, int indexAttribute, string value, long updateTime, string command, Socket current)
        {
            string attributeID = objectType + ";" + objectID + ";" + indexAttribute;
            if (!testing) communicator.Logger.LogWrite($"DataUpdated: {command}, {attributeID}, {value}, {updateTime}");

            Debug.WriteLine($"DataUpdated: {command}, {attributeID}, {value}, {updateTime}");
            if (objectType == "LAMA.Singletons.LarpEvent" && !attributesCache.containsKey(attributeID))
            {
                attributesCache.add(new TimeValue(updateTime, value, attributeID));
            }
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

                if (objectType == "LAMA.Models.ChatMessage")
                {
                    DatabaseHolder<Models.ChatMessage, Models.ChatMessageStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "LAMA.Models.EncyclopedyCategory")
                {
                    DatabaseHolder<Models.EncyclopedyCategory, Models.EncyclopedyCategoryStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "LAMA.Models.EncyclopedyRecord")
                {
                    DatabaseHolder<Models.EncyclopedyRecord, Models.EncyclopedyRecordStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "LAMA.Singletons.LarpEvent")
                {
                    Debug.WriteLine($"Updating LarpEvent {indexAttribute} ------- {value}");
                    Singletons.LarpEvent.Instance.setAttribute(indexAttribute, value);
                    if (indexAttribute == 2)
                    {
                        command = "DataUpdated" + ";" + objectType + ";" + objectID + ";" + indexAttribute + ";" + LarpEvent.Instance.chatChannels;
                    }
                }
                if (server)
                {
                    // Notify every client
                    if (!testing && !(objectType == "LAMA.Models.CP" &&
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.getByID(objectID).getAttributes()[indexAttribute] == "location")) 
                        communicator.SendCommand(new Command(command, updateTime, attributeID));
                }
            }
            else if (attributesCache.containsKey(attributeID) && server)
            {
                //Rollback
                string rollbackCommand = "Rollback;";
                rollbackCommand = "DataUpdated" + ";" + objectType + ";" + objectID + ";" + indexAttribute + ";" + attributesCache.getByKey(attributeID).value;
                try
                {
                    if (!testing) current.Send((new Command(rollbackCommand, attributesCache.getByKey(attributeID).time, attributeID)).Encode());
                }
                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Rollback previous data update
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="objectID"></param>
        /// <param name="indexAttribute"></param>
        /// <param name="value"></param>
        /// <param name="updateTime"></param>
        /// <param name="command"></param>
        public void RollbackDataUpdated(string objectType, long objectID, int indexAttribute, string value, long updateTime, string command)
        {
            string attributeID = objectType + ";" + objectID + ";" + indexAttribute;
            if (!testing) communicator.Logger.LogWrite($"RollbackDataUpdated: {command}, {attributeID}, {value}, {updateTime}");
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

                if (objectType == "LAMA.Models.ChatMessage")
                {
                    DatabaseHolder<Models.ChatMessage, Models.ChatMessageStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "LAMA.Models.EncyclopedyCategory")
                {
                    DatabaseHolder<Models.EncyclopedyCategory, Models.EncyclopedyCategoryStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "LAMA.Models.EncyclopedyRecord")
                {
                    DatabaseHolder<Models.EncyclopedyRecord, Models.EncyclopedyRecordStorage>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "LAMA.Singletons.LarpEvent")
                {
                    Singletons.LarpEvent.Instance.setAttribute(indexAttribute, value);
                }
            }
        }

        /// <summary>
        /// Item created event was triggered, communicator may have to send a message
        /// </summary>
        /// <param name="changed"></param>
        public void OnItemCreated(Serializable changed)
        {
            long objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string objectCacheID = objectType + ";" + objectID;
            Debug.WriteLine($"ItemCreated: {objectType}");

            if (objectIgnoreCreation == objectCacheID)
            {
                communicator.Logger.LogWrite("Ignore Creation: " + objectIgnoreCreation);
                objectIgnoreCreation = "";
                return;
            }

            long updateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string[] attributes = changed.getAttributes();
            string command = "ItemCreated" + ";" + objectType + ";" + String.Join("¦", attributes);

            communicator.Logger.LogWrite($"OnItemCreated: {command}");
            if (!objectsCache.containsKey(objectCacheID) || (objectsCache.getByKey(objectCacheID).command.StartsWith("ItemDeleted") && testing))
            {
                objectsCache.add(new Command(command, updateTime, objectCacheID));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.add(new TimeValue(updateTime, attributes[i], objectType + ";" + objectID + ";" + i));
                }
            }
            else
            {
                Debug.WriteLine(objectsCache.getByKey(objectCacheID).command);
            }
            if (!testing) communicator.SendCommand(new Command(command, updateTime, objectCacheID));
        }

        /// <summary>
        /// Message received describing creation of a new object
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="serializedObject"></param>
        /// <param name="updateTime"></param>
        /// <param name="command"></param>
        /// <param name="current"></param>
        public void ItemCreated(string objectType, string serializedObject, long updateTime, string command, Socket current)
        {
            if (!testing) communicator.Logger.LogWrite($"ItemCreated: {command}, {objectType}");
            if (objectType == "LAMA.Models.LarpActivity")
            {
                Models.LarpActivity activity = new Models.LarpActivity();
                Debug.WriteLine(serializedObject);
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                Debug.WriteLine(attributtes);
                Debug.WriteLine(attributtes.Length);
                activity.buildFromStrings(attributtes);
                string objectID = objectType + ";" + activity.getID();

                if (!objectsCache.containsKey(objectID) || (objectsCache.getByKey(objectID).command.StartsWith("ItemDeleted") && testing))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    if (!server || testing)
                    {
                        objectIgnoreCreation = objectID;
                    }
                    DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.add(activity);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    //if (server && !noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }

            if (objectType == "LAMA.Models.CP")
            {
                Models.CP cp = new Models.CP();
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                cp.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cp.getID();

                if (!objectsCache.containsKey(objectID) || (objectsCache.getByKey(objectID).command.StartsWith("ItemDeleted") && testing))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    if (!server || testing)
                    {
                        objectIgnoreCreation = objectID;
                    }
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.add(cp);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    //if (server && !noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }

            if (objectType == "LAMA.Models.InventoryItem")
            {
                Models.InventoryItem ii = new Models.InventoryItem();
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                ii.buildFromStrings(attributtes);
                string objectID = objectType + ";" + ii.getID();

                if (!objectsCache.containsKey(objectID) || (objectsCache.getByKey(objectID).command.StartsWith("ItemDeleted") && testing))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    if (!server || testing)
                    {
                        objectIgnoreCreation = objectID;
                    }
                    DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.add(ii);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    //if (server && !noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }

            if (objectType == "LAMA.Models.ChatMessage")
            {
                Models.ChatMessage cm = new Models.ChatMessage();
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                cm.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cm.getID();

                if (!objectsCache.containsKey(objectID) || (objectsCache.getByKey(objectID).command.StartsWith("ItemDeleted") && testing))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    if (!server || testing)
                    {
                        objectIgnoreCreation = objectID;
                    }
                    DatabaseHolder<Models.ChatMessage, Models.ChatMessageStorage>.Instance.rememberedList.add(cm);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    //if (server && !noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }

            if (objectType == "LAMA.Models.EncyclopedyCategory")
            {
                Models.EncyclopedyCategory cm = new Models.EncyclopedyCategory();
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                cm.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cm.getID();

                if (!objectsCache.containsKey(objectID) || (objectsCache.getByKey(objectID).command.StartsWith("ItemDeleted") && testing))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    if (!server || testing)
                    {
                        objectIgnoreCreation = objectID;
                    }
                    DatabaseHolder<Models.EncyclopedyCategory, Models.EncyclopedyCategoryStorage>.Instance.rememberedList.add(cm);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    //if (server && !noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }

            if (objectType == "LAMA.Models.EncyclopedyRecord")
            {
                Models.EncyclopedyRecord er = new Models.EncyclopedyRecord();
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                er.buildFromStrings(attributtes);
                string objectID = objectType + ";" + er.getID();

                if (!objectsCache.containsKey(objectID) || (objectsCache.getByKey(objectID).command.StartsWith("ItemDeleted") && testing))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    if (!server || testing)
                    {
                        objectIgnoreCreation = objectID;
                    }
                    DatabaseHolder<Models.EncyclopedyRecord, Models.EncyclopedyRecordStorage>.Instance.rememberedList.add(er);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every client of item creation
                    //if (server && !noCommandSending) communicator.SendCommand(new Command(command, updateTime, objectID));
                }
                else if (server) ItemCreatedSendRollback(objectID, current);
            }
        }

        /// <summary>
        /// Inform client, that an item creation should be rolled back
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="current"></param>
        private void ItemCreatedSendRollback(string objectID, Socket current)
        {
            communicator.Logger.LogWrite($"ItemCreatedSendRollback: {objectID}");
            string rollbackCommand = "Rollback;";
            rollbackCommand += objectsCache.getByKey(objectID).command;
            try
            {
                if (!testing) current.Send((new Command(rollbackCommand, objectsCache.getByKey(objectID).time, objectID)).Encode());
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                return;
            }
        }

        /// <summary>
        /// Message received informing client, that item creation should be rolled back
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="serializedObject">The object we should roll back to</param>
        /// <param name="updateTime"></param>
        /// <param name="command"></param>
        public void RollbackItemCreated(string objectType, string serializedObject, long updateTime, string command)
        {
            if (!testing) communicator.Logger.LogWrite($"RollbackItemCreated: {command}, {objectType}");
            if (objectType == "LAMA.Models.LarpActivity")
            {
                Models.LarpActivity activity = new Models.LarpActivity();
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                activity.buildFromStrings(attributtes);
                string objectID = objectType + ";" + activity.getID();
                long activityID = activity.getID();

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
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                cp.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cp.getID();

                long cpID = cp.getID();

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
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                ii.buildFromStrings(attributtes);
                string objectID = objectType + ";" + ii.getID();
                long itemID = ii.getID();

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

            if (objectType == "LAMA.Models.ChatMessage")
            {
                Models.ChatMessage cm = new Models.ChatMessage();
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                cm.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cm.getID();
                long messageID = cm.getID();

                if (objectsCache.containsKey(objectID))
                {

                    cm = DatabaseHolder<Models.ChatMessage, Models.ChatMessageStorage>.Instance.rememberedList.getByID(messageID);
                    objectsCache.getByKey(objectID).command = command;
                    objectsCache.getByKey(objectID).time = updateTime;
                    cm.buildFromStrings(attributtes);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.getByKey(objectID + ";" + i).value = attributtes[i];
                        attributesCache.getByKey(objectID + ";" + i).time = updateTime;
                    }
                }
            }


            if (objectType == "LAMA.Models.EncyclopedyCategory")
            {
                Models.EncyclopedyCategory ec = new Models.EncyclopedyCategory();
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                ec.buildFromStrings(attributtes);
                string objectID = objectType + ";" + ec.getID();
                long messageID = ec.getID();

                if (objectsCache.containsKey(objectID))
                {

                    ec = DatabaseHolder<Models.EncyclopedyCategory, Models.EncyclopedyCategoryStorage>.Instance.rememberedList.getByID(messageID);
                    objectsCache.getByKey(objectID).command = command;
                    objectsCache.getByKey(objectID).time = updateTime;
                    ec.buildFromStrings(attributtes);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.getByKey(objectID + ";" + i).value = attributtes[i];
                        attributesCache.getByKey(objectID + ";" + i).time = updateTime;
                    }
                }
            }


            if (objectType == "LAMA.Models.EncyclopedyRecord")
            {
                Models.EncyclopedyRecord er = new Models.EncyclopedyRecord();
                string[] attributtes = serializedObject.Split('¦');
                for (int i = 0; i < attributtes.Length; i++) attributtes[i] = attributtes[i].Trim('Â');
                er.buildFromStrings(attributtes);
                string objectID = objectType + ";" + er.getID();
                long messageID = er.getID();

                if (objectsCache.containsKey(objectID))
                {

                    er = DatabaseHolder<Models.EncyclopedyRecord, Models.EncyclopedyRecordStorage>.Instance.rememberedList.getByID(messageID);
                    objectsCache.getByKey(objectID).command = command;
                    objectsCache.getByKey(objectID).time = updateTime;
                    er.buildFromStrings(attributtes);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.getByKey(objectID + ";" + i).value = attributtes[i];
                        attributesCache.getByKey(objectID + ";" + i).time = updateTime;
                    }
                }
            }
        }

        /// <summary>
        /// Item was deleted, message might have to be sent
        /// </summary>
        /// <param name="changed"></param>
        public void OnItemDeleted(Serializable changed)
        {
            long objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string objectCacheID = objectType + ";" + objectID;

            if (objectIgnoreDeletion == objectCacheID)
            {
                objectIgnoreDeletion = "";
                return;
            }

            communicator.Logger.LogWrite($"OnItemDeleted: {objectCacheID}");

            long updateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string[] attributes = changed.getAttributes();
            string command = "ItemDeleted" + ";" + objectType + ";" + objectID;

            if (objectsCache.containsKey(objectCacheID))
            {
                objectsCache.getByKey(objectCacheID).command = command;
                objectsCache.getByKey(objectCacheID).time = updateTime;
                if (!testing) communicator.SendCommand(new Command(command, updateTime, objectCacheID));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.removeByKey(objectID + ";" + i);
                }
            }
        }

        /// <summary>
        /// Message received describing deletion of an object
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="objectID"></param>
        /// <param name="updateTime"></param>
        /// <param name="command"></param>
        public void ItemDeleted(string objectType, long objectID, long updateTime, string command)
        {
            if (!testing) communicator.Logger.LogWrite($"OnItemDeleted: {command}, {objectType}, {objectID}, {updateTime}");
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
                Models.ChatMessage removedChatMessage;
                if (objectType == "LAMA.Models.ChatMessage" && (removedChatMessage = DatabaseHolder<Models.ChatMessage, Models.ChatMessageStorage>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedChatMessage.numOfAttributes();
                    DatabaseHolder<Models.ChatMessage, Models.ChatMessageStorage>.Instance.rememberedList.removeByID(objectID);
                }
                Models.EncyclopedyCategory removedEncyclopedyCategory;
                if (objectType == "LAMA.Models.EncyclopedyCategory" && (removedEncyclopedyCategory = DatabaseHolder<Models.EncyclopedyCategory, Models.EncyclopedyCategoryStorage>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedEncyclopedyCategory.numOfAttributes();
                    DatabaseHolder<Models.EncyclopedyCategory, Models.EncyclopedyCategoryStorage>.Instance.rememberedList.removeByID(objectID);
                }
                Models.EncyclopedyRecord removedEncyclopedyRecord;
                if (objectType == "LAMA.Models.EncyclopedyRecord" && (removedEncyclopedyRecord = DatabaseHolder<Models.EncyclopedyRecord, Models.EncyclopedyRecordStorage>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedEncyclopedyRecord.numOfAttributes();
                    DatabaseHolder<Models.EncyclopedyRecord, Models.EncyclopedyRecordStorage>.Instance.rememberedList.removeByID(objectID);
                }

                for (int i = 0; i < nAttributes; i++)
                {
                    attributesCache.removeByKey(objectID + ";" + i);
                }
                objectsCache.getByKey(objectCacheID).command = command;
                objectsCache.getByKey(objectCacheID).time = updateTime;

                if (!server || testing)
                {
                    objectIgnoreDeletion = objectCacheID;
                }
            }
        }
    }
}
