using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace SharpOT
{
    public class Map
    {
        public const int Size = 1024;

        Tile[,,] tiles = new Tile[Size, Size, 14];

        private void FillTiles(ushort id)
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    Tile tile = new Tile();

                    Item ground = new Item(id);
                    tile.Ground = ground;

                    tile.Location = new Location(x, y, 7);

                    tiles[x, y, 7] = tile;
                }
            }
        }

        public void Load()
        {
            // FillTiles(4526);
            Database.GetMapTiles(this);
            Database.GetMapItems(this);
        }

        public Tile GetTile(Location location)
        {
            return GetTile(
                location.X,
                location.Y,
                location.Z
            );
        }

        public Tile GetTile(int x, int y, int z)
        {
            if (x < 0 || x >= Size || 
                y < 0 || y >= Size ||
                z < 0 || z >= 14)
            {
                return null;
            }

            return tiles[x, y, z];
        }

        public bool SetTile(Location location, Tile tile)
        {
            if (location.X < 0 || location.X >= Size ||
                location.Y < 0 || location.Y >= Size ||
                location.Z < 0 || location.Z >= 14)
            {
                return false;
            }
            tile.Location = location;
            tiles[location.X, location.Y, location.Z] = tile;
            return true;
        }
    }
}
