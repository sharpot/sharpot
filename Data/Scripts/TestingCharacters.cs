using System;
using System.Text;
using SharpOT;
using SharpOT.Scripting;

public class TestingCharacters : IScript
{
    Game game;

    public bool Start(Game game)
    {
        this.game = game;
        int id = -1;

        if (!Database.AccountNameExists("1"))
        {
            id = Database.CreateAccount("1", "1");
            if (id > 0 && !Database.PlayerNameExists("God"))
            {
                id = Database.CreatePlayer(id, "God", game.GenerateAvailableId());
                Player player = Database.GetPlayerById((uint)id);
            }
        }

        if (!Database.AccountNameExists("2"))
        {
            id = Database.CreateAccount("2", "2");
            if (id > 0 && !Database.PlayerNameExists("Bob"))
                Database.CreatePlayer(id, "Bob", game.GenerateAvailableId());
        }

        if (!Database.AccountNameExists("3") && !Database.PlayerNameExists("Alice"))
        {
            id = Database.CreateAccount("3", "3");
            if (id > 0 && !Database.PlayerNameExists("Alice"))
                Database.CreatePlayer(id, "Alice", game.GenerateAvailableId());
        }

        game.AfterLogin += AfterLogin;

        return true;
    }

    public void AfterLogin(Player player)
    {
        if (player.Name == "God")
        {
            player.Speed = 900;
            foreach (var spectator in game.GetSpectatorPlayers(player.Tile.Location))
            {
                spectator.Connection.BeginTransaction();
                spectator.Connection.SendCreatureChangeSpeed(player);
                spectator.Connection.SendEffect(player.Tile.Location, Effect.BlueShimmer);
                spectator.Connection.CommitTransaction();
            }
        }
    }

    public bool Stop()
    {
        game.AfterLogin -= AfterLogin;
        return true;
    }
}