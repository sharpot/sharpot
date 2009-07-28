using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class MapDescriptionPacket : Packet
    {
        public static void Add(NetworkMessage message, Location playerLocation)
        {
            message.AddByte((byte)ServerPacketType.MapDescription);
            message.AddLocation(playerLocation);
            message.AddMapDescription(
                playerLocation.X - 8,
                playerLocation.Y - 6,
                playerLocation.Z,
                18,
                14
            );
        }
    }
}
