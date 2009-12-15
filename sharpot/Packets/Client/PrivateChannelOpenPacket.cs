using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class PrivateChannelOpenPacket : Packet
    {
        public string Receiver { get; set; }

        public static PrivateChannelOpenPacket Parse(NetworkMessage message)
        {
            PrivateChannelOpenPacket p = new PrivateChannelOpenPacket();
            p.Receiver = message.GetString();
            return p;   
        }
    }
}