using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class PlayerFlagsPacket : Packet
    {
        public static void Add(NetworkMessage message, int flags)
        {
            message.AddByte((byte)ServerPacketType.PlayerFlags);
            message.AddUInt16((ushort)flags);
        }
    }
}
