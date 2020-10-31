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
        private readonly Dictionary<IPAddress, int> _clientStreamQueue;

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
                OnErrMessageRecived?.Invoke(
                       string.Format("ClientSocket {0} is already exist while trying AddClient", clientSocket.IPAddress.ToString())
                       );
                return searchedclient;
            }

            AhemClient chatClient = new AhemClient(
                clientSocket,
                clientConnectedinfo.ClientId
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
            {
                OnErrMessageRecived?.Invoke(
                    string.Format("ClientSocket {0} is not exist while trying RemoveClient", clientSocket.IPAddress.ToString())
                    );
            }
        }

        public void ClientStreamEnqueue(ClientSocket clientSocket, StreamHeader streamHeader)
        {
            if (_ahemClients.TryGetValue(clientSocket.IPAddress, out AhemClient client))
            {

            }
        }

        public void ClientStreamDequeue(ClientSocket clientSocket, byte[] value)
        {
            if (_ahemClients.TryGetValue(clientSocket.IPAddress, out AhemClient client))
            {

            }
        }
    }
}
