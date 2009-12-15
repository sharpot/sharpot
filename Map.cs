using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace SharpOT
{
    public class Map
    {
        public List<Town> Towns { get; private set; }
        public string Description { get; set; }
        public string SpawnFile { get; set; }
        public string HouseFile { get; set; }

        Dictionary<Location, Tile> tiles = new Dictionary<Location, Tile>();

        public Map()
        {
            Towns = new List<Town>();
        }

        public void Load()
        {
            OpenTibia.OtbmReader reader = new SharpOT.OpenTibia.OtbmReader(@"Data\map.otbm");
            reader.GetMapTiles(this);
        }

        public Location DefaultLocation
        {
            get
            {
                return Towns[0].TempleLocation;
            }
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
