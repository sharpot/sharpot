using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpOT.Packets;
using System.Data.SQLite;
using System.Net;
using System.Data.Common;

namespace SharpOT
{
    public class Game
    {
        #region Variables
        
        
        private Dictionary<uint, Creature> creatures = new Dictionary<uint, Creature>();
        Random random = new Random();

        #endregion

        #region Properties

        public Map Map { get; private set; }
        public Scripter Scripter { get; private set; }

        #endregion

        #region Events

        public delegate bool CreatureSpeechHandler(Creature creature, Speech speech);
        public CreatureSpeechHandler BeforeCreatureSpeech;

        public delegate bool CreatureTurnHandler(Creature creature, Direction direction);
        public CreatureTurnHandler BeforeCreatureTurn;

        public delegate bool PlayerChangeOutfitHandler(Creature creature, Outfit outfit);
        public PlayerChangeOutfitHandler BeforePlayerChangeOutfit;

        public delegate bool PrivateChannelOpenHandler(Player creature, string receiver);
        public PrivateChannelOpenHandler BeforePrivateChannelOpen;

        #endregion

        #region Constructor

        public Game(Server server)
        {
            Map = new Map();
            Scripter = new Scripter();
        }

        #endregion

        #region Public Helpers

        public void AddCreature(Creature creature)
        {
            creatures.Add(creature.Id, creature);
        }

        public void RemoveCreature(Creature creature)
        {
            creatures.Remove(creature.Id);
        }

        public IEnumerable<Creature> GetSpectators(Location location)
        {
            return creatures.Values.Where(creature => creature.Tile.Location.CanSee(location));
        }

        public IEnumerable<Player> GetSpectatorPlayers(Location location)
        {
            return GetPlayers().Where(player => player.Tile.Location.CanSee(location));
        }

        public IEnumerable<Player> GetPlayers()
        {
            return creatures.Values.OfType<Player>();
        }

        #endregion

        #region Public Actions

        public void WalkCancel(Player player)
        {
            player.Connection.SendCancelWalk();
        }

        public void CreatureTurn(Creature creature, Direction direction)
        {
            // TODO: we need a better, more generic calling method.
            // The return value is of the last delegate executed.
            if (BeforeCreatureTurn != null)
            {
                if (!BeforeCreatureTurn(creature, direction))
                    return;
            }

            if (creature.Direction != direction)
            {
                creature.Direction = direction;
                foreach (var player in GetSpectatorPlayers(creature.Tile.Location))
                {
                    player.Connection.SendCreatureTurn(creature);
                }
            }
        }

        public void PlayerChangeOutfit(Player player, Outfit outfit)
        {
            if (BeforePlayerChangeOutfit != null)
            {
                if (!BeforePlayerChangeOutfit(player, outfit))
                    return;
            }

            player.Outfit = outfit;
            foreach (var spectator in GetSpectatorPlayers(player.Tile.Location))
            {
                spectator.Connection.SendCreatureChangeOutfit(player);
            }
            Database.SavePlayerByName(player);
        }

