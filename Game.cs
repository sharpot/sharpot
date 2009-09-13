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

        public delegate bool PrivateChannelOpenHandler(Player player, string receiver);
        public PrivateChannelOpenHandler BeforePrivateChannelOpen;

        public delegate bool CreatureMoveHandler(Creature creature,Direction direction,Location fromLocation,Location toLocation, byte fromStackPosition, Tile toTile);
        public CreatureMoveHandler BeforeCreatureMove;

        public delegate bool ChannelHandler(Player creature, ChatChannel channel);
        public ChannelHandler BeforeChannelOpen;
        public ChannelHandler BeforeChannelClose;

        public delegate bool CreatureUpdateHealthHandler(Creature creature, ushort health);
        public CreatureUpdateHealthHandler BeforeCreatureUpdateHealth;

        public delegate bool VipAddHandler(Player player, string vipName);
        public VipAddHandler BeforeVipAdd;

        public delegate bool VipRemoveHandler(Player player, uint vipId);
        public VipRemoveHandler BeforeVipRemove;

        public delegate bool BeforeLoginHandler(Connection connection, string playerName);
        public BeforeLoginHandler BeforeLogin;

        public delegate void AfterLoginHandler(Player player);
        public AfterLoginHandler AfterLogin;

        public delegate void AfterLogoutHandler(Player player);
        public AfterLogoutHandler AfterLogout;

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
            // TODO: What should we do about recursion? A script shouldn't
            // call this method when handling this method...
            if (BeforeCreatureTurn != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureTurn.GetInvocationList())
                {
                    CreatureTurnHandler subscriber = (CreatureTurnHandler)del;
                    forward &= (bool)subscriber(creature, direction);
                }
                if (!forward) return;
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
                bool forward = true;
                foreach (Delegate del in BeforePlayerChangeOutfit.GetInvocationList())
                {
                    PlayerChangeOutfitHandler subscriber = (PlayerChangeOutfitHandler)del;
                    forward &= (bool)subscriber(player, outfit);
                }
                if (!forward) return;
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
                bool forward = true;
                foreach (Delegate del in BeforePrivateChannelOpen.GetInvocationList())
                {
                    PrivateChannelOpenHandler subscriber = (PrivateChannelOpenHandler)del;
                    forward &= (bool)subscriber(player, receiver);
                }
                if (!forward) return;
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
                bool forward = true;
                foreach (Delegate del in BeforeCreatureSpeech.GetInvocationList())
                {
                    CreatureSpeechHandler subscriber=(CreatureSpeechHandler)del;
                    forward &= (bool)subscriber(creature, speech);
                }
                if (!forward) return;
            }

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

        public void ChannelOpen(Player player, ChatChannel channel)
        {
            if (BeforeChannelOpen != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeChannelOpen.GetInvocationList())
                {
                    ChannelHandler subscriber = (ChannelHandler)del;
                    forward &= (bool)subscriber(player, channel);
                }
                if (!forward) return;
            }

            Channel selected = player.ChannelList.FirstOrDefault(c => c.Id == (ushort)channel);
            if (selected != null)
            {
                if (!player.OpenedChannelList.Contains(selected))
                {
                    player.OpenedChannelList.Add(selected);
                }

                player.Connection.SendChannelOpen(selected);
            }
        }

        public void ChannelClose(Player player, ChatChannel channel)
        {
            if (BeforeChannelClose != null)
            {
                // Happens client side, can't stop it
                BeforeChannelClose(player, channel);
            }

            Channel selected = player.OpenedChannelList.FirstOrDefault(c => c.Id == (ushort)channel);
            if (selected != null)
            {
                player.OpenedChannelList.Remove(selected);
            }
        }     

        public void CreatureMove(Creature creature, Direction direction)
        {
            Location fromLocation = creature.Tile.Location;
            byte fromStackPosition = creature.Tile.GetStackPosition(creature);
            Location toLocation = creature.Tile.Location.Offset(direction);
            Tile toTile = Map.GetTile(toLocation);

            if (BeforeCreatureMove != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureMove.GetInvocationList())
                {
                    CreatureMoveHandler subscriber = (CreatureMoveHandler)del;
                    forward &= (bool)subscriber(creature, direction, fromLocation, toLocation, fromStackPosition, toTile);
                }
                if (!forward) return;
            }

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

        public void CreatureUpdateHealth(Creature creature, ushort health)
        {
            if (BeforeCreatureUpdateHealth != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureUpdateHealth.GetInvocationList())
                {
                    CreatureUpdateHealthHandler subscriber = (CreatureUpdateHealthHandler)del;
                    forward &= (bool)subscriber(creature, health);
                }
                if (!forward) return;
            }

            foreach (var player in GetSpectatorPlayers(creature.Tile.Location))
            {
                if (player == creature)
                {
                    //TODO: composite packet
                    player.Connection.SendStatus();
                }

                player.Connection.SendCreatureUpdateHealth(creature);
            }
        }


        public void VipAdd(Player player, string vipName)
        {
            if (BeforeVipAdd != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeVipAdd.GetInvocationList())
                {
                    VipAddHandler subscriber = (VipAddHandler)del;
                    forward &= (bool)subscriber(player, vipName);
                }
                if (!forward) return;
            }

            KeyValuePair<uint,string> selected = Database.GetPlayerIdNameDictionary()
                .FirstOrDefault(pair => pair.Value.ToLower() == vipName.ToLower());
                        
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

        public void VipRemove(Player player, uint vipId)
        {
            if (BeforeVipRemove != null)
            {
                // Happens client side, can't stop it
                BeforeVipRemove(player, vipId);
            }

            if (player.VipList.ContainsKey(vipId))
            {
                player.VipList.Remove(vipId);
            }
        }

        public void ProcessLogin(Connection connection, string characterName)
        {
            if (BeforeLogin != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeLogin.GetInvocationList())
                {
                    BeforeLoginHandler subscriber = (BeforeLoginHandler)del;
                    forward &= (bool)subscriber(connection, characterName);
                }
                if (!forward)
                {
                    connection.Close();
                    return;
                }
            }

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

        public void PlayerLogout(Player player)
        {
            // TODO: Make sure the player can logout
            player.Connection.Close();
            if (AfterLogout != null)
            {
                AfterLogout(player);
            }
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

        #endregion

        #region Private Helpers

        private void PlayerLogin(Player player)
        {
            AddCreature(player);

            player.Connection.SendInitialPacket();

            if (AfterLogin != null)
            {
                AfterLogin(player);
            }

            //TODO: composite packet for players that are spectators AND vips?
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

        private void CreatureChannelSpeech(string sender, SpeechType type, ChatChannel channelId, string message)
        {
            var channelPlayers = GetPlayers()
                .Where(player => player.OpenedChannelList.Any(channel => channel.Id == (ushort)channelId));

            foreach (var player in channelPlayers)
            {
                player.Connection.SendChannelSpeech(sender, type, channelId, message);
            }
        }

        private void CreatureSaySpeech(Creature creature, SpeechType speechType, string message)
        {
            foreach (Player spectator in GetSpectatorPlayers(creature.Tile.Location))
            {
                spectator.Connection.SendCreatureSpeech(creature, speechType, message);
            }
        }

        private void CreatureYellSpeech(Creature creature, SpeechType speechType, string message)
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

        private void CreatureWhisperSpeech(Creature creature, SpeechType speechType, string message)
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

        private void CreaturePrivateSpeech(Creature creature, string receiver, string message)
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

        #endregion
    }
}
