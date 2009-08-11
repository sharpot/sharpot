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
                    else if (player.Tile.Location.CanSee(fromLocation) && player.Tile.Location.CanSee(toLocation))
                    {
                        player.Connection.SendCreatureMove(fromLocation, toLocation);
                    }
                    else if (player.Tile.Location.CanSee(fromLocation))
                    {
                        // Remove tile item
                        NetworkMessage message = new NetworkMessage();
                        TileRemoveThingPacket.Add(message, fromLocation, 1);
                        player.Connection.Send(message);
                    }
                    else if (player.Tile.Location.CanSee(toLocation))
                    {
                        // Add tile creature
                        NetworkMessage message = new NetworkMessage();
                        uint remove;
                        bool known = player.Connection.IsCreatureKnown(creature.Id, out remove);
                        TileAddCreaturePacket.Add(message, toLocation, 1, creature, known, remove);
                        player.Connection.Send(message);
                    }
                }
            }
        }
    }
}
