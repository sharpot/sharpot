using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class PlayerSpeechPacket : Packet
    {
        public Speech Speech { get; private set; }

        public static PlayerSpeechPacket Parse(NetworkMessage message)
        {
            PlayerSpeechPacket packet = new PlayerSpeechPacket();
            packet.Speech = new Speech();
            packet.Speech.Type = (SpeechType)message.GetByte();

            switch (packet.Speech.Type)
            {
                case SpeechType.Private:
                case SpeechType.PrivateRed:
                case SpeechType.RuleViolationAnswer:
                    packet.Speech.Receiver = message.GetString();
                    break;
                case SpeechType.ChannelYellow:
                case SpeechType.ChannelRed:
                case SpeechType.ChannelRedAnonymous:
                case SpeechType.ChannelWhite:
                    packet.Speech.ChannelId = (ChatChannel)message.GetUInt16();
                    break;
                default:
                    break;
            }

            packet.Speech.Message = message.GetString();

            return packet;
        }
    }
}
