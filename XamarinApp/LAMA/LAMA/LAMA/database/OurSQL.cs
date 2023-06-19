using System;
using System.Collections.Generic;
using SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LAMA.Models;
using LAMA.Singletons;
using LAMA.Communicator;
using LAMA.Database;

namespace LAMA
{

    /// <summary>
    /// This class contains static events for changes in the database. Specifically: created, changed and deleted events.
    /// </summary>
    public class SQLEvents
    {
        public delegate void DataChangedDelegate(Serializable changed, int changedAttributeIndex);
        public delegate void CreatedDelegate(Serializable created);
        public delegate void DataDeletedDelegate(Serializable deleted);
        /// <summary>
        /// Data changed in database. Item in question is passed as <see cref="Serializable"/> argument.
        /// Also index of changed attribute. See <see cref="Serializable"/> for clarification.
        /// </summary>
        public static event DataChangedDelegate dataChanged;

        /// <summary>
        /// Data created in database. Item in question is passed as <see cref="Serializable"/> argument.
        /// </summary>
        public static event CreatedDelegate created;

        /// <summary>
        /// Data deleted from database. Item in question is passed as <see cref="Serializable"/> argument.
        /// </summary>
        public static event DataDeletedDelegate dataDeleted;

        /// <summary>
        /// Calls <see cref="created"/> event.
        /// </summary>
        /// <param name="created"></param>
        public static void invokeCreated(Serializable created)
        {
            SQLEvents.created?.Invoke(created);
        }

        /// <summary>
        /// Calls <see cref="dataChanged"/> event.
        /// </summary>
        /// <param name="created"></param>
        public static void invokeChanged(Serializable changed, int index)
        {
            dataChanged?.Invoke(changed, index);
            changed.InvokeIGotUpdated(index);
        }

        /// <summary>
        /// Calls <see cref="dataDeleted"/> event.
        /// </summary>
        /// <param name="created"></param>
        public static void invokeDeleted(Serializable deleted)
        {
            dataDeleted?.Invoke(deleted);
        }

    }

    /// <summary>
    /// Singleton wrapper for <see cref="SQLiteAsyncConnection"/>. 
    /// </summary>
    class SQLConnectionWrapper
    {
        public static string databaseName = "LamaTotallyUniqueDatabaseDeffinitelyNotDuplicate.db";

        /// <summary>
        /// Instance of <see cref="SQLiteAsyncConnection"/>.
        /// </summary>
        public static SQLiteAsyncConnection connection { get { return _connection; } }
        private static SQLiteAsyncConnection _connection = null;

        
        /// <summary>
        /// Creates connection to a file at Local Application Data folder. On windows, this is AppData/Local.
        /// Name is based on the server name. 
        /// </summary>
        /// <returns></returns>
        public static SQLiteAsyncConnection makeConnection()
        {
            resetCaches();
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            databaseName = CommunicationInfo.Instance.ServerName + "_LAMA_Database";
            if (CommunicationInfo.Instance.IsServer)
                databaseName = databaseName + "_server";
            databaseName = databaseName + ".db";
            
            Console.WriteLine("making database with name " + databaseName);

            string path = Path.Combine(directory, databaseName);



            if (!File.Exists(path))
            {
                var file = File.Create(path);
                file.Flush();
                file.Close();
            }

            Debug.WriteLine("Before connection");
            _connection = new SQLiteAsyncConnection(path);
            Debug.WriteLine("Connection established");
            Debug.WriteLine(_connection);

            
            return _connection;
        }

        /// <summary>
        /// Deletes the database file and makes a new connection.
        /// </summary>
        public static void ResetDatabase()
        {
            Debug.WriteLine("Reseting database");
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = Path.Combine(directory, databaseName);

            if (File.Exists(path))
            {

                if (_connection != null)
                    _connection.CloseAsync().Wait();
                
                LarpEvent.Instance.DeleteData();
                _connection = null;
                File.Delete(path);

                resetCaches();

                makeConnection();
            }

        }

        /// <summary>
        /// Resets all database possible holders, LocalStorage, LarpEvent and DataBaseHolderStringDictionaries.
        /// </summary>
        static public void resetCaches()
        {
            DatabaseHolder<ChatMessage, ChatMessageStorage>.reset();
            DatabaseHolder<CP, CPStorage>.reset();
            DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.reset();
            DatabaseHolder<EncyclopedyRecord, EncyclopedyRecordStorage>.reset();
            DatabaseHolder<InventoryItem, InventoryItemStorage>.reset();
            DatabaseHolder<LarpActivity, LarpActivityStorage>.reset();
            DatabaseHolder<PointOfInterest, PointOfInterestStorage>.reset();
            DatabaseHolder<Road, RoadStorage>.reset();
            LocalStorage.reset();
            LarpEvent.reset();
            DatabaseHolderStringDictionary<ModelPropertyChangeInfo, ModelPropertyChangeInfoStorage>.reset();
            DatabaseHolderStringDictionary<Command, CommandStorage>.reset();
        }
    }


    
    /// <summary>
    /// Handles serialization, deserialization and communication with SQLite database.
    /// </summary>
    /// <typeparam name="Data"><see cref="Serializable"/> class to be serialized.</typeparam>
    /// <typeparam name="Storage"><see cref="StorageInterface"/> helper class for storing the Data.</typeparam>
    public class OurSQL<Data, Storage> where Data : Serializable, new() where Storage : StorageInterface, new()
    {

        SQLiteAsyncConnection connection { get; }

