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

                    Item ground = new Item();
                    ground.Id = id;
                    tile.Ground = ground;

                    tile.Location = new Location(x, y, 7);

                    tiles[x, y, 7] = tile;
                }
            }
        }

        private void LoadTilesFromDB()
        {
            using (SQLiteConnection conn = new SQLiteConnection(SharpOT.Properties.Settings.Default.ConnectionString))
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                conn.Open();
                cmd.CommandText = "select * from MapTile where Z";
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
                    tiles[x, y, z] = tile;
                }
                reader.Close();
            }
        }

        private void LoadItemsFromDB()
        {
            using (SQLiteConnection conn = new SQLiteConnection(SharpOT.Properties.Settings.Default.ConnectionString))
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                conn.Open();
                cmd.CommandText = "select * from MapItem order by StackPosition";
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int x = reader.GetInt32(0) - 32000;
                    int y = reader.GetInt32(1) - 32000;
                    int z = reader.GetInt32(2);
                    ushort id = (ushort)reader.GetInt16(4);
                    byte extra = reader.GetByte(5);

                    if (tiles[x, y, z] != null)
                    {
                        Item item = new Item();
                        item.Id = id;
                        item.Extra = extra;
                        tiles[x, y, z].Items.Add(item);
                    }
                }
                reader.Close();
            }
        }

        public void Load()
        {
            // FillTiles(4526);
            LoadTilesFromDB();
            LoadItemsFromDB();
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
    }
}
