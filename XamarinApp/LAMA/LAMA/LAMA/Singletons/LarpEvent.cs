using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
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
                    string name = typeof(LarpEvent).Name;
                    var a = SQLConnectionWrapper.connection.GetTableInfoAsync(name);
                    a.Wait();
                    if (a.Result.Count == 0)
                        SQLConnectionWrapper.connection.CreateTableAsync<LarpEvent>().Wait();
                    else
                    {
                        var get = SQLConnectionWrapper.connection.Table<LarpEvent>();
                        var res = get.CountAsync();
                        res.Wait();
                        var count = res.Result;
                        if (count != 0)
                        {
                            var getting = SQLConnectionWrapper.connection.GetAsync<LarpEvent>(0);
                            getting.Wait();
                            result = getting.Result;
                        }
                        else
                        {
                            result = null;
                        }
                    }
                    if (result != null)
                        instance = result;
                    else
                    {
                        instance = new LarpEvent();
                        SQLConnectionWrapper.connection.InsertAsync(instance).Wait();
                    }
                    instance.init();
                    return instance;
                }
            }
        }

        static EventList<DateTimeOffset> _Days = null;
        public static EventList<DateTimeOffset> Days
        {
            get
            {
                if(_Days == null)
                {
                    Instance.insurance = Instance.getTypeID();
                }
                return _Days;
            }
        }

        public static string Name
        {
            get { return Instance.name;}
            set 
            {
                Instance.name = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }

        volatile int insurance = -5;
        static EventList<string> _ChatChannels = null;
        public static EventList<string> ChatChannels { 
            get
            {
                if (_ChatChannels == null)
                {
                     Instance.insurance = Instance.getTypeID();
                }
                return _ChatChannels;
            }
        }

        public static long LastClientID
        {
            get { return Instance.lastClientID; }
            set 
            {
                Instance.lastClientID = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }
    
        public void init()
        {
            List<long> temp = Helpers.readLongField(days);

            _Days = new EventList<DateTimeOffset>();
            for (int i = 0; i < temp.Count; ++i) 
            {
                _Days.Add(DateTimeOffset.FromUnixTimeMilliseconds(temp[i]));
            }

            _Days.dataChanged += saveDays;

            List<string> channels = Helpers.readStringField(chatChannels);
            _ChatChannels = new EventList<string>();
            for (int i = 0; i < channels.Count; ++i)
            {
                _ChatChannels.Add(channels[i]);
            }
            _ChatChannels.dataChanged += saveChatChannels;

        }
        static void saveDays()
        {
            StringBuilder output = new StringBuilder();
            foreach (var day in Days)
            {
                if (output.Length > 0)
                {
                    output.Append("," + day.ToUnixTimeMilliseconds());
                }
                else
                {
                    output.Append(day.ToUnixTimeMilliseconds());
                }
            }
            Instance.days = output.ToString();
            SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
        }
        static void saveChatChannels()
        {
            StringBuilder output = new StringBuilder();
            foreach (var channel in _ChatChannels)
            {
                if (output.Length > 0)
                {
                    output.Append("," + channel);
                }
                else
                {
                    output.Append(channel);
                }
            }
            Instance.chatChannels = output.ToString();
            SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
        }

        [PrimaryKey]
        public long Id { get; set; } = 0;
        public string name {get;set;}
        public string days { get; set; }
        private string _chatChannels;
        public string chatChannels {
            get { return _chatChannels; }
            set { 
                _chatChannels = value;
                SQLEvents.invokeChanged(this, 2);
                List<string> channels = Helpers.readStringField(chatChannels);
                for (int i = 0; i < channels.Count; ++i)
                {
                    if (!ChatChannels.Contains(channels[i]))
                    {
                        ChatChannels.Add(channels[i]);
                    }
                }
            }
        }
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
