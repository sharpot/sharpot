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

        public BeforeCreatureWalkHandler BeforeCreatureWalk;
        public AfterCreatureWalkHandler AfterCreatureWalk;

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

        public BeforeThingMoveHandler BeforeThingMove;
        public AfterThingMoveHandler AfterThingMove;

        public BeforeItemUseHandler BeforeItemUse;
        public AfterItemUseHandler AfterItemUse;

        public BeforeItemUseOnHandler BeforeItemUseOn;
        public AfterItemUseOnHandler AfterItemUseOn;

        public BeforeItemUseOnCreatureHandler BeforeItemUseOnCreature;
        public AfterItemUseOnCreatureHandler AfterItemUseOnCreature;

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

        public void ItemUseOn(Player user, ushort fromSpriteId, Location fromLocation, byte fromStackPosition, ushort toSpriteId, Location toLocation, byte toStackPosition)
        {
            if (fromLocation.Type == LocationType.Ground)
            {
                if (user != null && !user.Tile.Location.IsNextTo(fromLocation))
                {
                    // TODO: move the player
                    return;
                }
            }

            Item item = GetThingAtLocation(user, fromLocation, fromStackPosition) as Item;

            if (item == null) return;

            if (BeforeItemUseOn != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeItemUseOn.GetInvocationList())
                {
                    BeforeItemUseOnHandler subscriber = (BeforeItemUseOnHandler)del;
                    forward &= (bool)subscriber(user, item, fromLocation, fromStackPosition, toLocation, toStackPosition);
                }
                if (!forward) return;
            }

            Thing thing = GetThingAtLocation(user, toLocation, toStackPosition);

            if (thing is Item)
            {
                ActionItems.ExecuteUseOnItem(this, user, fromLocation, fromStackPosition, item, toLocation, (Item)thing);
            }

            if (AfterItemUseOn != null)
                AfterItemUseOn(user, item, fromLocation, fromStackPosition, toLocation, toStackPosition);
        }

        public void ItemUseOnCreature(Player user, ushort spriteId, Location fromLocation, byte fromStackPosition, uint creatureId)
        {
            if (!creatures.ContainsKey(creatureId))
                return;

            if (fromLocation.Type == LocationType.Ground)
            {
                if (user != null && !user.Tile.Location.IsNextTo(fromLocation))
                {
                    // TODO: move the player
                    return;
                }
            }

            Item item = GetThingAtLocation(user, fromLocation, fromStackPosition) as Item;

            if (item == null)
                return;

            Creature creature = creatures[creatureId];

            if (BeforeItemUseOnCreature != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeItemUseOnCreature.GetInvocationList())
                {
                    BeforeItemUseOnCreatureHandler subscriber = (BeforeItemUseOnCreatureHandler)del;
                    forward &= (bool)subscriber(user, item, fromLocation, fromStackPosition, creature);
                }
                if (!forward) return;
            }

            ActionItems.ExecuteUseOnCreature(this, user, fromLocation, fromStackPosition, item, creature);

            if (AfterItemUseOnCreature != null)
                AfterItemUseOnCreature(user, item, fromLocation, fromStackPosition, creature);
        }

        public void ItemUse(Player user, ushort spriteId, Location fromLocation, byte fromStackPosition, byte index)
        {
            if (fromLocation.Type == LocationType.Ground)
            {
                if (user != null && !user.Tile.Location.IsNextTo(fromLocation))
                {
                    // TODO: move the player
                    return;
                }

                Tile fromTile = Map.GetTile(fromLocation);

                if (fromTile.FloorChange != FloorChangeDirection.None)
                {
                    Location moveToLocation = FloorChangeLocationOffset(fromTile.FloorChange, fromLocation);
                    CreatureMove(user, moveToLocation);
                    return;
                }
            }

            Item item = GetThingAtLocation(user, fromLocation, fromStackPosition) as Item;

            if (item == null) return;

            if (BeforeItemUse != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeItemUse.GetInvocationList())
                {
                    BeforeItemUseHandler subscriber = (BeforeItemUseHandler)del;
                    forward &= (bool)subscriber(user, item, fromLocation, fromStackPosition, index);
                }
                if (!forward) return;
            }

            if (ActionItems.ExecuteUse(this, user, fromLocation, fromStackPosition, index, item))
            {
                DefaultItemUse(user, fromLocation, fromStackPosition, index, item);
            }

            if (AfterItemUse != null)
                AfterItemUse(user, item, fromLocation, fromStackPosition, index);
        }

        private static void DefaultItemUse(Player user, Location fromLocation, byte fromStackPosition, byte index, Item item)
        {
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
                    switch (fromLocation.Type)
                    {
                        case LocationType.Ground:
                            containerId = user.Inventory.OpenContainer(container, fromLocation);
                            break;
                        case LocationType.Container:
                            container.Parent = user.Inventory.GetContainer(fromLocation.Container);
                            user.Inventory.OpenContainerAt(container, index);
                            break;
                        case LocationType.Slot:
                        default:
                            user.Inventory.OpenContainerAt(container, index);
                            break;
                    }
                    user.Connection.SendContainerOpen(container, index);
                }
            }
        }

        public void ThingMove(Player mover, ushort spriteId, Location fromLocation, byte fromStackPosition, Location toLocation, byte count)
        {
            if (fromLocation.Type == LocationType.Ground)
            {
                if (mover != null && !mover.Tile.Location.IsNextTo(fromLocation))
                {
                    // TODO: move the player to the item
                    return;
                }
            }

            Thing thing = GetThingAtLocation(mover, fromLocation, fromStackPosition);

            if (thing == null ||
                CheckMoveTo(mover, thing, fromLocation, toLocation) <= 0)
            {
                return;
            }

            if (BeforeThingMove != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeThingMove.GetInvocationList())
                {
                    BeforeThingMoveHandler subscriber = (BeforeThingMoveHandler)del;
                    forward &= (bool)subscriber(mover, thing, fromLocation, fromStackPosition, toLocation, count);
                }
                if (!forward) return;
            }

            Item item = thing as Item;

            if (item != null)
            {
                RemoveItem(mover, fromLocation, fromStackPosition);
            }

            if (fromLocation.Type == LocationType.Container)
            {
                if (toLocation.Type == LocationType.Container &&
                    fromLocation.Container == toLocation.Container &&
                    toLocation.ContainerPosition > fromLocation.ContainerPosition)
                {
                    --toLocation.ContainerPosition;
                }
            }

            if (toLocation.Type == LocationType.Ground)
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
                    AddItemToTile(item, toTile);
                }
                else if (thing is Creature)
                {
                    if (!toLocation.IsNextTo(fromLocation))
                    {
                        // TODO: send a message, can't move here
                        return;
                    }

                    // TODO: walk delays
                    CreatureMove((Creature)thing, toLocation);
                }
            }
            else if (toLocation.Type == LocationType.Slot)
            {
                if (item != null)
                    MoveItemToSlot(mover, item, toLocation.Slot);
            }
            else if (toLocation.Type == LocationType.Container)
            {
                if (item != null)
                    MoveItemToContainer(mover, item, toLocation.Container, toLocation.ContainerPosition);
            }
        }

        public void AddItemToTile(Item item, Tile toTile)
        {
            toTile.AddItem(item);

            foreach (var player in GetSpectatorPlayers(toTile.Location))
            {
                player.Connection.SendTileAddItem(toTile.Location, toTile.GetStackPosition(item), item);
            }
        }

        private static void MoveItemToContainer(Player mover, Item item, byte containerIndex, byte containerPos)
        {
            Container container = mover.Inventory.GetContainer(containerIndex);
            if (container != null)
            {
                Item itemAtPos = container.GetItem(containerPos);
                if (itemAtPos is Container)
                {
                    container = itemAtPos as Container;
                    container.AddItem(item);
                    int id = mover.Inventory.GetContainerId(container);
                    if (id >= 0)
                    {
                        mover.Connection.SendContainerAddItem((byte)id, item);
                    }
                }
                else if (itemAtPos != null && itemAtPos.Info.IsStackable && itemAtPos.Id == item.Id && itemAtPos.Extra != 100)
                {
                    // TODO: Stack item
                }
                else if (!container.IsFull())
                {
                    container.AddItem(item);
                    mover.Connection.SendContainerAddItem(containerIndex, item);
                }
            }
        }

        private static void MoveItemToSlot(Player mover, Item item, SlotType toSlot)
        {
            Item currentItem = mover.Inventory.GetItemInSlot(toSlot);
            if (currentItem == null)
            {
                mover.Inventory.SetItemInSlot(toSlot, item);
                mover.Connection.SendSlotUpdate(toSlot);
            }
            else if (currentItem is Container)
            {
                Container container = currentItem as Container;
                container.AddItem(item);
                int id = mover.Inventory.GetContainerId(container);
                if (id >= 0)
                {
                    mover.Connection.SendContainerAddItem((byte)id, item);
                }
            }
        }

        private int CheckMoveTo(Player mover, Thing thing, Location fromLocation, Location toLocation)
        {
            // TODO: error messages
            Container container;
            Item item = thing as Item;
            if (thing is Item)
            {
                if (((Item)thing).Info.IsMoveable == false)
                    return 0;
            }
            switch (toLocation.Type)
            {
                case LocationType.Container:
                    if (item == null) return 0;
                    container = mover.Inventory.GetContainer(toLocation.Container);
                    if (container == null) return 0;
                    if (item as Container != null)
                    {
                        Container parent = container;
                        while (parent != null)
                        {
                            if (item == parent) return 0;
                            parent = parent.Parent;
                        }
                    }
                    return 1;
                case LocationType.Slot:
                    if (item == null) return 0;
                    
                    Item current = mover.Inventory.GetItemInSlot(toLocation.Slot);
                    if (current == null)
                    {
                        if (!CheckItemSlot(item, toLocation.Slot))
                            return 0;
                        return item.Count;
                    }
                    else if (current is Container)
                    {
                        container = current as Container;
                        if (!container.IsFull())
                        {
                            return item.Count;
                        }
                    }
                    else if (current.Info.IsStackable && current.Id == item.Id && current.Extra != 100)
                    {
                        return Math.Min(100 - current.Extra, item.Extra);
                    }
                    return 0;
                case LocationType.Ground:
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

        public void ContainerOpenParent(Player player, byte containerIndex)
        {
            Container container = player.Inventory.GetContainer(containerIndex);
            if (container != null && container.Parent != null)
            {
                player.Inventory.OpenContainerAt(container.Parent, containerIndex);
                player.Connection.SendContainerOpen(container.Parent, containerIndex);
            }
        }

        public void ContainerClose(Player player, byte containerIndex)
        {
            if (player.Inventory.CloseContainer(containerIndex))
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

            Database.SavePlayerInfo(player);

            if(AfterPlayerChangeOutfit != null)
                AfterPlayerChangeOutfit(player, outfit);
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
            if (BeforeCreatureWalk != null)
            {
                bool forward = true;
                foreach (Delegate del in BeforeCreatureMove.GetInvocationList())
                {
                    BeforeCreatureWalkHandler subscriber = (BeforeCreatureWalkHandler)del;
                    forward &= (bool)subscriber(creature, direction);
                }
                if (!forward) return;
            }

            creature.LastStepTime = DateTime.Now.Ticks;
            if (direction > Direction.West) // moving diagonally
            {
                creature.LastStepCost = 2;
            }
            else
            {
                creature.LastStepCost = 1;
            }

            CreatureMove(creature, creature.Tile.Location.Offset(direction));



            if (AfterCreatureWalk != null)
                AfterCreatureWalk(creature, direction);
        }

        public void CreatureMove(Creature creature, Location toLocation)
        {
            Tile fromTile = creature.Tile;
            Location fromLocation = fromTile.Location;
            byte fromStackPosition = fromTile.GetStackPosition(creature);
            Tile toTile = Map.GetTile(toLocation);
            bool teleport = !fromLocation.IsNextTo(toLocation);
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

            if (!teleport && toTile.FloorChange != FloorChangeDirection.None &&
                toTile.FloorChange != FloorChangeDirection.Up) // Ladders must be used
            {
                if (toTile != null && toTile.IsWalkable)
                {
                    // We have to move the creature to the tile of the floor change item
                    // THEN we offset that location and move him from there up a floor.
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

            // TODO: actually check if it is walkable
            if (toTile != null && (teleport || toTile.IsWalkable))
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
                            player.Connection.BeginTransaction();
                            player.Connection.SendPlayerTeleport(fromLocation, fromStackPosition, toLocation);
                            player.Connection.CommitTransaction();
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

                if (creature.IsPlayer)
                {
                    SetPlayerFlags((Player)creature, toTile);
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
                player.SavedLocation = Map.DefaultLocation;
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
            if (player == null || player.IsDead) return;

            // TODO: Make sure the player can logout

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

            var spectators = GetSpectatorPlayers(player.Tile.Location).Where(s => s != player);
            foreach (var spectator in spectators)
            {
                spectator.Connection.BeginTransaction();
                spectator.Connection.SendEffect(player.Tile.Location, Effect.Puff);
                spectator.Connection.SendCreatureRemove(player);
                spectator.Connection.CommitTransaction();
            }

            player.Connection.Close();

            player.Tile.Creatures.Remove(player);
            RemoveCreature(player);

            foreach (Player p in GetPlayers().Where(b => b.VipList.ContainsKey(player.Id)))
            {
                p.VipList[player.Id].LoggedIn = false;
                p.Connection.SendVipLogout(player.Id);
            }

            Database.SavePlayer(player);

            if (AfterLogout != null)
            {
                AfterLogout(player);
            }
        }

        public void CreatureDie(Creature creature)
        {
            if (creature == null) return;

            AddItemToTile(creature.GetCorpse(), creature.Tile);

            var spectators = GetSpectatorPlayers(creature.Tile.Location);
            foreach (var spectator in spectators)
            {
                spectator.Connection.BeginTransaction();
                spectator.Connection.SendCreatureRemove(creature);
                spectator.Connection.CommitTransaction();
            }

            creature.Tile.Creatures.Remove(creature);
            RemoveCreature(creature);

            Player player = creature as Player;
            if (player != null)
            {
                player.Connection.SendDeath();

                foreach (Player p in GetPlayers().Where(b => b.VipList.ContainsKey(player.Id)))
                {
                    p.VipList[player.Id].LoggedIn = false;
                    p.Connection.SendVipLogout(player.Id);
                }
                player.Tile = Map.GetTile(Map.DefaultLocation);
                Database.SavePlayer(player);
            }
        }

        public void PlayerLookAt(Player player, ushort id, Location location, byte stackPosition)
        {
            if (location.Type != LocationType.Ground || player.Tile.Location.CanSee(location))
            {
                Thing thing = GetThingAtLocation(player, location, stackPosition);
                if (thing != null)
                    player.Connection.SendTextMessage(TextMessageType.DescriptionGreen, String.Format(
                        "{0} [Id:{1}, Location:{2}]", thing.GetLookAtString(), thing.GetThingId(), location.ToString()));
            }
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
            if (!dictionary.Any())
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

        private Thing GetThingAtLocation(Player player, Location location, byte stackPosition)
        {
            switch (location.Type)
            {
                case LocationType.Ground:
                    Thing thing = Map.GetTile(location).GetThingAtStackPosition(stackPosition);
                    return thing;
                case LocationType.Container:
                    Container container = player.Inventory.GetContainer(location.Container);
                    if (container != null)
                    {
                        return container.GetItem(location.ContainerPosition);
                    }
                    break;
                case LocationType.Slot:
                    return player.Inventory.GetItemInSlot(location.Slot);
            }
            return null;
        }

        public Item TransformItem(Player player, Location location, byte stackPosition, Item newItem)
        {
            Item item = null;

            switch (location.Type)
            {
                case LocationType.Ground:
                    Tile tile = Map.GetTile(location);
                    if (tile != null)
                    {
                        item = tile.GetThingAtStackPosition(stackPosition) as Item;
                        if (item != null)
                        {
                            tile.RemoveItem(item);
                            tile.AddItem(newItem);
                            byte newStackPosition = tile.GetStackPosition(newItem);
                            foreach (var p in GetSpectatorPlayers(location))
                            {
                                p.Connection.BeginTransaction();
                                p.Connection.SendTileRemoveThing(location, stackPosition);
                                p.Connection.SendTileAddItem(location, newStackPosition, newItem);
                                p.Connection.CommitTransaction();
                            }
                        }
                    }
                    break;
                case LocationType.Slot:
                    if (player != null)
                    {
                        item = player.Inventory.GetItemInSlot(location.Slot);
                        player.Inventory.SetItemInSlot(location.Slot, newItem);
                        player.Connection.SendSlotUpdate(location.Slot);
                    }
                    break;
                case LocationType.Container:
                    if (player != null)
                    {
                        Container container = player.Inventory.GetContainer(location.Container);
                        if (container != null)
                        {
                            item = container.GetItem(location.ContainerPosition);
                            container.UpdateItem(location.ContainerPosition, newItem);
                            player.Connection.SendContainerUpdateItem(location.Container, location.ContainerPosition, newItem);
                        }
                    }
                    break;
            }

            return item;
        }

        public Item RemoveItem(Player player, Location location, byte stackPosition)
        {
            Item item = null;
            switch (location.Type)
            {
                case LocationType.Ground:
                    Tile tile = Map.GetTile(location);
                    if (tile != null)
                    {
                        item = tile.GetThingAtStackPosition(stackPosition) as Item;
                        if (item != null)
                        {
                            tile.RemoveItem(item);
                            foreach (var p in GetSpectatorPlayers(location))
                            {
                                p.Connection.SendTileRemoveThing(location, stackPosition);
                            }
                        }
                    }
                    break;
                case LocationType.Slot:
                    if (player != null)
                    {
                        item = player.Inventory.GetItemInSlot(location.Slot);
                        player.Inventory.SetItemInSlot(location.Slot, null);
                        player.Connection.SendSlotUpdate(location.Slot);
                    }
                    break;
                case LocationType.Container:
                    if (player != null)
                    {
                        Container container = player.Inventory.GetContainer(location.Container);
                        if (container != null)
                        {
                            item = container.GetItem(location.ContainerPosition);
                            container.RemoveItem(location.ContainerPosition);
                            player.Connection.SendContainerRemoveItem(location.Container, location.ContainerPosition);
                        }
                    }
                    break;
            }
            return item;
        }

        #endregion

        #region Private Helpers

        private void SetPlayerFlags(Player player, Tile tile) {
            
            if (tile.IsProtectionZone)
            {
                player.Flags |= PlayerFlags.WithinProtectionZone;
            }
            else
            {
                player.Flags &= ~PlayerFlags.WithinProtectionZone;
            }
            player.Connection.SendPlayerFlags();
        }

        private void PlayerLogin(Player player)
        {
            player.Tile.Creatures.Add(player);
            AddCreature(player);

            player.Connection.BeginTransaction();
            player.Connection.SendInitialPacket();

            SetPlayerFlags(player, Map.GetTile(player.SavedLocation));
            
            player.Connection.CommitTransaction();
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
         
            player.LastLogin = DateTime.Now;

            Database.SavePlayerInfo(player);

            if (AfterLogin != null)
            {
                AfterLogin(player);
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