        /// <summary>
        /// Uses <see cref="SQLConnectionWrapper"/> to create a new connection.
        /// </summary>
        /// <param name="databaseName"></param>
        public OurSQL(string databaseName = null)
        {

            if (SQLConnectionWrapper.connection == null)
                SQLConnectionWrapper.makeConnection();
            connection = SQLConnectionWrapper.connection;

            string name = new Storage().GetType().Name;
            var a = connection.GetTableInfoAsync(name);
            a.Wait();
            if (a.Result.Count == 0)
                connection.CreateTableAsync<Storage>().Wait();
        }

        /// <summary>
        /// Store data in the database using <see cref="StorageInterface"/>.
        /// </summary>
        /// <param name="value">Data to be stored.</param>
        /// <param name="invoke">True if <see cref="SQLEvents.invokeCreated(Serializable)"/> should be invoked.</param>
        public void addData(Data value, bool invoke)
        {
            if (value == null)
                return;
            Storage storage = new Storage();
            storage.makeFromStrings(value.getAttributes());
            storage.lastChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            connection.InsertAsync(storage).Wait();


            if (invoke)
                SQLEvents.invokeCreated(value);

        }




        // when called with wrong index or data type this will crash
        /// <summary>
        /// Reads all data from the current database.
        /// </summary>
        /// <returns>List of all items of <see cref="Data"/> type.</returns>
        public List<Data> ReadData()
        {

            var data = connection.Table<Storage>();


            var listData = data.ToArrayAsync();
            listData.Wait();

            var result = listData.Result;

            List<Data> output = new List<Data>();
            for (int i = 0; i < result.Length; ++i)
            {
                Data toAdd = new Data();
                toAdd.buildFromStrings(result[i].getStrings());
                output.Add(toAdd);
            }
            return output;
        }


        /// <summary>
        /// Changes specified attribute of an item in the database.
        /// </summary>
        /// <param name="attributeIndex">What attribute to change. See <see cref="Serializable"/>.</param>
        /// <param name="newAttributeValue">New attribute value...</param>
        /// <param name="who">Item to be changed.</param>
        public void changeData(int attributeIndex, string newAttributeValue, Data who)
        {
            Storage update = new Storage();
            update.makeFromStrings(who.getAttributes());
            update.lastChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            connection.UpdateAsync(update).Wait();
            SQLEvents.invokeChanged(who, attributeIndex);
        }

        /// <summary>
        /// Remove item from the database.
        /// </summary>
        /// <param name="who">Item to be changed.</param>
        public void removeAt(Data who)
        {

            connection.DeleteAsync<Storage>(who.getID()).Wait();

            SQLEvents.invokeDeleted(who);
        }

        /// <summary>
        /// Get list of IDs of items that were changed since specified time.
        /// </summary>
        /// <param name="when">Since when was the last change.</param>
        /// <returns>List of IDs.</returns>
        public List<long> getDataSince(long when)
        {

            List<long> output = new List<long>();

            var storages = connection.Table<Storage>().Where(a => a.lastChange >= when);

            var task = storages.ToArrayAsync();
            var arr = task.Result;

            foreach (var a in arr)
            {
                output.Add(a.getID());
            }

            return output;
        }

        public bool containsTable(string name)
        {

            var a = connection.GetTableInfoAsync(name);
            a.Wait();
            return a.Result.Count > 0;
        }
    }

    /// <summary>
    /// Eqivalent for <see cref="OurSQL{Data, Storage}"/>, but for storage of dictionaries.
    /// </summary>
    /// <typeparam name="Data">Dictionary to be stored.</typeparam>
    /// <typeparam name="Storage">Helper class to store in the database.</typeparam>
    public class OurSQLDictionary<Data, Storage> where Data : SerializableDictionaryItem, new() where Storage : DictionaryStorageInterface, new()
    {

        SQLiteAsyncConnection connection { get; }


        public OurSQLDictionary(string databaseName = null)
        {

            if (SQLConnectionWrapper.connection == null)
                SQLConnectionWrapper.makeConnection();
            connection = SQLConnectionWrapper.connection;


            makeTable();
        }



        public void makeTable()
        {
            connection.CreateTableAsync<Storage>().Wait();

        }

        public void addData(Data value)
        {
            if (value == null)
                return;
            Storage storage = new Storage();
            storage.makeFromStrings(value.getAttributes());
            connection.InsertAsync(storage).Wait();

        }




        // when called with wrong index or data type this will crash
        public List<Data> ReadData()
        {

            var data = connection.Table<Storage>();


            var listData = data.ToArrayAsync();
            listData.Wait();

            var result = listData.Result;

            List<Data> output = new List<Data>();
            for (int i = 0; i < result.Length; ++i)
            {
                Data toAdd = new Data();
                toAdd.buildFromStrings(result[i].getStrings());
                output.Add(toAdd);
            }
            return output;
        }


        //TODO - make the input safe (new attribute value can contain special characters and ruid the query through SQLinjection or just mistake)
        public void changeData(int attributeIndex, string newAttributeValue, Data who)
        {
            Storage update = new Storage();
            update.makeFromStrings(who.getAttributes());

            connection.UpdateAsync(update).Wait();
        }

        public void removeAt(Data who)
        {

            connection.DeleteAsync<Storage>(who.getKey()).Wait();

        }


        

        public bool containsTable(string name)
        {

            var a = connection.GetTableInfoAsync(name);
            a.Wait();
            return a.Result.Count > 0;
        }
    }
}
