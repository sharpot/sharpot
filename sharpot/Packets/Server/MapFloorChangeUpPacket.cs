using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class MapFloorChangeUpPacket : MapPacket
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
            //floor change up
            outMessage.AddByte((byte)ServerPacketType.FloorChangeUp);

            //going to surface
            if (toLocation.Z == 7)
            {
                int skip = -1;
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, 5, 18, 14, 3, skip); //(floor 7 and 6 already set)
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, 4, 18, 14, 4, skip);
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, 3, 18, 14, 5, skip);
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, 2, 18, 14, 6, skip);
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, 1, 18, 14, 7, skip);
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, 0, 18, 14, 8, skip);

                if (skip >= 0)
                {
                    outMessage.AddByte((byte)skip);
                    outMessage.AddByte(0xFF);
                }
            }
            //underground, going one floor up (still underground)
            else if (toLocation.Z > 7)
            {
                int skip = -1;
                skip = AddFloorDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, fromLocation.Z - 3, 18, 14, 3, skip);

                if (skip >= 0)
                {
                    outMessage.AddByte((byte)skip);
                    outMessage.AddByte(0xFF);
                }
            }

            //moving up a floor up makes us out of sync
            //west
            outMessage.AddByte((byte)ServerPacketType.MapSliceWest);
            AddMapDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y + 1 - 6, toLocation.Z, 1, 14);

            //north
            outMessage.AddByte((byte)ServerPacketType.MapSliceNorth);
            AddMapDescription(connection, outMessage, fromLocation.X - 8, fromLocation.Y - 6, toLocation.Z, 18, 1);
        }
    }
}
