using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    interface Communicator
    {
        long LastUpdate { get; set; }
        DebugLogger Logger { get; }
        void SendCommand(Command command);
    }
}
