using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SharpOT.Util;
using SharpOT.Packets;

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

        public bool ShouldRemove
        {
            get { return remove; }
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

            byte protocol = inMessage.GetByte(); // protocol id (1 = login, 2 = game)

            if (protocol == 0x01)
            {
                AccountPacket accountPacket = AccountPacket.Parse(inMessage);
                xteaKey = accountPacket.XteaKey;

                SendCharacterList(accountPacket);

                Close();
            }
            else if (protocol == 0x0A)
            {
                ParseLoginPacket(inMessage);

                stream.BeginRead(inMessage.Buffer, 0, 2,
                    new AsyncCallback(ClientReadCallBack), null);
            }
        }

        private void ClientReadCallBack(IAsyncResult ar)
        {
            if (!EndRead(ar)) return;

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

            Game.ProcessLogin(this, loginPacket);

        }

        private void ParseClientPacket(ClientPacketType type, NetworkMessage message)
        {
            switch (type)
            {
                case ClientPacketType.Logout:
                    ParseLogout(message);
                    break;
                //case ClientPacketType.ItemMove:
                //case ClientPacketType.ShopBuy:
                //case ClientPacketType.ShopSell:
                //case ClientPacketType.ShopClose:
                //case ClientPacketType.ItemUse:
                //case ClientPacketType.ItemUseOn:
                //case ClientPacketType.ItemRotate:
                //case ClientPacketType.LookAt:
                case ClientPacketType.PlayerSpeech:
                    ParsePlayerSpeech(message);
                    break;
                //case ClientPacketType.ChannelList:
                //case ClientPacketType.ChannelOpen:
                //case ClientPacketType.ChannelClose:
                //case ClientPacketType.Attack:
                //case ClientPacketType.Follow:
                //case ClientPacketType.CancelMove:
                //case ClientPacketType.ItemUseBattlelist:
                //case ClientPacketType.ContainerClose:
                //case ClientPacketType.ContainerOpenParent:
                case ClientPacketType.TurnNorth:
                    Game.CreatureTurn(Player, Direction.North);
                    break;
                case ClientPacketType.TurnWest:
                    Game.CreatureTurn(Player, Direction.East);
                    break;
                case ClientPacketType.TurnSouth:
                    Game.CreatureTurn(Player, Direction.South);
                    break;
                case ClientPacketType.TurnEast:
                    Game.CreatureTurn(Player, Direction.West);
                    break;
                //case ClientPacketType.AutoWalk:
                case ClientPacketType.AutoWalkCancel:
                    Game.WalkCancel(Player);
                    break;
                //case ClientPacketType.VipAdd:
                //case ClientPacketType.VipRemove:
                case ClientPacketType.RequestOutfit:
                    SendOutfitWindow();
                    break;
                case ClientPacketType.ChangeOutfit:
                    ParseChangeOutfit(message);
                    break;
                //case ClientPacketType.Ping:
                //case ClientPacketType.FightModes:
                //case ClientPacketType.ContainerUpdate:
                //case ClientPacketType.TileUpdate:
                //case ClientPacketType.PrivateChannelOpen:
                //case ClientPacketType.NpcChannelClose:
                //    break;
                case ClientPacketType.MoveNorth:
                    Game.CreatureMove(Player,  Direction.North);
                    break;
                case ClientPacketType.MoveEast:
                    Game.CreatureMove(Player,  Direction.East);
                    break;
                case ClientPacketType.MoveSouth:
                    Game.CreatureMove(Player,  Direction.South);
                    break;
                case ClientPacketType.MoveWest:
                    Game.CreatureMove(Player,  Direction.West);
                    break;
                case ClientPacketType.MoveNorthEast:
                    Game.CreatureMove(Player,  Direction.NorthEast);
                    break;
                case ClientPacketType.MoveSouthEast:
                    Game.CreatureMove(Player,  Direction.SouthEast);
                    break;
                case ClientPacketType.MoveSouthWest:
                    Game.CreatureMove(Player,  Direction.SouthWest);
                    break;
                case ClientPacketType.MoveNorthWest:
                    Game.CreatureMove(Player,  Direction.NorthWest);
                    break;
                default:
                    Server.Log("Unhandled packet from " + Player + ": " + type);
                    break;
            }
        }

        public void ParseLogout(NetworkMessage message)
        {
            Game.PlayerLogout(Player);
        }

        public void ParsePlayerSpeech(NetworkMessage message)
        {
            PlayerSpeechPacket packet = PlayerSpeechPacket.Parse(message);
            Game.CreatureSpeech(Player, packet.Message);
        }

        public void ParseChangeOutfit(NetworkMessage message)
        {
            ChangeOutfitPacket packet = ChangeOutfitPacket.Parse(message);
            Game.CreatureChangeOutfit(Player, packet.Outfit);
        }

        #endregion

        #region Send

        private void SendConnectionPacket()
        {
            NetworkMessage message = new NetworkMessage();

            GameServerConnectPacket.Add(message);

            Send(message, false);
        }

        private void SendCharacterList(AccountPacket accountPacket)
        {
            NetworkMessage message = new NetworkMessage();

            MessageOfTheDayPacket.Add(
                message,
                "1\nWelcome to Utopia!"
            );
            CharacterListPacket.Add(
                message,
                new CharacterListItem[] {
                    new CharacterListItem("Ian", "Utopia", new byte[] { 127, 0, 0, 1 }, 7172),
                    new CharacterListItem("Vura", "Utopia", new byte[] { 127, 0, 0, 1 }, 7172),
                    new CharacterListItem("God", "Utopia", new byte[] { 127, 0, 0, 1 }, 7172)
                },
                999
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

            EffectPacket.Add(
                message,
                Effect.EnergyDamage,
                Player.Tile.Location
            );

            // Inventory
            //message.AddBytes("78 01 1A 0D 78 03 26 0B 78 04 1E 0D 78 05 52 0D 78 06 D3 0C 78 08 E0 0D 78 0A 9B 0D".ToBytesAsHex());

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

            TextMessagePacket.Add(
                message,
                TextMessageType.EventDefault,
                "Welcome to Utopia! Last login: yesterday."
            );

            PlayerStatusPacket.Add(
                message,
                100,
                100,
                100,
                100,
                1,
                0,
                100,
                100,
                0,
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

            OutfitWindowPacket.Add(
                message   ,
                Player
            );

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

        public void SendCreatureLogout(Creature creature)
        {
            NetworkMessage message = new NetworkMessage();
            EffectPacket.Add(
                message,
                Effect.Puff, // TODO: find the new value for poof
                creature.Tile.Location
            );
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

            EffectPacket.Add(
                message,
                Effect.EnergyDamage,
                creature.Tile.Location
            );

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

        public void SendPlayerMove(Location fromLocation, byte fromStackPosition, Location toLocation)
        {
            NetworkMessage outMessage = new NetworkMessage();

            CreatureMovePacket.Add(
                outMessage,
                fromLocation,
                fromStackPosition,
                toLocation
            );

            MapSlicePacket.Add(
                this,
                outMessage,
                fromLocation,
                toLocation
            );

            Send(outMessage);
        }

        public void SendCreatureSpeech(Creature creature, string message)
        {

            NetworkMessage outMessage = new NetworkMessage();
            CreatureSpeechPacket.Add(
                outMessage,
                creature.Name,
                1,
                SpeechType.Say,
                message,
                creature.Tile.Location,
                ChatChannel.None,
                0000
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

        public void Send(NetworkMessage message)
        {
            Send(message, true);
        }

        public void Send(NetworkMessage message, bool useEncryption)
        {
            if (useEncryption)
                message.PrepareToSend(xteaKey);
            else
                message.PrepareToSendWithoutEncryption();

            stream.BeginWrite(message.Buffer, 0, message.Length, null, null);
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
    }
}
