using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SharpOT.Packets;
using SharpOT.Util;

namespace SharpOT
{
    public class Connection
    {
        #region Variables

        Socket socket;
        NetworkStream stream;
        NetworkMessage inMessage = new NetworkMessage(0);
        uint[] xteaKey = new uint[4];
        bool remove = false;
        HashSet<uint> knownCreatures = new HashSet<uint>();
        Queue<Direction> walkDirections;

        #endregion

        #region Constructor

        public Connection(Game game)
        {
            this.Game = game;
        }

        #endregion

        #region Properties

        public Player Player { get; set; }

        public Game Game { get; set; }

        public long AccountId { get; set; }

        public bool ShouldRemove
        {
            get { return remove; }
        }

        public string Ip
        {
            get
            {
                return socket.RemoteEndPoint.ToString();
            }
        }

        #endregion

        #region Callbacks

        public void LoginListenerCallback(IAsyncResult ar)
        {
            TcpListener clientListener = (TcpListener)ar.AsyncState;
            socket = clientListener.EndAcceptSocket(ar);
            stream = new NetworkStream(socket);

            stream.BeginRead(inMessage.Buffer, 0, 2,
                new AsyncCallback(ClientReadFirstCallBack), null);
        }

        public void GameListenerCallback(IAsyncResult ar)
        {
            TcpListener gameListener = (TcpListener)ar.AsyncState;
            socket = gameListener.EndAcceptSocket(ar);
            stream = new NetworkStream(socket);

            SendConnectionPacket();

            stream.BeginRead(inMessage.Buffer, 0, 2,
                new AsyncCallback(ClientReadFirstCallBack), null);
        }

        private void ClientReadFirstCallBack(IAsyncResult ar)
        {
            if (!EndRead(ar)) return;

            try
            {
                byte protocol = inMessage.GetByte(); // protocol id (1 = login, 2 = game)

                if (protocol == 0x01)
                {
                    AccountPacket accountPacket = AccountPacket.Parse(inMessage);
                    xteaKey = accountPacket.XteaKey;

                    long accountId = Game.CheckLoginInfo(this, accountPacket, true);

                    if (accountId >= 0)
                    {
                        this.AccountId = accountId;
                        SendCharacterList(
                            SharpOT.Properties.Settings.Default.MessageOfTheDay,
                            999,
                            Database.GetCharacterList(accountId)
                        );
                    }

                    Close();
                }
                else if (protocol == 0x0A)
                {
                    ParseLoginPacket(inMessage);

                    if (stream.CanRead)
                    {
                        stream.BeginRead(inMessage.Buffer, 0, 2,
                            new AsyncCallback(ClientReadCallBack), null);
                    }
                }
            }
            catch
            {
                // Invalid data from the client
                Close();
            }
        }

        private void ClientReadCallBack(IAsyncResult ar)
        {
            if (!EndRead(ar))
            {
                // Client crashed or disconnected
                Game.PlayerLogout(Player);
                return;
            }

            inMessage.XteaDecrypt(xteaKey);
            ushort length = inMessage.GetUInt16();
            byte type = inMessage.GetByte();

            ParseClientPacket((ClientPacketType)type, inMessage);
            
            if (!remove)
            {
                stream.BeginRead(inMessage.Buffer, 0, 2,
                    new AsyncCallback(ClientReadCallBack), null);
            }
        }

        private bool EndRead(IAsyncResult ar)
        {
            int read = stream.EndRead(ar);

            if (read == 0)
            {
                // client disconnected
                Close();
                return false;
            }

            int size = (int)BitConverter.ToUInt16(inMessage.Buffer, 0) + 2;

            while (read < size)
            {
                if (stream.CanRead)
                    read += stream.Read(inMessage.Buffer, read, size - read);
            }
            inMessage.Length = size;

            inMessage.Position = 0;

            inMessage.GetUInt16(); // total length
            inMessage.GetUInt32(); // adler

            return true;
        }

        #endregion

        #region Parse

        private void ParseLoginPacket(NetworkMessage message)
        {
            LoginPacket loginPacket = LoginPacket.Parse(message);
            xteaKey = loginPacket.XteaKey;

            long accountId = Game.CheckLoginInfo(this, loginPacket, false);

            if (accountId >= 0)
            {
                this.AccountId = accountId;
                Game.ProcessLogin(this, loginPacket.CharacterName);
            }
            else
            {
                Close();
            }
        }

