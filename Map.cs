using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpOT
{
    // Singleton pattern from http://www.yoda.arachsys.com/csharp/singleton.html
    public class Map
    {
        #region Singleton Pattern

        static readonly Map instance = new Map();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Map() { }

        Map() { }

        public static Map Instance
        {
            get { return instance; }
        }

        #endregion

        public const int Size = 100;

        Tile[,] tiles = new Tile[Size, Size];

        public void Load()
        {
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    Tile tile = new Tile();

                    Item ground = new Item();
                    ground.Id = 4526;
                    tile.Ground = ground;

                    tile.Location = new Location(x, y, 7);

                    tiles[x, y] = tile;
                }
            }
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
            if (z != 7 || x < 0 || x >= Size || y < 0 || y >= Size)
            {
                return null;
            }

            return tiles[x, y];
        }
    }
}
