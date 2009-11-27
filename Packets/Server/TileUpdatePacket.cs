using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class TileUpdatePacket : Packet
    {
        public static void Add
        (
            Connection connection,
            NetworkMessage message,
            Tile tile
        )
        {
            message.AddByte((byte)ServerPacketType.TileUpdate);

            message.AddLocation(tile.Location);
            MapPacket.AddTileDescription(connection, message, tile);
        }
    }
}