        private void ParseClientPacket(ClientPacketType type, NetworkMessage message)
        {
            switch (type)
            {
                case ClientPacketType.Logout:
                    ParseLogout();
                    break;
                case ClientPacketType.ItemMove:
                    ParseItemMove(message);
                    break;
                //case ClientPacketType.ShopBuy:
                //case ClientPacketType.ShopSell:
                //case ClientPacketType.ShopClose:
                case ClientPacketType.ItemUse:
                    ParseItemUse(message);
                    break;
                case ClientPacketType.ItemUseOn:
                    ParseItemUseOn(message);
                    break;
                //case ClientPacketType.ItemRotate:
                case ClientPacketType.LookAt:
                    ParseLookAt(message);
                    break;
                case ClientPacketType.PlayerSpeech:
                    ParsePlayerSpeech(message);
                    break;
                case ClientPacketType.ChannelList:
                    SendChannelList(Player);
                    break;
                case ClientPacketType.ClientChannelOpen:
                    ParseClientChannelOpen(message);
                    break;
                case ClientPacketType.ChannelClose:
                    ParseChannelClose(message);
                    break;
                //case ClientPacketType.Attack:
                //case ClientPacketType.Follow:
                //case ClientPacketType.CancelMove:
                //case ClientPacketType.ItemUseBattlelist:
                case ClientPacketType.ContainerClose:
                    ParseContainerClose(message);
                    break;
                case ClientPacketType.ContainerOpenParent:
                    ParseContainerOpenParent(message);
                    break;
                case ClientPacketType.TurnNorth:
                    ParseTurn(Direction.North);
                    break;
                case ClientPacketType.TurnWest:
                    ParseTurn(Direction.East);
                    break;
                case ClientPacketType.TurnSouth:
                    ParseTurn(Direction.South);
                    break;
                case ClientPacketType.TurnEast:
                    ParseTurn(Direction.West);
                    break;
                case ClientPacketType.AutoWalk:
                    ParseAutoWalk(message);
                    break;
                case ClientPacketType.AutoWalkCancel:
                    ParseAutoWalkCancel();
                    break;                
                case ClientPacketType.VipAdd:
                    ParseVipAdd(message);
                    break;
                case ClientPacketType.VipRemove:
                    ParseVipRemove(message);
                    break;
                case ClientPacketType.RequestOutfit:
                    SendOutfitWindow();
                    break;
                case ClientPacketType.ChangeOutfit:
                    ParseChangeOutfit(message);
                    break;
                //case ClientPacketType.Ping:
                case ClientPacketType.FightModes:
                    ParseFightModes(message);
                    break;
                //case ClientPacketType.ContainerUpdate:
                //case ClientPacketType.TileUpdate:
                case ClientPacketType.PrivateChannelOpen:
                    ParsePrivateChannelOpen(message);
                    break;
                //case ClientPacketType.NpcChannelClose:
                //    break;
                case ClientPacketType.MoveNorth:
                    ParseWalk(Direction.North);
                    break;
                case ClientPacketType.MoveEast:
                    ParseWalk(Direction.East);
                    break;
                case ClientPacketType.MoveSouth:
                    ParseWalk(Direction.South);
                    break;
                case ClientPacketType.MoveWest:
                    ParseWalk(Direction.West);
                    break;
                case ClientPacketType.MoveNorthEast:
                    ParseWalk(Direction.NorthEast);
                    break;
                case ClientPacketType.MoveSouthEast:
                    ParseWalk(Direction.SouthEast);
                    break;
                case ClientPacketType.MoveSouthWest:
                    ParseWalk(Direction.SouthWest);
                    break;
                case ClientPacketType.MoveNorthWest:
                    ParseWalk(Direction.NorthWest);
                    break;
                default:
                    Server.Log("Unhandled packet from {0}: {1}", Player.ToString(), type);
                    break;
            }
        }

        public void ParseAutoWalk(NetworkMessage message)
        {
            AutoWalkPacket packet = AutoWalkPacket.Parse(message);
            walkDirections = packet.Directions;
            DoAutoWalk();
        }

