using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class ContainerClosePacket : Packet
    {
        public byte ContainerIndex { get; set; }

        public static ContainerClosePacket Parse(NetworkMessage message)
        {
            ContainerClosePacket p = new ContainerClosePacket();
            p.ContainerIndex = message.GetByte();
            return p;
        }

        public static void Add
        (
            NetworkMessage message,
            byte containerId
        )
        {
            message.AddByte((byte)ServerPacketType.ContainerClose);

            message.AddByte(containerId);
        }
    }
}