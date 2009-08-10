using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public void AddCreature(Creature creature)
        {
            creatures.Add(creature.Id, creature);
        }

        public void RemoveCreature(Creature creature)
        {
            creatures.Remove(creature.Id);
        }

        public IEnumerable<Creature> GetSpectators(Location location)
        {
            return creatures.Values.Where(creature => creature.Tile.Location.CanSee(location));
        }

        public IEnumerable<Player> GetSpectatorPlayers(Location location)
        {
            return GetSpectators(location).OfType<Player>();
        }

        public IEnumerable<Player> GetPlayers()
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

                foreach (var player in GetSpectatorPlayers(fromLocation).Union(GetSpectatorPlayers(toLocation)))
                {
                    if (player == creature)
                    {
                        player.Connection.SendPlayerMove(fromLocation, toLocation);
                    }
                    else
                    {
                        player.Connection.SendCreatureMove(fromLocation, toLocation);
                    }
                }
            }
        }
    }
}