        public void ParseContainerClose(NetworkMessage message)
        {
            ContainerClosePacket packet = ContainerClosePacket.Parse(message);
            Game.ContainerClose(Player, packet.ContainerIndex);
        }

        public void ParseContainerOpenParent(NetworkMessage message)
        {
            ContainerOpenParentPacket packet = ContainerOpenParentPacket.Parse(message);
            Game.ContainerOpenParent(Player, packet.ContainerIndex);
        }

        public void ParseItemUseOn(NetworkMessage message)
        {
            ItemUseOnPacket packet = ItemUseOnPacket.Parse(message);
            Game.ItemUseOn(Player, packet.FromSpriteId, packet.FromLocation, packet.FromStackPosition, packet.ToSpriteId, packet.ToLocation, packet.ToStackPosition);
        }

        public void ParseItemUse(NetworkMessage message)
        {
            ItemUsePacket packet = ItemUsePacket.Parse(message);
            Game.ItemUse(Player, packet.SpriteId, packet.FromLocation, packet.FromStackPosition, packet.Index);
        }

        public void ParseItemMove(NetworkMessage message)
        {
            ItemMovePacket packet = ItemMovePacket.Parse(message);
            Game.ItemMove(Player, packet.SpriteId, packet.FromLocation, packet.FromStackPosition, packet.ToLocation, packet.Count);
        }

        public void ParseAutoWalkCancel()
        {
            Game.WalkCancel(Player);
        }

        public void ParseTurn(Direction direction)
        {
            Game.CreatureTurn(Player, direction);
        }

        public void ParseWalk(Direction direction)
        {
            if (Player.LastStepTime + Player.GetStepDuration() < DateTime.Now.Ticks)
            {
                Game.CreatureWalk(Player, direction);
            }
        }

        public void ParseLogout()
        {
            Game.PlayerLogout(Player);
        }

        public void ParsePlayerSpeech(NetworkMessage message)
        {
            PlayerSpeechPacket packet = PlayerSpeechPacket.Parse(message);

            Game.CreatureSpeech(this.Player, packet.Speech);
        }

        public void ParseClientChannelOpen(NetworkMessage message)
        {
            ClientChannelOpenPacket packet = ClientChannelOpenPacket.Parse(message);
            Game.ChannelOpen(Player, packet.Channel);
        }

        public void ParseChannelClose(NetworkMessage message)
        {
            ChannelClosePacket packet = ChannelClosePacket.Parse(message);
            Game.ChannelClose(Player, packet.Channel);
        }
        
        public void ParseVipAdd(NetworkMessage message)
        {
            VipAddPacket packet = VipAddPacket.Parse(message);
            Game.VipAdd(Player, packet.Name);
        }

        public void ParseVipRemove(NetworkMessage message)
        {
            VipRemovePacket packet = VipRemovePacket.Parse(message);
            Game.VipRemove(Player, packet.Id);
        }

        public void ParseChangeOutfit(NetworkMessage message)
        {
            ChangeOutfitPacket packet = ChangeOutfitPacket.Parse(message);
            Game.PlayerChangeOutfit(Player, packet.Outfit);
        }

        public void ParseFightModes(NetworkMessage message)
        {
            FightModesPacket packet = FightModesPacket.Parse(message);
            Player.FightMode = (FightMode)packet.FightMode;
            Player.ChaseMode = packet.ChaseMode;
            Player.SafeMode = packet.SafeMode;
        }

        public void ParsePrivateChannelOpen(NetworkMessage message)
        {
            PrivateChannelOpenPacket packet = PrivateChannelOpenPacket.Parse(message);
            Game.PrivateChannelOpen(Player, packet.Receiver);
        }

        public void ParseLookAt(NetworkMessage message)
        {
            LookAtPacket packet = LookAtPacket.Parse(message);
            Game.PlayerLookAt(Player, packet.Id, packet.Location, packet.StackPosition);
        }
        
        #endregion

        #region Send

        public void SendContainerOpen(Container container, byte containerId)
        {
            NetworkMessage message = new NetworkMessage();

            ContainerOpenPacket.Add(
                message,
                containerId,
                container.Info.SpriteId,
                "Container",
                container.Volume,
                container.Parent != null,
                container.Items
            );

            Send(message);
        }

