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
        Socket socket;
        NetworkStream stream;
        NetworkMessage message = new NetworkMessage(0);
        uint[] xteaKey = new uint[4];
        bool remove = false;
        Creature player;
        HashSet<uint> knownCreatures = new HashSet<uint>();

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

        public bool ShouldRemove
        {
            get { return remove; }
        }

        public void LoginListenerCallback(IAsyncResult ar)
        {
            TcpListener clientListener = (TcpListener)ar.AsyncState;
            socket = clientListener.EndAcceptSocket(ar);
            stream = new NetworkStream(socket);

            stream.BeginRead(message.Buffer, 0, 2,
                new AsyncCallback(ClientReadFirstCallBack), null);
        }

        public void GameListenerCallback(IAsyncResult ar)
        {
            TcpListener clientListener = (TcpListener)ar.AsyncState;
            socket = clientListener.EndAcceptSocket(ar);
            stream = new NetworkStream(socket);

            SendConnectionPacket();

            stream.BeginRead(message.Buffer, 0, 2,
                new AsyncCallback(ClientReadFirstCallBack), null);
        }

        private void ClientReadFirstCallBack(IAsyncResult ar)
        {
            if (!EndRead(ar)) return;

            byte protocol = message.GetByte(); // protocol id (1 = login, 2 = game)

            if (protocol == 0x01)
            {
                AccountPacket accountPacket = AccountPacket.Parse(message);
                xteaKey = accountPacket.XteaKey;

                SendCharacterList();

                Close();
            }
            else if (protocol == 0x0A)
            {
                LoginPacket loginPacket = LoginPacket.Parse(message);
                xteaKey = loginPacket.XteaKey;

                SendInitialPacket();

                stream.BeginRead(message.Buffer, 0, 2,
                    new AsyncCallback(ClientReadCallBack), null);
            }
        }

        private void ClientReadCallBack(IAsyncResult ar)
        {
            if (!EndRead(ar)) return;

            message.XteaDecrypt(xteaKey);
            ushort length = message.GetUInt16();
            byte type = message.GetByte();

            switch ((ClientPacketType)type)
            {
                case ClientPacketType.MoveNorth:
                    ParseMove(message, Direction.North);
                    break;
                case ClientPacketType.MoveEast:
                    ParseMove(message, Direction.East);
                    break;
                case ClientPacketType.MoveSouth:
                    ParseMove(message, Direction.South);
                    break;
                case ClientPacketType.MoveWest:
                    ParseMove(message, Direction.West);
                    break;
                case ClientPacketType.MoveNorthEast:
                    ParseMove(message, Direction.NorthEast);
                    break;
                case ClientPacketType.MoveSouthEast:
                    ParseMove(message, Direction.SouthEast);
                    break;
                case ClientPacketType.MoveSouthWest:
                    ParseMove(message, Direction.SouthWest);
                    break;
                case ClientPacketType.MoveNorthWest:
                    ParseMove(message, Direction.NorthWest);
                    break;
            }

            stream.BeginRead(message.Buffer, 0, 2,
                new AsyncCallback(ClientReadCallBack), null);
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
            
            int size = (int)BitConverter.ToUInt16(message.Buffer, 0) + 2;

            while (read < size)
            {
                if (stream.CanRead)
                    read += stream.Read(message.Buffer, read, size - read);
            }
            message.Length = size;

            message.Position = 0;

            message.GetUInt16(); // total length
            message.GetUInt32(); // adler

            return true;
        }

        private void SendConnectionPacket()
        {
            NetworkMessage message = new NetworkMessage();

            GameServerConnectPacket.Add(message);

            message.PrepareToSendWithoutEncryption();

            stream.BeginWrite(message.Buffer, 0, message.Length, null, null);
        }

        private void SendCharacterList()
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

            message.PrepareToSend(xteaKey);

            stream.BeginWrite(message.Buffer, 0, message.Length, null, null);
        }

        private void SendInitialPacket()
        {
            NetworkMessage message = new NetworkMessage();

            Location playerLocation = new Location(50, 50, 7);
            player = new Creature();
            player.Id = 0x01020304;
            player.Name = "Ian";
            player.Health = 100;
            player.MaxHealth = 100;
            Tile tile = Map.Instance.GetTile(playerLocation);
            tile.Creatures.Add(player);
            player.Tile = tile;

            SelfAppearPacket.Add(
                message,
                player.Id,
                true
            );

            MapDescriptionPacket.Add(
                this,
                message,
                playerLocation
            );

            EffectPacket.Add(
                message,
                Effect.EnergyDamage,
                playerLocation
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
                player.Id,
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

            message.PrepareToSend(xteaKey);

            stream.BeginWrite(message.Buffer, 0, message.Length, null, null);
        }

        public void Close()
        {
            remove = true;
            stream.Close();
            socket.Close();
        }

        public void ParseMove(NetworkMessage message, Direction direction)
        {
            Location fromLocation = player.Tile.Location;
            Location toLocation = player.Tile.Location.Offset(direction);
            Tile toTile = Map.Instance.GetTile(toLocation);

            player.Tile.Creatures.Remove(player);
            toTile.Creatures.Add(player);
            player.Tile = toTile;

            // TODO: calculate new direction

            message.AddByte(0x6D);
            message.AddLocation(fromLocation);
            message.AddByte(1); // from stack location
            message.AddLocation(toLocation);
            
            if (fromLocation.Y > toLocation.Y)
            { // north, for old x
                message.AddByte(0x65);
                MapPacket.AddMapDescription(this, message, fromLocation.X - 8, toLocation.Y - 6, toLocation.Z, 18, 1);
            }
            else if (fromLocation.Y < toLocation.Y)
            { // south, for old x
                message.AddByte(0x67);
                MapPacket.AddMapDescription(this, message, fromLocation.X - 8, toLocation.Y + 7, toLocation.Z, 18, 1);
            }

            if (fromLocation.X < toLocation.X)
            { // east, [with new y]
                message.AddByte(0x66);
                MapPacket.AddMapDescription(this, message, toLocation.X + 9, toLocation.Y - 6, toLocation.Z, 1, 14);
            }
            else if (fromLocation.X > toLocation.X)
            { // west, [with new y]
                message.AddByte(0x68);
                MapPacket.AddMapDescription(this, message, toLocation.X - 8, toLocation.Y - 6, toLocation.Z, 1, 14);
            }

            message.PrepareToSend(xteaKey);
            Send(message);
        }

        public void Send(NetworkMessage message)
        {
            stream.BeginWrite(message.Buffer, 0, message.Length, null, null);
        }
    }
}
