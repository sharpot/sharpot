using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpOT.Packets;

namespace SharpOT
{
    public class Game
    {
        public Map Map { get; private set; }
        public Dictionary<uint, Creature> creatures = new Dictionary<uint, Creature>();

        public Game()
        {
            Map = new Map();
        }

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
            return GetSpectators(location).OfType<Player>();
        }

        private IEnumerable<Player> GetPlayers()
        {
            return creatures.Values.OfType<Player>();
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

                // TODO: Calculate new direction

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
    }
}
