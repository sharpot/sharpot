using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpOT.Packets;
using System.Data.SQLite;
using System.Net;

namespace SharpOT
{
    public class Game
    {
        #region Variables

        public Map Map { get; private set; }
        public Dictionary<uint, Creature> creatures = new Dictionary<uint, Creature>();
        Random random = new Random();

        #endregion

        #region Constructor

        public Game()
        {
            Map = new Map();
        }

        #endregion

        #region Private Helpers

        private void AddCreature(Creature creature)
        {
            creatures.Add(creature.Id, creature);
        }

        private void RemoveCreature(Creature creature)
        {
            creatures.Remove(creature.Id);
        }

        private IEnumerable<Creature> GetSpectators(Location location)
        {
            return creatures.Values.Where(creature => creature.Tile.Location.CanSee(location));
        }

        private IEnumerable<Player> GetSpectatorPlayers(Location location)
        {
            return GetPlayers().Where(player => player.Tile.Location.CanSee(location));
        }

        private IEnumerable<Player> GetPlayers()
        {
            return creatures.Values.OfType<Player>();
        }

        #endregion

        #region Public Actions

        public void WalkCancel(Player player)
        {
            player.Connection.SendCancelWalk();
        }

        public void CreatureTurn(Creature creature, Direction direction)
        {
            if (creature.Direction != direction)
            {
                creature.Direction = direction;
                foreach (var player in GetSpectatorPlayers(creature.Tile.Location))
                {
                    player.Connection.SendCreatureTurn(creature);
                }
            }
        }

        public void CreatureChangeOutfit(Creature creature, Outfit outfit)
        {
            creature.Outfit = outfit;
            foreach (var player in GetSpectatorPlayers(creature.Tile.Location))
            {
                player.Connection.SendCreatureChangeOutfit(creature);
            }
        }

        public void CreatureMove(Creature creature, Direction direction)
        {
            Location fromLocation = creature.Tile.Location;
            byte fromStackPosition = creature.Tile.GetStackPosition(creature);
            Location toLocation = creature.Tile.Location.Offset(direction);
            Tile toTile = Map.GetTile(toLocation);

            if (toTile != null && toTile.IsWalkable)
            {
                creature.Tile.Creatures.Remove(creature);
                toTile.Creatures.Add(creature);
                creature.Tile = toTile;

                if (fromLocation.Y > toLocation.Y)
                    creature.Direction = Direction.North;
                else if (fromLocation.Y < toLocation.Y)
                    creature.Direction = Direction.South;
                if (fromLocation.X < toLocation.X)
                    creature.Direction = Direction.East;
                else if (fromLocation.X > toLocation.X)
                    creature.Direction = Direction.West;

                foreach (var player in GetPlayers())
                {
                    if (player == creature)
                    {
                        player.Connection.SendPlayerMove(fromLocation, fromStackPosition, toLocation);
                    }
                    else if (player.Tile.Location.CanSee(fromLocation) && player.Tile.Location.CanSee(toLocation))
                    {
                        player.Connection.SendCreatureMove(fromLocation, fromStackPosition, toLocation);
                    }
                    else if (player.Tile.Location.CanSee(fromLocation))
                    {
                        player.Connection.SendTileRemoveThing(fromLocation, fromStackPosition);
                    }
                    else if (player.Tile.Location.CanSee(toLocation))
                    {
                        player.Connection.SendTileAddCreature(creature);
                    }
                }
            }
        }

        private int x = 97;
        public void ProcessLogin(Connection connection, string characterName)
        {
            Player player = GetPlayer(connection.AccountId, characterName);
            Location playerLocation = new Location(x++, 205, 7);
            player.Id = 0x01000000 + (uint)random.Next(0xFFFFFF);
            Tile tile = Map.GetTile(playerLocation);
            player.Tile = tile;
            tile.Creatures.Add(player);
            connection.Player = player;
            player.Connection = connection;

            PlayerLogin(player);
        }

        private void PlayerLogin(Player player)
        {
            AddCreature(player);

            player.Connection.SendInitialPacket();

            var spectators = GetSpectatorPlayers(player.Tile.Location);

            foreach (Player spectator in spectators)
            {
                if (spectator != player)
                {
                    spectator.Connection.SendCreatureAppear(player);
                }
            }
        }

        public void PlayerLogout(Player player)
        {
            // TODO: Make sure the player can logout
            player.Connection.Close();
            player.Tile.Creatures.Remove(player);
            RemoveCreature(player);

            var spectators = GetSpectatorPlayers(player.Tile.Location);

            foreach (Player spectator in spectators)
            {
                if (spectator != player)
                {
                    spectator.Connection.SendCreatureLogout(player);
                }
            }
        }

