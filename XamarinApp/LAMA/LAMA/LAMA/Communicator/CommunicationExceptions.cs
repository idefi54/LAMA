using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    public class WrongCreadintialsException : Exception
    {
        public WrongCreadintialsException() { }
        public WrongCreadintialsException(string message)
            : base(message) { }
        public WrongCreadintialsException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class WrongNameFormatException : Exception
    {
        public WrongNameFormatException() { }
        public WrongNameFormatException(string message)
            : base(message) { }
        public WrongNameFormatException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class NonExistentServerException : Exception
    {
        public NonExistentServerException() { }
        public NonExistentServerException(string message)
            : base(message) { }
        public NonExistentServerException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class CantConnectToCentralServerException : Exception
    {
        public CantConnectToCentralServerException() { }
        public CantConnectToCentralServerException(string message)
            : base(message) { }
        public CantConnectToCentralServerException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class CantConnectToDatabaseException : Exception
    {
        public CantConnectToDatabaseException() { }
        public CantConnectToDatabaseException(string message)
            : base(message) { }
        public CantConnectToDatabaseException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class NotAnIPAddressException : Exception
    {
        public NotAnIPAddressException() { }
        public NotAnIPAddressException(string message)
            : base(message) { }
        public NotAnIPAddressException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class WrongPortException : Exception
    {
        public WrongPortException() { }
        public WrongPortException(string message)
            : base(message) { }
        public WrongPortException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class ServerConnectionRefusedException : Exception
    {
        public ServerConnectionRefusedException() { }
        public ServerConnectionRefusedException(string message)
            : base(message) { }
        public ServerConnectionRefusedException(string message, Exception inner)
            : base(message, inner) { }
    }
}
