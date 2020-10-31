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

        public IAhemClientManager IAhemClientManager => AhemClientManager;

        private SocketServer SocketServer;
        private AhemClientManager AhemClientManager;

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

            AhemClientManager = new AhemClientManager();
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
                if (!AhemClientManager.ReadOnlyAhemClients.ContainsKey(client.IPAddress))
                {
                    AhemClient chatClient = AhemClientManager.AddClient(client, jObject.ToObject<ClientConnected>());
                    OnMessageRecived?.Invoke(string.Format("Client {0} authenticized", client.IPAddress.ToString()));
                    OnClientConnected?.Invoke(chatClient);
                }
                else
                    OnErrMessageRecived?.Invoke(string.Format("Client {0} trying authenticize mulitiple times!", client.IPAddress.ToString()));

                return;
            }

            if (AhemClientManager.ReadOnlyAhemClients.ContainsKey(client.IPAddress) == false)
            {
                OnErrMessageRecived?.Invoke(string.Format("Unauthenticized or Disposed client {0} trying send data!", client.IPAddress.ToString()));
                return;
            }

            AhemClient indexedClient = AhemClientManager.ReadOnlyAhemClients[client.IPAddress];

            switch (packetType)
            {
                case Model.PacketType.ClientDisConnect:
                    if (SocketServer.ClientSockets.ContainsKey(client.IPAddress))
                    {
                        indexedClient.ClientSocket.Dispose();
                        OnMessageRecived?.Invoke(string.Format("client {0} disposed", client.IPAddress.ToString()));
                    }
                    break;

                case Model.PacketType.StreamHeader:
                    switch (jObject.ToObject<StreamHeader>().StreamPacketType)
                    {
                        case StreamPacketType.Image:
                            AhemClientManager.ClientStreamEnqueue(client, jObject.ToObject<ImageStream>());
                            break;
                        default:
                            break;
                    }
                    break;

                default:
                    OnErrMessageRecived?.Invoke(string.Format("Unidentified packet {0} recived from client {1}", ((int)packetType).ToString(), client.IPAddress.ToString()));
                    break;
            }
        }

        private void SocketServer_OnClientByteStreamDataRecived(ClientSocket client, byte[] content)
        {
            OnMessageRecived?.Invoke($"SocketServer_OnClientByteStreamDataRecived Length:{content.Length}");
            AhemClientManager.ClientStreamDequeue(client, content);
        }

        private void SocketServer_OnClientDisConnect(ClientSocket client)
        {
            AhemClientManager.RemoveClient(client);
            OnClientDisConnect?.Invoke(client);
        }
        private void SocketServer_OnClientDisConnected(ClientSocket client)
        {
            if (AhemClientManager.ReadOnlyAhemClients.ContainsKey(client.IPAddress))
                AhemClientManager.RemoveClient(client);
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
            AhemClientManager.Dispose();
            OnMessageRecived?.Invoke("Server stopped");
        }

        public void RunSyncRoutine() => SocketServer.RunSyncRoutine();

        public void RunSyncRoutine(int delay) => SocketServer.RunSyncRoutine(delay);

        public IEnumerator GetSyncRoutine() => SocketServer.GetSyncRoutine();
    }
}
