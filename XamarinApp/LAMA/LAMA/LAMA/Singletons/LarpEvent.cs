using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
namespace LAMA.Singletons
{
    internal class LarpEvent
    {

        static LarpEvent instance = null;
        static LarpEvent Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    string name = new LarpEvent().GetType().Name;
                    var a = SQLConnectionWrapper.connection.GetTableInfoAsync(name);
                    a.Wait();
                    if (a.Result.Count == 0)
                        SQLConnectionWrapper.connection.CreateTableAsync<LarpEvent>().Wait();

                    var getting = SQLConnectionWrapper.connection.GetAsync<LarpEvent>(0);
                    getting.Wait();
                    var result = getting.Result;
                    if (result != null)
                        instance = result;
                    else
                    {
                        instance = new LarpEvent();
                        SQLConnectionWrapper.connection.InsertAsync(instance).Wait();
                    }
                    return instance;
                }
            }
        }

        static public EventList<DateTime> Days = new EventList<DateTime>();

        public static string Name
        {
            get { return Instance.name;}
            set 
            {
                Instance.name = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }

        public LarpEvent()
        {
            List<long> temp = new List<long>();


        }

        [PrimaryKey]
        public int Id { get; set; }
        public string name {get;set;}
        public string days { get; set; }
    }
}