        public void SendContainerClose(byte containerId)
        {
            NetworkMessage message = new NetworkMessage();

            ContainerClosePacket.Add(
                message,
                containerId
            );

            Send(message);
        }

        public void SendContainerAddItem(byte containerIndex, Item item)
        {
            NetworkMessage message = new NetworkMessage();

            ContainerAddItemPacket.Add(
                message,
                containerIndex,
                item
            );

            Send(message);
        }

        public void SendContainerRemoveItem(byte containerIndex, byte containerPosition)
        {
            NetworkMessage message = new NetworkMessage();

            ContainerRemoveItemPacket.Add(
                message,
                containerIndex,
                containerPosition
            );

            Send(message);
        }

        public void SendContainerUpdateItem(byte containerIndex, byte containerPosition, Item item)
        {
            NetworkMessage message = new NetworkMessage();

            ContainerUpdateItemPacket.Add(
                message,
                containerIndex,
                containerPosition,
                item
            );

            Send(message);
        }

        private void SendConnectionPacket()
        {
            NetworkMessage message = new NetworkMessage();

            GameServerConnectPacket.Add(message);

            Send(message, false);
        }

        public void SendCharacterList(string motd, ushort premiumDays, IEnumerable<CharacterListItem> chars)
        {
            NetworkMessage message = new NetworkMessage();

            if (motd != string.Empty)
            {
                MessageOfTheDayPacket.Add(
                    message,
                    motd
                );
            }
            CharacterListPacket.Add(
                message,
                chars,
                premiumDays
            );

            Send(message);
        }

        public void SendInitialPacket()
        {
            NetworkMessage message = new NetworkMessage();

            SelfAppearPacket.Add(
                message,
                Player.Id,
                true
            );

            MapDescriptionPacket.Add(
                this,
                message,
                Player.Tile.Location
            );

            EffectPacket.Add(message, Player.Tile.Location, Effect.Teleport);

            foreach (var kvp in Player.Inventory.GetSlotItems())
            {
                InventorySetSlotPacket.Add(
                    message,
                    kvp.Key,
                    kvp.Value
                );
            }

            WorldLightPacket.Add(
                message,
                LightLevel.World,
                LightColor.White
            );

            CreatureLightPacket.Add(
                message,
                Player.Id,
                LightLevel.None,
                LightColor.None
            );

            string welcomeMessage = "Welcome to Utopia!";
            if (Player.LastLogin > DateTime.MinValue)
            {
                welcomeMessage += String.Format(" Last login: {0}.", Player.LastLogin);
            }
            TextMessagePacket.Add(
                message,
                TextMessageType.EventDefault,
                welcomeMessage
            );

            PlayerStatusPacket.Add(
                message,
                Player.Health,
                Player.MaxHealth,
                Player.Capacity,
                Player.Experience,
                Player.Level,
                0, // TODO: level system
                Player.Mana,
                Player.MaxMana,
                Player.MagicLevel,
                0,
                0,
                0
            );


            // Player skills
            //message.AddBytes("A1 0A 02 0A 00 0E 44 0B 62 0A 0D 0F 3E 13 26".ToBytesAsHex());

            // Fight modes
            //message.AddBytes("A0 02 00 01".ToBytesAsHex());

            Send(message);
        }

