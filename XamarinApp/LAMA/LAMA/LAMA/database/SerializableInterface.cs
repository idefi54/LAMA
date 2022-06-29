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
        //TODO
        int getID();
        //TODO
        void buildFromStrings(string[] input);
        //TODO 
        string getAttribute(int index);
        

    }

    public interface SerializableDictionaryItem
    {
        void setAttribute(int index, string value);
        string[] getAttributeNames();
        string[] getAttributes();
        int numOfAttributes();
        //TODO
        string getKey();
        //TODO
        void buildFromStrings(string[] input);
        //TODO 
        string getAttribute(int index);
    }
}
