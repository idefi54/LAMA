using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace LAMA.Database
{
    public class Interval
    {
        [PrimaryKey]
        public int ID { get; set; }
        public int start { get; set; }
        public int end { get; set; }
        public int lastTaken { get; set; }
        public int ownerID { get; set; }
        public int typeID { get; set; }
        public Interval()
        {

        }
        public Interval(int ID, int start, int end, int ownerID)
        {
            this.ID = ID;
            this.start = start;
            this.end = end;
            this.ownerID = ownerID;
            lastTaken = start - 1;
        }
    }
}
