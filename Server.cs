using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SharpOT
{
    class Server
    {
        static void Main(string[] args)
        {
            new Server().Run();
        }

        Game game;

        TcpListener clientLoginListener = new TcpListener(IPAddress.Any, 7171);
        TcpListener clientGameListener = new TcpListener(IPAddress.Any, 7172);

        List<Connection> connections = new List<Connection>();

        void Run()
        {
            game = new Game();

            LogStart("Loading map");
            game.Map.Load();
            LogDone();

            LogStart("Listening for clients");
            clientLoginListener.Start();
            clientLoginListener.BeginAcceptSocket(new AsyncCallback(LoginListenerCallback), clientLoginListener);
            clientGameListener.Start();
            clientGameListener.BeginAcceptSocket(new AsyncCallback(GameListenerCallback), clientGameListener);
            LogDone();

            Console.ReadLine();
            connections.ForEach(c => c.Close());
            clientGameListener.Stop();
            clientLoginListener.Stop();
        }

        public static void LogStart(string text)
        {
            Console.Write(DateTime.Now + ": " + text + "...");
        }

        public static void LogDone()
        {
            Console.WriteLine("Done");
        }

        public static void Log(string text)
        {
            Console.WriteLine(DateTime.Now + ": " + text);
        }

        private void LoginListenerCallback(IAsyncResult ar)
        {
            Connection connection = new Connection(game);
            connection.LoginListenerCallback(ar);
            connections.Add(connection);

            clientLoginListener.BeginAcceptSocket(new AsyncCallback(LoginListenerCallback), clientLoginListener);
            Log("New client connected to login server.");
        }

        private void GameListenerCallback(IAsyncResult ar)
        {
            Connection connection = new Connection(game);
            connection.GameListenerCallback(ar);
            connections.Add(connection);

            clientGameListener.BeginAcceptSocket(new AsyncCallback(GameListenerCallback), clientGameListener);
            Log("New client connected to game server.");
        }
    }
}
