using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using LAMA.Extensions;
using SQLite;
namespace LAMA.Singletons
{
    internal class LarpEvent : Serializable
    {

        public delegate void DataChangedDelegate(int field);
        public static event DataChangedDelegate DataChanged;

        static LarpEvent instance = null;

        public static void reset()
        {
            instance = null;
        }
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
                    instance.setChatChannels();
                    return instance;
                }
            }
        }

        public void DeleteData()
        {
            instance = null;
        }

        
        /// <summary>
        /// remember start and end day
        /// </summary>
        public static Pair<DateTime, DateTime> Days
        {
            get
            {

                var times =  Helpers.readLongPair(Instance.days);
                return new Pair<DateTime, DateTime>(DateTimeExtension.UnixTimeStampMillisecondsToDateTime(times.first), DateTimeExtension.UnixTimeStampMillisecondsToDateTime(times.second));
            }

            set 
            {
                var unixPair = new Pair<long, long>(value.first.ToUnixTimeMilliseconds(), value.second.ToUnixTimeMilliseconds());
                Instance.days = unixPair.ToString();
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
                SQLEvents.invokeChanged(instance, 1);
            }
        }

        public static string Name
        {
            get { return Instance.name; }
            set
            {
                Instance.name = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
                SQLEvents.invokeChanged(instance, 2);
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
                SQLEvents.invokeChanged(instance, 4);
            }
        }

        public static double minX 
        { 
            get { return Instance._minX; } 
            set 
            { 
                Instance._minX = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
                SQLEvents.invokeChanged(instance, 5);
            } 
        }
        public static double minY 
        {
            get { return Instance._minY; }
            set 
            { 
                Instance._minY = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
                SQLEvents.invokeChanged(instance, 6);
            }
        }
        public static double maxX
        {
            get { return Instance._maxX; }
            set
            {
                Instance._maxX = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
                SQLEvents.invokeChanged(instance, 7);
            }
        }
        public static double maxY
        {
            get { return Instance._maxY; }
            set
            {
                Instance._maxY = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
                SQLEvents.invokeChanged(instance, 8);
            }
        }
        public static double minZoom 
        {
            get { return Instance._minZoom; }
            set
            {
                Instance._minZoom = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
                SQLEvents.invokeChanged(instance, 9);
            }
        }
        public static double maxZoom 
        {
            get { return Instance._maxZoom; }
            set
            {
                Instance._maxZoom = value;
                SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
                SQLEvents.invokeChanged(instance, 10);
            }
        }



    
        public void setChatChannels()
        {


            _chatChannels = Helpers.readStringField(chatChannels);
            

            _chatChannels.dataChanged += saveChatChannels;

        }
        
        static void saveChatChannels()
        {
            StringBuilder output = new StringBuilder();
            foreach (var channel in _chatChannels)
            {
                if (output.Length > 0)
                {
                    output.Append(Helpers.separator + channel);
                }
                else
                {
                    output.Append(channel);
                }
            }
            Instance.chatChannels = output.ToString();
            SQLEvents.invokeChanged(Instance, 3);
            SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
        }

        [PrimaryKey]
        public long Id { get; set; } = 0;
        public string name {get;set;}
        public string days { get; set; }
        public string chatChannels { get; set; }
        public long lastClientID { get; set; }

        public double _minX { get; set; } = 0;
        public double _minY { get; set; } = 0;
        public double _maxX { get; set; } = 0;
        public double _maxY { get; set; } = 0;
        public double _minZoom { get; set; } = -1;
        public double _maxZoom { get; set; } = -1;






        static string[] atributes = { "days", "name", "chatChannels", "lastClientID", "minX", "minY", "maxX", "maxY", 
        "minZoom", "maxZoom"};
        public string[] getAttributeNames()
        { return atributes; }
        public string[] getAttributes()
        {
            return new string[] { days.ToString(), name, chatChannels.ToString(), LastClientID.ToString(), minX.ToString(), minY.ToString()
            , maxX.ToString(), maxY.ToString(), minZoom.ToString(), maxZoom.ToString()};
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
            SQLEvents.invokeChanged(this, i);
            SQLConnectionWrapper.connection.UpdateAsync(Instance).Wait();
        }

        public void setAttribute(int i, string value)
        {

            switch(i)
            {
                case 1: days = value; break;
                case 2: name = value; break;
                case 3: 
                    chatChannels = value;
                    setChatChannels();
                    break;
                case 4: lastClientID = Helpers.readLong(value); break;
                case 5: _minX = Helpers.readDouble(value);break;
                case 6: _minY = Helpers.readDouble(value);break;
                case 7: _maxX = Helpers.readDouble(value);break;
                case 8: _maxY = Helpers.readDouble(value);break;
                case 9: _minZoom = Helpers.readDouble(value);break;
                case 10: _maxZoom = Helpers.readDouble(value);break;

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
                case 1: return days;
                case 2: return name;
                case 3: return chatChannels;
                case 4: return lastClientID.ToString();
                case 5: return _minX.ToString();
                case 6: return _minY.ToString();
                case 7: return _maxX.ToString();
                case 8: return _maxY.ToString();
                case 9: return _minZoom.ToString();
                case 10: return _maxZoom.ToString();
            }
            throw new System.NotImplementedException();
        }
        public int getTypeID()
        {
            return 12345;
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
