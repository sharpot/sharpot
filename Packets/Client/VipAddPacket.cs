using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class VipAddPacket
    {
        public string Name { get; set; }

        public static VipAddPacket Parse(NetworkMessage message)
        {
            VipAddPacket p = new VipAddPacket();

            p.Name = message.GetString();

            return p;
        }
    }
}
