using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace LAMA.database
{



    public class LocalStorage
    {

        static LocalStorage instance = null;
        static LocalStorage Instance { get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    string name =  new LocalStorage().GetType().Name;
                    var a = SQLConnectionWrapper.connection.GetTableInfoAsync(name);
                    a.Wait();
                    if (a.Result.Count == 0)
                        SQLConnectionWrapper.connection.CreateTableAsync<LocalStorage>().Wait();

                    var getting = SQLConnectionWrapper.connection.GetAsync<LocalStorage>(0);
                    getting.Wait();
                    var result = getting.Result;
                    if (result != null)
                        instance = result;
                    else
                    {
                        instance = new LocalStorage();
                        SQLConnectionWrapper.connection.InsertAsync(instance).Wait();
                    }
                    return instance;
                }
            } }

        public static string serverName 
        {
            get { return Instance._serverName; }
            set 
            { 
                Instance._serverName = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }
        public static string clientName
        {
            get { return Instance._clientName; }
            set 
            { 
                Instance._clientName = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }
        public static int clientID
        {
            get { return Instance._clientID; }
            set 
            { 
                Instance._clientID = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }

        public static int cpID
        {
            get { return Instance._cpID; }
            set
            {
                Instance._cpID = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }

        public static int MaxClientID
        {
            get { return Instance._maxClientID; }
            set 
            { 
                Instance._maxClientID = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }

        [PrimaryKey]
        public int ID { get; set; } = 0;
        public string _serverName { get; set; }
        public string _clientName { get; set; }
        public int _clientID { get; set; }
        public int _cpID { get; set; } = -1;
        public int _maxClientID { get; set; } = -1;

    }

}
