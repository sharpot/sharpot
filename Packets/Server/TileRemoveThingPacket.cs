using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class TileRemoveThingPacket : Packet
    {
        public static void Add(NetworkMessage message, Location fromLocation, byte fromStackPosition)
        {
            if (fromStackPosition < 10)
            {
                message.AddByte((byte)ServerPacketType.TileRemoveThing);
                message.AddLocation(fromLocation);
                message.AddByte(fromStackPosition);
            }
        }
    }
}
