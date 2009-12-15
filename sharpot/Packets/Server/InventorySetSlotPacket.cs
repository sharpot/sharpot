using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class InventorySetSlotPacket : Packet
    {
        public static void Add(NetworkMessage message, SlotType slot, Item item)
        {
            message.AddByte((byte)ServerPacketType.InventorySetSlot);
            message.AddByte((byte)slot);
            message.AddItem(item);
        }
    }
}
