using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class AutoWalkPacket : Packet
    {
        public Queue<Direction> Directions { get; private set; }

        public static AutoWalkPacket Parse(NetworkMessage message)
        {
            AutoWalkPacket packet = new AutoWalkPacket();
            packet.Directions = new Queue<Direction>();

            byte count = message.GetByte();

            for (int i = 0; i < count; i++)
            {
                Direction direction;
                byte dir = message.GetByte();

                switch (dir)
                {
                    case 1: direction = Direction.East; break;
                    case 2: direction = Direction.NorthEast; break;
                    case 3: direction = Direction.North; break;
                    case 4: direction = Direction.NorthWest; break;
                    case 5: direction = Direction.West; break;
                    case 6: direction = Direction.SouthWest; break;
                    case 7: direction = Direction.South; break;
                    case 8: direction = Direction.SouthEast; break;
                    default: continue;
                }

                packet.Directions.Enqueue(direction);
            }

            return packet;
        }
    }
}
