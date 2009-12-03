using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class AnimatedTextPacket : Packet
    {
        public static void Add(NetworkMessage message, Location location, TextColor color, string text)
        {
            message.AddByte((byte)ServerPacketType.AnimatedText);
            message.AddLocation(location);
            message.AddByte((byte)color);
            message.AddString(text);
        }
    }
}
