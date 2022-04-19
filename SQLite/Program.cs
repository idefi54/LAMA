
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;


namespace SQLiteTest
{
    class Program
    {


        static void Main(string[] args)
        {
            string path;
            if (args.Length > 0)
                path = args[0];
            else
                path = "database.db";




            Console.WriteLine("Hello World!");
        }










    }
}
