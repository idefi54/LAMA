using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteTest
{
    public interface Serializable
    {
        void setAttribute(int index, string value);
        string[] getAttributeNames();
        string[] getAttributes();
        int numOfAttributes();
        //TODO
        int getID();
        //TODO
        void buildFromStrings(string[] input);
        //TODO 
        RememberedList<Serializable> getStorage();
        //TODO 
        string getAttribute(int index);
        

    }
}
