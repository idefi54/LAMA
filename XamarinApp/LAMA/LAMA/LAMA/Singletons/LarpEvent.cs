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

        static public EventList<DateTimeOffset> Days = new EventList<DateTimeOffset>();

        public static string Name
        {
            get { return Instance.name;}
            set 
            {
                Instance.name = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }

        public static EventList<string> ChatChannels = new EventList<string>();

        public LarpEvent()
        {
            List<long> temp = Helpers.readLongField(Instance.days);

            for (int i = 0; i < temp.Count; ++i) 
            {
                Days.Add(DateTimeOffset.FromUnixTimeMilliseconds(temp[i]));
            }

            Days.dataChanged += saveDays;

            List<string> channels = Helpers.readStringField(Instance.chatChannels);
            for (int i = 0; i < channels.Count; ++i)
            {
                ChatChannels.Add(channels[i]);
            }
            ChatChannels.dataChanged += saveChatChannels;

        }
        static void saveDays()
        {
            
            StringBuilder output = new StringBuilder();
            foreach (var day in Days)
            {
                output.Append("," + day.ToUnixTimeMilliseconds());
            }
            Instance.days = output.ToString();
        }
        static void saveChatChannels()
        {
            StringBuilder output = new StringBuilder();
            foreach (var channel in ChatChannels)
            {
                output.Append("," + channel);
            }
            Instance.chatChannels = output.ToString();
        }

        [PrimaryKey]
        public int Id { get; set; } = 0;
        public string name {get;set;}
        public string days { get; set; }
        public string chatChannels { get; set; }
    }
}
