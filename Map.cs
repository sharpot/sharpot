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

            using (SQLiteConnection conn = new SQLiteConnection(SharpOT.Properties.Settings.Default.ConnectionString))
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                conn.Open();
                cmd.CommandText = "select * from Tile where Z = 7";
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Tile tile = new Tile();
                    tile.Ground = new Item();
                    int x = reader.GetInt32(0) - 32000;
                    int y = reader.GetInt32(1) - 32000;
                    int z = reader.GetInt32(2);
                    tile.Ground.Id = (ushort)reader.GetInt16(3);
                    tile.Location = new Location(x, y, z);
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
