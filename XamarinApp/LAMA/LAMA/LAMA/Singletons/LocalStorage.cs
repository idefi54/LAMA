using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using LAMA.Models;
using SQLite;

namespace LAMA
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
                    Debug.WriteLine(name);
                    var a = SQLConnectionWrapper.connection.GetTableInfoAsync(name);
                    a.Wait();
                    Debug.WriteLine("After GetTableInfoAsync");
                    if (a.Result.Count == 0)
                    {
                        Debug.WriteLine("a.Result.Count != 0");
                        SQLConnectionWrapper.connection.CreateTableAsync<LocalStorage>().Wait();
                    }

                    var data = SQLConnectionWrapper.connection.Table<LocalStorage>();
                    var listData = data.ToArrayAsync();
                    listData.Wait();

                    if(listData.Result.Length == 0)
                    {
                        instance = new LocalStorage();
                        SQLConnectionWrapper.connection.InsertAsync(instance).Wait();
                    }
                    else
                    {
                        instance = listData.Result[0];
                    }
                    return instance;
                    /*
                    var getting = SQLConnectionWrapper.connection.GetAsync<LocalStorage>(0);
                    getting.Wait();
                    var result = getting.Result;
                    if (result != null)
                        instance = result;
                    else
                    {
                        Debug.WriteLine("not creating table");
                    }
                    try
                    {
                        var getting = SQLConnectionWrapper.connection.GetAsync<LocalStorage>(0);
                        Debug.WriteLine("Created getting task");
                        getting.Wait();
                        Debug.WriteLine("getting.Wait() finished");
                        var result = getting.Result;
                        Debug.WriteLine("Got local storage result");
                        if (result != null)
                            instance = result;
                        else
                        {
                            instance = new LocalStorage();
                            SQLConnectionWrapper.connection.InsertAsync(instance).Wait();
                        }
                        Debug.WriteLine("before returning instance");
                        return instance;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception thrown ---------------------------------------------------------------");
                        Debug.WriteLine(e.Message);
                        throw e;
                    }
                    return instance;
                    */
                }
            }
        }

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

        public static CP cp { get { return DatabaseHolder<CP, CPStorage>.Instance.rememberedList.getByID(cpID); } }


        [PrimaryKey]
        public int ID { get; set; } = 0;
        public string _serverName { get; set; }
        public string _clientName { get; set; }
        public int _clientID { get; set; }
        public int _cpID { get; set; } = -1;
        public int _maxClientID { get; set; } = -1;

    }

}
