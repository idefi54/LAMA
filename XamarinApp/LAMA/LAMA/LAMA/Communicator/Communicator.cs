using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    interface Communicator
    {
        long LastUpdate { get; set; }
        Compression CompressionManager { get; set; }
        void SendCommand(Command command);

        void EndCommunication();
    }
}
