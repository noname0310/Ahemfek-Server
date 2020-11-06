using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using AhemfekServer.Model;
using TinyTCPServer.ClientProcess;

namespace AhemfekServer.Server
{
    public interface IAhemClientManager
    {
        IReadOnlyDictionary<IPAddress, AhemClient> ReadOnlyAhemClients { get; }
    }

    class AhemClientManager : IAhemClientManager
    {
        public delegate void MessageHandler(string msg);
        //public event MessageHandler OnMessageRecived;
        public event MessageHandler OnErrMessageRecived;

        public IReadOnlyDictionary<IPAddress, AhemClient> ReadOnlyAhemClients => _ahemClients;

        private readonly Dictionary<IPAddress, AhemClient> _ahemClients;

        public AhemClientManager()
        {
            _ahemClients = new Dictionary<IPAddress, AhemClient>();
        }

        public void Dispose()
        {
            _ahemClients.Clear();
        }

        public AhemClient AddClient(ClientSocket clientSocket, ClientConnected clientConnectedinfo)
        {
            if (_ahemClients.TryGetValue(clientSocket.IPAddress, out AhemClient searchedclient))
            {
                OnErrMessageRecived?.Invoke($"ClientSocket {clientSocket.IPAddress} is already exist while trying AddClient");
                return searchedclient;
            }

            AhemClient chatClient = new AhemClient(
                clientSocket,
                clientConnectedinfo.User
                );

            _ahemClients.Add(clientSocket.IPAddress, chatClient);
            return chatClient;
        }

        public void RemoveClient(ClientSocket clientSocket)
        {
            if (_ahemClients.TryGetValue(clientSocket.IPAddress , out AhemClient client))
            {
                client.SendData(new ClientDisConnect());
                _ahemClients.Remove(client.ClientSocket.IPAddress);
            }
            else
                OnErrMessageRecived?.Invoke($"ClientSocket {clientSocket.IPAddress} is not exist while trying RemoveClient");
        }

        public void ClientStreamEnqueue(ClientSocket clientSocket, StreamHeader streamHeader) => _ahemClients[clientSocket.IPAddress].StreamEnqueue(streamHeader);

        public AhemClient ClientStreamDequeue(ClientSocket clientSocket, byte[] content)
        {
            AhemClient ahemClient = _ahemClients[clientSocket.IPAddress];
            if (ahemClient.StreamHeaderQueueCount == 0)
            {
                OnErrMessageRecived?.Invoke($"ClientSocket {clientSocket.IPAddress} Stream queue error");
                return null;
            }
            else
            {
                ahemClient.StreamDequeue(clientSocket, content);
                return ahemClient;
            }
        }
    }
}
