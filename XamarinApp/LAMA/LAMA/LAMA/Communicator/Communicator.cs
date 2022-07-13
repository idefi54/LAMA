using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    interface Communicator
    {
        DebugLogger Logger { get; }
        void SendCommand(Command command);
    }
}
