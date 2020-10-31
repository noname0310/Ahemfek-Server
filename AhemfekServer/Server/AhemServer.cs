using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using TinyTCPServer;
using TinyTCPServer.ClientProcess;
using AhemfekServer.Model;

namespace AhemfekServer.Server
{
    public class AhemServer
    {
        public delegate void MessageHandler(string msg);
        public event MessageHandler OnMessageRecived;
        public event MessageHandler OnErrMessageRecived;

        public delegate void ClientJsonDataHandler(AhemClient client, JObject packet);
        public event ClientJsonDataHandler OnClientJsonDataRecived;
        public delegate void ClientStreamDataHandler(AhemClient client, byte[] packet);
        public event ClientStreamDataHandler OnClientStreamDataRecived;

        public delegate void ChatSocketHandler(AhemClient client);
        public event ChatSocketHandler OnClientConnected;

        public delegate void SocketHandler(ClientSocket client);
        public event SocketHandler OnClientDisConnect;
        public event SocketHandler OnClientDisConnected;

        public IChatClientManager IChatClientManager => ChatClientManager;

        private SocketServer SocketServer;
        private AhemClientManager ChatClientManager;

        public AhemServer()
        {
            SocketServer = new SocketServer(512);

            SocketServer.OnErrMessageRecived += SocketServer_OnErrMessageRecived;
            SocketServer.OnMessageRecived += SocketServer_OnMessageRecived;
            SocketServer.OnClientConnected += SocketServer_OnClientConnected;
            SocketServer.OnClientUTF8JsonDataRecived += SocketServer_OnClientJsonDataRecived;
            SocketServer.OnClientByteStreamDataRecived += SocketServer_OnClientByteStreamDataRecived;
            SocketServer.OnClientDisConnect += SocketServer_OnClientDisConnect;
            SocketServer.OnClientDisConnected += SocketServer_OnClientDisConnected;

            ChatClientManager = new AhemClientManager();
        }

        private void SocketServer_OnMessageRecived(string msg) => OnMessageRecived?.Invoke(msg);
        private void SocketServer_OnErrMessageRecived(string msg) => OnErrMessageRecived?.Invoke(msg);

        private void SocketServer_OnClientConnected(ClientSocket client) => OnMessageRecived?.Invoke(string.Format("SocketServer_OnClientConnected {0}", client.IPAddress.ToString()));

        private void SocketServer_OnClientJsonDataRecived(ClientSocket client, string msg)
        {
            OnMessageRecived?.Invoke(string.Format("SocketServer_OnClientDataRecived {0}", msg));
            JObject jObject = JObject.Parse(msg);

            //Model.PacketType packetType = (Model.PacketType)Enum.Parse(typeof(Model.PacketType), jObject.ToObject<Packet>().PacketType);
            Model.PacketType packetType = jObject.ToObject<Packet>().PacketType;

            if (packetType == Model.PacketType.ClientConnected)
            {
                if (!ChatClientManager.ReadOnlyChatClients.ContainsKey(client.IPAddress))
                {
                    AhemClient chatClient = ChatClientManager.AddClient(client, jObject.ToObject<ClientConnected>());
                    OnMessageRecived?.Invoke(string.Format("Client {0} authenticized", client.IPAddress.ToString()));
                    OnClientConnected?.Invoke(chatClient);
                }
                else
                    OnErrMessageRecived?.Invoke(string.Format("Client {0} trying authenticize mulitiple times!", client.IPAddress.ToString()));

                return;
            }

            if (ChatClientManager.ReadOnlyChatClients.ContainsKey(client.IPAddress) == false)
            {
                OnErrMessageRecived?.Invoke(string.Format("Unauthenticized or Disposed client {0} trying send data!", client.IPAddress.ToString()));
                return;
            }

            AhemClient indexedClient = ChatClientManager.ReadOnlyChatClients[client.IPAddress];

            switch (packetType)
            {
                case Model.PacketType.ClientDisConnect:
                    if (SocketServer.ClientSockets.ContainsKey(client.IPAddress))
                    {
                        indexedClient.ClientSocket.Dispose();
                        OnMessageRecived?.Invoke(string.Format("client {0} disposed", client.IPAddress.ToString()));
                    }
                    break;

                //case Model.PacketType.Message:
                //    indexedClient.OnRootMessageRecived(jObject.ToObject<Message>());
                //    OnMessageRecived?.Invoke(string.Format("Message recived from client {0}", client.IPAddress.ToString()));
                //    OnClientJsonDataRecived?.Invoke(indexedClient, jObject);
                //    break;

                default:
                    OnErrMessageRecived?.Invoke(string.Format("Unidentified packet {0} recived from client {1}", ((int)packetType).ToString(), client.IPAddress.ToString()));
                    break;
            }
        }

        private void SocketServer_OnClientByteStreamDataRecived(ClientSocket client, byte[] content)
        {
            throw new NotImplementedException();
        }

        private void SocketServer_OnClientDisConnect(ClientSocket client)
        {
            ChatClientManager.RemoveClient(client);
            OnClientDisConnect?.Invoke(client);
        }
        private void SocketServer_OnClientDisConnected(ClientSocket client)
        {
            if (ChatClientManager.ReadOnlyChatClients.ContainsKey(client.IPAddress))
                ChatClientManager.RemoveClient(client);
            OnClientDisConnected?.Invoke(client);
        }

        public void Start()
        {
            SocketServer.Start(20310);
            OnMessageRecived?.Invoke("Server started");
        }

        public void Stop()
        {
            SocketServer.Stop();
            ChatClientManager.Dispose();
            OnMessageRecived?.Invoke("Server stopped");
        }

        public void RunSyncRoutine() => SocketServer.RunSyncRoutine();

        public IEnumerator GetSyncRoutine() => SocketServer.GetSyncRoutine();
    }
}