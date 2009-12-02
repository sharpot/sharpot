using System;
using System.Linq;
using SharpOT.Scripting;

namespace SharpOT.Scripting
{
    public class SampleRune : IActionItem
    {
        public static Random random = new Random();
        public ushort GetItemId() { return 2268; }

        public void UseOnItem(Game game, Player user, Location fromLocation, byte fromStackPosition, Item item, Location toLocation, Item onItem)
        {
            if (toLocation.Type != LocationType.Ground) return;
            Location projectileStart = fromLocation.Type == LocationType.Ground ? fromLocation : user.Tile.Location;
            foreach (var player in game.GetSpectatorPlayers(fromLocation).Union(game.GetSpectatorPlayers(toLocation)))
            {
                player.Connection.SendProjectile(projectileStart, toLocation, ProjectileType.Fire);
            }
        }

        public bool Use(Game game, Player user, Location fromLocation, byte fromStackPosition, byte index, Item item) { return true; }

        public void UseOnCreature(Game game, Player user, Location fromLocation, byte fromStackPosition, Item item, Creature creature)
        {
            Location projectileStart = fromLocation.Type == LocationType.Ground ? fromLocation : user.Tile.Location;
            Location toLocation = creature.Tile.Location;
            ushort damage = (ushort)random.Next(creature.MaxHealth);
            creature.ApplyDamage(damage);

            Location loc;
            foreach (var player in game.GetSpectatorPlayers(fromLocation).Union(game.GetSpectatorPlayers(toLocation)))
            {
                loc = player.Tile.Location;
                bool canSeeFrom = loc.CanSee(projectileStart);
                bool canSeeTo = loc.CanSee(toLocation);

                if (canSeeFrom || canSeeTo)
                {
                    player.Connection.BeginTransaction();
                    player.Connection.SendProjectile(projectileStart, toLocation, ProjectileType.Fire);
                }
                if (canSeeTo)
                {
                    player.Connection.BeginTransaction();
                    player.Connection.SendAnimatedText("" + damage, TextColor.DarkRed);
                    player.Connection.SendCreatureHealth(creature.Id, creature.HealthPercent);
                }

                if (canSeeFrom || canSeeTo)
                    player.Connection.CommitTransaction();
            }
        }
    }
}
