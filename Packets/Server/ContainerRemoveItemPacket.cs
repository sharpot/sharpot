using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ContainerRemoveItemPacket : Packet
    {
        public static void Add
        (
            NetworkMessage message,
            byte containerIndex,
            byte containerPosition
        )
        {
            message.AddByte((byte)ServerPacketType.ContainerRemoveItem);

            message.AddByte(containerIndex);
            message.AddByte(containerPosition);
        }
    }
}
