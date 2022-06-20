using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.database
{
    public interface StorageInterface
    {

        long lastChange { get; set; }

        void makeFromStrings(string[] input);
        string[] getStrings();

        int getID();

    }
}
