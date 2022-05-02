
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;


namespace SQLiteTest
{
    class Program
    {


        struct TestStruct:Serializable
        {
            public int a;
            public string b;

            public void setAttribute(int i , string value)
            {
                if (i == 0)
                    a = Helpers.readInt(value);
                if (i == 1)
                    b = value;
            }
            public string[] getAttributeNames()
            {
                return new string[] { "a", "b" };
            }
            public int numOfAttributes()
            {
                return 2;
            }
            public string[] getAttributes()
            {
                return new string[] { a.ToString(), b };
            }
        }

        static void Main(string[] args)
        {
            string path;
            if (args.Length > 0)
                path = args[0];
            else
                path = "database.db";

            OurSQL<TestStruct> testSQL = new OurSQL<TestStruct>(path);
            RememberedList<TestStruct> test = new RememberedList<TestStruct>(testSQL);

            TestStruct first = new TestStruct();
            TestStruct second = new TestStruct();
            first.a = 5;
            first.b = "first";
            second.a = 666;
            second.b = "second";

            if (test.Count == 0)
            {
                test.add(first);
                test.add(second);
            }


            
           
        }










    }
}
