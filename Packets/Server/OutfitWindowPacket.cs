using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class OutfitWindowPacket : Packet
    {
        public static void Add(NetworkMessage message, Player player)
        {
            message.AddByte((byte)ServerPacketType.OutfitWindow);
            message.AddOutfit(player.Outfit);
            //TODO: send list of outfits
            message.AddByte(1);
            message.AddUInt16((ushort)player.Outfit.LookType);
            message.AddString("");
            message.AddByte(player.Outfit.Addons);
        }
    }
}
