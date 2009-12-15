using SharpOT;
using SharpOT.Scripting;
using System;

public class TeleportCommand : ICommand
{
    public string GetWords()
    {
        return "/t";
    }

    public bool CanBeUsedBy(Player player)
    {
        return true;
    }

    public bool Action(Game game, Player player, string args)
    {
        int amount;
        if (!int.TryParse(args, out amount))
            amount = 1;
        Location toLocation = player.Tile.Location.Offset(player.Direction, amount);
        player.Connection.BeginTransaction();
        player.Connection.SendEffect(player.Tile.Location, Effect.Teleport);
        game.CreatureMove(player, toLocation);
        player.Connection.SendEffect(toLocation, Effect.Teleport);
        return false;
    }
}

public class TestEffectCommand : ICommand
{
    public string GetWords()
    {
        return "/e";
    }

    public bool CanBeUsedBy(Player player)
    {
        return true;
    }

    public bool Action(Game game, Player player, string args)
    {
        try
        {
            object effect = Enum.Parse(typeof(Effect), args, true);
            if (effect != null)
            {
                player.Connection.SendEffect(player.Tile.Location.Offset(player.Direction), (Effect)effect);
            }
        }
        catch
        {
            player.Connection.SendTextMessage(TextMessageType.EventDefault, "The specified effect is invalid.");
        }
        return false;
    }
}

public class TestProjectileCommand : ICommand
{
    public string GetWords()
    {
        return "/p";
    }

    public bool CanBeUsedBy(Player player)
    {
        return true;
    }

    public bool Action(Game game, Player player, string args)
    {
        try
        {
            object projectile = Enum.Parse(typeof(ProjectileType), args, true);
            if (projectile != null)
            {
                player.Connection.SendProjectile(player.Tile.Location, player.Tile.Location.Offset(player.Direction, 3), (ProjectileType)projectile);
            }
        }
        catch
        {
            player.Connection.SendTextMessage(TextMessageType.EventDefault, "The specified projectile is invalid.");
        }
        return false;
    }
}