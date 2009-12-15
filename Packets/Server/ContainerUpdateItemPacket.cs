using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ContainerUpdateItemPacket : Packet
    {
        public static void Add
        (
            NetworkMessage message,
            byte containerIndex,
            byte containerPosition,
            Item item
        )
        {
            message.AddByte((byte)ServerPacketType.ContainerUpdateItem);

            message.AddByte(containerIndex);
            message.AddByte(containerPosition);
            message.AddItem(item);
        }
    }
}
