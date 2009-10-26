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
        Location toLocation = player.Tile.Location.Offset(player.Direction);
        game.CreatureMove(player, toLocation, true);
        return false;
    }
}