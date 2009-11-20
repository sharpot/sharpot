using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ContainerOpenParentPacket : Packet
    {
        public byte ContainerIndex { get; set; }

        public static ContainerOpenParentPacket Parse(NetworkMessage message)
        {
            ContainerOpenParentPacket p = new ContainerOpenParentPacket();
            p.ContainerIndex = message.GetByte();
            return p;
        }
    }
}