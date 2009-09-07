using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class FightModesPacket : Packet
    {
        public FightModes FightMode { get; set; }
        public bool ChaseMode { get; set; }
        public bool SafeMode { get; set; }


        public static FightModesPacket Parse(NetworkMessage msg)
        {
            FightModesPacket p = new FightModesPacket();

            p.FightMode = (FightModes)msg.GetByte();
            p.ChaseMode =Convert.ToBoolean(msg.GetByte());
            p.SafeMode = Convert.ToBoolean(msg.GetByte());

            return p;
        }

    }
}