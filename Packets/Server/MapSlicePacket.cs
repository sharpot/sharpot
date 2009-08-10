using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class MapSlicePacket : MapPacket
    {
        public static void Add
        (
            Connection connection, 
            NetworkMessage outMessage, 
            Location fromLocation, 
            Location toLocation
        )
        {
            if (fromLocation.Y > toLocation.Y)
            { // north, for old x
                outMessage.AddByte((byte)ServerPacketType.MapSliceNorth);
                AddMapDescription(connection, outMessage, fromLocation.X - 8, toLocation.Y - 6, toLocation.Z, 18, 1);
            }
            else if (fromLocation.Y < toLocation.Y)
            { // south, for old x
                outMessage.AddByte((byte)ServerPacketType.MapSliceSouth);
                AddMapDescription(connection, outMessage, fromLocation.X - 8, toLocation.Y + 7, toLocation.Z, 18, 1);
            }

            if (fromLocation.X < toLocation.X)
            { // east, [with new y]
                outMessage.AddByte((byte)ServerPacketType.MapSliceEast);
                AddMapDescription(connection, outMessage, toLocation.X + 9, toLocation.Y - 6, toLocation.Z, 1, 14);
            }
            else if (fromLocation.X > toLocation.X)
            { // west, [with new y]
                outMessage.AddByte((byte)ServerPacketType.MapSliceWest);
                AddMapDescription(connection, outMessage, toLocation.X - 8, toLocation.Y - 6, toLocation.Z, 1, 14);
            }
        }
    }
}
