using BruTile.Wmts.Generated;
using LAMA.Database;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    class PointOfInterestStorage:StorageInterface
    {
        [PrimaryKey]
        public long Id { get; set; }
        public string Coordinates { get; set; }
        public int Icon { get; set; }
        public long lastChange { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public void makeFromStrings(string[] input)
        {
            Id = Helpers.readLong(input[0]);
            Coordinates = input[1];
            lastChange = Helpers.readLong(input[2]);
            name = input[3];
            description = input[4];
        }
        public string[] getStrings()
        {
            return new string[] { Id.ToString(), Coordinates, Icon.ToString(), name, description };
        }
        public long getID()
        {
            return Id;
        }

    }
}
