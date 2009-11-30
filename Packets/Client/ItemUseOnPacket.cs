using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ItemUseOnPacket : Packet
    {
        public Location FromLocation { get; set; }
        public ushort FromSpriteId { get; set; }
        public byte FromStackPosition { get; set; }
        public Location ToLocation { get; set; }
        public ushort ToSpriteId { get; set; }
        public byte ToStackPosition { get; set; }

        public static ItemUseOnPacket Parse(NetworkMessage message)
        {
            ItemUseOnPacket packet = new ItemUseOnPacket();

            packet.FromLocation = message.GetLocation();
            packet.FromSpriteId = message.GetUInt16();
            packet.FromStackPosition = message.GetByte();
            packet.ToLocation = message.GetLocation();
            packet.ToSpriteId = message.GetUInt16();
            packet.ToStackPosition = message.GetByte();

            return packet;
        }
    }
}
