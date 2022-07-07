using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    interface Communicator
    {
        void SendCommand(Command command);
    }
}
