using System;
using SharpOT.Scripting;

namespace SharpOT.Scripting
{
    public class Harp : IActionItem
    {

        public ushort GetItemId() { return 2084; }
        public bool Use(Game game, Player user, Location fromLocation, byte fromStackPosition, byte index, Item item)
        {
            Location location = fromLocation.Type == LocationType.Ground ? fromLocation : user.Tile.Location;
            foreach (var player in game.GetSpectatorPlayers(location))
            {
                player.Connection.SendEffect(location, Effect.RedNotes);
            }
            return true;
        }
        public void UseOnItem(Game game, Player user, Location fromLocation, byte fromStackPosition, Item item, Location toLocation, Item onItem) { }
        public void UseOnCreature(Game game, Player user, Location fromLocation, byte fromStackPosition, Item item, Creature creature) { }
    }
}
