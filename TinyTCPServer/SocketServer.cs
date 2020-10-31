using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using TinyTCPServer.ClientProcess;

namespace TinyTCPServer
{
    public class SocketServer
    {
        public delegate void MessageHandler(string msg);
        public event MessageHandler OnMessageRecived;
        public event MessageHandler OnErrMessageRecived;

        public delegate void ClientStringDataHandler(ClientSocket client, string content);
        public event ClientStringDataHandler OnClientUTF8JsonDataRecived;
        public delegate void ClientByteDataHandler(ClientSocket client, byte[] content);
        public event ClientByteDataHandler OnClientByteStreamDataRecived;

        public delegate void SocketHandler(ClientSocket client);
        public event SocketHandler OnClientConnected;
        public event SocketHandler OnClientDisConnect;
        public event SocketHandler OnClientDisConnected;

        public IReadOnlyDictionary<IPAddress, ClientSocket> ClientSockets;

        private ConcurrentQueue<Action> ActionsConcurrentQueue;

        private SocketListener SocketListener;
        private ClientSocketManager ClientSocketManager;
        private bool Running;

        public SocketServer(uint packetsize)
        {
            ActionsConcurrentQueue = new ConcurrentQueue<Action>();

            ClientSocketManager = new ClientSocketManager(packetsize);
            ClientSocketManager.AsyncOnMessageRecived += ClientSocketManager_AsyncOnMessageRecived;
            ClientSocketManager.AsyncOnErrMessageRecived += ClientSocketManager_AsyncOnErrMessageRecived;
            ClientSocketManager.AsyncOnClientUTF8JsonDataRecived += ClientSocketManager_AsyncOnClientUTF8JsonDataRecived;
            ClientSocketManager.AsyncOnClientByteStreamDataRecived += ClientSocketManager_AsyncOnClientByteStreamDataRecived;
            ClientSocketManager.AsyncOnClientDisConnect += ClientSocketManager_AsyncOnClientDisConnect;
            ClientSocketManager.AsyncOnClientDisConnected += ClientSocketManager_AsyncOnClientDisConnected;

            ClientSockets = ClientSocketManager.ReadOnlyClientSockets;

            SocketListener = new SocketListener();
            SocketListener.AsyncOnMessageRecived += SocketListener_AsyncOnMessageRecived;
            SocketListener.AsyncOnErrMessageRecived += SocketListener_AsyncOnErrMessageRecived;
            SocketListener.AsyncOnClientConnected += SocketListener_AsyncOnClientConnected;
            Running = false;
        }

        private void ClientSocketManager_AsyncOnMessageRecived(string msg) => 
            ActionsConcurrentQueue.Enqueue(() => OnMessageRecived?.Invoke(msg));
        private void ClientSocketManager_AsyncOnErrMessageRecived(string msg) => 
            ActionsConcurrentQueue.Enqueue(() => OnErrMessageRecived?.Invoke(msg));
        private void ClientSocketManager_AsyncOnClientUTF8JsonDataRecived(ClientSocket client, string content) => 
            ActionsConcurrentQueue.Enqueue(() => OnClientUTF8JsonDataRecived?.Invoke(client, content));
        private void ClientSocketManager_AsyncOnClientByteStreamDataRecived(ClientSocket client, byte[] content) =>
            ActionsConcurrentQueue.Enqueue(() => OnClientByteStreamDataRecived?.Invoke(client, content));
        private void ClientSocketManager_AsyncOnClientDisConnect(ClientSocket client) => 
            ActionsConcurrentQueue.Enqueue(() => OnClientDisConnect?.Invoke(client));
        private void ClientSocketManager_AsyncOnClientDisConnected(ClientSocket client) =>
            ActionsConcurrentQueue.Enqueue(() => OnClientDisConnected?.Invoke(client));

        private void SocketListener_AsyncOnMessageRecived(string msg) =>
            ActionsConcurrentQueue.Enqueue(() => OnMessageRecived?.Invoke(msg));
        private void SocketListener_AsyncOnErrMessageRecived(string msg) =>
            ActionsConcurrentQueue.Enqueue(() => OnErrMessageRecived?.Invoke(msg));
        private void SocketListener_AsyncOnClientConnected(Socket ClientSocket)
        {
            ClientSocket clientSocket = ClientSocketManager.ClientProcess(ClientSocket);
            ActionsConcurrentQueue.Enqueue(() => OnClientConnected?.Invoke(clientSocket));
        }

        public void Start(IPEndPoint iPEndPoint)
        {
            if (Running == true)
            {
                OnErrMessageRecived?.Invoke("Server is already running");
                return;
            }
            Running = true;
            SocketListener.Start(iPEndPoint);
        }
        public void Start(int port)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            Start(new IPEndPoint(ipAddress, port));
        }

        public void Stop()
        {
            if (Running == false)
            {
                OnErrMessageRecived?.Invoke("Server is not running");
                return;
            }

            SocketListener.Stop();
            ClientSocketManager.Dispose();
            Running = false;
            Thread.Sleep(1000);
        }

        public void RunSyncRoutine()
        {
            IEnumerator enumerator = GetSyncRoutine();
            while (true)
            {
                enumerator.MoveNext();

                if ((int)enumerator.Current == 0)
                    break;
            }
        }

        public IEnumerator GetSyncRoutine()
        {
            while (Running)
            {
                while (ActionsConcurrentQueue.Count != 0)
                {
                    Action action;
                    if (ActionsConcurrentQueue.TryDequeue(out action))
                        action.Invoke();
                }
                yield return 1;
            }

            yield return 0;
        }
    }
}