        public void PrivateChannelOpen(Player player, string receiver)
        {
            if (BeforePrivateChannelOpen != null)
            {
                if (!BeforePrivateChannelOpen(player, receiver))
                    return;
            }

            string selected = Database.GetPlayerIdNameDictionary().FirstOrDefault(pair => pair.Value.ToLower() == receiver.ToLower()).Value;
            if (selected != null)
            {
                player.Connection.SendChannelOpenPrivate(selected);
            }
            else
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "A player with this name does not exist.");
            }
        }

        public void CreatureSpeech(Creature creature, Speech speech)
        {
            if (BeforeCreatureSpeech != null)
            {
                if (!BeforeCreatureSpeech(creature, speech))
                    return;
            }

            if (creature.Name != "Account Manager")
            {
                switch (speech.Type)
                {
                    case SpeechType.Say:
                        CreatureSaySpeech(creature, speech.Type, speech.Message);
                        break;
                    case SpeechType.Whisper:
                        CreatureWhisperSpeech(creature, speech.Type, speech.Message);
                        break;
                    case SpeechType.Yell:
                        CreatureYellSpeech(creature, speech.Type, speech.Message);
                        break;
                    case SpeechType.Private:
                        CreaturePrivateSpeech(creature, speech.Receiver, speech.Message);
                        break;
                    case SpeechType.ChannelOrange:
                    case SpeechType.ChannelRed:
                    case SpeechType.ChannelWhite:
                    case SpeechType.ChannelYellow:
                        CreatureChannelSpeech(creature.Name, speech.Type, speech.ChannelId, speech.Message);
                        break;
                }
            }
            else
            {
                ((Player)creature).Connection.SendCreatureSpeech(creature, SpeechType.Whisper, speech.Message);
            }
        }

        public void CreaturePrivateSpeech(Creature creature, string receiver, string message)
        {
            Player selected = GetPlayers().FirstOrDefault(p => p.Name == receiver);
            if (selected != null)
            {
                selected.Connection.SendCreatureSpeech(creature, SpeechType.Private, message);
                if (creature.IsPlayer)
                    ((Player)creature).Connection.SendTextMessage(TextMessageType.StatusSmall, "Message sent to " + receiver + ".");
            }
            else
            {
                if (creature.IsPlayer)
                    ((Player)creature).Connection.SendTextMessage(TextMessageType.StatusSmall, "A player with this name is not online.");
            }
        }

        public void ChannelOpen(Player player, ChatChannel channel)
        {
            Channel selected = player.ChannelList.FirstOrDefault(c => c.Id == (ushort)channel);
            if (selected != null)
            {
                if (!player.OpenedChannelList.Contains(selected))
                {
                    player.OpenedChannelList.Add(selected);
                }
                else
                {
                    //shouldn't happen

                }
                player.Connection.SendChannelOpen(selected);
            }
            else
            {
                //shouldn't happen
            }
        }

        public void ChannelClose(Player player, ushort channelId)
        {
            Channel selected = player.OpenedChannelList.FirstOrDefault(c => c.Id == channelId);
            if (selected != null)
            {
                player.OpenedChannelList.Remove(selected);
            }
            else
            {
                //shouldn't happen
            }
        }

        public void CreatureChannelSpeech(string sender, SpeechType type, ChatChannel channelId, string message)
        {
            var channelPlayers = GetPlayers()
                .Where(player => player.OpenedChannelList.Any(channel => channel.Id == (ushort)channelId));

            foreach (var player in channelPlayers)
            {
                player.Connection.SendChannelSpeech(sender, type, channelId, message);
            }
        }

        public void CreatureSaySpeech(Creature creature, SpeechType speechType, string message)
        {
            foreach (Player spectator in GetSpectatorPlayers(creature.Tile.Location))
            {
                spectator.Connection.SendCreatureSpeech(creature, speechType, message);
            }
        }

        public void CreatureYellSpeech(Creature creature, SpeechType speechType, string message)
        {
            if (creature.IsPlayer)
            {
                Player player = (Player)creature;
                if (System.Environment.TickCount - player.LastYellTime <= 30000)
                {
                    player.Connection.SendTextMessage(TextMessageType.StatusSmall, "You are exhausted.");
                    return;
                }
                else player.LastYellTime = System.Environment.TickCount;
            }

            bool sameFloor = creature.Tile.Location.Z > 7;
            foreach (Player player in GetPlayers().Where(p => p.Tile.Location.IsInRange(creature.Tile.Location, sameFloor, 50)))
            {
                player.Connection.SendCreatureSpeech(creature, speechType, message.ToUpper());
            }
        }

        public void CreatureWhisperSpeech(Creature creature, SpeechType speechType, string message)
        {
            foreach (Player spectator in GetSpectatorPlayers(creature.Tile.Location))
            {
                if (spectator.Tile.Location.IsInRange(creature.Tile.Location, true, 1.42))
                {
                    spectator.Connection.SendCreatureSpeech(creature, speechType, message);
                }
                else
                {
                    spectator.Connection.SendCreatureSpeech(creature, speechType, "pspsps");
                }
            }
        }       

        public void CreatureMove(Creature creature, Direction direction)
        {
            Location fromLocation = creature.Tile.Location;
            byte fromStackPosition = creature.Tile.GetStackPosition(creature);
            Location toLocation = creature.Tile.Location.Offset(direction);
            Tile toTile = Map.GetTile(toLocation);

            if (toTile != null && toTile.IsWalkable)
            {
                creature.Tile.Creatures.Remove(creature);
                toTile.Creatures.Add(creature);
                creature.Tile = toTile;

                if (fromLocation.Y > toLocation.Y)
                    creature.Direction = Direction.North;
                else if (fromLocation.Y < toLocation.Y)
                    creature.Direction = Direction.South;
                if (fromLocation.X < toLocation.X)
                    creature.Direction = Direction.East;
                else if (fromLocation.X > toLocation.X)
                    creature.Direction = Direction.West;

                foreach (var player in GetPlayers())
                {
                    if (player == creature)
                    {
                        player.Connection.SendPlayerMove(fromLocation, fromStackPosition, toLocation);
                    }
                    else if (player.Tile.Location.CanSee(fromLocation) && player.Tile.Location.CanSee(toLocation))
                    {
                        player.Connection.SendCreatureMove(fromLocation, fromStackPosition, toLocation);
                    }
                    else if (player.Tile.Location.CanSee(fromLocation))
                    {
                        player.Connection.SendTileRemoveThing(fromLocation, fromStackPosition);
                    }
                    else if (player.Tile.Location.CanSee(toLocation))
                    {
                        player.Connection.SendTileAddCreature(creature);
                    }
                }
            }
        }

        public void CreatureUpdateHealth(Creature creature)
        {
            foreach (var player in GetSpectatorPlayers(creature.Tile.Location))
            {
                if (player == creature)//should be composite packet?
                {
                    player.Connection.SendStatus();
                }

                player.Connection.SendCreatureUpdateHealth(creature);
            }
        }


        public void VipAdd(Player player, string buddy)
        {
             KeyValuePair<uint,string> selected = Database.GetPlayerIdNameDictionary()
                 .FirstOrDefault(pair => pair.Value.ToLower() == buddy.ToLower());
                        
            if (player.VipList.Count >= 100)
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "You cannot add more buddies.");
            }
            else if (selected.Key != 0 && player.VipList.ContainsKey(selected.Key))
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "This player is already in your list.");
            }
            else if (selected.Key != 0)
            {
                bool state = GetPlayers().Any(p => p.Id == selected.Key);
                player.VipList.Add(selected.Key, new Vip
                {
                    Id = selected.Key,
                    Name = selected.Value,
                    LoggedIn = state
                });
                player.Connection.SendVipState(selected.Key, selected.Value, state);
            }
            else
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "A player with this name does not exit.");
            }

        }

        public void VipRemove(Player player, uint id)
        {
            if (player.VipList.ContainsKey(id))
            {
                player.VipList.Remove(id);
            }
            else
            {
                //shouldn't happen
            }
        }

        public void ProcessLogin(Connection connection, string characterName)
        {
            Player player = Database.GetPlayerByName(connection.AccountId, characterName);
            if (player.SavedLocation == null || Map.GetTile(player.SavedLocation) == null)
            {
                player.SavedLocation = new Location(97, 205, 7);
            }
            //player.Id = 0x01000000 + (uint)random.Next(0xFFFFFF);
            Tile tile = Map.GetTile(player.SavedLocation);
            player.Tile = tile;
            tile.Creatures.Add(player);
            connection.Player = player;
            player.Connection = connection;
            player.Game = this;

            PlayerLogin(player);
        }

        private void PlayerLogin(Player player)
        {
            AddCreature(player);

            player.Connection.SendInitialPacket();

            if (player.Name == "Account Manager")
            {
                player.Connection.SendTextMessage(TextMessageType.ConsoleBlue, "Say anything to start a dialogue.");
            }

            //should be composite packet for players that are spectators AND vips?
            var spectators = GetSpectatorPlayers(player.Tile.Location).Where(s => s != player);
            foreach (var spectator in spectators)
            {
                spectator.Connection.SendCreatureAppear(player);
            }


            foreach (Player p in GetPlayers().Where(b => b.VipList.ContainsKey(player.Id)))
            {
                p.VipList[player.Id].LoggedIn = true;
                p.Connection.SendVipLogin(player.Id);
            }
                        
        }

        public void PlayerLogout(Player player)
        {
            // TODO: Make sure the player can logout
            player.Connection.Close();

            //should be composite packet for players that are spectators AND vips?
            var spectators = GetSpectatorPlayers(player.Tile.Location).Where(s => s != player);
            foreach (var spectator in spectators)
            {
                spectator.Connection.SendCreatureLogout(player);
            }

            player.Tile.Creatures.Remove(player);
            RemoveCreature(player);

            //maybe player object should have a list of other players that have added it to their vips
            foreach (Player p in GetPlayers().Where(b => b.VipList.ContainsKey(player.Id)))
            {
                p.VipList[player.Id].LoggedIn = false;
                p.Connection.SendVipLogout(player.Id);
            }


            Database.SavePlayerById(player);
        }

        public long CheckAccount(Connection connection, string accountName, string password)
        {
            long accountId = Database.GetAccountId(accountName, password);
            
            if (accountId < 0)
            {
                connection.SendDisconnect("Account name or password incorrect.");
            }

            return accountId;
        }

        public uint GenerateAvailableId()
        {
            var dictionary = Database.GetPlayerIdNameDictionary();
            uint baseId = 0x40000001;
            if (dictionary.Count() == 0)
            {
                return baseId;
            }
            for (uint i = 1; i < 0xFFFFFFFF; i++)
            {
                baseId |= i;
                if (!dictionary.ContainsKey(baseId))
                {
                    return baseId;
                }
            }
            throw new Exception("No available player ids.");
        }

        public bool IsIdAvailable(uint id)
        {
            return Database.GetPlayerIdNamePair(id).Key == 0;
        }

        public bool IsCharacterNameAvailable(string name)
        {
            //make a sql query that ignores case when searching if possible
            return !Database.GetPlayerIdNameDictionary().Any(pair => pair.Value.ToLower() == name.ToLower());
        }

        public bool IsAccountNameAvailable(string accName)
        {
            return !Database.CheckAccountName(accName);
        }

        #endregion
    }
}
