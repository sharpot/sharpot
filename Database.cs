using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Net;
using System.Security.Cryptography;

namespace SharpOT
{
    public class Database
    {
        private static SQLiteConnection connection = new SQLiteConnection(
            SharpOT.Properties.Settings.Default.ConnectionString
        );

        private static SQLiteCommand insertAccountCommand = new SQLiteCommand(
            @"insert into Account (Name, Password)
              values (@accountName, @password)",
            connection
        );

        private static SQLiteCommand insertPlayerCommand = new SQLiteCommand(
            @"insert into Player 
                (AccountId, Name, Gender, Vocation, Level, MagicLevel, 
                 Experience, MaxHealth,  MaxMana, Capacity, OutfitLookType, 
                 OutfitHead, OutfitBody, OutfitLegs, OutfitFeet, OutfitAddons)
              values
                (@accountId, @name, @gender, @vocation, @level, @magicLevel,
                 @experience, @maxHealth, @maxMana, @capacity, @outfitLookType,
                 @outfitHead, @outfitBody, @outfitLegs, @outfitFeet, @outfitAddons)",
            connection
        );

        private static SQLiteCommand selectAccountIdCommand = new SQLiteCommand(
            @"select Id
              from Account
              where Name = @accountName
              and Password = @password",
            connection
        );

        private static SQLiteCommand selectPlayersCommand = new SQLiteCommand(
            @"select Name
              from Player
              where AccountId = @accountId",
            connection
        );

        private static SQLiteCommand selectMapTilesCommand = new SQLiteCommand(
            @"select * from MapTile",
            connection
        );

        private static SQLiteCommand selectMapItemsCommand = new SQLiteCommand(
            @"select * from MapItem order by StackPosition",
            connection
        );

        private static SQLiteCommand selectPlayerCommand = new SQLiteCommand(
            @"select
                Gender, Vocation, Level, MagicLevel,Experience, MaxHealth, 
                MaxMana, Capacity, OutfitLookType, OutfitHead, OutfitBody, 
                OutfitLegs, OutfitFeet, OutfitAddons, LocationX, LocationY, 
                LocationZ, Direction
              from Player
              where AccountId = @accountId and Name = @name",
            connection
        );

        private static SQLiteCommand updatePlayerCommand = new SQLiteCommand(
            @"update Player
              set
                  Gender = @gender,
                  Vocation = @vocation,
                  Level = @level,
                  MagicLevel = @magicLevel,
                  Experience = @experience,
                  MaxHealth = @maxHealth,
                  MaxMana = @maxMana,
                  Capacity = @capacity,
                  OutfitLookType = @outfitLookType,
                  OutfitHead = @outfitHead,
                  OutfitBody = @outfitBody,
                  OutfitLegs = @outfitLegs,
                  OutfitFeet = @outfitFeet,
                  OutfitAddons = @outfitAddons,
                  LocationX = @locationX,
                  LocationY = @locationY,
                  LocationZ = @locationZ,
                  Direction = @direction
              where AccountId = @accountId and Name = @name",
            connection
        );

        private static SQLiteParameter accountNameParam = new SQLiteParameter("accountName");
        private static SQLiteParameter passwordParam = new SQLiteParameter("password");
        private static SQLiteParameter accountIdParam = new SQLiteParameter("accountId");
        private static SQLiteParameter nameParam = new SQLiteParameter("name");

        private static SQLiteParameter genderParam = new SQLiteParameter("gender");
        private static SQLiteParameter vocationParam = new SQLiteParameter("vocation");
        private static SQLiteParameter levelParam = new SQLiteParameter("level");
        private static SQLiteParameter magicLevelParam = new SQLiteParameter("magicLevel");
        private static SQLiteParameter experienceParam = new SQLiteParameter("experience");
        private static SQLiteParameter maxHealthParam = new SQLiteParameter("maxHealth");
        private static SQLiteParameter maxManaParam = new SQLiteParameter("maxMana");
        private static SQLiteParameter capacityParam = new SQLiteParameter("capacity");
        private static SQLiteParameter outfitLookTypeParam = new SQLiteParameter("outfitLookType");
        private static SQLiteParameter outfitHeadParam = new SQLiteParameter("outfitHead");
        private static SQLiteParameter outfitBodyParam = new SQLiteParameter("outfitBody");
        private static SQLiteParameter outfitLegsParam = new SQLiteParameter("outfitLegs");
        private static SQLiteParameter outfitFeetParam = new SQLiteParameter("outfitFeet");
        private static SQLiteParameter outfitAddonsParam = new SQLiteParameter("outfitAddons");
        private static SQLiteParameter locationXParam = new SQLiteParameter("locationX");
        private static SQLiteParameter locationYParam = new SQLiteParameter("locationY");
        private static SQLiteParameter locationZParam = new SQLiteParameter("locationZ");
        private static SQLiteParameter directionParam = new SQLiteParameter("direction");

