using System;
using System.Collections.Generic;
using SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
        }
        public static void invokeDeleted(Serializable deleted)
        {
            dataDeleted?.Invoke(deleted);
        }

    }

    //singleton connection 
    class SQLConnectionWrapper
    {

        

        public static SQLiteAsyncConnection connection {get{ return _connection; } }
        private static SQLiteAsyncConnection _connection;

        public static SQLiteAsyncConnection makeConnection ()
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = Path.Combine(directory, "database.db");

            if (!File.Exists(path))
                File.Create(path);

            _connection = new SQLiteAsyncConnection(path);
            
            return _connection;
        }
        
    }

    public class OurSQL<Data, Storage> where Data : Serializable, new() where Storage: LAMA.database.StorageInterface, new()
    {

        SQLiteAsyncConnection connection { get; }


        public OurSQL(string path)
        {

            if (SQLConnectionWrapper.connection == null)
                SQLConnectionWrapper.makeConnection();
            connection = SQLConnectionWrapper.connection;

            
        }



        public void makeTable()
        {

            connection.CreateTableAsync<Storage>().Wait();

        }

        public void addData(Data value, bool invoke)
        {
            if (value == null)
                return;
            Storage storage = new Storage();
            storage.makeFromStrings(value.getAttributes());
            storage.lastChange = DateTimeOffset.Now.ToUnixTimeSeconds();
            connection.InsertAsync(storage).Wait();


            if(invoke)
                SQLEvents.invokeCreated(value);
            
        }




        // when called with wrong index or data type this will crash
        public List<Data> ReadData()
        {

            var data =  connection.Table<Storage>();


            var listData = data.ToArrayAsync();
            listData.Wait();

            var result = listData.Result;

            List<Data> output = new List<Data>();
            for (int i = 0; i < result.Length; ++i)
            {
                Data toAdd = new Data();
                toAdd.buildFromStrings(result[i].getStrings());
            }
            return output;
        }

        
        //TODO - make the input safe (new attribute value can contain special characters and ruid the query through SQLinjection or just mistake)
        public void changeData(int attributeIndex, string newAttributeValue, Data who)
        {
            Storage update = new Storage();
            update.makeFromStrings(who.getAttributes());
            update.lastChange = DateTimeOffset.Now.ToUnixTimeSeconds();

            connection.UpdateAsync(update).Wait();
            SQLEvents.invokeChanged(who, attributeIndex);
        }

        public void removeAt(Data who)
        {
            
            connection.DeleteAsync<Storage>(who.getID()).Wait(); 

            SQLEvents.invokeDeleted(who);
        }
        

        public List<int> getDataSince(long when)
        {

            List<int> output = new List<int>();

            var storages = connection.Table<Storage>().Where(a => a.lastChange >= when);

            var task = storages.ToArrayAsync();
            var arr = task.Result;
            
            foreach(var a in arr)
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

    public class OurSQLDictionary<Data, Storage> where Data : SerializableDictionaryItem, new() where Storage : LAMA.database.StorageInterface, new()
    {

        SQLiteAsyncConnection connection { get; }


        public OurSQLDictionary(string path)
        {

            if (SQLConnectionWrapper.connection == null)
                SQLConnectionWrapper.makeConnection();
            connection = SQLConnectionWrapper.connection;


        }



        public void makeTable(string tableName)
        {
            connection.CreateTableAsync<Storage>().Wait();

        }

        public void addData(Data value)
        {
            if (value == null)
                return;
            Storage storage = new Storage();
            storage.makeFromStrings(value.getAttributes());
            storage.lastChange = DateTimeOffset.Now.ToUnixTimeSeconds();
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
            }
            return output;
        }


        //TODO - make the input safe (new attribute value can contain special characters and ruid the query through SQLinjection or just mistake)
        public void changeData(int attributeIndex, string newAttributeValue, Data who)
        {
            Storage update = new Storage();
            update.makeFromStrings(who.getAttributes());
            update.lastChange = DateTimeOffset.Now.ToUnixTimeSeconds();

            connection.UpdateAsync(update).Wait();
        }

        public void removeAt(Data who)
        {

            connection.DeleteAsync<Storage>(who.getID()).Wait();

        }


        public List<int> getDataSince(long when)
        {

            List<int> output = new List<int>();

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
}
