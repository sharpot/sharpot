using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class CreatureSpeechPacket : Packet
    {
        public static void Add
        (
            NetworkMessage message, 
            string senderName, 
            ushort senderLevel, 
            SpeechType speechType, 
            string text, 
            Location location, 
            ChatChannel channelId, 
            uint time
        )
        {
            message.AddByte((byte)ServerPacketType.CreatureSpeech);

            message.AddUInt32(0x00000000);
            message.AddString(senderName);
            message.AddUInt16(senderLevel);
            message.AddByte((byte)speechType);

            switch (speechType)
            {
                case SpeechType.Say:
                case SpeechType.Whisper:
                case SpeechType.Yell:
                case SpeechType.MonsterSay:
                case SpeechType.MonsterYell:
                case SpeechType.PrivateNPCToPlayer:
                    message.AddLocation(location);
                    break;
                case SpeechType.ChannelRed:
                case SpeechType.ChannelRedAnonymous:
                case SpeechType.ChannelOrange:
                case SpeechType.ChannelYellow:
                case SpeechType.ChannelWhite:
                    message.AddUInt16((ushort)channelId);
                    break;
                case SpeechType.RuleViolationReport:
                    message.AddUInt32(time);
                    break;
                default:
                    break;

            }

            message.AddString(text);
        }
    }
}
