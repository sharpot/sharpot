using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpOT.Packets;
using System.Data.SQLite;
using System.Net;
using System.Data.Common;
using SharpOT.Scripting;

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

        #endregion

        #region Events

        public BeforeCreatureSpeechHandler BeforeCreatureSpeech;

        public AfterCreatureDefaultSpeechHandler AfterCreatureWhisperSpeech;
        public AfterCreatureDefaultSpeechHandler AfterCreatureSaySpeech;
        public AfterCreatureDefaultSpeechHandler AfterCreatureYellSpeech;

        public AfterCreaturePrivateSpeechHandler AfterCreaturePrivateSpeech;

        public AfterCreatureChannelSpeechHandler AfterCreatureChannelSpeech;

        public BeforeCreatureTurnHandler BeforeCreatureTurn;

        public AfterCreatureTurnHandler AfterCreatureTurn;

        public BeforePlayerChangeOutfitHandler BeforePlayerChangeOutfit;

        public AfterPlayerChangeOutfitHandler AfterPlayerChangeOutfit;

        public BeforePrivateChannelOpenHandler BeforePrivateChannelOpen;

        public AfterPrivateChannelOpenHandler AfterPrivateChannelOpen;

        public BeforeCreatureMoveHandler BeforeCreatureMove;

        public AfterCreatureMoveHandler AfterCreatureMove;

        public BeforeChannelHandler BeforeChannelOpen;
        public BeforeChannelHandler BeforeChannelClose;

        public AfterChannelOpenHandler AfterChannelOpen;

        public BeforeCreatureUpdateHealthHandler BeforeCreatureUpdateHealth;

        public AfterCreatureUpdateHealthHandler AfterCreatureUpdateHealth;

        public BeforeVipAddHandler BeforeVipAdd;

        public AfterVipAddHandler AfterVipAdd;

        public VipRemoveHandler BeforeVipRemove;

        public BeforeLoginHandler BeforeLogin;

        public AfterLoginHandler AfterLogin;

        public BeforeLogoutHandler BeforeLogout;

        public AfterLogoutHandler AfterLogout;

        public BeforeCreatureTurnHandler BeforeWalkCancel;

        public AfterWalkCancelHandler AfterWalkCancel;

        #endregion

        #region Constructor

        public Game()
        {
            Map = new Map();
        }

        #endregion

        #region Private Helpers

        private void AddCreature(Creature creature)
        {
            creatures.Add(creature.Id, creature);
        }

        private void RemoveCreature(Creature creature)
        {
            creatures.Remove(creature.Id);
        }

        #endregion

        #region Public Helpers

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
            if (BeforeWalkCancel != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeWalkCancel.GetInvocationList())
                {
                    BeforeWalkCancelHandler subscriber = (BeforeWalkCancelHandler)del;
                    forward &= (bool)subscriber();
                }
                if (!forward) return;
            }
            player.Connection.SendCancelWalk();
            if (AfterWalkCancel != null)
                AfterWalkCancel();
        }

        public void CreatureTurn(Creature creature, Direction direction)
        {
            if (BeforeCreatureTurn != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureTurn.GetInvocationList())
                {
                    BeforeCreatureTurnHandler subscriber = (BeforeCreatureTurnHandler)del;
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
            if(AfterCreatureTurn!=null)
                AfterCreatureTurn(creature, direction);
        }

        public void PlayerChangeOutfit(Player player, Outfit outfit)
        {
            if (BeforePlayerChangeOutfit != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforePlayerChangeOutfit.GetInvocationList())
                {
                    BeforePlayerChangeOutfitHandler subscriber = (BeforePlayerChangeOutfitHandler)del;
                    forward &= (bool)subscriber(player, outfit);
                }
                if (!forward) return;
            }

            player.Outfit = outfit;
            foreach (var spectator in GetSpectatorPlayers(player.Tile.Location))
            {
                spectator.Connection.SendCreatureChangeOutfit(player);
            }
            if(AfterPlayerChangeOutfit!=null)
                AfterPlayerChangeOutfit(player, outfit);
            Database.SavePlayerById(player);
        }

        public void PrivateChannelOpen(Player player, string receiver)
        {
            if (BeforePrivateChannelOpen != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforePrivateChannelOpen.GetInvocationList())
                {
                    BeforePrivateChannelOpenHandler subscriber = (BeforePrivateChannelOpenHandler)del;
                    forward &= (bool)subscriber(player, receiver);
                }
                if (!forward) return;
            }

            string selected = Database.GetPlayerIdNameDictionary().FirstOrDefault(pair => pair.Value.ToLower() == receiver.ToLower()).Value;
            if (selected != null)
            {
                player.Connection.SendChannelOpenPrivate(selected);
                if (AfterPrivateChannelOpen != null)
                    AfterPrivateChannelOpen(player, selected);
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
                    BeforeCreatureSpeechHandler subscriber = (BeforeCreatureSpeechHandler)del;
                    forward &= (bool)subscriber(creature, speech);
                }
                if (!forward) return;
            }

            if (creature.IsPlayer)
            {
                bool doPropagate =
                    Commands.ExecuteCommand(this, (Player)creature, speech.Message);
                if (!doPropagate) return;
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
                    BeforeChannelHandler subscriber = (BeforeChannelHandler)del;
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

            if(AfterChannelOpen!=null)
                AfterChannelOpen(player, channel);
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
            Tile fromTile = creature.Tile;
            Location fromLocation = fromTile.Location;
            byte fromStackPosition = fromTile.GetStackPosition(creature);
            Location toLocation = fromTile.Location.Offset(direction);
            Tile toTile = Map.GetTile(toLocation);

            if (toTile.FloorChange != FloorChangeDirection.None)
            {
                if (toTile != null && toTile.IsWalkable)
                {
                    //We have to move the creature to the tile of the floor change item
                    //THEN we offset that location and move him from there up a floor.
                    //Sorta messy but I'm feeling lazy ATM, at least its working
                    if (creature.IsPlayer)
                    {
                        ((Player)creature).Connection.BeginTransaction();
                        ((Player)creature).Connection.SendPlayerMove(fromLocation, fromStackPosition, toLocation);
                    }

                    fromTile.Creatures.Remove(creature);
                    toTile.Creatures.Add(creature);
                    creature.Tile = toTile;
                    fromTile = toTile;
                    fromLocation = toLocation;
                    fromStackPosition = toTile.GetStackPosition(creature);

                    Location newToLocation = new Location(toTile.Location);
                    switch (toTile.FloorChange)
                    {
                        case FloorChangeDirection.North:
                            newToLocation.Z -= 1;
                            newToLocation.Y -= 1;
                            break;
                        case FloorChangeDirection.South:
                            newToLocation.Z -= 1;
                            newToLocation.Y += 1;
                            break;
                        case FloorChangeDirection.East:
                            newToLocation.Z -= 1;
                            newToLocation.X += 1;
                            break;
                        case FloorChangeDirection.West:
                            newToLocation.Z -= 1;
                            newToLocation.X -= 1;
                            break;
                        case FloorChangeDirection.Down:
                            newToLocation.Z += 1;
                            switch (Map.GetTile(newToLocation).FloorChange)
                            {
                                case FloorChangeDirection.North:
                                    newToLocation.Y += 1;
                                    break;
                                case FloorChangeDirection.South:
                                    newToLocation.Y -= 1;
                                    break;
                                case FloorChangeDirection.East:
                                    newToLocation.X -= 1;
                                    break;
                                case FloorChangeDirection.West:
                                    newToLocation.X += 1;
                                    break;
                            }
                            break;
                    }

                    toLocation = newToLocation;
                    toTile = Map.GetTile(newToLocation);
                }
            }

            if (BeforeCreatureMove != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureMove.GetInvocationList())
                {
                    BeforeCreatureMoveHandler subscriber = (BeforeCreatureMoveHandler)del;
                    forward &= (bool)subscriber(creature, direction, fromLocation, toLocation, fromStackPosition, toTile);
                }
                if (!forward) return;
            }

            if (toTile != null && toTile.IsWalkable)
            {
                fromTile.Creatures.Remove(creature);
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
                        player.Connection.BeginTransaction();
                        player.Connection.SendPlayerMove(fromLocation, fromStackPosition, toLocation);
                        player.Connection.CommitTransaction();
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

                if(AfterCreatureMove != null)
                    AfterCreatureMove(creature, direction, fromLocation, toLocation, fromStackPosition, toTile);
                
            }
        }

        public void CreatureUpdateHealth(Creature creature, ushort health)
        {
            if (BeforeCreatureUpdateHealth != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureUpdateHealth.GetInvocationList())
                {
                    BeforeCreatureUpdateHealthHandler subscriber = (BeforeCreatureUpdateHealthHandler)del;
                    forward &= (bool)subscriber(creature, health);
                }
                if (!forward) return;
            }

            foreach (var player in GetSpectatorPlayers(creature.Tile.Location))
            {
                if (player == creature)
                {
                    player.Connection.BeginTransaction();
                    player.Connection.SendStatus();
                }

                player.Connection.BeginTransaction();
                player.Connection.SendCreatureUpdateHealth(creature);
                player.Connection.CommitTransaction();

            }
            if (AfterCreatureUpdateHealth != null)
                AfterCreatureUpdateHealth(creature, health);
        }

        public void VipAdd(Player player, string vipName)
        {
            if (BeforeVipAdd != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeVipAdd.GetInvocationList())
                {
                    BeforeVipAddHandler subscriber = (BeforeVipAddHandler)del;
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
                if (AfterVipAdd != null)
                    AfterVipAdd(player, vipName);
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

            if (creatures.ContainsKey(player.Id))
            {
                connection.SendDisconnectGame("You are already logged in.");
                return;
            }
            
            if (player.SavedLocation == null || Map.GetTile(player.SavedLocation) == null)
            {
                player.SavedLocation = new Location(97, 205, 7);
            }
            Tile tile = Map.GetTile(player.SavedLocation);
            player.Tile = tile;
            connection.Player = player;
            player.Connection = connection;
            player.Game = this;

            PlayerLogin(player);
        }

        public void PlayerLogout(Player player)
        {
            if (BeforeLogout != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeLogout.GetInvocationList())
                {
                    BeforeLogoutHandler subscriber = (BeforeLogoutHandler)del;
                    forward &= (bool)subscriber(player);
                }
                if (!forward) return;
            }
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

        public void PlayerLookAt(Player player, ushort id, Location location, byte stackPosition)
        {
            if (player.Tile.Location.CanSee(location))
            {
                Tile tile = Map.GetTile(location);
                Thing thing = tile.GetThingAtStackPosition(stackPosition);
                player.Connection.SendTextMessage(TextMessageType.DescriptionGreen, thing.GetLookAtString());
            }
        }

        public long CheckAccount(Connection connection, string accountName, string password)
        {
            long accountId = Database.GetAccountId(accountName, password);
            
            if (accountId < 0)
            {
                connection.SendDisconnectLogin("Account name or password incorrect.");
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

        public void TileAddItem(Location location, Item item)
        {
            Tile tile = Map.GetTile(location);
            tile.Items.Add(item);
            byte stackPosition = tile.GetStackPosition(item);

            foreach (Player player in GetSpectatorPlayers(location))
            {
                player.Connection.SendTileAddItem(
                    location,
                    stackPosition,
                    item
                );
            }
        }

        #endregion

        #region Private Helpers

        private void PlayerLogin(Player player)
        {
            player.Tile.Creatures.Add(player);
            AddCreature(player);

            player.Connection.SendInitialPacket();

            if (AfterLogin != null)
            {
                AfterLogin(player);
            }

            foreach (Player p in GetPlayers())
            {
                if (p != player)
                {
                    p.Connection.BeginTransaction();

                    if (p.Tile.Location.CanSee(player.Tile.Location))
                    {
                        p.Connection.SendCreatureAppear(player);
                    }

                    if (p.VipList.ContainsKey(player.Id))
                    {
                        p.VipList[player.Id].LoggedIn = true;
                        p.Connection.SendVipLogin(player.Id);
                    }

                    p.Connection.CommitTransaction();
                }
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

            if (AfterCreatureChannelSpeech != null)
                AfterCreatureChannelSpeech(sender, type, channelId, message);
        }

        private void CreatureSaySpeech(Creature creature, SpeechType speechType, string message)
        {
            foreach (Player spectator in GetSpectatorPlayers(creature.Tile.Location))
            {
                spectator.Connection.SendCreatureSpeech(creature, speechType, message);
            }

            if (AfterCreatureSaySpeech != null)
                AfterCreatureSaySpeech(creature, speechType, message);
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

            if (AfterCreatureYellSpeech != null)
                AfterCreatureYellSpeech(creature, speechType, message);
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

            if (AfterCreatureWhisperSpeech != null)
                AfterCreatureWhisperSpeech(creature, speechType, message);
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
            if (AfterCreaturePrivateSpeech != null)
                AfterCreaturePrivateSpeech(creature, receiver, message);
        }

        #endregion
    }
}
