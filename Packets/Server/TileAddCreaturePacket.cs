using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class TileAddCreaturePacket : Packet
    {
        public static void Add
        (
            NetworkMessage message,
            Location location,
            byte stackPosition,
            Creature creature,
            bool known,
            uint removeKnown
        )
        {
            message.AddByte((byte)ServerPacketType.TileAddThing);

            message.AddLocation(location);
            message.AddByte(stackPosition);
            message.AddCreature(creature, known, removeKnown);
        }
    }
}
