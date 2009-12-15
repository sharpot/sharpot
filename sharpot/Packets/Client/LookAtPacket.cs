using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class LookAtPacket : Packet
    {
        public Location Location { get; private set; }
        public ushort Id { get; private set; }
        public byte StackPosition { get; private set; }

        public static LookAtPacket Parse(NetworkMessage message)
        {
            LookAtPacket packet = new LookAtPacket();

            packet.Location = message.GetLocation();
            packet.Id = message.GetUInt16();
            packet.StackPosition = message.GetByte();

            return packet;
        }
    }
}
