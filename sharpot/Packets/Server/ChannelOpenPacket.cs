using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ChannelOpenPacket : Packet
    {
        public static void Add(NetworkMessage message, ushort channelId, string channelName)
        {
            message.AddByte((byte)ServerPacketType.ChannelOpen);
            message.AddUInt16(channelId);
            message.AddString(channelName);
        }
    }
}