        static Database()
        {
            connection.Open();

            insertAccountCommand.Parameters.Add(accountNameParam);
            insertAccountCommand.Parameters.Add(passwordParam);

            insertPlayerCommand.Parameters.Add(accountIdParam);
            insertPlayerCommand.Parameters.Add(nameParam);
            insertPlayerCommand.Parameters.Add(genderParam);
            insertPlayerCommand.Parameters.Add(vocationParam);
            insertPlayerCommand.Parameters.Add(levelParam);
            insertPlayerCommand.Parameters.Add(magicLevelParam);
            insertPlayerCommand.Parameters.Add(experienceParam);
            insertPlayerCommand.Parameters.Add(maxHealthParam);
            insertPlayerCommand.Parameters.Add(maxManaParam);
            insertPlayerCommand.Parameters.Add(capacityParam);
            insertPlayerCommand.Parameters.Add(outfitLookTypeParam);
            insertPlayerCommand.Parameters.Add(outfitHeadParam);
            insertPlayerCommand.Parameters.Add(outfitBodyParam);
            insertPlayerCommand.Parameters.Add(outfitLegsParam);
            insertPlayerCommand.Parameters.Add(outfitFeetParam);
            insertPlayerCommand.Parameters.Add(outfitAddonsParam);

            selectAccountIdCommand.Parameters.Add(accountNameParam);
            selectAccountIdCommand.Parameters.Add(passwordParam);

            selectPlayerCommand.Parameters.Add(accountIdParam);
            selectPlayerCommand.Parameters.Add(nameParam);

            selectPlayersCommand.Parameters.Add(accountIdParam);

            updatePlayerCommand.Parameters.Add(genderParam);
            updatePlayerCommand.Parameters.Add(vocationParam);
            updatePlayerCommand.Parameters.Add(levelParam);
            updatePlayerCommand.Parameters.Add(magicLevelParam);
            updatePlayerCommand.Parameters.Add(experienceParam);
            updatePlayerCommand.Parameters.Add(maxHealthParam);
            updatePlayerCommand.Parameters.Add(maxManaParam);
            updatePlayerCommand.Parameters.Add(capacityParam);
            updatePlayerCommand.Parameters.Add(outfitLookTypeParam);
            updatePlayerCommand.Parameters.Add(outfitHeadParam);
            updatePlayerCommand.Parameters.Add(outfitBodyParam);
            updatePlayerCommand.Parameters.Add(outfitLegsParam);
            updatePlayerCommand.Parameters.Add(outfitFeetParam);
            updatePlayerCommand.Parameters.Add(outfitAddonsParam);
            updatePlayerCommand.Parameters.Add(accountIdParam);
            updatePlayerCommand.Parameters.Add(nameParam);
            updatePlayerCommand.Parameters.Add(locationXParam);
            updatePlayerCommand.Parameters.Add(locationYParam);
            updatePlayerCommand.Parameters.Add(locationZParam);
            updatePlayerCommand.Parameters.Add(directionParam);
        }

        public static void Close()
        {
            connection.Close();
        }

        public static bool CreateAccount(string name, string password)
        {
            accountNameParam.Value = name;
            passwordParam.Value = Util.Hash.SHA256Hash(password);
            try
            {
                return (1 == insertAccountCommand.ExecuteNonQuery());
            }
            catch (SQLiteException)
            {
                return false;
            }
        }

        public static bool CreatePlayer(long accountId, string name)
        {
            accountIdParam.Value = accountId;
            nameParam.Value = name;
            genderParam.Value = Gender.Male;
            vocationParam.Value = Vocation.None;
            levelParam.Value = 1;
            magicLevelParam.Value = 0;
            experienceParam.Value = 0;
            maxHealthParam.Value = 100;
            maxManaParam.Value = 0;
            capacityParam.Value = 100;
            outfitLookTypeParam.Value = 128;
            outfitHeadParam.Value = 0;
            outfitBodyParam.Value = 0;
            outfitLegsParam.Value = 0;
            outfitFeetParam.Value = 0;
            outfitAddonsParam.Value = 0;
            try
            {
                return (1 == insertPlayerCommand.ExecuteNonQuery());
            }
            catch (SQLiteException)
            {
                return false;
            }
        }

