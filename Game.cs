using System;
using System.Collections.Generic;
using System.Linq;
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

        public void ItemUse(Player user, ushort spriteId, Location fromLocation, byte fromStackPosition, byte index)
        {
            if (fromLocation.GetItemLocationType() == ItemLocationType.Ground)
            {
                if (user != null && !user.Tile.Location.IsNextTo(fromLocation))
                {
                    // TODO: move the player
                    return;
                }

                Tile fromTile = Map.GetTile(fromLocation);

                // TODO: read teleport location from map for ladders

                if (fromTile.FloorChange != FloorChangeDirection.None)
                {
                    Location moveToLocation = FloorChangeLocationOffset(fromTile.FloorChange, fromLocation);
                    CreatureMove(user, moveToLocation, true);
                    return;
                }
            }

            Item item = GetItemAtLocation(user, fromLocation, fromStackPosition);

            if (item == null) return;

            if (item is Container)
            {
                Container container = (Container)item;
                int existingId = user.Inventory.GetContainerId(container);
                if (existingId >= 0)
                {
                    user.Inventory.CloseContainer((byte)existingId);
                    user.Connection.SendContainerClose((byte)existingId);
                }
                else
                {
                    byte containerId;
                    if (fromLocation.GetItemLocationType() == ItemLocationType.Ground)
                    {
                        containerId = user.Inventory.OpenContainer(container, fromLocation);
                    }
                    else
                    {
                        containerId = user.Inventory.OpenContainer(container);
                    }
                    user.Connection.SendContainerOpen(container, containerId);
                }
            }
        }

        private Item GetItemAtLocation(Player player, Location location, byte stackPosition)
        {
            switch (location.GetItemLocationType())
            {
                case ItemLocationType.Ground:
                    Thing thing = Map.GetTile(location).GetThingAtStackPosition(stackPosition);
                    return thing as Item;
                case ItemLocationType.Slot:
                    return player.Inventory.GetItemInSlot(location.GetSlot());
            }
            return null;
        }

        public void ItemMove(Player mover, ushort spriteId, Location fromLocation, byte fromStackPosition, Location toLocation, byte count)
        {
            Thing thing = null;

            if (fromLocation.GetItemLocationType() == ItemLocationType.Ground)
            {
                if (mover != null && !mover.Tile.Location.IsNextTo(fromLocation))
                {
                    // TODO: move the player to the item
                    return;
                }

                Tile fromTile = Map.GetTile(fromLocation);
                thing = fromTile.GetThingAtStackPosition(fromStackPosition);

                if (CheckMoveTo(mover, thing, fromLocation, toLocation) <= 0)
                    return;

                fromTile.RemoveItem((Item)thing);

                foreach (var spec in GetSpectatorPlayers(fromLocation))
                {
                    spec.Connection.SendTileRemoveThing(fromLocation, fromStackPosition);
                }
            }
            else if (fromLocation.GetItemLocationType() == ItemLocationType.Slot)
            {
                SlotType fromSlot = fromLocation.GetSlot();
                thing = mover.Inventory.GetItemInSlot(fromSlot);

                if (CheckMoveTo(mover, thing, fromLocation, toLocation) <= 0)
                    return;

                mover.Inventory.ClearSlot(fromSlot);
                mover.Connection.SendSlotUpdate(fromSlot);
            }
            else if (fromLocation.GetItemLocationType() == ItemLocationType.Container)
            {
                byte containerIndex = fromLocation.GetContainer();
                byte containerPos = fromLocation.GetContainerPosition();
                Container container = mover.Inventory.GetContainer(containerIndex).Container;
                if (container == null || container.ItemCount < containerPos + 1)
                    return;
                thing = container.GetItem(containerPos);
                if (CheckMoveTo(mover, thing, fromLocation, toLocation) <= 0)
                    return;
                container.RemoveItem(containerPos);
                mover.Connection.SendContainerRemoveItem(containerIndex, containerPos);
            }

            if (toLocation.GetItemLocationType() == ItemLocationType.Ground)
            {
                Tile toTile = Map.GetTile(toLocation);

                if (toTile.FloorChange != FloorChangeDirection.None)
                {
                    toLocation = FloorChangeLocationOffset(toTile.FloorChange, toLocation);
                    toTile = Map.GetTile(toLocation);
                }

                if (thing is Item)
                {
                    // TODO: move only count
                    // TODO: check line of throwing
                    Item item = (Item)thing;
                    toTile.AddItem(item);

                    foreach (var player in GetSpectatorPlayers(toLocation))
                    {
                        player.Connection.SendTileAddItem(toLocation, toTile.GetStackPosition(item), item);
                    }
                }
                else if (thing is Creature)
                {
                    if (!toLocation.IsNextTo(fromLocation))
                    {
                        // TODO: send a message, can't move here
                        return;
                    }

                    // TODO: walk delays
                    CreatureMove((Creature)thing, toLocation, false);
                }
            }
            else if (toLocation.GetItemLocationType() == ItemLocationType.Slot)
            {
                if (thing is Item)
                {
                    SlotType toSlot = toLocation.GetSlot();
                    Item currentItem = mover.Inventory.GetItemInSlot(toSlot);
                    if (currentItem == null)
                    {
                        mover.Inventory.SetItemInSlot(toSlot, (Item)thing);
                        mover.Connection.SendSlotUpdate(toSlot);
                    }
                }
            }
            else if (toLocation.GetItemLocationType() == ItemLocationType.Container)
            {
                Item item = (Item)thing;
                byte containerIndex = toLocation.GetContainer();
                byte containerPos = toLocation.GetContainerPosition();
                var container = mover.Inventory.GetContainer(containerIndex);
                if (container != null)
                {
                    container.Container.AddItem(item);
                    mover.Connection.SendContainerAddItem(containerIndex, item);
                }
            }
        }

        private int CheckMoveTo(Player mover, Thing thing, Location fromLocation, Location toLocation)
        {
            Item item = thing as Item;
            if (thing is Item)
            {
                if (((Item)thing).Info.IsMoveable == false)
                    return 0;
            }
            switch (toLocation.GetItemLocationType())
            {
                case ItemLocationType.Container:
                    if (item == null) return 0;
                    Container container = mover.Inventory.GetContainer(toLocation.GetContainer()).Container;
                    if (item == container) return 0;
                    return 1;
                case ItemLocationType.Slot:
                    if (item == null) return 0;
                    if (!CheckItemSlot(item, toLocation.GetSlot()))
                        return 0;
                    Item current = mover.Inventory.GetItemInSlot(toLocation.GetSlot());
                    if (current == null)
                    {
                        if (item.Info.IsStackable)
                            return item.Extra;
                        else
                            return 1;
                    }
                    else if (current.Info.IsStackable && current.Id == item.Id && current.Extra != 100)
                    {
                        return Math.Min(100 - current.Extra, item.Extra);
                    }
                    return 0;
                case ItemLocationType.Ground:
                    // TODO: check line of throwing
                    if (thing is Creature && !fromLocation.IsNextTo(toLocation))
                        return 0;
                    return 1;
                default:
                    return 0;
            }
        }

        private bool CheckItemSlot(Item item, SlotType slot)
        {
            switch (slot)
            {
                case SlotType.Ammo:
                case SlotType.Left:
                case SlotType.Right:
                    return true;
                default:
                    return item.Info.SlotType == slot;
            }
        }

        public void ContainerClose(Player player, byte containerIndex)
        {
            player.Inventory.CloseContainer(containerIndex);
            player.Connection.SendContainerClose(containerIndex);
        }

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
        public void CreatureWalk(Creature creature, Direction direction)
        {
            creature.LastStepTime = DateTime.Now.Ticks;
            if (direction > Direction.West) // moving diagonally
            {
                creature.LastStepCost = 2;
            }
            else
            {
                creature.LastStepCost = 1;
            }

            CreatureMove(creature, creature.Tile.Location.Offset(direction), false);
        }
            
        public void CreatureMove(Creature creature, Location toLocation, bool teleport)
        {
            Tile fromTile = creature.Tile;
            Location fromLocation = fromTile.Location;
            byte fromStackPosition = fromTile.GetStackPosition(creature);
            Tile toTile = Map.GetTile(toLocation);

            if (!teleport && toTile.FloorChange != FloorChangeDirection.None &&
                toTile.FloorChange != FloorChangeDirection.Up) // Ladders must be used
            {
                if (toTile != null && toTile.IsWalkable)
                {
                    // TODO: Clean up
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

                    toLocation = FloorChangeLocationOffset(toTile.FloorChange, toLocation); ;
                    toTile = Map.GetTile(toLocation);
                }
            }

            if (BeforeCreatureMove != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureMove.GetInvocationList())
                {
                    BeforeCreatureMoveHandler subscriber = (BeforeCreatureMoveHandler)del;
                    forward &= (bool)subscriber(creature, fromLocation, toLocation, fromStackPosition, toTile);
                }
                if (!forward) return;
            }

            // TODO: actually check if it is walkable
            if (toTile != null && toTile.IsWalkable)
            {
                Player mover = creature as Player;
                if (mover != null)
                {
                    mover.Connection.BeginTransaction();
                    foreach (byte i in mover.Inventory.GetContainersToClose(toLocation))
                    {
                        mover.Inventory.CloseContainer(i);
                        mover.Connection.SendContainerClose(i);
                    }
                }

                fromTile.Creatures.Remove(creature);
                toTile.Creatures.Add(creature);
                creature.Tile = toTile;

                if (!teleport)
                {
                    if (fromLocation.Y > toLocation.Y)
                        creature.Direction = Direction.North;
                    else if (fromLocation.Y < toLocation.Y)
                        creature.Direction = Direction.South;
                    if (fromLocation.X < toLocation.X)
                        creature.Direction = Direction.East;
                    else if (fromLocation.X > toLocation.X)
                        creature.Direction = Direction.West;
                }

                foreach (var player in GetPlayers())
                {
                    if (player == creature)
                    {
                        if (teleport)
                        {
                            player.Connection.SendPlayerTeleport(fromLocation, fromStackPosition, toLocation);
                        }
                        else
                        {
                            player.Connection.BeginTransaction();
                            player.Connection.SendPlayerMove(fromLocation, fromStackPosition, toLocation);
                            player.Connection.CommitTransaction();
                        }
                    }
                    else if (player.Tile.Location.CanSee(fromLocation) && player.Tile.Location.CanSee(toLocation))
                    {
                        if (teleport)
                        {
                            player.Connection.BeginTransaction();
                            player.Connection.SendTileRemoveThing(fromLocation, fromStackPosition);
                            player.Connection.SendTileAddCreature(creature);
                            player.Connection.CommitTransaction();
                        }
                        else
                        {
                            player.Connection.SendCreatureMove(fromLocation, fromStackPosition, toLocation);
                        }
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
                    AfterCreatureMove(creature, fromLocation, toLocation, fromStackPosition, toTile);
                
            }
        }

        private Location FloorChangeLocationOffset(FloorChangeDirection floorChange, Location oldToLocation)
        {
            Location newToLocation = new Location(oldToLocation);
            switch (floorChange)
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
                case FloorChangeDirection.Up:
                    newToLocation.Z -= 1;
                    newToLocation.Y += 1;
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
            return newToLocation;
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
                player.SavedLocation = Map.GetDefaultLocation();
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
            Thing thing = null;
            switch (location.GetItemLocationType())
            {
                case ItemLocationType.Ground:
                    if (player.Tile.Location.CanSee(location))
                    {
                        Tile tile = Map.GetTile(location);
                        thing = tile.GetThingAtStackPosition(stackPosition);
                    }
                    break;
                case ItemLocationType.Slot:
                    thing = player.Inventory.GetItemInSlot(location.GetSlot());
                    break;
            }
            if (thing != null)
                player.Connection.SendTextMessage(TextMessageType.DescriptionGreen, thing.GetLookAtString());
        }

        public long CheckLoginInfo(Connection connection, ILoginInfo info, bool isLoginProtocol)
        {
            long accountId = -1;
            string disconnectReason = "";

            if (info.Version != Properties.Settings.Default.ClientVersion)
            {
                disconnectReason = String.Format(
                    "You need client version {0} to connect to this server.",
                    Properties.Settings.Default.ClientVersion);
            }
            else
            {
                accountId = Database.GetAccountId(info.AccountName, info.Password);

                if (accountId < 0)
                {
                    disconnectReason = "Account name or password incorrect.";
                }
            }

            if (disconnectReason.Length > 0)
            {
                if (isLoginProtocol)
                    connection.SendDisconnectLogin(disconnectReason);
                else
                    connection.SendDisconnectGame(disconnectReason);
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
            tile.AddItem(item);
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

            player.LastLogin = DateTime.Now;

            Database.SavePlayerById(player);

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
            foreach (Player player in GetPlayers().Where(p => p.Tile.Location.IsInRange(creature.Tile.Location, 50, sameFloor)))
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
                if (spectator.Tile.Location.IsInRange(creature.Tile.Location, 1.42, true))
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
