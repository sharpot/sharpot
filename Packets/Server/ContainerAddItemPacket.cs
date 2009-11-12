using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ContainerAddItemPacket : Packet
    {
        public static void Add
        (
            NetworkMessage message,
            byte containerIndex,
            Item item
        )
        {
            message.AddByte((byte)ServerPacketType.ContainerAddItem);

            message.AddByte(containerIndex);
            message.AddItem(item);
        }
    }
}
