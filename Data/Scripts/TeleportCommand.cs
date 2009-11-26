using SharpOT;
using SharpOT.Scripting;

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
        int effect;
        if (!int.TryParse(args, out effect))
            effect = 1;
        player.Connection.SendEffect(player.Tile.Location.Offset(player.Direction), (Effect)effect);
        return false;
    }
}