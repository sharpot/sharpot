using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class AnimatedTextPacket : Packet
    {
        public static void Add(NetworkMessage message, TextColor color, string text)
        {
            message.AddByte((byte)ServerPacketType.AnimatedText);
            message.AddByte((byte)color);
            message.AddString(text);
        }
    }
}
