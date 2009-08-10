using System;
using System.Collections.Generic;

namespace SharpOT
{
    public class Outfit
    {
        public UInt16 LookType;
        public UInt16 LookItem;
        public byte Head;
        public byte Body;
        public byte Legs;
        public byte Feet;
        public byte Addons;

        public Outfit(UInt16 lookType, byte head, byte body, byte legs, byte feet, byte addons)
        {
            LookType = lookType;
            Head = head;
            Body = body;
            Legs = legs;
            Feet = feet;
            Addons = addons;
            LookItem = 0;
        }

        public Outfit(UInt16 lookType, UInt16 lookItem)
        {
            LookType = lookType;
            LookItem = lookItem;
            Head = 0;
            Body = 0;
            Legs = 0;
            Feet = 0;
            Addons = 0;
        }
    }

    public class Item
    {
        public ushort Id;
    }

    public class Creature
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

        public override string ToString()
        {
            return Name + " [" + Id + "]";
        }
    }

    public class Player : Creature
    {
        public Connection Connection;
    }

    public class CharacterListItem
    {
        public string Name;
        public string World;
        public byte[] Ip;
        public ushort Port;

        public CharacterListItem(string name, string world, byte[] ip, ushort port)
        {
            Name = name;
            World = world;
            Ip = ip;
            Port = port;
        }
    }
}