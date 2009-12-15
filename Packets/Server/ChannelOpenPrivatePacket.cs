using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ChannelOpenPrivatePacket : Packet
    {
        public static void Add(NetworkMessage message,string Name)
        {
            message.AddByte((byte)ServerPacketType.ChannelOpenPrivate);
            message.AddString(Name);
        }

    }
}