using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class EffectPacket : Packet
    {
        public static void Add(NetworkMessage message, Effect effect, Location location)
        {
            message.AddByte((byte)ServerPacketType.Effect);
            message.AddLocation(location);
            message.AddByte((byte)effect);
        }
    }
}
