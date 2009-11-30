using System;
using System.Linq;
using SharpOT.Scripting;

namespace SharpOT.Scripting
{
    public class SampleRune : IActionItem
    {

        public ushort GetItemId() { return 2268; }
        public void UseOnItem(Game game, Player user, Location fromLocation, byte fromStackPosition, Item item, Location toLocation, Item onItem)
        {
            if (toLocation.Type != LocationType.Ground) return;
            Location projectileStart = fromLocation.Type == LocationType.Ground ? fromLocation : user.Tile.Location;
            foreach (var player in game.GetSpectatorPlayers(fromLocation).Union(game.GetSpectatorPlayers(toLocation)))
            {
                player.Connection.SendProjectile(projectileStart, toLocation, ProjectileType.Death);
            }
        }

        public bool Use(Game game, Player user, Location fromLocation, byte fromStackPosition, byte index, Item item) { return true; }
        public void UseOnCreature(Game game, Player user, Location fromLocation, byte fromStackPosition, Item item, Creature creature) { }
    }
}
