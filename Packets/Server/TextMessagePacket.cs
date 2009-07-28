using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class TextMessagePacket : Packet
    {
        public static void Add(NetworkMessage message, TextMessageType type, string text)
        {
            message.AddByte((byte)ServerPacketType.TextMessage);
            message.AddByte((byte)type);
            message.AddString(text);
        }
    }
}
