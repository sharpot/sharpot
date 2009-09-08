using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class VipLoginPacket : Packet
    {
        public static void Add(NetworkMessage message,uint id)
        {
            message.AddByte((byte)ServerPacketType.VipLogin);
            message.AddUInt32(id);
        }
    }
}

