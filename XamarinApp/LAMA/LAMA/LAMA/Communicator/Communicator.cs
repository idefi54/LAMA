using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    ///<summary>
    ///Interface for both of the communicator classes(ClientCommunicator, ServerCommunicator)
    ///</summary>
    public interface Communicator
    {
        ///<summary>
        ///Last time the communicator received an update over the network
        ///</summary>
        long LastUpdate { get; set; }
        ///<summary>
        ///An object capable of doing the message compression using the Huffman coding
        ///</summary>
        Compression CompressionManager { get; set; }
        ///<summary>
        ///Method used to send a message over the network
        ///</summary>
        void SendCommand(Command command);
        ///<summary>
        ///End the communication gracefully, clean up when necessary
        ///</summary>
        void EndCommunication();
    }
}
