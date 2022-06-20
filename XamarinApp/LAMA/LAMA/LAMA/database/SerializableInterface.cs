using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{
    public interface Serializable
    {
        void setAttribute(int index, string value);

        //THERE HAS TO BE AN ATTRIBUTE CALLED ID
        string[] getAttributeNames();
        string[] getAttributes();
        int numOfAttributes();
        int getID();
        void buildFromStrings(string[] input);
        string getAttribute(int index);
        
    }

    public interface SerializableDictionaryItem
    {
        void setAttribute(int index, string value);

        //THERE HAS TO BE AN ATTRIBUTE CALLED ID
        int getID();
        string[] getAttributeNames();
        string[] getAttributes();
        int numOfAttributes();
        string getKey();
        void buildFromStrings(string[] input);
        string getAttribute(int index);
    }
}
