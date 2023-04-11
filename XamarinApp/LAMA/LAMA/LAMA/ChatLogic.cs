using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LAMA
{
    public class ChatLogic
    {
        //DatabaseHolder<Models.
        //>

        

        private static ChatLogic instance = null;
        public static ChatLogic Instance 
        { 
            get 
            { 
                if (instance == null)
                    instance = new ChatLogic();
                return instance;
            }
        }

        RememberedList<Models.ChatMessage, Models.ChatMessageStorage> database;
        public ChatLogic()
        {
            database = DatabaseHolder<Models.ChatMessage, Models.ChatMessageStorage>.Instance.rememberedList;
        }


        public List<Models.ChatMessage> GetMessages(int channelID, long sinceWhen, int howMany)
        {
            //SQLConnectionWrapper.connection.Table<Models.ChatMessageStorage>().
            var query = SQLConnectionWrapper.connection.QueryAsync<Models.ChatMessageStorage>(
                "SELECT * FROM ChatMessageStorage WHERE (channel = ?) AND (sentAt > ?) ORDER BY sentAt DESC LIMIT ?",
                new object[] { channelID, sinceWhen, howMany });
            query.Wait();

            List<Models.ChatMessage> output = new List<Models.ChatMessage>();
            foreach (var a in query.Result) 
            {
                output.Add(new Models.ChatMessage());
                output[output.Count - 1].buildFromStrings(a.getStrings());
            }
            return output;
        }


        
        public void SendMessage(int channelID, string message)
        {
            // just save the sent message and the remembered list does the rest by itself
            Debug.WriteLine(DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.Count);
            database.add(new Models.ChatMessage( 
                DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.getByID(LocalStorage.cpID).name, 
                channelID, message,DateTimeOffset.Now.ToUnixTimeMilliseconds(), LocalStorage.cpID == 0));
        }

    }
}
