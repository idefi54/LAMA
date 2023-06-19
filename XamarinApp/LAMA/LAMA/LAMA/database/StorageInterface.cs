using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Database
{
    /// <summary>
    /// Helper storage class for serialized Data to be stored in the database.
    /// </summary>
    public interface StorageInterface
    {

        long lastChange { get; set; }

        void makeFromStrings(string[] input);
        string[] getStrings();

        long getID();

    }

    /// <summary>
    /// Equivalent for <see cref="StorageInterface"/> but data is a Dictionary.
    /// </summary>
    public interface DictionaryStorageInterface
    {
        void makeFromStrings(string[] input);
        string[] getStrings();
        string getKey();

        
    }
}
