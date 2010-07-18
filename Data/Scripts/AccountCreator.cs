using SharpOT;
using SharpOT.Scripting;
using System.Collections.Generic;
using System;

public class AccountCreator:IScript
{
    Game game;
    Dictionary<Connection, CreationInfo> creators = new Dictionary<Connection, CreationInfo>();
    static long AccountCreatorId = -1;
    const string ManagerNamePrefix = "Account Manager";
    const string AccountCreatorPlayerName = "Account Creator";
    const string AccountCreatorAccountName = "creator";
    const string AccountCreatorAccountPassword = "creator";

    public bool Start(Game game)
    {
        this.game = game;

        int id = -1;
        if (!Database.AccountNameExists(AccountCreatorAccountName))
        {
            id = Database.CreateAccount(AccountCreatorAccountName, AccountCreatorAccountPassword);
            if (id > 0 && !Database.PlayerNameExists(AccountCreatorPlayerName))
            {
                Database.CreatePlayer(id, AccountCreatorPlayerName, game.GenerateAvailableId());
            }
            AccountCreatorId = id;
        }
        else
        {
            AccountCreatorId = Database.GetAccountId(AccountCreatorAccountName, AccountCreatorAccountPassword);
        }

        game.BeforeCreatureSpeech += BeforeCreatureSpeech;
        game.BeforeCreatureMove += BeforeCreatureMove;
        game.BeforeThingMove += ThingMove;
        game.AfterLogin += AfterLogin;
        game.AfterLogout += AfterLogout;
        return true;
    }

    public bool ThingMove(Player mover, Thing thing, Location fromLocation, byte fromStackPosition, Location toLocation, byte count)
    {
        return !mover.Name.Contains(ManagerNamePrefix);
    }

    public void AfterLogin(Player player)
    {
        if (player.Id == AccountCreatorId)
        {
            player.Connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your account name to be?");
            creators.Add(player.Connection, new CreationInfo { State = DialogueState.AskName });
        }
    }

    public void AfterLogout(Player player)
    {
        if (creators.ContainsKey(player.Connection))
        {
            creators.Remove(player.Connection);
        }
    }

    public bool BeforeCreatureSpeech(Creature creature, Speech speech)
    {
        if (creature.IsPlayer && creature.Id == AccountCreatorId)
        {
            Player p = (Player)creature;

            //only the account creator should know what he said...
            p.Connection.SendCreatureSpeech(p, SpeechType.Whisper, speech.Message);
            Parse(p.Connection, creators[p.Connection].State, speech);

            //Account manager's messages can't be visible to everyone for security
            return false;
        }

        return true;

    }

    public bool BeforeCreatureMove(Creature creature, Location fromLocation, Location toLocation, byte fromStackPosition, Tile toTile)
    {
        if (creature.IsPlayer && creature.Id == AccountCreatorId)
        {
            ((Player)creature).Connection.SendCancelWalk();
            return false;
        }
        return true;
    }

    private void Parse(Connection connection,DialogueState state, Speech speech)
    {
        switch (state)
        {
                //maybe define better rules for account names and passwords
            case DialogueState.AskName:
                if (speech.Message.Length > 30)
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, "Account names must be no more than 30 characters.");
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your account name to be?");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(speech.Message, "^[a-zA-Z0-9]+$"))
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, "Only alphanumeric characters(a-z A-Z 0-9) can be used in account names.");
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your account name to be?");
                }
                else if (Database.AccountNameExists(speech.Message))
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, "The account name " + speech.Message + " is already being used.");
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your account name to be?");
                    creators[connection].State = DialogueState.AskName;
                }
                else
                {
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your password to be?");
                    creators[connection].AccName = speech.Message;
                    creators[connection].State = DialogueState.AskPassword;
                }
                break;
            case DialogueState.AskPassword:
                if (speech.Message.Length > 29)
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, "Passwords must be shorter or as long as 29 characters.");
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your password to be?");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(speech.Message, "^[a-zA-Z0-9]+$"))
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, "Only alphanumeric characters(a-z A-Z 0-9) can be used in passwords.");
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your password to be?");
                }
                else
                {
                    creators[connection].Password = speech.Message;
                    int id = Database.CreateAccount(creators[connection].AccName, creators[connection].Password);
                    if (id > 0)
                    {
                        uint pId = game.GenerateAvailableId();
                        if (-1 != Database.CreatePlayer(id, "Account Manager " + Convert.ToBase64String(BitConverter.GetBytes(pId)), pId))
                        {
                            connection.SendTextMessage(TextMessageType.ConsoleBlue, "Congratulations! Your account has been created successfully.");
                            connection.SendTextMessage(TextMessageType.ConsoleBlue, "Details:");
                            connection.SendTextMessage(TextMessageType.ConsoleBlue, "Account Name:   " + creators[connection].AccName);
                            connection.SendTextMessage(TextMessageType.ConsoleBlue, "Password:   " + creators[connection].Password);
                            connection.SendTextMessage(TextMessageType.ConsoleBlue, "You can now login using your account and manage it.");
                            return;
                        }
                        else
                        {
                            connection.SendTextMessage(TextMessageType.ConsoleRed, "An error has ocurred and your account could not be created. Sorry for the inconvenience.");
                            Database.DeleteAccount(id);
                            return;
                        }
                    }
                    else
                    {
                        connection.SendTextMessage(TextMessageType.ConsoleRed, "An error has ocurred and your account could not be created. Sorry for the inconvenience.");
                        return;
                    }
                }
                break;

        }
    }

    public bool Stop()
    {
        game.BeforeCreatureSpeech -= BeforeCreatureSpeech;
        game.BeforeCreatureMove -= BeforeCreatureMove;
        game.BeforeThingMove -= ThingMove;
        game.AfterLogin -= AfterLogin;
        game.AfterLogout -= AfterLogout;
        return true;
    }

    class CreationInfo
    {
        public string AccName { get; set; }
        public string Password { get; set; }
        public DialogueState State { get; set; }
    }

    enum DialogueState
    {
        AskName,
        AskPassword,
    }
}
