using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpOT
{
    public class Channel
    {
        public ushort Id { get; set; }
        public string Name { get; set; }
        public uint CooldownTime { get; set; }
        
        public Channel(ushort id, string name, uint cooldownTime)
        {
            Id = id;
            Name = name;
            CooldownTime = cooldownTime;
        }
    }
}
