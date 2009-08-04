using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class PlayerSpeechPacket : Packet
    {
        public SpeechType SpeechType { get; set; }
        public string Receiver { get; set; }
        public string Message { get; set; }
        public ChatChannel ChannelId { get; set; }

        public static PlayerSpeechPacket Parse(NetworkMessage message)
        {
            PlayerSpeechPacket packet = new PlayerSpeechPacket();

            packet.SpeechType = (SpeechType)message.GetByte();

            switch (packet.SpeechType)
            {
                case SpeechType.Private:
                case SpeechType.PrivateRed:
                case SpeechType.RuleViolationAnswer:
                    packet.Receiver = message.GetString();
                    break;
                case SpeechType.ChannelYellow:
                case SpeechType.ChannelRed:
                case SpeechType.ChannelRedAnonymous:
                case SpeechType.ChannelWhite:
                    packet.ChannelId = (ChatChannel)message.GetUInt16();
                    break;
                default:
                    break;
            }

            packet.Message = message.GetString();

            return packet;
        }
    }
}
