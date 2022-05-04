using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
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

    class SQLConnectionWrapper
    {

        

        public static SQLiteConnection connection {get{ return _connection; } }
        private static SQLiteConnection _connection;

        public static SQLiteConnection makeConnection (string path, bool makeNew)
        {
            _connection = new SQLiteConnection("Data Source=" + path + "; Version = 3; New = " + (makeNew ? "True" : "False") +
                "; Compress = True; ");
            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
                _connection = null;
            }
            return _connection;
        }
        public static SQLiteConnection makeConnection(string path)
        {
            return makeConnection(path, !File.Exists(path));
        }
    }

    public class OurSQL<T> where T : Serializable, new()
    {

        SQLiteConnection connection { get; }


        public OurSQL(string path)
        {

            if (SQLConnectionWrapper.connection == null)
                SQLConnectionWrapper.makeConnection(path);
            connection = SQLConnectionWrapper.connection;

            
        }



        public void makeTable(string tableName)
        {
            SQLiteCommand command;

            command = connection.CreateCommand();

            StringBuilder commandText = new StringBuilder();

            commandText.Append("CREATE TABLE " + tableName + "(");

            var columns = (new T()).getAttributeNames();

            bool first = true;
            foreach (var column in columns)
            {
                if (!first)
                {
                    commandText.Append(", ");
                }
                else
                    first = false;
                commandText.Append(column + " " + "TEXT");
            }

            // last change parameter, so we can get items since some time
            commandText.Append(", lastChange INTEGER");


            commandText.Append(")");
            command.CommandText = commandText.ToString();

            command.ExecuteNonQuery();

        }

        public void addData(string table, T value, bool invoke)
        {
            if (value == null)
                return;

            SQLiteCommand command = connection.CreateCommand();
            StringBuilder commandText = new StringBuilder();

            commandText.Append("INSERT INTO " + table + " (");

            var columns = value.getAttributeNames();


            bool first = true;
            foreach (var a in columns)
            {
                if (first)
                    first = false;
                else
                    commandText.Append(", ");
                commandText.Append(a);
            }
            commandText.Append(", lastChange");
            commandText.Append(") VALUES(");

            first = true;
            foreach (var a in value.getAttributes())
            {
                if (first)
                    first = false;
                else
                    commandText.Append(", ");
                commandText.Append("'"+a+"'");
            }


            commandText.Append(", " + DateTimeOffset.Now.ToUnixTimeMilliseconds());

            commandText.Append(")");
            command.CommandText = commandText.ToString();

            
           command.ExecuteNonQuery();

            if(invoke)
                SQLEvents.invokeCreated(value);
        }




        // when called with wrong index or data type this will crash
        public List<T> ReadData(string tableName)
        {
            SQLiteDataReader reader;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM " + tableName;

            reader = command.ExecuteReader();

            List<T> output = new List<T>();
            int size = 0;
            while (reader.Read())
            {
                ++size;
                T newStuff = new T();


                for (int i = 0; i < newStuff.numOfAttributes(); ++i)
                {
                    newStuff.setAttribute(i, reader.GetString(i));
                }
                output.Add(newStuff);
            }

            return output;
        }

        
        public void changeData(string tableName, int attributeIndex, string newAttributeValue, T who)
        {
            SQLiteCommand command = connection.CreateCommand();

            string attributeName = who.getAttributeNames()[attributeIndex];

            command.CommandText = "UPDATE " + tableName + " SET " + attributeName + " = " + newAttributeValue + 
                ", lastChange = "+ DateTimeOffset.Now.ToUnixTimeMilliseconds() + " WHERE ID = " + who.getID();
            command.ExecuteNonQuery();

            SQLEvents.invokeChanged(who, attributeIndex);
        }

        public void removeAt(string table, T who)
        {
            SQLiteCommand command = connection.CreateCommand();
            StringBuilder commandText = new StringBuilder();

            commandText.Append("DELETE FROM " + table + " WHERE ID = " + who.getID());
            command.CommandText = commandText.ToString();
            command.ExecuteNonQuery();

            SQLEvents.invokeDeleted(who);
        }
        

        public List<int> getDataSince(string tableName, long when)
        {

            SQLiteDataReader reader;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT ID FROM " + tableName + "WHERE lastChange < " + when;

            reader = command.ExecuteReader();
            List<int> output = new List<int>();
            while(reader.Read())
            {
                output.Add(reader.GetInt32(0));
            }
            return output;
        }

        public bool containsTable(string name)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' AND name = '" + name + "'";
            var reader = command.ExecuteReader();
            return reader.HasRows;
        }
    }
}
