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
        game.CreatureMove(player, toLocation, true);
        return false;
    }
}