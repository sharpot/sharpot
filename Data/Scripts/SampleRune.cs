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
            int damage = random.Next(creature.MaxHealth);
            if (damage % 10 == 0) damage *= -1;
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
                    if (damage > 0)
                        player.Connection.SendProjectile(projectileStart, toLocation, ProjectileType.Fire);
                    else
                        player.Connection.SendEffect(toLocation, Effect.BlueShimmer);
                }
                if (canSeeTo)
                {
                    player.Connection.BeginTransaction();
                    if (damage > 0)
                        player.Connection.SendAnimatedText(toLocation, "-" + damage, TextColor.DarkRed);
                    else
                        player.Connection.SendAnimatedText(toLocation, "+" + -1 * damage, TextColor.Blue);
                    player.Connection.SendCreatureHealth(creature.Id, creature.HealthPercent);
                }

                if (canSeeFrom || canSeeTo)
                    player.Connection.CommitTransaction();
            }

            if (creature.IsDead)
            {
                game.CreatureDie(creature);
            }
        }
    }
}
