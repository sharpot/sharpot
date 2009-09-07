using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpOT.Packets;
using System.Data.SQLite;
using System.Net;
using System.Data.Common;

namespace SharpOT
{
    public class Game
    {
        #region Variables
        
        private Dictionary<uint, Creature> creatures = new Dictionary<uint, Creature>();
        Random random = new Random();

        #endregion

        #region Properties

        public Map Map { get; private set; }
        public Scripter Scripter { get; private set; }

        #endregion

        #region Constructor

        public Game()
        {
            Map = new Map();
            Scripter = new Scripter();
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

        public void PlayerChangeOutfit(Player player, Outfit outfit)
        {
            player.Outfit = outfit;
            foreach (var spectator in GetSpectatorPlayers(player.Tile.Location))
            {
                spectator.Connection.SendCreatureChangeOutfit(player);
            }
            Database.SavePlayer(player);
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

        public void ProcessLogin(Connection connection, string characterName)
        {
            Player player = Database.GetPlayer(connection.AccountId, characterName);
            if (player.SavedLocation == null || Map.GetTile(player.SavedLocation) == null)
            {
                player.SavedLocation = new Location(97, 205, 7);
            }
            player.Id = 0x01000000 + (uint)random.Next(0xFFFFFF);
            Tile tile = Map.GetTile(player.SavedLocation);
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

            Database.SavePlayer(player);
        }

        public void CreatureSpeech(Creature creature, SpeechType speechType, string message)
        {
            // TODO: Add exhaustion for yelling, and checks to make sure the player has the
            // permission to use the selected speech type
            // TODO: this should only send to players who can see this player speak (same floor)
            if (Scripter.RaiseEvent(EventType.OnPlayerSay, new EventProperties(0, 0, 0, message), new object[] { message }))
            {
                foreach (Player spectator in GetSpectatorPlayers(creature.Tile.Location))
                {
                    spectator.Connection.SendCreatureSpeech(creature, speechType, message);
                }
            }
        }        

        public long CheckAccount(Connection connection, string accountName, string password)
        {
            long accountId = Database.GetAccountId(accountName, password);
            
            if (accountId < 0)
            {
                connection.SendDisconnect("Account name or password incorrect.");
            }

            return accountId;
        }

        #endregion
    }
}