        public static List<CharacterListItem> GetCharacterList(long accountId)
        {
            List<CharacterListItem> chars = new List<CharacterListItem>();

            var ipAddress = IPAddress.Parse(SharpOT.Properties.Settings.Default.Ip);
            var ipBytes = ipAddress.GetAddressBytes();

           
            accountIdParam.Value = accountId;
            SQLiteDataReader reader = selectPlayersCommand.ExecuteReader();
            while (reader.Read())
            {
                chars.Add(new CharacterListItem(
                    reader.GetString(0),
                    SharpOT.Properties.Settings.Default.WorldName,
                    ipBytes,
                    SharpOT.Properties.Settings.Default.Port
                ));
            }
            reader.Close();
            return chars;
        }

        public static Player GetPlayer(long accountId, string name)
        {
            accountIdParam.Value = accountId;
            nameParam.Value = name;

            SQLiteDataReader reader = selectPlayerCommand.ExecuteReader();
            if (reader.Read())
            {
                Player player = new Player();
                player.Name = name;
                player.Gender = (Gender)reader.GetByte(0);
                player.Vocation = (Vocation)reader.GetByte(1);
                player.Level = (ushort)reader.GetInt16(2);
                player.MagicLevel = reader.GetByte(3);
                player.Experience = (uint)reader.GetInt32(4);
                player.MaxHealth = (ushort)reader.GetInt16(5);
                player.MaxMana = (ushort)reader.GetInt16(6);
                player.Capacity = (uint)reader.GetInt32(7);
                player.Outfit.LookType = (ushort)reader.GetInt16(8);
                player.Outfit.Head = reader.GetByte(9);
                player.Outfit.Body = reader.GetByte(10);
                player.Outfit.Legs = reader.GetByte(11);
                player.Outfit.Feet = reader.GetByte(12);
                player.Outfit.Addons = reader.GetByte(13);
                if (!reader.IsDBNull(14))
                {
                    int x = reader.GetInt32(14);
                    int y = reader.GetInt32(15);
                    int z = reader.GetInt32(16);
                    player.SavedLocation = new Location(x, y, z);
                    player.Direction = (Direction)reader.GetByte(17);
                }
                reader.Close();

                player.Speed = (ushort)(220 + (2 * (player.Level - 1)));
                return player;
            }
            return null;
        }

        public static long GetAccountId(string accountName, string password)
        {
            accountNameParam.Value = accountName;
            passwordParam.Value = Util.Hash.SHA256Hash(password);
            var result = selectAccountIdCommand.ExecuteScalar();
            if (result == null)
            {
                return -1;
            }
            return (long)result;
        }

        public static void GetMapTiles(Map map)
        {
            SQLiteDataReader reader = selectMapTilesCommand.ExecuteReader();
            while (reader.Read())
            {
                Tile tile = new Tile();
                tile.Ground = new Item();
                int x = reader.GetInt32(0) - 32000;
                int y = reader.GetInt32(1) - 32000;
                int z = reader.GetInt32(2);
                tile.Ground.Id = (ushort)reader.GetInt16(3);
                Location location = new Location(x, y, z);
                map.SetTile(location, tile);
            }
            reader.Close();
        }

        public static void GetMapItems(Map map)
        {
            SQLiteDataReader reader = selectMapItemsCommand.ExecuteReader();
            while (reader.Read())
            {
                int x = reader.GetInt32(0) - 32000;
                int y = reader.GetInt32(1) - 32000;
                int z = reader.GetInt32(2);
                ushort id = (ushort)reader.GetInt16(4);
                byte extra = reader.GetByte(5);

                Tile tile = map.GetTile(x, y, z);
                if (tile != null)
                {
                    Item item = new Item();
                    item.Id = id;
                    item.Extra = extra;
                    tile.Items.Add(item);
                }
            }
            reader.Close();
        }

        public static bool SavePlayer(Player player)
        {
            genderParam.Value = player.Gender;
            vocationParam.Value = player.Vocation;
            levelParam.Value = player.Level;
            magicLevelParam.Value = player.MagicLevel;
            experienceParam.Value = player.Experience;
            maxHealthParam.Value = player.MaxHealth;
            maxManaParam.Value = player.MaxMana;
            capacityParam.Value = player.Capacity;
            outfitLookTypeParam.Value = player.Outfit.LookType;
            outfitHeadParam.Value = player.Outfit.Head;
            outfitBodyParam.Value = player.Outfit.Body;
            outfitLegsParam.Value = player.Outfit.Legs;
            outfitFeetParam.Value = player.Outfit.Feet;
            outfitAddonsParam.Value = player.Outfit.Addons;
            locationXParam.Value = player.Tile.Location.X;
            locationYParam.Value = player.Tile.Location.Y;
            locationZParam.Value = player.Tile.Location.Z;
            directionParam.Value = player.Direction;
            accountIdParam.Value = player.Connection.AccountId;

            return (1 == updatePlayerCommand.ExecuteNonQuery());
        }
    }
}
