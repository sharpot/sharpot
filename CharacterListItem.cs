using System;
using System.Collections.Generic;

namespace SharpOT
{
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