using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class GameServerConnectPacket : Packet
    {
        public static void Add(NetworkMessage message)
        {
            message.AddByte(0x1F); // type

            message.AddUInt32(0x1337); // time in seconds since server start

            message.AddByte(0x10); // fractional time?
        }

        public GameServerConnectPacket Parse(NetworkMessage message)
        {
            return new GameServerConnectPacket();
        }
    }
}