        public void SendOutfitWindow()
        {
            NetworkMessage message = new NetworkMessage();

            // TODO: put somewhere else, xml?
            List<Outfit> outfits;

            if (Player.Gender == Gender.Male)
            {
                outfits = new List<Outfit>
                {
                    new Outfit("Citizen", 128, 7),
                    new Outfit("Hunter", 129, 7),
                    new Outfit("Mage", 130, 7),
                    new Outfit("Knight", 131, 7),
                    new Outfit("Nobleman", 132, 7),
                    new Outfit("Summoner", 133, 7),
                    new Outfit("Warrior", 134, 7),
                    new Outfit("Barbarian", 143, 7),
                    new Outfit("Druid", 144, 7),
                    new Outfit("Wizard", 145, 7),
                    new Outfit("Oriental", 146, 7),
                    new Outfit("Pirate", 151, 7),
                    new Outfit("Assassin", 152, 7),
                    new Outfit("Beggar", 153, 7),
                    new Outfit("Shaman", 154, 7),
                    new Outfit("Norseman", 251, 7),
                    new Outfit("Nightmare", 268, 7),
                    new Outfit("Jester", 273, 7),
                    new Outfit("Brotherhood", 278, 7),
                    new Outfit("Demonhunter", 289, 7),
                    new Outfit("Yalaharian", 325, 7),
                    new Outfit("Wedding", 328, 7),
                    new Outfit("Gamemaster", 75, 7),
                    new Outfit("Old Com. Manager", 266, 7),
                    new Outfit("Com. Manager", 302, 7)
                };
            }
            else
            {
                outfits = new List<Outfit>
                {
                    new Outfit("Citizen", 136, 7),
                    new Outfit("Hunter", 137, 7),
                    new Outfit("Mage", 138, 7),
                    new Outfit("Knight", 139, 7),
                    new Outfit("Noblewoman", 140, 7),
                    new Outfit("Summoner", 141, 7),
                    new Outfit("Warrior", 142, 7),
                    new Outfit("Barbarian", 147, 7),
                    new Outfit("Druid", 148, 7),
                    new Outfit("Wizard", 149, 7),
                    new Outfit("Oriental", 150, 7),
                    new Outfit("Pirate", 155, 7),
                    new Outfit("Assassin", 156, 7),
                    new Outfit("Beggar", 157, 7),
                    new Outfit("Shaman", 158, 7),
                    new Outfit("Norsewoman", 252, 7),
                    new Outfit("Nightmare", 269, 7),
                    new Outfit("Jester", 270, 7),
                    new Outfit("Brotherhood", 279, 7),
                    new Outfit("Demonhunter", 288, 7),
                    new Outfit("Yalaharian", 324, 7),
                    new Outfit("Wedding", 329, 7),
                    new Outfit("Gamemaster", 75, 7),
                    new Outfit("Old Com. Manager", 266, 7),
                    new Outfit("Com. Manager", 302, 0)
                };
            }
            OutfitWindowPacket.Add(
                message,
                Player,
                outfits
            );

            Send(message);
        }

        public void SendStatus()
        {
            NetworkMessage outMessage = new NetworkMessage();
            PlayerStatusPacket.Add(
                outMessage,
                Player.Health,
                Player.MaxHealth,
                Player.Capacity,
                Player.Experience,
                Player.Level,
                0, // TODO: level system
                Player.Mana,
                Player.MaxMana,
                Player.MagicLevel,
                0,
                0,
                0
            );

            Send(outMessage);
        }

        public void SendEffect(Location location, Effect effect)
        {
            NetworkMessage outMessage = new NetworkMessage();
            EffectPacket.Add(outMessage, location, effect);
            Send(outMessage);
        }

        public void SendProjectile(Location from, Location to, ProjectileType projectile)
        {
            NetworkMessage outMessage = new NetworkMessage();
            ProjectilePacket.Add(
                outMessage,
                from,
                to,
                projectile
            );
            Send(outMessage);
        }

        public void SendSlotUpdate(SlotType slot)
        {
            if (slot < SlotType.First || slot > SlotType.Last) return;

            Item item = Player.Inventory.GetItemInSlot(slot);

            NetworkMessage message = new NetworkMessage();

            if (item == null)
            {
                InventoryClearSlotPacket.Add(
                    message,
                    slot
                );
            }
            else
            {
                InventorySetSlotPacket.Add(
                    message,
                    slot,
                    item
                );
            }

            Send(message);
        }

        public void SendCreatureChangeOutfit(Creature creature)
        {
            NetworkMessage message = new NetworkMessage();

            CreatureChangeOutfitPacket.Add(
                message,
                creature
            );

            Send(message);
        }

        public void SendCreatureMove(Location fromLocation, byte fromStackPosition, Location toLocation)
        {
            NetworkMessage outMessage = new NetworkMessage();

            CreatureMovePacket.Add(
                outMessage,
                fromLocation,
                fromStackPosition,
                toLocation
            );

            Send(outMessage);
        }

        public void SendCreatureUpdateHealth(Creature creature)
        {
            NetworkMessage outMessage = new NetworkMessage();

            CreatureHealthPacket.Add(
                outMessage,
                creature.Id,
                creature.HealthPercent);

            Send(outMessage);
        }

