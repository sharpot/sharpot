using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace SharpOT
{
    public class Map
    {
        Dictionary<Location, Tile> tiles = new Dictionary<Location, Tile>();
        Location defaultLocation = new Location(32097, 32205, 7);

        public void Load()
        {
            Database.GetMapTiles(this);
            Database.GetMapItems(this);
        }

        public Location GetDefaultLocation()
        {
            return new Location(defaultLocation);
        }

        public Tile GetTile(Location location)
        {
            if (tiles.ContainsKey(location))
            {
                return tiles[location];
            }
            else
            {
                return null;
            }
        }

        public Tile GetTile(int x, int y, int z)
        {
            return GetTile(
                new Location(x, y, z)
            );
        }

        public bool SetTile(Location location, Tile tile)
        {
            tile.Location = location;
            tiles[location] = tile;
            return true;
        }
    }
}
