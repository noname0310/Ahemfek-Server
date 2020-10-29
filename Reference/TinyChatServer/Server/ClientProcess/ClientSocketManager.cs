using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TinyChatServer.Server.ClientProcess
{
    class ClientSocketManager
    {
        public delegate void MessageHandler(string msg);
        public event MessageHandler AsyncOnMessageRecived;
        public event MessageHandler AsyncOnErrMessageRecived;

        public delegate void SocketHandler(ClientSocket client);
        public event SocketHandler AsyncOnClientDisConnect;
        public event SocketHandler AsyncOnClientDisConnected;

        public delegate void ClientDataHandler(ClientSocket client, string content);
        public event ClientDataHandler AsyncOnClientDataRecived;

        public IReadOnlyDictionary<IPAddress, ClientSocket> ReadOnlyClientSockets;

        private readonly uint PacketSize;
        private Dictionary<IPAddress, ClientSocket> ClientSockets;
        private object ClientSocketManagerLickObject;

        public ClientSocketManager(uint packetsize)
        {
            PacketSize = packetsize;
            ClientSockets = new Dictionary<IPAddress, ClientSocket>();
            ReadOnlyClientSockets = ClientSockets;
            ClientSocketManagerLickObject = new object();
        }

        public void Dispose()
        {
            Dictionary<IPAddress, ClientSocket> clonelist = new Dictionary<IPAddress, ClientSocket>(ClientSockets);
            foreach (var item in clonelist)
            {
                item.Value.Dispose();
            }
        }

        public ClientSocket ClientProcess(Socket Client)
        {
            ClientSocket clientSocket = new ClientSocket(PacketSize, Client);

            lock (ClientSocketManagerLickObject)
            {
                IPAddress iPAddress = (Client.RemoteEndPoint as IPEndPoint).Address;
                if (ClientSockets.ContainsKey(iPAddress))
                {
                    ClientSockets[iPAddress].Dispose();
                }
                ClientSockets.Add((Client.RemoteEndPoint as IPEndPoint).Address, clientSocket);
            }

            clientSocket.AsyncOnMessageRecived += ClientSocket_AsyncOnMessageRecived;
            clientSocket.AsyncOnErrMessageRecived += ClientSocket_AsyncOnErrMessageRecived;
            clientSocket.AsyncOnClientDisConnect += ClientSocket_AsyncOnClientDisConnect;
            clientSocket.AsyncOnClientDisConnected += ClientSocket_AsyncOnClientDisConnected;
            clientSocket.AsyncOnClientDataRecived += ClientSocket_AsyncOnClientDataRecived;
            clientSocket.StartProcess();
            return clientSocket;
        }

        private void ClientSocket_AsyncOnErrMessageRecived(string msg) => AsyncOnErrMessageRecived?.Invoke(msg);
        private void ClientSocket_AsyncOnMessageRecived(string msg) => AsyncOnMessageRecived?.Invoke(msg);
        private void ClientSocket_AsyncOnClientDisConnect(ClientSocket client) => AsyncOnClientDisConnect?.Invoke(client);
        private void ClientSocket_AsyncOnClientDisConnected(ClientSocket client)
        {
            client.AsyncOnMessageRecived -= ClientSocket_AsyncOnMessageRecived;
            client.AsyncOnErrMessageRecived -= ClientSocket_AsyncOnErrMessageRecived;
            client.AsyncOnClientDisConnect -= ClientSocket_AsyncOnClientDisConnect;
            client.AsyncOnClientDisConnected -= ClientSocket_AsyncOnClientDisConnected;
            client.AsyncOnClientDataRecived -= ClientSocket_AsyncOnClientDataRecived;
            AsyncOnClientDisConnected?.Invoke(client);

            lock (ClientSocketManagerLickObject)
                ClientSockets.Remove(client.IPAddress);
        }

        private void ClientSocket_AsyncOnClientDataRecived(ClientSocket clientSocket, byte[] ReceivedData, ParseResult parseResult)
        {
            string content;

            if (parseResult.ContentLength == 0)
                content = "";
            else
            {
                byte[] contentByte = new byte[parseResult.ContentLength];
                Buffer.BlockCopy(ReceivedData, sizeof(int), contentByte, 0, contentByte.Length);
                content = Encoding.UTF8.GetString(contentByte);
            }

            AsyncOnClientDataRecived?.Invoke(clientSocket, content);
        }
    }
}