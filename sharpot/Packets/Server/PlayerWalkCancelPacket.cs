using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class PlayerWalkCancelPacket : Packet
    {
        public static void Add(NetworkMessage message, Direction direction)
        {
            message.AddByte((byte)ServerPacketType.PlayerWalkCancel);
            message.AddByte((byte)direction);
        }
    }
}
