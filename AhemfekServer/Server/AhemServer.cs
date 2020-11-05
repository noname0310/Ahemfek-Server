using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using TinyTCPServer;
using TinyTCPServer.ClientProcess;
using AhemfekServer.Model;
using AhemfekServer.Storage;
using System.Threading.Tasks;

namespace AhemfekServer.Server
{
    public class AhemServer
    {
        public delegate void MessageHandler(string msg);
        public event MessageHandler OnMessageRecived;
        public event MessageHandler OnErrMessageRecived;

        public delegate void ClientPacketHandler(AhemClient client, Packet packet);
        public event ClientPacketHandler OnClientPacketRecived;
        public delegate void ClientStreamHandler(AhemClient client, byte[] packet);
        public event ClientStreamHandler OnClientStreamRecived;

        public delegate void ChatSocketHandler(AhemClient client);
        public event ChatSocketHandler OnClientConnected;

        public delegate void SocketHandler(ClientSocket client);
        public event SocketHandler OnClientDisConnect;
        public event SocketHandler OnClientDisConnected;

        public IAhemClientManager IAhemClientManager => AhemClientManager;

        public bool Running { get; set; }

        private SocketServer SocketServer;
        private AhemClientManager AhemClientManager;
        private AhemStorage AhemStorage;
        private StorageSaver StorageSaver;

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
            AhemStorage = new AhemStorage();
            StorageSaver = new StorageSaver(AhemStorage, 10);
            AhemStorage.Load();
            Running = false;
        }

        private void SocketServer_OnMessageRecived(string msg) => OnMessageRecived?.Invoke(msg);
        private void SocketServer_OnErrMessageRecived(string msg) => OnErrMessageRecived?.Invoke(msg);

        private void SocketServer_OnClientConnected(ClientSocket client) => OnMessageRecived?.Invoke($"SocketServer_OnClientConnected {client.IPAddress}");

        private void SocketServer_OnClientJsonDataRecived(ClientSocket client, string msg)
        {
            OnMessageRecived?.Invoke($"SocketServer_OnClientDataRecived {msg}");
            JObject jObject = JObject.Parse(msg);

            //Model.PacketType packetType = (Model.PacketType)Enum.Parse(typeof(Model.PacketType), jObject.ToObject<Packet>().PacketType);
            Model.PacketType packetType = jObject.ToObject<Packet>().PacketType;

            if (packetType == Model.PacketType.ClientConnected)
            {
                if (!AhemClientManager.ReadOnlyAhemClients.ContainsKey(client.IPAddress))
                {
                    ClientConnected clientConnected = jObject.ToObject<ClientConnected>();
                    AhemClient chatClient = AhemClientManager.AddClient(client, clientConnected);
                    OnMessageRecived?.Invoke($"Client {client.IPAddress} authenticized");
                    OnClientConnected?.Invoke(chatClient);
                    OnClientPacketRecived?.Invoke(chatClient, clientConnected);
                }
                else
                    OnErrMessageRecived?.Invoke($"Client {client.IPAddress} trying authenticize mulitiple times!");

                return;
            }

            AhemClient indexedClient;
            if (AhemClientManager.ReadOnlyAhemClients.TryGetValue(client.IPAddress, out indexedClient) == false)
            {
                OnErrMessageRecived?.Invoke($"Unauthenticized or Disposed client {client.IPAddress} trying send data!");
                return;
            }

            switch (packetType)
            {
                case Model.PacketType.ClientDisConnect:
                    if (SocketServer.ClientSockets.ContainsKey(client.IPAddress))
                    {
                        indexedClient.ClientSocket.Dispose();
                        OnMessageRecived?.Invoke($"client {client.IPAddress} disposed");
                        OnClientPacketRecived?.Invoke(indexedClient, jObject.ToObject<ClientDisConnect>());
                    }
                    break;

                case Model.PacketType.StreamHeader:
                    switch (jObject.ToObject<StreamHeader>().StreamPacketType)
                    {
                        case StreamPacketType.Image:
                            ImageStream imageStream = jObject.ToObject<ImageStream>();
                            AhemClientManager.ClientStreamEnqueue(client, jObject.ToObject<ImageStream>());
                            OnClientPacketRecived?.Invoke(indexedClient, imageStream);
                            break;
                        default:
                            break;
                    }
                    break;

                default:
                    OnErrMessageRecived?.Invoke($"Unidentified packet {(int)packetType} recived from client {client.IPAddress}");
                    break;
            }
        }

        private void SocketServer_OnClientByteStreamDataRecived(ClientSocket client, byte[] content)
        {
            OnMessageRecived?.Invoke($"SocketServer_OnClientByteStreamDataRecived Length:{content.Length}");
            if (AhemClientManager.ReadOnlyAhemClients.TryGetValue(client.IPAddress, out AhemClient indexedClient) == false)
            {
                OnErrMessageRecived?.Invoke($"Unauthenticized or Disposed client {client.IPAddress} trying send data!");
                return;
            }

            AhemClient ahemClient = AhemClientManager.ClientStreamDequeue(client, content);
            if (ahemClient != null)
                OnClientStreamRecived?.Invoke(ahemClient, content);
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
            StorageSaver.Start();
            Running = true;
            OnMessageRecived?.Invoke("Server started");
        }

        public void Stop()
        {
            SocketServer.Stop();
            AhemClientManager.Dispose();
            StorageSaver.Stop();
            Running = false;
            OnMessageRecived?.Invoke("Server stopped");
        }

        public void RunSyncRoutine()
        {
            var socketRoutine = SocketServer.GetSyncRoutine();

            while (Running)
            {
                socketRoutine.MoveNext();
                if(StorageSaver.TimerQueue.TryDequeue(out Action action))
                    action.Invoke();
            }
        }

        public void RunSyncRoutine(int delay)
        {
            var socketRoutine = SocketServer.GetSyncRoutine();

            while (Running)
            {
                socketRoutine.MoveNext();
                if (StorageSaver.TimerQueue.TryDequeue(out Action action))
                    action.Invoke();
                Task.Delay(delay).Wait();
            }
        }

        public IEnumerator GetSyncRoutine()
        {
            var socketRoutine = SocketServer.GetSyncRoutine();

            while (Running)
            {
                socketRoutine.MoveNext();
                if (StorageSaver.TimerQueue.TryDequeue(out Action action))
                    action.Invoke();
                yield return 1;
            }
        }
    }
}
