using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ItemUsePacket : Packet
    {
        public Location FromLocation { get; set; }
        public ushort SpriteId { get; set; }
        public byte FromStackPosition { get; set; }
        public byte Index { get; set; }

        public static ItemUsePacket Parse(NetworkMessage message)
        {
            ItemUsePacket packet = new ItemUsePacket();

            packet.FromLocation = message.GetLocation();
            packet.SpriteId = message.GetUInt16();
            packet.FromStackPosition = message.GetByte();
            packet.Index = message.GetByte();

            return packet;
        }
    }
}
