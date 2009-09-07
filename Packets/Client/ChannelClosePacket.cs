using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ChannelClosePacket : Packet
    {
        public ushort ChannelId { get; set; }

        public static ChannelClosePacket Parse(NetworkMessage message)
        {
            ChannelClosePacket p = new ChannelClosePacket();
            p.ChannelId = message.GetUInt16();
            return p;
        }
    }
}