using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class CreatureTurnPacket : Packet
    {
        public static void Add
        (
            NetworkMessage message,
            Creature creature
        )
        {
            message.AddByte((byte)ServerPacketType.TileTransformThing);

            message.AddLocation(creature.Tile.Location);
            message.AddByte(creature.Tile.GetStackPosition(creature));
            message.AddUInt16(0x63);
            message.AddUInt32(creature.Id);
            message.AddByte((byte)creature.Direction);
        }
    }
}
