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

        TcpListener clientLoginListener = new TcpListener(IPAddress.Any, 7171);
        TcpListener clientGameListener = new TcpListener(IPAddress.Any, 7172);

        List<Connection> connections = new List<Connection>();

        void Run()
        {
            Start("Loading map");
            Map.Instance.Load();
            Done();

            Start("Listening for clients");
            clientLoginListener.Start();
            clientLoginListener.BeginAcceptSocket(new AsyncCallback(LoginListenerCallback), clientLoginListener);
            clientGameListener.Start();
            clientGameListener.BeginAcceptSocket(new AsyncCallback(GameListenerCallback), clientGameListener);
            Done();

            Console.ReadLine();
            connections.ForEach(c => c.Close());
            clientGameListener.Stop();
            clientLoginListener.Stop();
        }

        void Start(string text)
        {
            Console.Write(DateTime.Now + ": " + text + "...");
        }

        void Done()
        {
            Console.WriteLine("Done");
        }

        void Log(string text)
        {
            Console.WriteLine(DateTime.Now + ": " + text);
        }

        private void LoginListenerCallback(IAsyncResult ar)
        {
            Connection connection = new Connection();
            connection.LoginListenerCallback(ar);
            connections.Add(connection);

            clientLoginListener.BeginAcceptSocket(new AsyncCallback(LoginListenerCallback), clientLoginListener);
            Log("New client connected to login server.");
        }

        private void GameListenerCallback(IAsyncResult ar)
        {
            Connection connection = new Connection();
            connection.GameListenerCallback(ar);
            connections.Add(connection);

            clientGameListener.BeginAcceptSocket(new AsyncCallback(GameListenerCallback), clientGameListener);
            Log("New client connected to game server.");
        }
    }
}
