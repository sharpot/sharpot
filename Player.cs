using System;
using System.Collections.Generic;

namespace SharpOT
{
    public class Player : Creature
    {
        public Connection Connection;
        public Gender Gender = Gender.Male;
        public Vocation Vocation = Vocation.None;
        public ushort Level;
        public byte MagicLevel;
        public uint Experience;
        public uint Capacity;
        public Location SavedLocation = null;
        public List<Channel> ChannelList;
        public List<Channel> OpenedChannelList;
        public FightModes FightMode;
        public bool ChaseMode;
        public bool SafeMode;

        public Player()
        {
            ChannelList = new List<Channel>();
            OpenedChannelList = new List<Channel>();
            ChannelList.Add(new Channel((ushort)ChatChannel.Game, "Game-Chat", 0));
            ChannelList.Add(new Channel((ushort)ChatChannel.RealLife, "RL-Chat", 0));
            ChannelList.Add(new Channel((ushort)ChatChannel.Help, "Help", 0));
        }

        public void Say(string Text)
        {
            Connection.Game.CreatureSaySpeech(this, SpeechType.Say, Text);
        }
    }
}