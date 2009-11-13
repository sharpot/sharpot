using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Net;
using System.Security.Cryptography;
using System.IO;

namespace SharpOT
{
    public class Database
    {
        private static SQLiteConnection connection = new SQLiteConnection();

        #region Account Commands

        private static SQLiteCommand insertAccountCommand = new SQLiteCommand(
            @"insert into Account (Name, Password)
              values (@accountName, @password)",
            connection
        );

        private static SQLiteCommand deleteAccountCommand = new SQLiteCommand(
            @"delete from Account
                where Id=@accountId",
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

        private static SQLiteCommand checkAccountNameCommand = new SQLiteCommand(
            @"select Name
              from Account
              where Name=@accountName",
            connection
        );

        private static SQLiteCommand checkPlayerNameCommand = new SQLiteCommand(
            @"select Name
              from Player
              where Name=@name",
            connection
        );

        private static SQLiteCommand checkPlayerIdCommand = new SQLiteCommand(
            @"select Id
              from Player
              where Id=@playerId",
            connection
        );

        private static SQLiteCommand selectLastInsertId = new SQLiteCommand(
            "select last_insert_rowid()",
            connection
        );

        private static SQLiteCommand updatePasswordByAccountId = new SQLiteCommand(
            @"update Account
                set
                    Password=@password
                where Id=@accountId",
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

        private static SQLiteCommand deletePlayerByName = new SQLiteCommand(
            @"delete from Player
                where Name=@name",
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
                Id, Name, Gender, Vocation, Level, MagicLevel,Experience, MaxHealth, 
                MaxMana, Capacity, OutfitLookType, OutfitHead, OutfitBody, 
                OutfitLegs, OutfitFeet, OutfitAddons, LocationX, LocationY, 
                LocationZ, Direction, LastLogin
              from Player
              where Name = @name",
            connection
        );

        private static SQLiteCommand selectPlayerByIdCommand = new SQLiteCommand(
            @"select
                Id, Name, Gender, Vocation, Level, MagicLevel,Experience, MaxHealth, 
                MaxMana, Capacity, OutfitLookType, OutfitHead, OutfitBody, 
                OutfitLegs, OutfitFeet, OutfitAddons, LocationX, LocationY, 
                LocationZ, Direction, LastLogin
              from Player
              where Id = @playerId",
            connection
        );

        private static SQLiteCommand selectAllPlayersCommand = new SQLiteCommand(
            @"select
                Id, Name, Gender, Vocation, Level, MagicLevel,Experience, MaxHealth, 
                MaxMana, Capacity, OutfitLookType, OutfitHead, OutfitBody, 
                OutfitLegs, OutfitFeet, OutfitAddons, LocationX, LocationY, 
                LocationZ, Direction, LastLogin
              from Player",
            connection
        );

        private static SQLiteCommand selectPlayerIdNamePairsCommand = new SQLiteCommand(
            @"select
                Id, Name
            from Player",
            connection
        );

        private static SQLiteCommand selectPlayerNameByIdCommand = new SQLiteCommand(
            @"select
                Name
            from Player
            where Id=@playerId",
            connection
        );

        private static SQLiteCommand selectPlayerIdByNameCommand = new SQLiteCommand(
            @"select
                Id
            from Player
            where Name=@name",
            connection
        );

        private static SQLiteCommand updatePlayerByIdCommand = new SQLiteCommand(
            @"update Player
              set
                  Name = @name,
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
                  Direction = @direction,
                  LastLogin = @lastLogin
              where Id = @playerId",
            connection
        );

        #endregion

        #region Parameters

        private static SQLiteParameter accountNameParam = new SQLiteParameter("accountName");
        private static SQLiteParameter passwordParam = new SQLiteParameter("password");
        private static SQLiteParameter accountIdParam = new SQLiteParameter("accountId");
        private static SQLiteParameter playerNameParam = new SQLiteParameter("name");

        private static SQLiteParameter playerIdParam = new SQLiteParameter("playerId");
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
        private static SQLiteParameter lastLoginParam = new SQLiteParameter("lastLogin");

        #endregion

        #region Setup

        public static void ExecuteScript(string sql)
        {
            var transaction = connection.BeginTransaction();
            SQLiteCommand command = new SQLiteCommand(connection);
            foreach (string query in sql.Split(';'))
            {
                command.CommandText = query.Trim();
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public static void Initialize(string databaseFile)
        {
            if (!File.Exists(databaseFile))
            {
                SQLiteConnection.CreateFile(databaseFile);
            }

            connection.ConnectionString = "data source=" + databaseFile + ";synchronous=Full";
            
            connection.Open();

            // Make sure the database structure exists
            // If it does not, create it
            try
            {
                selectAllAccountNamesCommand.ExecuteScalar();
            }
            catch (SQLiteException e)
            {
                if (e.Message.Contains("no such table"))
                {
                    string sql = File.ReadAllText(SharpOT.Properties.Settings.Default.SchemaFile);
                    ExecuteScript(sql);
                }
            }

            //Account management parameters
            insertAccountCommand.Parameters.Add(accountNameParam);
            insertAccountCommand.Parameters.Add(passwordParam);

            deleteAccountCommand.Parameters.Add(accountIdParam);

            selectAccountIdCommand.Parameters.Add(accountNameParam);
            selectAccountIdCommand.Parameters.Add(passwordParam);

            selectPlayerNameByAccountIdCommand.Parameters.Add(accountIdParam);

            checkAccountNameCommand.Parameters.Add(accountNameParam);

            checkPlayerNameCommand.Parameters.Add(playerNameParam);

            checkPlayerIdCommand.Parameters.Add(playerIdParam);

            updatePasswordByAccountId.Parameters.Add(accountIdParam);
            updatePasswordByAccountId.Parameters.Add(passwordParam);
            //Players parameters
            insertPlayerCommand.Parameters.Add(accountIdParam);
            insertPlayerCommand.Parameters.Add(playerIdParam);
            insertPlayerCommand.Parameters.Add(playerNameParam);
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

            deletePlayerByName.Parameters.Add(playerNameParam);

            selectPlayerByNameCommand.Parameters.Add(playerNameParam);

            selectPlayerIdByNameCommand.Parameters.Add(playerNameParam);

            selectPlayerByIdCommand.Parameters.Add(playerIdParam);

            selectPlayerNameByIdCommand.Parameters.Add(playerIdParam);
            
            updatePlayerByIdCommand.Parameters.Add(playerNameParam);
            AddUpdateParams(updatePlayerByIdCommand);
            updatePlayerByIdCommand.Parameters.Add(playerIdParam);
        }

        public static void AddUpdateParams(SQLiteCommand command)
        {
            command.Parameters.Add(genderParam);
            command.Parameters.Add(vocationParam);
            command.Parameters.Add(levelParam);
            command.Parameters.Add(magicLevelParam);
            command.Parameters.Add(experienceParam);
            command.Parameters.Add(maxHealthParam);
            command.Parameters.Add(maxManaParam);
            command.Parameters.Add(capacityParam);
            command.Parameters.Add(outfitLookTypeParam);
            command.Parameters.Add(outfitHeadParam);
            command.Parameters.Add(outfitBodyParam);
            command.Parameters.Add(outfitLegsParam);
            command.Parameters.Add(outfitFeetParam);
            command.Parameters.Add(outfitAddonsParam);
            command.Parameters.Add(locationXParam);
            command.Parameters.Add(locationYParam);
            command.Parameters.Add(locationZParam);
            command.Parameters.Add(directionParam);
            command.Parameters.Add(lastLoginParam);
        }

        public static void Close()
        {
            connection.Close();
        }

        #endregion

        #region Account Management

        public static int GetLastInsertId()
        {
            return Convert.ToInt32(selectLastInsertId.ExecuteScalar());
        }

        public static int CreateAccount(string name, string password)
        {
            int id = -1;

            accountNameParam.Value = name;
            passwordParam.Value = Util.Hash.SHA256Hash(password);

            try
            {
                if (1 == insertAccountCommand.ExecuteNonQuery())
                {
                    id = GetLastInsertId();
                }
            }
            catch (SQLiteException)
            {
                return id;
            }

            return id;
        }

        public static bool DeleteAccount(long accountId)
        {
            accountIdParam.Value = accountId;

            return (1 == deleteAccountCommand.ExecuteNonQuery());
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

        public static int CreatePlayer(long accountId, string name, uint playerid)
        {
            int id = -1;

            accountIdParam.Value = accountId;
            playerIdParam.Value = playerid;
            playerNameParam.Value = name;
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
                if (1 == insertPlayerCommand.ExecuteNonQuery())
                {
                    id = GetLastInsertId();
                }
            }
            catch (SQLiteException)
            {
                return id;
            }

            return id;
        }

        public static bool DeletePlayerByName(string playerName)
        {
            playerNameParam.Value=playerName;
            return (1 == deletePlayerByName.ExecuteNonQuery());
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

        public static bool AccountNameExists(string accName)
        {
            accountNameParam.Value = accName;
            return null != checkAccountNameCommand.ExecuteScalar();
        }

        public static bool PlayerNameExists(string playerName)
        {
            playerNameParam.Value = playerName;
            return null != checkPlayerNameCommand.ExecuteScalar();
        }

        public static bool PlayerIdExists(uint id)
        {
            playerIdParam.Value = id;
            return null != checkPlayerIdCommand.ExecuteScalar();
        }

        public static bool UpdateAccountPassword(long accountId,string newPassword)
        {
            accountIdParam.Value = accountId;
            passwordParam.Value = Util.Hash.SHA256Hash(newPassword);

            return (1 == updatePasswordByAccountId.ExecuteNonQuery());
        }

        #endregion

        #region Players
        public static Player GetPlayerByName(long accountId, string name)
        {
            Player player = null;
            accountIdParam.Value = accountId;
            playerNameParam.Value = name;

            SQLiteDataReader reader = selectPlayerByNameCommand.ExecuteReader();
            if (reader.Read())
            {
                player = new Player();
                ReadPlayerInfo(reader, player);
            }
            reader.Close();
            return player;
        }
        
        public static Player GetPlayerById(uint playerId)
        {
            Player player = null;
            playerIdParam.Value = playerId;

            SQLiteDataReader reader = selectPlayerByIdCommand.ExecuteReader();
            if (reader.Read())
            {
                player = new Player();
                ReadPlayerInfo(reader, player);
            }
            reader.Close();
            return player;
        }

        public static bool SavePlayerById(Player player)
        {
            PlayerInfoToParams(player);

            return (1 == updatePlayerByIdCommand.ExecuteNonQuery());
        }

        private static void PlayerInfoToParams(Player player)
        {
            playerIdParam.Value = player.Id;
            playerNameParam.Value = player.Name;
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

            if (player.Tile != null)
            {
                locationXParam.Value = player.Tile.Location.X;
                locationYParam.Value = player.Tile.Location.Y;
                locationZParam.Value = player.Tile.Location.Z;
            }
            else if (player.SavedLocation != null)
            {
                locationXParam.Value = player.SavedLocation.X;
                locationYParam.Value = player.SavedLocation.Y;
                locationZParam.Value = player.SavedLocation.Z;
            }
            else
            {
                locationXParam.Value = null;
                locationYParam.Value = null;
                locationZParam.Value = null;
            }

            directionParam.Value = player.Direction;
            lastLoginParam.Value = player.LastLogin.Ticks;
        }

        public static IEnumerable<Player> GetAllPlayers()
        {
            SQLiteDataReader reader = selectAllPlayersCommand.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    Player player = new Player();
                    ReadPlayerInfo(reader, player);
                    yield return player;
                }
            }
            finally
            {
                reader.Close();
            }
        }

        private static void ReadPlayerInfo(SQLiteDataReader reader, Player player)
        {
            player.Id = (uint)reader.GetInt32(0);
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
            if (reader.GetInt64(20) > 0)
            {
                int x = reader.GetInt32(16);
                int y = reader.GetInt32(17);
                int z = reader.GetInt32(18);
                player.SavedLocation = new Location(x, y, z);
                player.Direction = (Direction)reader.GetByte(19);
                player.LastLogin = new DateTime(reader.GetInt64(20));
            }
            player.Speed = (ushort)(220 + (2 * (player.Level - 1)));
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

        public static KeyValuePair<uint, string> GetPlayerIdNamePair(uint playerId)
        {
            KeyValuePair<uint, string> pair =new KeyValuePair<uint,string>();
            playerIdParam.Value = playerId;
            SQLiteDataReader reader = selectPlayerNameByIdCommand.ExecuteReader();
            if (reader.Read()) 
                pair = new KeyValuePair<uint, string>(playerId, reader.GetString(0));
            reader.Close();
            return pair;
        }

        public static KeyValuePair<uint, string> GetPlayerIdNamePair(string playerName)
        {
            KeyValuePair<uint, string> pair = new KeyValuePair<uint, string>();
            playerNameParam.Value = playerName;
            SQLiteDataReader reader =selectPlayerIdByNameCommand.ExecuteReader();
            if (reader.Read()) 
                pair = new KeyValuePair<uint, string>((uint)reader.GetInt32(0), playerName);
            reader.Close();
            return pair;
        }

        #endregion

        #region Map

        public static void GetMapTiles(Map map)
        {
            SQLiteDataReader reader = selectMapTilesCommand.ExecuteReader();
            while (reader.Read())
            {
                Tile tile = new Tile();
                int x = reader.GetInt32(0) - 32000;
                int y = reader.GetInt32(1) - 32000;
                int z = reader.GetInt32(2);
                tile.Ground = Item.Create((ushort)reader.GetInt16(3));
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
                    Item item = Item.Create(id);
                    item.Extra = extra;
                    tile.AddItem(item);
                }
            }
            reader.Close();
        }

        #endregion
    }
}
