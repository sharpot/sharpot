using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class MapFloorChangeDownPacket : MapPacket
    {
        public static void Add
        (
            Connection connection, 
            NetworkMessage outMessage, 
            Location fromLocation,
            byte fromStackPosition,
            Location toLocation
        )
        {
            //floor change down
            outMessage.AddByte((byte)ServerPacketType.FloorChangeDown);

            //going from surface to underground
            if (toLocation.Z == 8)
            {
                int skip = -1;
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, toLocation.Z, 18, 14, -1, skip);
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, toLocation.Z + 1, 18, 14, -2, skip);
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, toLocation.Z + 2, 18, 14, -3, skip);

                if (skip >= 0)
                {
                    outMessage.AddByte((byte)skip);
                    outMessage.AddByte(0xFF);
                }
            }
            //going further down
            else if (toLocation.Z > fromLocation.Z && toLocation.Z > 8 && toLocation.Z < 14)
            {
                int skip = -1;
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, toLocation.Z + 2, 18, 14, -3, skip);

                if (skip >= 0)
                {
                    outMessage.AddByte((byte)skip);
                    outMessage.AddByte(0xFF);
                }
            }

            //moving down a floor makes us out of sync
            //east
            outMessage.AddByte((byte)ServerPacketType.MapSliceEast);
            AddMapDescription(connection, outMessage, fromLocation.X + 9, fromLocation.Y - 1 - 6, toLocation.Z, 1, 14);

            //south
            outMessage.AddByte((byte)ServerPacketType.MapSliceSouth);
            AddMapDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y + 7, toLocation.Z, 18, 1);
        }
    }
}
