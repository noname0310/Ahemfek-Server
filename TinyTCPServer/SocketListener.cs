using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TinyTCPServer
{
    class SocketListener
    {
        public delegate void MessageHandler(string msg);
        public event MessageHandler AsyncOnMessageRecived;
        public event MessageHandler AsyncOnErrMessageRecived;

        public delegate void ClientConnected(Socket ClientSocket);
        public event ClientConnected AsyncOnClientConnected;

        public Socket Listener { get; private set; }

        private ManualResetEvent ManualResetEvent;
        private bool Listening;

        public SocketListener()
        {
            ManualResetEvent = new ManualResetEvent(false);
            Listening = false;
        }

        public void Start(IPEndPoint localEndPoint)
        {
            if (Listening)
            {
                AsyncOnErrMessageRecived?.Invoke("Listener is already running!");
                return;
            }

            new Thread(() =>
            {
                Listener = new Socket(localEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    Listener.Bind(localEndPoint);
                    Listener.Listen(100);

                    Listening = true;
                    while (Listening)
                    {
                        ManualResetEvent.Reset();
                        AsyncOnMessageRecived?.Invoke("Waiting for a connection...");
                        try
                        {
                            Listener.BeginAccept(new AsyncCallback(AcceptCallback), Listener);
                        }
                        catch (ObjectDisposedException e)
                        {
                            AsyncOnErrMessageRecived?.Invoke(e.ToString());
                            continue;
                        }
                        ManualResetEvent.WaitOne();
                    }

                }
                catch (SocketException e)
                {
                    AsyncOnErrMessageRecived?.Invoke(e.ToString());
                    AsyncOnMessageRecived?.Invoke("Try listen again...");
                    Stop();
                    Start(localEndPoint);
                }
            }).Start();
        }

        public void Stop()
        {
            if (!Listening)
            {
                AsyncOnErrMessageRecived?.Invoke("Listener is not running!");
                return;
            }

            Listener.Close();
            Listener.Dispose();
            Listening = false;
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            ManualResetEvent.Set();
            try
            {
                Socket clientSocket = Listener.EndAccept(ar);
                AsyncOnClientConnected?.Invoke(clientSocket);
            }
            catch (ObjectDisposedException e)
            {
                AsyncOnErrMessageRecived?.Invoke(e.Message);
            }
        }
    }
}
