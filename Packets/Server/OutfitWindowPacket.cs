using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class OutfitWindowPacket : Packet
    {
        public static void Add(NetworkMessage message, Player player, IEnumerable<Outfit> outfits)
        {
            message.AddByte((byte)ServerPacketType.OutfitWindow);
            message.AddOutfit(player.Outfit);
            //TODO: send list of outfits
            message.AddByte((byte)outfits.Count());

            foreach (Outfit outfit in outfits)
            {
                message.AddUInt16((ushort)outfit.LookType);
                message.AddString(outfit.Name);
                message.AddByte(outfit.Addons);
            }
        }
    }
}
