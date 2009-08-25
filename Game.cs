using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpOT.Packets;

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
        public void ProcessLogin(Connection connection, LoginPacket loginPacket)
        {
            Player player;
            Location playerLocation = new Location(x++, 205, 7);
            player = new Player();
            player.Id = 0x01000000 + (uint)random.Next(0xFFFFFF);
            player.Name = loginPacket.CharacterName;
            player.Health = 100;
            player.MaxHealth = 100;
            player.Speed = 600;
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

        #endregion
    }
}