        public void SendCreatureLogout(Creature creature)
        {
            NetworkMessage message = new NetworkMessage();
            EffectPacket.Add(message, // TODO: find the new value for poof
                creature.Tile.Location, Effect.Puff);
            TileRemoveThingPacket.Add(
                message,
                creature.Tile.Location,
                creature.Tile.GetStackPosition(creature)
            );
            Send(message);
        }

        public void SendTileRemoveThing(Location fromLocation, byte fromStackPosition)
        {
            NetworkMessage message = new NetworkMessage();
            TileRemoveThingPacket.Add(
                message, 
                fromLocation, 
                fromStackPosition
            );
            Send(message);
        }

        public void SendCreatureAppear(Creature creature)
        {
            NetworkMessage message = new NetworkMessage();

            EffectPacket.Add(message, creature.Tile.Location, Effect.Teleport);

            uint remove;
            bool known = IsCreatureKnown(creature.Id, out remove);
            TileAddCreaturePacket.Add(
                message, 
                creature.Tile.Location, 
                creature.Tile.GetStackPosition(creature), 
                creature, 
                known, 
                remove
            );

            Send(message);
        }

        public void SendTileAddItem(Location location, byte stackPosition, Item item)
        {
            NetworkMessage message = new NetworkMessage();

            TileAddItemPacket.Add(
                message,
                location,
                stackPosition,
                item
            );

            Send(message);
        }

        public void SendTileAddCreature(Creature creature)
        {
            NetworkMessage message = new NetworkMessage();
            uint remove;
            bool known = IsCreatureKnown(creature.Id, out remove);
            TileAddCreaturePacket.Add(
                message, 
                creature.Tile.Location, 
                creature.Tile.GetStackPosition(creature), 
                creature, 
                known, 
                remove
            );
            Send(message);
        }

        public void SendPlayerTeleport(Location fromLocation, byte fromStackPosition, Location toLocation)
        {
            NetworkMessage outMessage = new NetworkMessage();

            TileRemoveThingPacket.Add(
                outMessage, 
                fromLocation, 
                fromStackPosition
            );

            MapDescriptionPacket.Add(
                this, 
                outMessage, 
                toLocation
            );

            Send(outMessage);
        }

        public void SendPlayerMove(Location fromLocation, byte fromStackPosition, Location toLocation)
        {
            NetworkMessage outMessage = new NetworkMessage();

            if (fromLocation.Z == 7 && toLocation.Z >= 8)
            {
                TileRemoveThingPacket.Add(
                    outMessage, 
                    fromLocation, 
                    fromStackPosition
                );
            }
            else
            {
                CreatureMovePacket.Add(
                    outMessage,
                    fromLocation,
                    fromStackPosition,
                    toLocation
                );
            }

            //floor change down
            if (toLocation.Z > fromLocation.Z)
            {
                MapFloorChangeDownPacket.Add(
                    this,
                    outMessage,
                    fromLocation,
                    fromStackPosition,
                    toLocation
                );
            }
            //floor change up
            else if (toLocation.Z < fromLocation.Z)
            {
                MapFloorChangeUpPacket.Add(
                    this,
                    outMessage, 
                    fromLocation, 
                    fromStackPosition, 
                    toLocation
                );
            }

            MapSlicePacket.Add(
                this,
                outMessage,
                fromLocation,
                toLocation
            );

            Send(outMessage);
        }

        public void SendCreatureSpeech(Creature creature, SpeechType speechType, string message)
        {

            NetworkMessage outMessage = new NetworkMessage();
            CreatureSpeechPacket.Add(
                outMessage,
                creature.Name,
                1,
                speechType,
                message,
                creature.Tile.Location,
                ChatChannel.None,
                0000
            );
            Send(outMessage);
        }

        public void SendChannelSpeech(string sender, SpeechType type, ChatChannel channelId, string message)
        {
            NetworkMessage outMessage = new NetworkMessage();
            CreatureSpeechPacket.Add(
                outMessage,
                sender,
                1,
                type,
                message,
                null,
                channelId,
                0
            );
            Send(outMessage);
        }   

        public void SendCreatureTurn(Creature creature)
        {
            NetworkMessage message = new NetworkMessage();
            CreatureTurnPacket.Add(
                message,
                creature
            );
            Send(message);
        }

