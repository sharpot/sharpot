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
            Location toLocation = creature.Tile.Location.Offset(direction);
            Tile toTile = Map.GetTile(toLocation);

            if (toTile.IsWalkable())
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
                        player.Connection.SendPlayerMove(fromLocation, toLocation);
                    }
                    else if (player.Tile.Location.CanSee(fromLocation) && player.Tile.Location.CanSee(toLocation))
                    {
                        player.Connection.SendCreatureMove(fromLocation, toLocation);
                    }
                    else if (player.Tile.Location.CanSee(fromLocation))
                    {
                        player.Connection.SendTileRemoveThing(fromLocation, 1);
                    }
                    else if (player.Tile.Location.CanSee(toLocation))
                    {
                        player.Connection.SendTileAddCreature(creature, toLocation, 1);
                    }
                }
            }
        }

        public void PlayerLogin(Player player)
        {
            AddCreature(player);

            var spectators = GetSpectatorPlayers(player.Tile.Location);

            foreach (Player spectator in spectators)
            {
                if (spectator != player)
                {
                    spectator.Connection.SendTileAddCreature(player, player.Tile.Location, 1);
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
                    // TODO: Send a poof
                    spectator.Connection.SendTileRemoveThing(player.Tile.Location, 1);
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
