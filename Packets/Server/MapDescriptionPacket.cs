using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class MapDescriptionPacket : MapPacket
    {
        public static void Add(Connection connection, NetworkMessage message, Location playerLocation)
        {
            message.AddByte((byte)ServerPacketType.MapDescription);
            message.AddLocation(playerLocation);
            AddMapDescription(
                connection,
                message,
                playerLocation.X - 8,
                playerLocation.Y - 6,
                playerLocation.Z,
                18,
                14
            );
        }
    }
}
