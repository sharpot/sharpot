using System;
using System.Collections.Generic;

namespace SharpOT
{
    public class Creature : Thing
    {
        public uint Id;
        public string Name;
        public uint Health = 100;
        public uint MaxHealth = 100;
        public uint Mana = 100;
        public uint MaxMana = 100;
        public Outfit Outfit = new Outfit(128, 0);
        public Direction Direction = Direction.North;
        public byte LightLevel = 0;
        public byte LightColor = 0;
        public Skull Skull = Skull.None;
        public Party Party = Party.None;
        public ushort Speed = 200;
        public Tile Tile;

        protected override ushort GetId()
        {
            return 0x63;
        }

        public override string ToString()
        {
            return Name + " [" + Id + "]";
        }
    }

    public class Player : Creature
    {
        public Connection Connection;
    }
}