using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Singletons
{
    internal class CommunicationInfo
    {
        static CommunicationInfo instance = null;
        public static CommunicationInfo Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    instance = new CommunicationInfo();
                    return instance;
                }
            }
        }

        private Communicator.Communicator communicator;
        public Communicator.Communicator Communicator
        {
            get { return communicator; }
            set { 
                if (communicator != null) {
                    communicator.EndCommunication();
                }
                communicator = value;
            }
        }
        public string ServerName;
    }
}
