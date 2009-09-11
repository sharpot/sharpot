using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ChannelClosePacket : Packet
    {
        public ChatChannel Channel { get; set; }

        public static ChannelClosePacket Parse(NetworkMessage message)
        {
            ChannelClosePacket p = new ChannelClosePacket();
            p.Channel = (ChatChannel)message.GetUInt16();
            return p;
        }
    }
}