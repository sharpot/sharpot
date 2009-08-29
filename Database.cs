using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Net;

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
            @"insert into Player (AccountId, Name)
              values (@accountId, @name)",
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

        private static SQLiteCommand selectPlayerCommand = new SQLiteCommand(
            @"select Level, MagicLevel, Experience, 
                  MaxHealth, MaxMana, Capacity, 
                  OutfitLookType, OutfitHead, OutfitBody,
                  OutfitLegs, OutfitFeet, OutfitAddons
              from Player
              where AccountId = @accountId and Name = @name",
            connection
        );

        private static SQLiteCommand updatePlayerCommand = new SQLiteCommand(
            @"update Player
              set
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
                  OutfitAddons = @outfitAddons
              where AccountId = @accountId and Name = @name",
            connection
        );

        private static SQLiteParameter accountNameParam = new SQLiteParameter("accountName");
        private static SQLiteParameter passwordParam = new SQLiteParameter("password");
        private static SQLiteParameter accountIdParam = new SQLiteParameter("accountId");
        private static SQLiteParameter nameParam = new SQLiteParameter("name");

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

        static Database()
        {
            connection.Open();

            insertAccountCommand.Parameters.Add(accountNameParam);
            insertAccountCommand.Parameters.Add(passwordParam);

            insertPlayerCommand.Parameters.Add(accountIdParam);
            insertPlayerCommand.Parameters.Add(nameParam);

            selectAccountIdCommand.Parameters.Add(accountNameParam);
            selectAccountIdCommand.Parameters.Add(passwordParam);

            selectPlayerCommand.Parameters.Add(accountIdParam);
            selectPlayerCommand.Parameters.Add(nameParam);

            selectPlayersCommand.Parameters.Add(accountIdParam);

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
        }

        public static void Close()
        {
            connection.Close();
        }

        public static bool CreateAccount(string name, string password)
        {
            accountNameParam.Value = name;
            passwordParam.Value = password;
            return (1 == insertAccountCommand.ExecuteNonQuery());
        }

        public static bool CreatePlayer(long accountId, string name)
        {
            accountIdParam.Value = accountId;
            nameParam.Value = name;
            return (1 == insertPlayerCommand.ExecuteNonQuery());
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
                player.Level = (ushort)reader.GetInt16(0);
                player.MagicLevel = reader.GetByte(1);
                player.Experience = (uint)reader.GetInt32(2);
                player.MaxHealth = (ushort)reader.GetInt16(3);
                player.MaxMana = (ushort)reader.GetInt16(4);
                player.Capacity = (uint)reader.GetInt32(5);
                player.Outfit.LookType = (ushort)reader.GetInt16(6);
                player.Outfit.Head = reader.GetByte(7);
                player.Outfit.Body = reader.GetByte(8);
                player.Outfit.Legs = reader.GetByte(9);
                player.Outfit.Feet = reader.GetByte(10);
                player.Outfit.Addons = reader.GetByte(11);
                reader.Close();

                // TODO: Calculate player speed
                player.Speed = 600;
                return player;
            }
            return null;
        }

        public static long GetAccountId(string accountName, string password)
        {
            accountNameParam.Value = accountName;
            passwordParam.Value = password;
            var result = selectAccountIdCommand.ExecuteScalar();
            if (result == null)
            {
                return -1;
            }
            return (long)result;
        }

        public static bool UpdatePlayer(Player player)
        {
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

            accountIdParam.Value = player.Connection.AccountId;

            return (1 == updatePlayerCommand.ExecuteNonQuery());
        }
    }
}
