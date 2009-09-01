using System;
using System.Collections.Generic;

namespace SharpOT
{
    public class Player : Creature
    {
        public Connection Connection;
        public ushort Level;
        public byte MagicLevel;
        public uint Experience;
        public uint Capacity;
        public Location SavedLocation = null;
    }
}