using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ItemUseBattlelistPacket: Packet
    {
        public Location FromLocation { get; set; }
        public ushort SpriteId { get; set; }
        public byte FromStackPosition { get; set; }
        public uint CreatureId { get; set; }

        public static ItemUseBattlelistPacket Parse(NetworkMessage message)
        {
            ItemUseBattlelistPacket packet = new ItemUseBattlelistPacket();

            packet.FromLocation = message.GetLocation();
            packet.SpriteId = message.GetUInt16();
            packet.FromStackPosition = message.GetByte();
            packet.CreatureId = message.GetUInt32();

            return packet;
        }
    }
}
