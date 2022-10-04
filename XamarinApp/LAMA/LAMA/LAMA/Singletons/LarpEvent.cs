using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
namespace LAMA.Singletons
{
    internal class LarpEvent : Serializable
    {

        public delegate void DataChangedDelegate(int field);
        public static event DataChangedDelegate DataChanged;

        static LarpEvent instance = null;
        public static LarpEvent Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    LarpEvent result = null;
                    string name = new LarpEvent().GetType().Name;
                    var a = SQLConnectionWrapper.connection.GetTableInfoAsync(name);
                    a.Wait();
                    if (a.Result.Count == 0)
                        SQLConnectionWrapper.connection.CreateTableAsync<LarpEvent>().Wait();
                    else
                    {
                        var getting = SQLConnectionWrapper.connection.GetAsync<LarpEvent>(0);
                        getting.Wait();
                        result = getting.Result;
                    }
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

        public static long LastClientID
        {
            get { return Instance.lastClientID; }
            set 
            {
                Instance.lastClientID = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }
    
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
        public long Id { get; set; } = 0;
        public string name {get;set;}
        public string days { get; set; }
        public string chatChannels { get; set; }
        public long lastClientID { get; set; }


        static string[] atributes = { "name", "days", "chatChannels", "lastClientID" };
        public string[] getAttributeNames()
        { return atributes; }
        public string[] getAttributes()
        {
            return new string[] { name, days.ToString(), chatChannels.ToString().ToString(), lastClientID.ToString() };
        }
        public int numOfAttributes()
        {
            return atributes.Length;
        }
        public long getID()
        {
            return Id;
        }
        public void setAttribute(int i, string value)
        {
            switch(i)
            {
                case 0: 
                    name = value;
                    break;
                case 1: 
                    days = value;
                    break;
                case 2:
                    chatChannels = value;
                    break;
                case 3:
                    lastClientID = Helpers.readLong(value);
                    break;
            }
        }
        public void buildFromStrings(string[] input)
        {
            for (int i = 0; i < input.Length; ++i)
            {
                setAttribute(i, input[i]);
            }
        }
        public string getAttribute(int i)
        {
            switch(i)
            {
                case 0: return name;
                case 1: return days;
                case 2: return chatChannels;
                case 3: return lastClientID.ToString();
            }
            return null;
        }
        public int getTypeID()
        {
            return 1234;
        }


        public void removed()
        { }
        public void addedInto(object holder) { }
    }
}
