using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class MessageOfTheDayPacket : Packet
    {
        public static void Add(NetworkMessage message, string messageOfTheDay)
        {
            message.AddByte((byte)ServerPacketType.MessageOfTheDay);
            message.AddString("1\n" + messageOfTheDay);
        }

        public MessageOfTheDayPacket Parse(NetworkMessage message)
        {
            return new MessageOfTheDayPacket();
        }
    }
}
