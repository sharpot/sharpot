using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class LoginPacket : Packet
    {
        public ushort Os { get; set; }
        public ushort Version { get; set; }
        public uint[] XteaKey { get; set; }
        public byte GmMode { get; set; }
        public string AccountName { get; set; }
        public string CharacterName { get; set; }
        public string Password { get; set; }

        public static void Add(NetworkMessage message)
        {
            
        }

        public static LoginPacket Parse(NetworkMessage message)
        {
            LoginPacket packet = new LoginPacket();
            packet.Os = message.GetUInt16(); // OS
            packet.Version = message.GetUInt16(); // version

            message.RSADecrypt();

            message.GetByte(); // should be zero

            packet.XteaKey = new uint[4];
            packet.XteaKey[0] = message.GetUInt32();
            packet.XteaKey[1] = message.GetUInt32();
            packet.XteaKey[2] = message.GetUInt32();
            packet.XteaKey[3] = message.GetUInt32();

            packet.GmMode = message.GetByte();
            packet.AccountName = message.GetString();
            packet.CharacterName = message.GetString();
            packet.Password = message.GetString();

            message.SkipBytes(6); // 841 specific (according to OT)

            return packet;
        }
    }
}
