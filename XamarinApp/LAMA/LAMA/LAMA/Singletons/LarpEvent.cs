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
                    if (SQLConnectionWrapper.connection == null)
                    {
                        SQLConnectionWrapper.makeConnection();
                    }

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

        public void DeleteData()
        {
            instance = null;
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
            get { return Instance.name; }
            set
            {
                Instance.name = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
            }
        }

        volatile int insurance = -5;
        static EventList<string> _chatChannels = null;
        public static EventList<string> ChatChannels { 
            get
            {
                if (_chatChannels == null)
                {
                     Instance.insurance = Instance.getTypeID();
                }
                return _chatChannels;
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
            _chatChannels = new EventList<string>();

            List<string> channels = Helpers.readStringField(chatChannels);
            _chatChannels = new EventList<string>();
            for (int i = 0; i < channels.Count; ++i)
            {
                _chatChannels.Add(channels[i]);
            }
            _chatChannels.dataChanged += saveChatChannels;

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
            foreach (var channel in _chatChannels)
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
            SQLEvents.invokeChanged(Instance, 2);
            SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
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

        public void setAttributeDatabase(int i, string value)
        {
            setAttribute(i, value);
            SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
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
                    List<string> channels = Helpers.readStringField(chatChannels);
                    for (int j = 0; j < channels.Count; ++j)
                    {
                        if (!ChatChannels.Contains(channels[j]) || j >= ChatChannels.Count)
                        {
                            ChatChannels.Add(channels[j]);
                        }
                    }
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

        public event EventHandler<int> IGotUpdated;
        public void InvokeIGotUpdated(int index)
        {
            IGotUpdated?.Invoke(this, index);
        }
    }
}
