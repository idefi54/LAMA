using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteTest
{
    public class OurSQL<T> where T : Serializable, new()
    {

        SQLiteConnection connection { get; }


        Dictionary<string, int> lastID = new Dictionary<string, int>();

        public OurSQL(string path) : this(path, !File.Exists(path))
        {
        }

        public OurSQL(string path, bool makeNew)
        {
            connection = new SQLiteConnection("Data Source=" + path + "; Version = 3; New = " + (makeNew ? "True" : "False") +
                "; Compress = True; ");
            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
                connection = null;
            }
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
                if (first)
                {
                    first = false;
                    commandText.Append(", ");
                }
                commandText.Append(column + " " + "TEXT");
            }

            // add key so the records can be cognised
            commandText.Append(", key INTEGER");


            commandText.Append(")");
            command.CommandText = commandText.ToString();

            command.ExecuteNonQuery();
        }

        public int addData(string table, T value)
        {

            if (value == null)
                return 0;
            SQLiteCommand command = connection.CreateCommand();
            StringBuilder commandText = new StringBuilder();

            commandText.Append("INSERT INTO" + table + " (");

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
            commandText.Append(", key");
            commandText.Append(") VALUES(");

            first = true;
            foreach (var a in value.getAttributes())
            {
                if (first)
                    first = false;
                else
                    commandText.Append(", ");
                commandText.Append(a);
            }

            commandText.Append(", " + lastID[table]);
            lastID[table] += 1;

            commandText.Append(");");
            command.CommandText = commandText.ToString();
            return command.ExecuteNonQuery();
        }




        // when called with wrong index or data type this will crash
        public List<T> ReadData(string tableName)
        {
            SQLiteDataReader reader;
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM " + tableName;

            reader = command.ExecuteReader();

            List<T> output = new List<T>();

            while (reader.Read())
            {


                T newStuff = new T();


                for (int i = 0; i < newStuff.numOfAttributes(); ++i)
                {
                    newStuff.setAttribute(i, reader.GetString(i));
                }
                output.Add(newStuff);
            }


            return output;

        }

        public void changeData(string tableName, T newContent, int index)
        {
            SQLiteCommand command = connection.CreateCommand();
            StringBuilder commandText = new StringBuilder();

            commandText.Append("UPDATE " + tableName + " SET ");

            var columns = newContent.getAttributeNames();
            var values = newContent.getAttributes();
            for (int i = 0; i < columns.Length; ++i)
            {
                if (i != 0)
                    commandText.Append(", ");
                commandText.Append(columns[i]);
                commandText.Append(" = ");
                commandText.Append(values[i]);
            }

            commandText.Append(" WHERE key = " + index);

            command.CommandText = commandText.ToString();
            command.ExecuteNonQuery();
        }

        public void removeAt(string table, int index)
        {
            SQLiteCommand command = connection.CreateCommand();
            StringBuilder commandText = new StringBuilder();

            commandText.Append("DELETE FROM " + table + " WHERE key = " + index);
            command.CommandText = command.ToString();
            command.ExecuteNonQuery();

            for (int i = index + 1; i < lastID[table]; ++i)
            {
                changeKey(table, i, i - 1);
            }
            lastID[table] -= 1;
        }
        void changeKey(string table, int oldValue, int newValue)
        {
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = "UPDATE" + table + " SET key = " + oldValue + " WHERE key = " + newValue;
            command.ExecuteNonQuery();

        }
    }
}
