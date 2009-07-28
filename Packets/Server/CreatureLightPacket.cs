using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class CreatureLightPacket : Packet
    {
        public static void Add(NetworkMessage message, uint creatureId, byte lightLevel, byte lightColor)
        {
            message.AddByte((byte)ServerPacketType.CreatureLight);
            message.AddUInt32(creatureId);
            message.AddByte(lightLevel);
            message.AddByte(lightColor);
        }
    }
}
