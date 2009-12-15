using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ContainerOpenPacket : Packet
    {
        public static void Add
        (
            NetworkMessage message,
            byte containerId,
            ushort containerSpriteId,
            string containerName,
            byte volume,
            bool hasParent,
            IEnumerable<Item> items
        )
        {
            message.AddByte((byte)ServerPacketType.ContainerOpen);

            message.AddByte(containerId);
            message.AddUInt16(containerSpriteId);
            message.AddString(containerName);
            message.AddByte(volume);
            message.AddByte(Convert.ToByte(hasParent));
            message.AddByte((byte)items.Count());
            foreach (Item item in items)
            {
                message.AddItem(item);
            }
        }
    }
}
