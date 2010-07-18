using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class CreatureChangeSpeedPacket : Packet
    {
        public static void Add(NetworkMessage message, Creature creature)
        {
            message.AddByte((byte)ServerPacketType.CreatureSpeed);
            message.AddUInt32(creature.Id);
            message.AddUInt16(creature.Speed);
        }
    }
}
