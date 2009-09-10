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
        public Server Server { get; private set; }

        #endregion

        #region Constructor

        public Game(Server server)
        {
            Map = new Map();
            Scripter = new Scripter();
            Server = server;
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

        private IEnumerable<Creature> GetSpectators(Location location)
        {
            return creatures.Values.Where(creature => creature.Tile.Location.CanSee(location));
        }

        private IEnumerable<Player> GetSpectatorPlayers(Location location)
        {
            return GetPlayers().Where(player => player.Tile.Location.CanSee(location));
        }

        private IEnumerable<Player> GetPlayers()
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
            player.Outfit = outfit;
            foreach (var spectator in GetSpectatorPlayers(player.Tile.Location))
            {
                spectator.Connection.SendCreatureChangeOutfit(player);
            }
            Database.SavePlayerByName(player);
        }

        public void PrivateChannelOpen(Player player, string receiver)
        {
            string selected = Database.GetAllPlayers().FirstOrDefault(p => p.Name.ToLower() == receiver.ToLower()).Name;
            if (selected != null)
            {
                player.Connection.SendChannelOpenPrivate(selected);
            }
            else
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "A player with this name does not exist.");
            }
        }

        public void CreaturePrivateSpeech(Player player, string receiver, string message)
        {
            Player selected = GetPlayers().FirstOrDefault(p => p.Name == receiver);
            if (selected != null)
            {
                selected.Connection.SendCreaturePrivateSpeech(player, SpeechType.Private, message);
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "Message sent to " + receiver + ".");
            }
            else
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "A player with this name is not online.");
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
            // TODO: Add exhaustion for yelling, and checks to make sure the player has the
            // permission to use the selected speech type
            // TODO: this should only send to players who can see this player speak (same floor)
            if (Scripter.RaiseEvent(EventType.OnPlayerSay, new EventProperties(0, 0, 0, message), (Player)creature, new object[] { message }))
            {
                foreach (Player spectator in GetSpectatorPlayers(creature.Tile.Location))
                {
                    spectator.Connection.SendCreatureDefaultSpeech(creature, speechType, message);
                }
            }
        }

        public void CreatureYellSpeech(Creature creature, SpeechType speechType, string message)
        {
            //TODO: Make the IsPlayer function work so we only check this on players (For the chance an NPC might be yelling, ect)
            Player P = (Player)creature;
            if (System.Environment.TickCount - P.YellTime <= 30000)
            {
                P.Connection.SendTextMessage(TextMessageType.StatusSmall, "You are exhausted");
                return;
            }
            else
            {
                P.YellTime = System.Environment.TickCount;
            }

            bool sameFloor = creature.Tile.Location.Z > 7;
            foreach (Player player in GetPlayers().Where(p => p.Tile.Location.IsInRange(creature.Tile.Location, sameFloor, 50)))
            {
                player.Connection.SendCreatureDefaultSpeech(creature, speechType, message.ToUpper());
            }
        }

        public void CreatureWhisperSpeech(Creature creature, SpeechType speechType, string message)
        {
            foreach (Player spectator in GetSpectatorPlayers(creature.Tile.Location))
            {
                if (spectator.Tile.Location.IsInRange(creature.Tile.Location, true, 1.42))
                {
                    spectator.Connection.SendCreatureDefaultSpeech(creature, speechType, message);
                }
                else
                {
                    spectator.Connection.SendCreatureDefaultSpeech(creature, speechType, "pspsps");
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
                if (player == creature)
                {
                    player.Connection.SendStatus();
                }

                player.Connection.SendCreatureUpdateHealth(creature);
            }
        }


        public void VipAdd(Player player, string buddy)
        {
            Player selected = Database.GetAllPlayers().FirstOrDefault(p => p.Name.ToLower() == buddy.ToLower());
            
            if (player.VipList.Count >= 100)
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "You cannot add more buddies.");
            }
            else if (selected != null && player.VipList.ContainsKey(selected.Id))
            {
                player.Connection.SendTextMessage(TextMessageType.StatusSmall, "This player is already in your list.");
            }
            else if (selected != null)
            {
                bool state = GetPlayers().Any(p => p.Id == selected.Id);
                player.VipList.Add(selected.Id, new Vip
                {
                    Id = selected.Id,
                    Name = selected.Name,
                    LoggedIn = state
                });
                player.Connection.SendVipState(selected.Id, selected.Name, state);
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
            player.game = this;

            PlayerLogin(player);
        }

        private void PlayerLogin(Player player)
        {
            AddCreature(player);

            player.Connection.SendInitialPacket();


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
            var spectators = GetSpectatorPlayers(player.Tile.Location).Where(s => s != player);
            foreach (var spectator in spectators)
            {
                spectator.Connection.SendCreatureLogout(player);
            }

            player.Tile.Creatures.Remove(player);
            RemoveCreature(player);


            foreach (Player p in GetPlayers().Where(b => b.VipList.ContainsKey(player.Id)))
            {
                p.VipList[player.Id].LoggedIn = false;
                p.Connection.SendVipLogout(player.Id);
            }


            Database.SavePlayerByName(player);
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
            IEnumerable<Player> players = Database.GetAllPlayers();
            uint baseId = 0x01000000;
            if (players.Count() == 0)
            {
                return baseId;
            }
            for (uint i = 1; i <= 0xFFFFFF; i++)
            {
                baseId |= i;
                if (!players.Any(p => p.Id == baseId))
                {
                    return baseId;
                }
            }
            throw new Exception("No available player ids.");
        }

        public bool IsIdAvailable(uint id)
        {
            return Database.GetAllPlayers().Any(p => p.Id == id);
        }

        #endregion
    }
}
