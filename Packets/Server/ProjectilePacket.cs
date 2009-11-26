using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ProjectilePacket : Packet
    {
        public static void Add(NetworkMessage message, Location fromLocation, Location toLocation, ProjectileType projectile)
        {
            message.AddByte((byte)ServerPacketType.Projectile);
            message.AddLocation(fromLocation);
            message.AddLocation(toLocation);
            message.AddByte((byte)projectile);
        }
    }
}
