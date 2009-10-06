using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class TileAddItemPacket : Packet
    {
        public static void Add
        (
            NetworkMessage message,
            Location location,
            byte stackPosition,
            Item item
        )
        {
            message.AddByte((byte)ServerPacketType.TileAddThing);

            message.AddLocation(location);
            message.AddByte(stackPosition);
            message.AddItem(item);
        }
    }
}
