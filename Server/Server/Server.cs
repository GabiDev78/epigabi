using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Server
    {

        public static int MaxPlayers { get; private set; }

        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>(); 
        public static TcpListener tcpListener;

        public static void Start(int _MaxP, int _Port)
        {
            MaxPlayers = _MaxP;
            Port = _Port;
            
            Console.WriteLine("Starting server.");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();

            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started on {Port}");


        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Receiving connection from {_client.Client.RemoteEndPoint}!");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect : Server full");

        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }
        }
    }
}
