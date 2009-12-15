using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class DeathPacket : Packet
    {
        public static void Add(NetworkMessage message)
        {
            message.AddByte((byte)ServerPacketType.Death);
        }
    }
}
