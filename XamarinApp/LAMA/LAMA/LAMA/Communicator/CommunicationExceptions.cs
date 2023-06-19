using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    /// <summary>
    /// User didn't enter a correct ngrok endpoint address
    /// </summary>
    public class WrongNgrokAddressFormatException : Exception
    {
        public WrongNgrokAddressFormatException() { }
    }

    /// <summary>
    /// The passwords must have a minimum length to increase the security
    /// </summary>
    public class PasswordTooShortException : Exception
    {
        public PasswordTooShortException() { }
    }

    /// <summary>
    /// The user forgot to fill some entry (it was empty or filled with just white characters)
    /// </summary>
    public class EntryMissingException : Exception
    {
        public EntryMissingException(string message)
            : base(message) { }
    }

    /// <summary>
    /// The user filled entries have a character limit, which can not be overreached
    /// </summary>
    public class EntryTooLongException : Exception
    {
        public int length;
        public string fieldName;
        public EntryTooLongException(int length, string fieldName)
        {
            this.length = length;
            this.fieldName = fieldName;
        }
    }

    /// <summary>
    /// Wrong client name, client password, server password or a server with this name already exists
    /// </summary>
    public class WrongCredentialsException : Exception
    {
        public WrongCredentialsException() { }
        public WrongCredentialsException(string message)
            : base(message) { }
        public WrongCredentialsException(string message, Exception inner)
            : base(message, inner) { }
    }

    /// <summary>
    /// Server name can not contain special symbols
    /// </summary>
    public class WrongNameFormatException : Exception
    {
        public WrongNameFormatException() { }
        public WrongNameFormatException(string message)
            : base(message) { }
        public WrongNameFormatException(string message, Exception inner)
            : base(message, inner) { }
    }


    /// <summary>
    /// Unable to connect to the central (remote) server - probably problem with connection
    /// </summary>
    public class CantConnectToCentralServerException : Exception
    {
        public CantConnectToCentralServerException() { }
        public CantConnectToCentralServerException(string message)
            : base(message) { }
        public CantConnectToCentralServerException(string message, Exception inner)
            : base(message, inner) { }
    }

    /// <summary>
    /// Unable to connect to the database with the LARP server specifications
    /// </summary>
    public class CantConnectToDatabaseException : Exception
    {
        public CantConnectToDatabaseException() { }
        public CantConnectToDatabaseException(string message)
            : base(message) { }
        public CantConnectToDatabaseException(string message, Exception inner)
            : base(message, inner) { }
    }

    /// <summary>
    /// Supplied IP isn't a valid IP address
    /// </summary>
    public class NotAnIPAddressException : Exception
    {
        public NotAnIPAddressException() { }
        public NotAnIPAddressException(string message)
            : base(message) { }
        public NotAnIPAddressException(string message, Exception inner)
            : base(message, inner) { }
    }

    /// <summary>
    /// Can not create server with this port number
    /// </summary>
    public class WrongPortException : Exception
    {
        public WrongPortException() { }
        public WrongPortException(string message)
            : base(message) { }
        public WrongPortException(string message, Exception inner)
            : base(message, inner) { }
    }

    /// <summary>
    /// LARP server refused connection
    /// </summary>
    public class ServerConnectionRefusedException : Exception
    {
        public ServerConnectionRefusedException() { }
        public ServerConnectionRefusedException(string message)
            : base(message) { }
        public ServerConnectionRefusedException(string message, Exception inner)
            : base(message, inner) { }
    }

    /// <summary>
    /// Server refused the client
    /// </summary>
    public class ClientRefusedException : Exception
    {
        public ClientRefusedException() { }
        public ClientRefusedException(string message)
            : base(message) { }
        public ClientRefusedException(string message, Exception inner)
            : base(message, inner) { }
    }
}
