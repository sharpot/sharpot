using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class FightModesPacket : Packet
    {
        public FightMode FightMode { get; set; }
        public bool ChaseMode { get; set; }
        public bool SafeMode { get; set; }


        public static FightModesPacket Parse(NetworkMessage msg)
        {
            FightModesPacket p = new FightModesPacket();

            p.FightMode = (FightMode)msg.GetByte();
            p.ChaseMode =Convert.ToBoolean(msg.GetByte());
            p.SafeMode = Convert.ToBoolean(msg.GetByte());

            return p;
        }

    }
}