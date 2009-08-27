using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class AccountPacket : Packet
    {
        public ushort Os { get; set; }
        public ushort Version { get; set; }
        public uint[] XteaKey { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public static AccountPacket Parse(NetworkMessage message)
        {
            AccountPacket packet = new AccountPacket();
            packet.Os = message.GetUInt16(); // OS
            packet.Version = message.GetUInt16(); // version

            // File checks
            message.GetUInt32();
            message.GetUInt32();
            message.GetUInt32();

            message.RSADecrypt();

            message.GetByte(); // should be zero

            packet.XteaKey = new uint[4];
            packet.XteaKey[0] = message.GetUInt32();
            packet.XteaKey[1] = message.GetUInt32();
            packet.XteaKey[2] = message.GetUInt32();
            packet.XteaKey[3] = message.GetUInt32();

            packet.Name = message.GetString(); // account name
            packet.Password = message.GetString(); // password

            return packet;
        }
    }
}