        public void CreatureSpeech(Creature creature, string message)
        {
            // TODO: this should only send to players who can see this player speak (same floor)
            foreach (Player spectator in GetSpectatorPlayers(creature.Tile.Location))
            {
                spectator.Connection.SendCreatureSpeech(creature, message);
            }
        }

        public bool CreateAccount(string name, string password)
        {
            using (SQLiteConnection conn = new SQLiteConnection(SharpOT.Properties.Settings.Default.ConnectionString))
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                conn.Open();
                cmd.CommandText = "insert into Account (Name, Password) values (@name, @password)";
                SQLiteParameter nameParam = new SQLiteParameter("name", name.ToLower());
                SQLiteParameter passwordParam = new SQLiteParameter("password", Util.Hash.SHA256Hash(password));
                cmd.Parameters.Add(nameParam);
                cmd.Parameters.Add(passwordParam);
                return (1 == cmd.ExecuteNonQuery());
            }
        }

        public bool CreatePlayer(long accountId, string name)
        {
            using (SQLiteConnection conn = new SQLiteConnection(SharpOT.Properties.Settings.Default.ConnectionString))
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                conn.Open();
                cmd.CommandText = "insert into Player (AccountId, Name) values (@accountId, @name)";
                SQLiteParameter accountIdParam = new SQLiteParameter("accountId", accountId);
                SQLiteParameter nameParam = new SQLiteParameter("name", name);
                cmd.Parameters.Add(accountIdParam);
                cmd.Parameters.Add(nameParam);
                return (1 == cmd.ExecuteNonQuery());
            }
        }

        public List<CharacterListItem> GetCharacterList(long accountId)
        {
            List<CharacterListItem> chars = new List<CharacterListItem>();

            var ipAddress = IPAddress.Parse(SharpOT.Properties.Settings.Default.Ip);
            var ipBytes = ipAddress.GetAddressBytes();

            using (SQLiteConnection conn = new SQLiteConnection(SharpOT.Properties.Settings.Default.ConnectionString))
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                conn.Open();
                cmd.CommandText = "select Name from Player where AccountId = @accountId";
                SQLiteParameter accountIdParam = new SQLiteParameter("accountId", accountId);
                cmd.Parameters.Add(accountIdParam);

                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    chars.Add(new CharacterListItem(
                        reader.GetString(0),
                        SharpOT.Properties.Settings.Default.WorldName,
                        ipBytes,
                        SharpOT.Properties.Settings.Default.Port
                    ));
                }
            }
            return chars;
        }

        public Player GetPlayer(long accountId, string name)
        {
            using (SQLiteConnection conn = new SQLiteConnection(SharpOT.Properties.Settings.Default.ConnectionString))
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                conn.Open();
                cmd.CommandText = @"select Level, MagicLevel, Experience, 
                                           MaxHealth, MaxMana, Capacity, 
                                           OutfitLookType, OutfitHead, OutfitBody,
                                           OutfitLegs, OutfitFeet, OutfitAddons
                                    from Player
                                    where AccountId = @accountId and Name = @name";
                SQLiteParameter accountIdParam = new SQLiteParameter("accountId", accountId);
                SQLiteParameter nameParam = new SQLiteParameter("name", name);
                cmd.Parameters.Add(accountIdParam);
                cmd.Parameters.Add(nameParam);

                SQLiteDataReader reader = cmd.ExecuteReader();
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

                    // TODO: Calculate player speed
                    player.Speed = 600;
                    return player;
                }
                return null;
            }
        }

        public long CheckAccount(Connection connection, string accountName, string password)
        {
            using (SQLiteConnection conn = new SQLiteConnection(SharpOT.Properties.Settings.Default.ConnectionString))
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                conn.Open();
                cmd.CommandText = @"select Id
                                    from Account
                                    where Name = @name
                                    and Password = @Password";
                SQLiteParameter nameParam = new SQLiteParameter("name", accountName.ToLower());
                SQLiteParameter passwordParam = new SQLiteParameter("password", Util.Hash.SHA256Hash(password));
                cmd.Parameters.Add(nameParam);
                cmd.Parameters.Add(passwordParam);

                var result = cmd.ExecuteScalar();
                if (result == null)
                {
                    connection.SendDisconnect("Account name or password incorrect.");
                    return -1;
                }

                return (long)result;
            }
        }

        #endregion
    }
}
