using SharpOT;
using SharpOT.Scripting;

public class OnlineCommand : ICommand
{
    public string GetWords()
    {
        return "/online";
    }

    public bool CanBeUsedBy(Player player)
    {
        return true;
    }

    public bool Action(Game game, Player player, string args)
    {
        string online = "";
        foreach (Player p in game.GetPlayers())
        {
            if (online.Length > 0)
            {
                online += ", ";
            }
            online += p.Name;
        }
        player.Connection.SendTextMessage(TextMessageType.EventDefault, "Online: " + online);
        return false;
    }
}