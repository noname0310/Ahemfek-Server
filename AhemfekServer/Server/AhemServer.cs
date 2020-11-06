using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using TinyTCPServer;
using TinyTCPServer.ClientProcess;
using AhemfekServer.Model;
using AhemfekServer.Storage;
using System.Threading.Tasks;
using System.Collections.Generic;
using AhemfekServer.Storage.Document;

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

        public IAhemClientManager IAhemClientManager => _ahemClientManager;

        public bool Running { get; set; }

        private SocketServer _socketServer;
        private AhemClientManager _ahemClientManager;
        private AhemStorage _ahemStorage;
        private StorageSaver _storageSaver;

        public AhemServer()
        {
            _socketServer = new SocketServer(512);

            _socketServer.OnErrMessageRecived += SocketServer_OnErrMessageRecived;
            _socketServer.OnMessageRecived += SocketServer_OnMessageRecived;
            _socketServer.OnClientConnected += SocketServer_OnClientConnected;
            _socketServer.OnClientUTF8JsonDataRecived += SocketServer_OnClientJsonDataRecived;
            _socketServer.OnClientByteStreamDataRecived += SocketServer_OnClientByteStreamDataRecived;
            _socketServer.OnClientDisConnect += SocketServer_OnClientDisConnect;
            _socketServer.OnClientDisConnected += SocketServer_OnClientDisConnected;

            _ahemClientManager = new AhemClientManager();
            _ahemStorage = new AhemStorage();
            _storageSaver = new StorageSaver(_ahemStorage, 10);
            _ahemStorage.Load();
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
                if (!_ahemClientManager.ReadOnlyAhemClients.ContainsKey(client.IPAddress))
                {
                    ClientConnected clientConnected = jObject.ToObject<ClientConnected>();
                    AhemClient chatClient = _ahemClientManager.AddClient(client, clientConnected);
                    OnMessageRecived?.Invoke($"Client {client.IPAddress} authenticized");
                    OnClientConnected?.Invoke(chatClient);
                    OnClientPacketRecived?.Invoke(chatClient, clientConnected);
                }
                else
                    OnErrMessageRecived?.Invoke($"Client {client.IPAddress} trying authenticize mulitiple times!");

                return;
            }

            AhemClient indexedClient;
            if (_ahemClientManager.ReadOnlyAhemClients.TryGetValue(client.IPAddress, out indexedClient) == false)
            {
                OnErrMessageRecived?.Invoke($"Unauthenticized or Disposed client {client.IPAddress} trying send data!");
                return;
            }

            switch (packetType)
            {
                case Model.PacketType.ClientDisConnect:
                    if (_socketServer.ClientSockets.ContainsKey(client.IPAddress))
                    {
                        indexedClient.ClientSocket.Dispose();
                        OnMessageRecived?.Invoke($"client {client.IPAddress} disposed");
                        OnClientPacketRecived?.Invoke(indexedClient, jObject.ToObject<ClientDisConnect>());
                    }
                    break;

                case Model.PacketType.UploadDocument:
                    _ahemStorage.TryAddDoc(jObject.ToObject<UploadDocument>().Doc);
                    break;
                case Model.PacketType.RemoveDocument:
                    RemoveDocument removeDocument = jObject.ToObject<RemoveDocument>();
                    _ahemStorage.RemoveDoc(removeDocument.Theme, indexedClient?.User.Id, removeDocument.DocName);
                    break;
                case Model.PacketType.UploadPrivateDocument:
                    UploadPrivateDocument uploadPrivateDocument = jObject.ToObject<UploadPrivateDocument>();
                    _ahemStorage.TryAddPrivateDoc(uploadPrivateDocument.Doc);
                    break;
                case Model.PacketType.RemovePrivateDocument:
                    RemovePrivateDocument removePrivateDocument = jObject.ToObject<RemovePrivateDocument>();
                    _ahemStorage.RemovePrivateDoc(indexedClient.User.Id, removePrivateDocument.DocName);
                    break;
                case Model.PacketType.Like:
                    Like like = jObject.ToObject<Like>();
                    _ahemStorage.TryLike(like.Theme, like.DocName);
                    break;
                case Model.PacketType.Unlike:
                    UnLike unlike = jObject.ToObject<UnLike>();
                    _ahemStorage.TryUnlike(unlike.Theme, unlike.DocName);
                    break;
                case Model.PacketType.Follow:
                    Follow follow = jObject.ToObject<Follow>();
                    _ahemStorage.AddFollower(follow.Target.Id, indexedClient.User.Id);
                    break;
                case Model.PacketType.UnFollow:
                    UnFollow unFollow = jObject.ToObject<UnFollow>();
                    _ahemStorage.RemoveFollower(unFollow.Target.Id, indexedClient.User.Id);
                    break;
                case Model.PacketType.ReqPageData:
                    ReqPageData reqPageData = jObject.ToObject<ReqPageData>();
                    List<DocThumbnail> publicDocThumbs = _ahemStorage.GetPublicDocThumb(reqPageData.Theme, reqPageData.StartIndex, reqPageData.Count, reqPageData.DocOrder);
                    indexedClient.SendData(new PageData(publicDocThumbs));
                    break;
                case Model.PacketType.ReqPrivatePageData:
                    ReqPrivatePageData reqPrivatePageData = jObject.ToObject<ReqPrivatePageData>();
                    List<DocThumbnail> privateDocThumbs = _ahemStorage.GetPrivateDocThumb(indexedClient.User.Id, reqPrivatePageData.StartIndex, reqPrivatePageData.Count);
                    indexedClient.SendData(new PrivatePageData(privateDocThumbs));
                    break;
                case Model.PacketType.ReqDoc:
                    ReqDoc reqDoc = jObject.ToObject<ReqDoc>();
                    indexedClient.SendData(new PDoc(_ahemStorage.GetUserDoc(reqDoc.Author.Id, reqDoc.DocName)));
                    break;
                case Model.PacketType.ReqThemes:
                    indexedClient.SendData(new Themes(_ahemStorage.GetTheme()));
                    break;

                case Model.PacketType.StreamHeader:
                    switch (jObject.ToObject<StreamHeader>().StreamPacketType)
                    {
                        case StreamPacketType.Image:
                            ImageStream imageStream = jObject.ToObject<ImageStream>();
                            _ahemClientManager.ClientStreamEnqueue(client, jObject.ToObject<ImageStream>());
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
            if (_ahemClientManager.ReadOnlyAhemClients.TryGetValue(client.IPAddress, out AhemClient indexedClient) == false)
            {
                OnErrMessageRecived?.Invoke($"Unauthenticized or Disposed client {client.IPAddress} trying send data!");
                return;
            }

            AhemClient ahemClient = _ahemClientManager.ClientStreamDequeue(client, content);
            if (ahemClient != null)
                OnClientStreamRecived?.Invoke(ahemClient, content);
        }

        private void SocketServer_OnClientDisConnect(ClientSocket client)
        {
            _ahemClientManager.RemoveClient(client);
            OnClientDisConnect?.Invoke(client);
        }
        private void SocketServer_OnClientDisConnected(ClientSocket client)
        {
            if (_ahemClientManager.ReadOnlyAhemClients.ContainsKey(client.IPAddress))
                _ahemClientManager.RemoveClient(client);
            OnClientDisConnected?.Invoke(client);
        }

        public void Start()
        {
            _socketServer.Start(20310);
            _storageSaver.Start();
            Running = true;
            OnMessageRecived?.Invoke("Server started");
        }

        public void Stop()
        {
            _socketServer.Stop();
            _ahemClientManager.Dispose();
            _storageSaver.Stop();
            Running = false;
            OnMessageRecived?.Invoke("Server stopped");
        }

        public void RunSyncRoutine()
        {
            var socketRoutine = _socketServer.GetSyncRoutine();

            while (Running)
            {
                socketRoutine.MoveNext();
                if(_storageSaver.TimerQueue.TryDequeue(out Action action))
                    action.Invoke();
            }
        }

        public void RunSyncRoutine(int delay)
        {
            var socketRoutine = _socketServer.GetSyncRoutine();

            while (Running)
            {
                socketRoutine.MoveNext();
                if (_storageSaver.TimerQueue.TryDequeue(out Action action))
                    action.Invoke();
                Task.Delay(delay).Wait();
            }
        }

        public IEnumerator GetSyncRoutine()
        {
            var socketRoutine = _socketServer.GetSyncRoutine();

            while (Running)
            {
                socketRoutine.MoveNext();
                if (_storageSaver.TimerQueue.TryDequeue(out Action action))
                    action.Invoke();
                yield return 1;
            }
        }
    }
}