        public void SendCancelWalk()
        {
            NetworkMessage message = new NetworkMessage();
            PlayerWalkCancelPacket.Add(
                message,
                Player.Direction
            );
            Send(message);
        }

        public void SendChannelOpenPrivate(string name)
        {
            NetworkMessage message = new NetworkMessage();
            ChannelOpenPrivatePacket.Add(
                message,
                name
            );
            Send(message);
        }

        public void SendChannelList(Player player)
        {
            NetworkMessage message = new NetworkMessage();
            ChannelListPacket.Add(
                message,
                player.ChannelList
            );
            Send(message);
        }

        public void SendChannelOpen(Channel channel)
        {
            NetworkMessage message = new NetworkMessage();
            ChannelOpenPacket.Add(
                message,
                channel.Id,
                channel.Name
            );
            Send(message);
        }

        public void SendTextMessage(TextMessageType type, string text)
        {
            NetworkMessage message = new NetworkMessage();
            TextMessagePacket.Add(
                message,
                type,
                text
            );
            Send(message);
        }

        public void SendVipState(uint id, string name, bool loggedIn)
        {
            NetworkMessage message = new NetworkMessage();
            VipStatePacket.Add(
                message,
                id,
                name,
                Convert.ToByte(loggedIn)
            );
            Send(message);
        }

        public void SendVipLogin(uint id)
        {
            NetworkMessage message = new NetworkMessage();
            VipLoginPacket.Add(
                message,
                id
            );
            Send(message);
        }

        public void SendVipLogout(uint id)
        {
            NetworkMessage message = new NetworkMessage();
            VipLogoutPacket.Add(
                message,
                id
            );
            Send(message);
        }

        public void SendDisconnectLogin(string reason)
        {
            NetworkMessage message = new NetworkMessage();
            message.AddByte((byte)ServerPacketType.Disconnect);
            message.AddString(reason);
            Send(message);
        }

        public void SendDisconnectGame(string reason)
        {
            NetworkMessage message = new NetworkMessage();
            message.AddByte((byte)ServerPacketType.ErrorMessage);
            message.AddString(reason);
            Send(message);
        }

        private bool isInTransaction = false;
        private NetworkMessage transactionMessage = new NetworkMessage();

        public void BeginTransaction()
        {
            if (!isInTransaction)
            {
                transactionMessage.Reset();
                isInTransaction = true;
            }
        }

        public void CommitTransaction()
        {
            SendMessage(transactionMessage, true);
            isInTransaction = false;
        }

        private void SendMessage(NetworkMessage message, bool useEncryption)
        {
            if (useEncryption)
                message.PrepareToSend(xteaKey);
            else
                message.PrepareToSendWithoutEncryption();

            stream.BeginWrite(message.Buffer, 0, message.Length, null, null);
        }

        public void Send(NetworkMessage message)
        {
            Send(message, true);
        }

        public void Send(NetworkMessage message, bool useEncryption)
        {
            if (isInTransaction)
            {
                if (useEncryption == false)
                    throw new Exception("Cannot send a packet without encryption as part of a transaction.");
                
                transactionMessage.AddBytes(message.GetPacket());
            }
            else
            {
                SendMessage(message, useEncryption);
            }
        }

        #endregion

        #region Other

        public bool IsCreatureKnown(uint id, out uint removeId)
        {
            if (knownCreatures.Contains(id))
            {
                removeId = 0;
                return true;
            }
            else
            {
                // TODO: Fix this logic, as it is it never removes
                knownCreatures.Add(id);
                removeId = 0;
                return false;
            }
        }

        public void Close()
        {
            remove = true;
            stream.Close();
            socket.Close();
        }

        #endregion

        #region Private

        private void DoAutoWalk()
        {
            if (walkDirections.Count > 0)
            {
                if (Player.LastStepTime + Player.GetStepDuration() < DateTime.Now.Ticks)
                {
                    Direction direction = walkDirections.Dequeue();
                    Game.CreatureWalk(Player, direction);
                }
                if (walkDirections.Count > 0)
                {
                    Scheduler.AddTask(
                        this.DoAutoWalk,
                        null,
                        (int)Player.GetStepDuration());
                }
            }
        }

        #endregion
    }
}
