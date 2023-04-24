using System;
using System.Collections.Generic;
using SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LAMA.Models;
using LAMA.Singletons;

namespace LAMA
{


    class SQLEvents
    {
        public delegate void DataChangedDelegate(Serializable changed, int changedAttributeIndex);
        public delegate void CreatedDelegate(Serializable created);
        public delegate void DataDeletedDelegate(Serializable deleted);
        public static event DataChangedDelegate dataChanged;
        public static event CreatedDelegate created;
        public static event DataDeletedDelegate dataDeleted;

        public static void invokeCreated(Serializable created)
        {
            SQLEvents.created?.Invoke(created);
        }
        public static void invokeChanged(Serializable changed, int index)
        {
            dataChanged?.Invoke(changed, index);
            changed.InvokeIGotUpdated(index);
        }
        public static void invokeDeleted(Serializable deleted)
        {
            dataDeleted?.Invoke(deleted);
        }

    }

    //singleton connection 
    class SQLConnectionWrapper
    {
        public static string databaseName = "LamaTotallyUniqueDatabaseDeffinitelyNotDuplicate.db";


        public static SQLiteAsyncConnection connection { get { return _connection; } }
        private static SQLiteAsyncConnection _connection = null;

        

        public static SQLiteAsyncConnection makeConnection(string newName = null)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (newName != null && newName != string.Empty)
                databaseName = newName;
            
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



                makeConnection();
            }

        }
    }


    

    public class OurSQL<Data, Storage> where Data : Serializable, new() where Storage : LAMA.Database.StorageInterface, new()
    {

        SQLiteAsyncConnection connection { get; }


        public OurSQL(string databaseName = null)
        {

            if (SQLConnectionWrapper.connection == null)
                SQLConnectionWrapper.makeConnection(databaseName);
            connection = SQLConnectionWrapper.connection;

            string name = new Storage().GetType().Name;
            var a = connection.GetTableInfoAsync(name);
            a.Wait();
            if (a.Result.Count == 0)
                connection.CreateTableAsync<Storage>().Wait();
        }



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



        public void changeData(int attributeIndex, string newAttributeValue, Data who)
        {
            Storage update = new Storage();
            update.makeFromStrings(who.getAttributes());
            update.lastChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            connection.UpdateAsync(update).Wait();
            SQLEvents.invokeChanged(who, attributeIndex);
        }

        public void removeAt(Data who)
        {

            connection.DeleteAsync<Storage>(who.getID()).Wait();

            SQLEvents.invokeDeleted(who);
        }


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

    public class OurSQLDictionary<Data, Storage> where Data : SerializableDictionaryItem, new() where Storage : LAMA.Database.DictionaryStorageInterface, new()
    {

        SQLiteAsyncConnection connection { get; }


        public OurSQLDictionary(string databaseName = null)
        {

            if (SQLConnectionWrapper.connection == null)
                SQLConnectionWrapper.makeConnection(databaseName);
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
