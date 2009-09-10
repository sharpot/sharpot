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
        //Player IDs and names are static and stored in the database
        //p.s.: Names with different casing are considered equal and
		// shall not be allowed.
        //e.g.: rIcHarD==richard==Richard
        private static SQLiteConnection connection = new SQLiteConnection(
            SharpOT.Properties.Settings.Default.ConnectionString
        );

        #region Account Commands
        private static SQLiteCommand insertAccountCommand = new SQLiteCommand(
            @"insert into Account (Name, Password)
              values (@accountName, @password)",
            connection
        );

        private static SQLiteCommand selectAccountIdCommand = new SQLiteCommand(
            @"select Id
              from Account
              where Name = @accountName
              and Password = @password",
            connection
        );

        private static SQLiteCommand selectAllAccountNamesCommand = new SQLiteCommand(
            @"select Name
            from Account",
            connection
        );
        #endregion

        #region Map Commands
        private static SQLiteCommand selectMapTilesCommand = new SQLiteCommand(
            @"select * from MapTile",
            connection
        );

        private static SQLiteCommand selectMapItemsCommand = new SQLiteCommand(
            @"select * from MapItem order by StackPosition",
            connection
        );
        #endregion

        #region Player Commands
        private static SQLiteCommand insertPlayerCommand = new SQLiteCommand(
            @"insert into Player 
                (AccountId, Id, Name, Gender, Vocation, Level, MagicLevel, 
                 Experience, MaxHealth,  MaxMana, Capacity, OutfitLookType, 
                 OutfitHead, OutfitBody, OutfitLegs, OutfitFeet, OutfitAddons)
              values
                (@accountId, @playerId, @name, @gender, @vocation, @level, @magicLevel,
                 @experience, @maxHealth, @maxMana, @capacity, @outfitLookType,
                 @outfitHead, @outfitBody, @outfitLegs, @outfitFeet, @outfitAddons)",
            connection
        );

        private static SQLiteCommand selectPlayerNameByAccountIdCommand = new SQLiteCommand(
            @"select Name
              from Player
              where AccountId = @accountId",
            connection
        );

        private static SQLiteCommand selectPlayerByNameCommand = new SQLiteCommand(
            @"select
                Id, Gender, Vocation, Level, MagicLevel,Experience, MaxHealth, 
                MaxMana, Capacity, OutfitLookType, OutfitHead, OutfitBody, 
                OutfitLegs, OutfitFeet, OutfitAddons, LocationX, LocationY, 
                LocationZ, Direction
              from Player
              where Name = @name",
            connection
        );

        private static SQLiteCommand selectPlayerByIdCommand = new SQLiteCommand(
            @"select
                Name, Gender, Vocation, Level, MagicLevel,Experience, MaxHealth, 
                MaxMana, Capacity, OutfitLookType, OutfitHead, OutfitBody, 
                OutfitLegs, OutfitFeet, OutfitAddons, LocationX, LocationY, 
                LocationZ, Direction
              from Player
              where Id = @playerId",
            connection
        );

        private static SQLiteCommand selectAllPlayersCommand = new SQLiteCommand(
            @"select
                Id, Name, Gender, Vocation, Level, MagicLevel,Experience, MaxHealth, 
                MaxMana, Capacity, OutfitLookType, OutfitHead, OutfitBody, 
                OutfitLegs, OutfitFeet, OutfitAddons, LocationX, LocationY, 
                LocationZ, Direction
              from Player",
            connection
        );

        private static SQLiteCommand selectPlayerIdNamePairsCommand = new SQLiteCommand(
            @"select
                Id, Name
            from Player",
            connection
        );
        
        private static SQLiteCommand updatePlayerByNameCommand = new SQLiteCommand(
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
              where Name = @name",
            connection
        );

        private static SQLiteCommand updatePlayerByIdCommand = new SQLiteCommand(
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
              where Id = @playerId",
            connection
        );
        #endregion

        private static SQLiteParameter accountNameParam = new SQLiteParameter("accountName");
        private static SQLiteParameter passwordParam = new SQLiteParameter("password");
        private static SQLiteParameter accountIdParam = new SQLiteParameter("accountId");
        private static SQLiteParameter nameParam = new SQLiteParameter("name");

        private static SQLiteParameter idParam = new SQLiteParameter("playerId");
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

            //Account management parameters
            insertAccountCommand.Parameters.Add(accountNameParam);
            insertAccountCommand.Parameters.Add(passwordParam);

            selectAccountIdCommand.Parameters.Add(accountNameParam);
            selectAccountIdCommand.Parameters.Add(passwordParam);

            selectPlayerNameByAccountIdCommand.Parameters.Add(accountIdParam);
            
            //Players parameters
            insertPlayerCommand.Parameters.Add(accountIdParam);
            insertPlayerCommand.Parameters.Add(idParam);
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
            
            selectPlayerByNameCommand.Parameters.Add(nameParam);

            selectPlayerByIdCommand.Parameters.Add(idParam);
            
            updatePlayerByNameCommand.Parameters.Add(idParam);
            updatePlayerByNameCommand.Parameters.Add(genderParam);
            updatePlayerByNameCommand.Parameters.Add(vocationParam);
            updatePlayerByNameCommand.Parameters.Add(levelParam);
            updatePlayerByNameCommand.Parameters.Add(magicLevelParam);
            updatePlayerByNameCommand.Parameters.Add(experienceParam);
            updatePlayerByNameCommand.Parameters.Add(maxHealthParam);
            updatePlayerByNameCommand.Parameters.Add(maxManaParam);
            updatePlayerByNameCommand.Parameters.Add(capacityParam);
            updatePlayerByNameCommand.Parameters.Add(outfitLookTypeParam);
            updatePlayerByNameCommand.Parameters.Add(outfitHeadParam);
            updatePlayerByNameCommand.Parameters.Add(outfitBodyParam);
            updatePlayerByNameCommand.Parameters.Add(outfitLegsParam);
            updatePlayerByNameCommand.Parameters.Add(outfitFeetParam);
            updatePlayerByNameCommand.Parameters.Add(outfitAddonsParam);
            updatePlayerByNameCommand.Parameters.Add(locationXParam);
            updatePlayerByNameCommand.Parameters.Add(locationYParam);
            updatePlayerByNameCommand.Parameters.Add(locationZParam);
            updatePlayerByNameCommand.Parameters.Add(directionParam);
            updatePlayerByNameCommand.Parameters.Add(nameParam);
            
            updatePlayerByIdCommand.Parameters.Add(nameParam);
            updatePlayerByIdCommand.Parameters.Add(genderParam);
            updatePlayerByIdCommand.Parameters.Add(vocationParam);
            updatePlayerByIdCommand.Parameters.Add(levelParam);
            updatePlayerByIdCommand.Parameters.Add(magicLevelParam);
            updatePlayerByIdCommand.Parameters.Add(experienceParam);
            updatePlayerByIdCommand.Parameters.Add(maxHealthParam);
            updatePlayerByIdCommand.Parameters.Add(maxManaParam);
            updatePlayerByIdCommand.Parameters.Add(capacityParam);
            updatePlayerByIdCommand.Parameters.Add(outfitLookTypeParam);
            updatePlayerByIdCommand.Parameters.Add(outfitHeadParam);
            updatePlayerByIdCommand.Parameters.Add(outfitBodyParam);
            updatePlayerByIdCommand.Parameters.Add(outfitLegsParam);
            updatePlayerByIdCommand.Parameters.Add(outfitFeetParam);
            updatePlayerByIdCommand.Parameters.Add(outfitAddonsParam);
            updatePlayerByIdCommand.Parameters.Add(locationXParam);
            updatePlayerByIdCommand.Parameters.Add(locationYParam);
            updatePlayerByIdCommand.Parameters.Add(locationZParam);
            updatePlayerByIdCommand.Parameters.Add(directionParam);
            updatePlayerByIdCommand.Parameters.Add(idParam);
        }

        public static void Close()
        {
            connection.Close();
        }

        #region Account Management
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

        public static bool CreatePlayer(long accountId, string name, uint playerid)
        {
            accountIdParam.Value = accountId;
            idParam.Value = playerid;
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

        public static IEnumerable<string> GetAllAccountNames()
        {
            SQLiteDataReader reader = selectAllAccountNamesCommand.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    yield return reader.GetString(0);
                }
            }
            finally
            {
                reader.Close();
            }

        }

        public static IEnumerable<CharacterListItem> GetCharacterList(long accountId)
        {
            var ipAddress = IPAddress.Parse(SharpOT.Properties.Settings.Default.Ip);
            var ipBytes = ipAddress.GetAddressBytes();

           
            accountIdParam.Value = accountId;
            SQLiteDataReader reader = selectPlayerNameByAccountIdCommand.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    yield return new CharacterListItem(
                        reader.GetString(0),
                        SharpOT.Properties.Settings.Default.WorldName,
                        ipBytes,
                        SharpOT.Properties.Settings.Default.Port
                    );
                }
            }
            finally
            {
                reader.Close();
            }
        }
        #endregion

        #region Players
        public static Player GetPlayerByName(long accountId, string name)
        {
            accountIdParam.Value = accountId;
            nameParam.Value = name;

            SQLiteDataReader reader = selectPlayerByNameCommand.ExecuteReader();
            if (reader.Read())
            {
                Player player = new Player();
                player.Name = name;
                player.Id =(uint) reader.GetInt32(0);
                player.Gender = (Gender)reader.GetByte(1);
                player.Vocation = (Vocation)reader.GetByte(2);
                player.Level = (ushort)reader.GetInt16(3);
                player.MagicLevel = reader.GetByte(4);
                player.Experience = (uint)reader.GetInt32(5);
                player.MaxHealth = (ushort)reader.GetInt16(6);
                player.MaxMana = (ushort)reader.GetInt16(7);
                player.Capacity = (uint)reader.GetInt32(8);
                player.Outfit.LookType = (ushort)reader.GetInt16(9);
                player.Outfit.Head = reader.GetByte(10);
                player.Outfit.Body = reader.GetByte(11);
                player.Outfit.Legs = reader.GetByte(12);
                player.Outfit.Feet = reader.GetByte(13);
                player.Outfit.Addons = reader.GetByte(14);
                if (!reader.IsDBNull(15))
                {
                    int x = reader.GetInt32(15);
                    int y = reader.GetInt32(16);
                    int z = reader.GetInt32(17);
                    player.SavedLocation = new Location(x, y, z);
                    player.Direction = (Direction)reader.GetByte(18);
                }
                reader.Close();

                player.Speed = (ushort)(220 + (2 * (player.Level - 1)));
                return player;
            }
            return null;
        }
        
        public static Player GetPlayerById(uint playerId)
        {
            idParam.Value = playerId;

            SQLiteDataReader reader = selectPlayerByIdCommand.ExecuteReader();
            if (reader.Read())
            {
                Player player = new Player();
                player.Name = reader.GetString(0);
                player.Id = playerId;
                player.Gender = (Gender)reader.GetByte(1);
                player.Vocation = (Vocation)reader.GetByte(2);
                player.Level = (ushort)reader.GetInt16(3);
                player.MagicLevel = reader.GetByte(4);
                player.Experience = (uint)reader.GetInt32(5);
                player.MaxHealth = (ushort)reader.GetInt16(6);
                player.MaxMana = (ushort)reader.GetInt16(7);
                player.Capacity = (uint)reader.GetInt32(8);
                player.Outfit.LookType = (ushort)reader.GetInt16(9);
                player.Outfit.Head = reader.GetByte(10);
                player.Outfit.Body = reader.GetByte(11);
                player.Outfit.Legs = reader.GetByte(12);
                player.Outfit.Feet = reader.GetByte(13);
                player.Outfit.Addons = reader.GetByte(14);
                if (!reader.IsDBNull(15))
                {
                    int x = reader.GetInt32(15);
                    int y = reader.GetInt32(16);
                    int z = reader.GetInt32(17);
                    player.SavedLocation = new Location(x, y, z);
                    player.Direction = (Direction)reader.GetByte(18);
                }
                reader.Close();

                player.Speed = (ushort)(220 + (2 * (player.Level - 1)));
                return player;
            }
            return null;
        }

        public static bool SavePlayerByName(Player player)
        {
            idParam.Value = player.Id;
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
            nameParam.Value = player.Name;

            return (1 == updatePlayerByNameCommand.ExecuteNonQuery());
        }

        public static bool SavePlayerById(Player player)
        {
            idParam.Value = player.Id;

            nameParam.Value = player.Name;
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

            return (1 == updatePlayerByIdCommand.ExecuteNonQuery());
        }

        public static IEnumerable<Player> GetAllPlayers()
        {
            SQLiteDataReader reader = selectAllPlayersCommand.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    Player player = new Player();
                    player.Id =(uint) reader.GetInt32(0);
                    player.Name = reader.GetString(1);
                    player.Gender = (Gender)reader.GetByte(2);
                    player.Vocation = (Vocation)reader.GetByte(3);
                    player.Level = (ushort)reader.GetInt16(4);
                    player.MagicLevel = reader.GetByte(5);
                    player.Experience = (uint)reader.GetInt32(6);
                    player.MaxHealth = (ushort)reader.GetInt16(7);
                    player.MaxMana = (ushort)reader.GetInt16(8);
                    player.Capacity = (uint)reader.GetInt32(9);
                    player.Outfit.LookType = (ushort)reader.GetInt16(10);
                    player.Outfit.Head = reader.GetByte(11);
                    player.Outfit.Body = reader.GetByte(12);
                    player.Outfit.Legs = reader.GetByte(13);
                    player.Outfit.Feet = reader.GetByte(14);
                    player.Outfit.Addons = reader.GetByte(15);
                    if (!reader.IsDBNull(16))
                    {
                        int x = reader.GetInt32(16);
                        int y = reader.GetInt32(17);
                        int z = reader.GetInt32(18);
                        player.SavedLocation = new Location(x, y, z);
                        player.Direction = (Direction)reader.GetByte(19);
                    }
                    yield return player;
                }
            }
            finally
            {
                reader.Close();
            }
        }

        public static Dictionary<uint, string> GetPlayerIdNameDictionary()
        {
            Dictionary<uint, string> dictionary = new Dictionary<uint, string>();

            SQLiteDataReader reader = selectPlayerIdNamePairsCommand.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    dictionary.Add((uint)reader.GetInt32(0), reader.GetString(1));
                }
            }
            finally
            {
                reader.Close();
            }
            return dictionary;
        }

        #endregion

        #region Map
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
        #endregion
    }
}
