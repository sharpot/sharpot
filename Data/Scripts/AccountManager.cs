using SharpOT;
using SharpOT.Scripting;
using System.Collections.Generic;
using System;

public class AccountManager : IScript
{
    Game game;
    Dictionary<Connection, ManagementInfo> managers = new Dictionary<Connection, ManagementInfo>();
    string vocationsText;
    static string ManagerNamePrefix = "Account Manager";
    static string HelpPrompt = "Would you like to {create}/{delete} a character or change your {password}?";
    static string CantUnderstand = "I can't understand what you mean.";
    public bool Start(Game game)
    {
        this.game = game;
        vocationsText = "What would you like it to be? The available vocations are: ";
        string[] vocations = Enum.GetNames(typeof(Vocation));
        for (int i = 0; i < vocations.Length; i++)
        {
            vocationsText += "{" + vocations[i] + "}";
            if (i == vocations.Length - 1) vocationsText += ".";
            else vocationsText += ", ";
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
        if (player.Name.Contains(ManagerNamePrefix))
        {
            player.Connection.SendTextMessage(TextMessageType.ConsoleBlue, HelpPrompt);
            managers.Add(player.Connection, new ManagementInfo { State = DialogueState.AskTask });
        }
    }

    public void AfterLogout(Player player)
    {
        if (managers.ContainsKey(player.Connection)) managers.Remove(player.Connection);
    }

    public bool BeforeCreatureSpeech(Creature creature, Speech speech)
    {
        if (creature.IsPlayer && creature.Name.Contains(ManagerNamePrefix))
        {
            Player p = (Player)creature;

            //only the account manager should know what he said...
            p.Connection.SendCreatureSpeech(p, SpeechType.Whisper, speech.Message);
            Parse(p.Connection, managers[p.Connection].State, speech);

            //Account manager's messages can't be visible to everyone for security
            return false;
        }

        return true;

    }

    public bool BeforeCreatureMove(Creature creature, Location fromLocation, Location toLocation, byte fromStackPosition, Tile toTile)
    {
        if (creature.IsPlayer && creature.Name.Contains(ManagerNamePrefix))
        {
            ((Player)creature).Connection.SendCancelWalk();
            return false;
        }
        return true;
    }

    private void Parse(Connection connection, DialogueState state, Speech speech)
    {
        switch (state)
        {
            #region AskTask
            case DialogueState.AskTask:
                switch (speech.Message.ToLower())
                {
                    case "create":
                        connection.SendTextMessage(TextMessageType.ConsoleBlue, "Would you like it to be {male} or {femal}'?");
                        managers[connection].State = DialogueState.AskGender;
                        break;
                    case "delete":
                        managers[connection].CharactersList =new List<CharacterListItem>(Database.GetCharacterList(connection.AccountId));
                        if (managers[connection].CharactersList.Count > 1)
                        {
                            connection.SendTextMessage(TextMessageType.ConsoleBlue, "Your account contains the following characters:");
                            for (int i = 1; i < managers[connection].CharactersList.Count; i++)
                            {
                                connection.SendTextMessage(TextMessageType.ConsoleBlue, i + ". " + managers[connection].CharactersList[i].Name);
                            }
                            connection.SendTextMessage(TextMessageType.ConsoleBlue, "What character would you like to delete (by index)?");
                            managers[connection].State = DialogueState.AskIndex;
                        }
                        else
                        {
                            connection.SendTextMessage(TextMessageType.ConsoleRed, "Your account doesn't contain any characters yet.");
                            connection.SendTextMessage(TextMessageType.ConsoleBlue, HelpPrompt);
                            managers[connection].State = DialogueState.AskTask;
                        }
                        break;
                    case "password":
                        connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your new password to be?");
                        managers[connection].State = DialogueState.AskPassword;
                        break;
                    default:
                        connection.SendTextMessage(TextMessageType.ConsoleRed, CantUnderstand);
                        connection.SendTextMessage(TextMessageType.ConsoleBlue, HelpPrompt);
                        break;
                }
                break;
            #endregion
            #region AskPassword
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
                else if (Database.UpdateAccountPassword(connection.AccountId, speech.Message))
                {
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "Your account password has been changed with success.");
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, HelpPrompt);
                    managers[connection].State = DialogueState.AskTask;
                }
                else
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, "An error has occurred and your password could not be changed. Sorry for the inconvenience.");
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, HelpPrompt);
                    managers[connection].State = DialogueState.AskTask;
                }
                break;
            #endregion
            #region AskGender
            case DialogueState.AskGender:
                switch (speech.Message.ToLower())
                {
                    case "male":
                        managers[connection].CharacterGender = Gender.Male;
                        break;
                    case "female":
                        managers[connection].CharacterGender = Gender.Female;
                        break;
                    default:
                        connection.SendTextMessage(TextMessageType.ConsoleRed, CantUnderstand);
                        connection.SendTextMessage(TextMessageType.ConsoleBlue, "Would you like it to be {male} or {female}?");
                        return;
                }
                connection.SendTextMessage(TextMessageType.ConsoleBlue, vocationsText);
                managers[connection].State = DialogueState.AskVocation;
                break;
            #endregion
            #region AskVocation
            case DialogueState.AskVocation:
                try
                {
                    managers[connection].CharacterVocation = (Vocation)Enum.Parse(typeof(Vocation), speech.Message, true);
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your character's name to be?");
                    managers[connection].State = DialogueState.AskName;
                }
                catch
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, CantUnderstand);
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, vocationsText);
                }
                break;
            #endregion
            #region AskName
            case DialogueState.AskName:
                if (speech.Message.Length > 29)
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, "Character names must be shorter or as long as 29 characters.");
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your character name to be?");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(speech.Message, "^[a-zA-Z-' ]+$"))
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, "Only letters(a-z A-Z), hyphens(-) and apostrophes(') can be used in character names.");
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your character name to be?");
                }
                else if (Database.PlayerNameExists(speech.Message))
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, "A character with that name already exists.");
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "What would you like your character's name to be?");
                }
                else
                {
                    managers[connection].CharacterName = speech.Message;
                    managers[connection].State = DialogueState.AskGender;
                    Player p = new Player();
                    p.Connection = connection;
                    p.Name = managers[connection].CharacterName;
                    p.Gender = managers[connection].CharacterGender;
                    p.Vocation = managers[connection].CharacterVocation;
                    p.Id=game.GenerateAvailableId();
                    p.Tile = new Tile();
                    p.Tile.Location = new Location(97, 205, 7);//"temple position"
                    if (p.Gender == Gender.Male)
                        p.Outfit = new Outfit(128, 0);
                    else
                        p.Outfit = new Outfit(136, 0);
                    Database.CreatePlayer(connection.AccountId, p.Name, p.Id);
                    Database.SavePlayerInfo(p);
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "Your character has been created sucessfully.You can now relog to access it.");

                    connection.SendTextMessage(TextMessageType.ConsoleBlue, HelpPrompt);
                    managers[connection].State = DialogueState.AskTask;
                }
                break;
            #endregion
            #region AskIndex
            case DialogueState.AskIndex:
                ushort us;
                if(ushort.TryParse(speech.Message,out us))
                {
                    if (us < managers[connection].CharactersList.Count && us != 0)
                    {
                        managers[connection].DeleteSelected = managers[connection].CharactersList[(int)us].Name;
                        connection.SendTextMessage(TextMessageType.ConsoleOrange, "Are you sure you want to delete the character " + managers[connection].DeleteSelected + " from your account('yes' or 'no')?This decision is irreversible.");
                        managers[connection].State = DialogueState.AskConfirmation;
                    }
                    else
                    {
                        connection.SendTextMessage(TextMessageType.ConsoleRed, "Index out of range(1-" + (managers[connection].CharactersList.Count - 1) + ").");
                        connection.SendTextMessage(TextMessageType.ConsoleBlue, "Which character would you like to delete (by index)?");
                    }
                }
                else
                {
                    connection.SendTextMessage(TextMessageType.ConsoleRed, CantUnderstand);
                    connection.SendTextMessage(TextMessageType.ConsoleBlue, "Which character would you like to delete (by index)?");
                }

                break;
            #endregion
            #region AskConfirmation
            case DialogueState.AskConfirmation:
                switch (speech.Message.ToLower())
                {
                    case "yes":
                        if (Database.DeletePlayerByName(managers[connection].DeleteSelected))
                        {
                            connection.SendTextMessage(TextMessageType.ConsoleBlue, "Your character has been deleted sucessfully.");
                        }
                        else
                        {
                            connection.SendTextMessage(TextMessageType.ConsoleRed, "An error has occurred and your character could not be deleted.Sorry for the inconvenience.");
                        }
                        connection.SendTextMessage(TextMessageType.ConsoleBlue, HelpPrompt);
                        managers[connection].State = DialogueState.AskTask;
                        break;
                    case "no":
                        connection.SendTextMessage(TextMessageType.ConsoleBlue, "Character deletion canceled.");
                        connection.SendTextMessage(TextMessageType.ConsoleBlue, HelpPrompt);
                        managers[connection].State = DialogueState.AskTask;
                        break;
                    default:
                        connection.SendTextMessage(TextMessageType.ConsoleRed, CantUnderstand);
                        connection.SendTextMessage(TextMessageType.ConsoleOrange, "Are you sure you want to delete the character " + managers[connection].DeleteSelected + " from your account ({yes} or {no})? This decision is irreversible.");
                        break;
                }
                break;
            #endregion
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

    class ManagementInfo
    {
        public string CharacterName { get; set; }
        public Gender CharacterGender { get; set; }
        public Vocation CharacterVocation { get; set; }
        public List<CharacterListItem> CharactersList { get; set; }
        public string DeleteSelected { get; set; }
        public DialogueState State { get; set; }
    }

    enum DialogueState
    {
        AskTask,
        AskPassword,
        AskName,
        AskGender,
        AskVocation,
        AskIndex,
        AskConfirmation
    }
}
