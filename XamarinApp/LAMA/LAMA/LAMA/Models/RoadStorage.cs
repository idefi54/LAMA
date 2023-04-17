using LAMA.Database;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    public class RoadStorage:StorageInterface
    {
        [PrimaryKey]
        public long ID { get; set; }
        public string Coordinates { get; set; }
        public string Color { get; set; }
        public double Thickness { get; set; }
        public long lastChange { get; set; }
        public void makeFromStrings(string[] input)
        {
            ID = Helpers.readLong(input[0]);
            Coordinates = input[1];
            Color = input[2];
            int i = 0;
            Thickness = Helpers.readDouble(input[3], ref i);
        }
        public string[] getStrings()
        {
            return new string[] { ID.ToString(), Coordinates, Color, Thickness.ToString() };
        }
        public long getID()
        {
            return ID;
        }

    }
}
