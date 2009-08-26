using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT.Packets
{
    public class MapPacket : Packet
    {
        public static void AddMapDescription(Connection connection, NetworkMessage message, int x, int y, int z, ushort width, ushort height)
        {
            int MAP_MAX_LAYERS = 16;
            int skip = -1;
            int startZ, endZ, zStep = 0;

            if (z > 7)
            {
                startZ = z - 2;
                endZ = Math.Min(MAP_MAX_LAYERS - 1, z + 2);
                zStep = 1;
            }
            else
            {
                startZ = 7;
                endZ = 0;
                zStep = -1;
            }

            for (int nz = startZ; nz != endZ + zStep; nz += zStep)
            {
                skip = AddFloorDescription(connection, message, x, y, nz, width, height, z - nz, skip);
            }

            if (skip >= 0)
            {
                message.AddByte((byte)skip);
                message.AddByte(0xFF);
            }
        }

        public static int AddFloorDescription(Connection connection, NetworkMessage message, int x, int y, int z, int width, int height, int offset, int skip)
        {
            Tile tile;

            for (int nx = 0; nx < width; nx++)
            {
                for (int ny = 0; ny < height; ny++)
                {
                    tile = connection.Game.Map.GetTile(x + nx + offset, y + ny + offset, z);
                    if (tile != null)
                    {
                        if (skip >= 0)
                        {
                            message.AddByte((byte)skip);
                            message.AddByte(0xFF);
                        }
                        skip = 0;

                        AddTileDescription(connection, message, tile);
                    }
                    else
                    {
                        skip++;
                        if (skip == 0xFF)
                        {
                            message.AddByte(0xFF);
                            message.AddByte(0xFF);
                            skip = -1;
                        }
                    }
                }
            }

            return skip;
        }

        public static void AddTileDescription(Connection connection, NetworkMessage message, Tile tile)
        {
            if (tile != null)
            {
                int count = 0;
                if (tile.Ground != null)
                {
                    //AddItem(tile.Ground);
                    message.AddUInt16(tile.Ground.Id);
                    count++;
                }

                foreach (Item item in tile.GetTopItems())
                {
                    message.AddItem(item);
                }

                foreach (Creature creature in tile.Creatures)
                {
                    if (true)// (player->canSeeCreature(*cit))
                    {
                        uint removeKnown;
                        bool known = connection.IsCreatureKnown(creature.Id, out removeKnown);
                        message.AddCreature(creature, known, removeKnown);
                        count++;
                    }
                }

                foreach (Item item in tile.GetDownItems())
                {
                    message.AddItem(item);
                }
            }
        }
    }
}
