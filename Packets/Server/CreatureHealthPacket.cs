using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class CreatureHealthPacket : Packet
    {
        public static void Add
       (
           NetworkMessage message,
           uint creatureID,
           byte newHealth
       )
        {
            message.AddByte((byte)ServerPacketType.CreatureHealth);

            message.AddUInt32(creatureID);
            message.AddByte(newHealth);
        }
    }
}
