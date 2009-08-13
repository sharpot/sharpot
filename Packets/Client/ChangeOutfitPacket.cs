using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ChangeOutfitPacket : Packet
    {
        public Outfit Outfit { get; set; }

        public static ChangeOutfitPacket Parse(NetworkMessage message)
        {
            ChangeOutfitPacket packet = new ChangeOutfitPacket();

            packet.Outfit = message.GetOutfit();

            return packet;
        }
    }
}
