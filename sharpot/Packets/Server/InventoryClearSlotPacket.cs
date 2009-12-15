using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class InventoryClearSlotPacket : Packet
    {
        public static void Add(NetworkMessage message, SlotType slot)
        {
            message.AddByte((byte)ServerPacketType.InventoryClearSlot);
            message.AddByte((byte)slot);
        }
    }
}
