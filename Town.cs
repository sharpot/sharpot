using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT
{
    public class Town
    {
        public uint Id { get; private set; }
        public string Name { get; private set; }
        public Location TempleLocation { get; private set; }

        public Town(uint id, string name, Location templeLocation)
        {
            Id = id;
            Name = name;
            TempleLocation = templeLocation;
        }
    }
}
