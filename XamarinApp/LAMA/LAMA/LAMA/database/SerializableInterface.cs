using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{
    public interface Serializable
    {

        
        event EventHandler<int> IGotUpdated;
        void InvokeIGotUpdated(int index);


        void setAttribute(int index, string value);

        
        string[] getAttributeNames();
        string[] getAttributes();
        int numOfAttributes();
        long getID();
        void buildFromStrings(string[] input);
        string getAttribute(int index);
        int getTypeID();

        void addedInto(Object list);
        void removed();
        
    }

    public interface SerializableDictionaryItem
    {
        void setAttribute(int index, string value);
        string[] getAttributeNames();
        string[] getAttributes();
        int numOfAttributes();
        string getKey();
        void buildFromStrings(string[] input);
        string getAttribute(int index);
    }
}
