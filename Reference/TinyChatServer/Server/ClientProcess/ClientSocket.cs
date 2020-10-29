using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

namespace TinyChatServer.Server.ClientProcess
{
    public class ClientSocket : IEquatable<ClientSocket>
    {
        public delegate void MessageHandler(string msg);
        public event MessageHandler AsyncOnMessageRecived;
        public event MessageHandler AsyncOnErrMessageRecived;

        public delegate void SocketHandler(ClientSocket client);
        public event SocketHandler AsyncOnClientDisConnect;
        public event SocketHandler AsyncOnClientDisConnected;

        public delegate void ClientDataRecived(ClientSocket clientSocket, byte[] receivedData, ParseResult parseResult);
        public event ClientDataRecived AsyncOnClientDataRecived;

        public readonly IPAddress IPAddress;

        private readonly uint PacketSize;
        private byte[] Buffer;

        private readonly Socket Client;

        private bool HeaderParsed;
        private byte[] ContentFullBuffer;
        private int LeftContentByte;
        private int ContentOffSet;
        private ParseResult parseResult;

        public ClientSocket(uint packetSize, Socket client)
        {
            PacketSize = packetSize;
            Client = client;
            IPAddress = (Client.RemoteEndPoint as IPEndPoint).Address;
            HeaderParsed = false;
        }

        public void StartProcess()
        {
            Buffer = new byte[PacketSize];
            Process(sizeof(int));
        }

        private void Process(int ReceiveLength)
        {
            for (int i = 0; i < Buffer.Length; i++)
                Buffer[i] = 0;

            Client.BeginReceive(Buffer, 0, (Buffer.Length < ReceiveLength) ? Buffer.Length : ReceiveLength, SocketFlags.None, Receivecallback, Client);
        }

        private void Receivecallback(IAsyncResult ar)
        {
            try
            {
                int receivedByte = Client.EndReceive(ar);
                if (receivedByte == 0)
                {
                    AsyncOnMessageRecived?.Invoke("Error Handled, Client force disconnected");
                    Dispose();
                }

                if (!HeaderParsed)
                {
                    parseResult = PacketParser.HeaderParse(Buffer, (uint)receivedByte);

                    if (parseResult.HeaderParsed == false)
                    {
                        Process(sizeof(int) - receivedByte);
                    }
                    else if (parseResult.ContentLength == parseResult.ReceivedContentLength)
                    {
                        byte[] bufferClone = new byte[Buffer.Length];
                        System.Buffer.BlockCopy(Buffer, 0, bufferClone, 0, sizeof(int) + parseResult.ContentLength);

                        HeaderParsed = false;
                        AsyncOnClientDataRecived?.Invoke(this, bufferClone, parseResult);
                        Process(sizeof(int));
                    }
                    else
                    {
                        LeftContentByte = parseResult.ContentLength - parseResult.ReceivedContentLength;
                        ContentOffSet = sizeof(int) + parseResult.ReceivedContentLength;

                        ContentFullBuffer = new byte[sizeof(int) + parseResult.ContentLength];
                        System.Buffer.BlockCopy(Buffer, 0, ContentFullBuffer, 0, receivedByte);

                        HeaderParsed = true;
                        Process(LeftContentByte);
                    }
                }
                else
                {
                    System.Buffer.BlockCopy(Buffer, 0, ContentFullBuffer, ContentOffSet, receivedByte);

                    LeftContentByte -= receivedByte;
                    ContentOffSet += receivedByte;

                    if (LeftContentByte <= 0)
                    {
                        byte[] bufferClone = new byte[ContentFullBuffer.Length];
                        System.Buffer.BlockCopy(ContentFullBuffer, 0, bufferClone, 0, sizeof(int) + parseResult.ContentLength);

                        HeaderParsed = false;
                        AsyncOnClientDataRecived?.Invoke(this, bufferClone, parseResult);
                        Process(sizeof(int));
                    }
                    else
                        Process(LeftContentByte);
                }
            }
            catch(SocketException e)
            {
                AsyncOnErrMessageRecived?.Invoke(e.Message);
                AsyncOnMessageRecived?.Invoke("Error Handled, Client force disconnected");
                Dispose();
            }
            catch(ObjectDisposedException e)
            {
                AsyncOnErrMessageRecived?.Invoke(e.Message);
                AsyncOnMessageRecived?.Invoke("Error Handled, Client force disconnected");
                Dispose();
            }
        }

        public void AsyncSend(string content)
        {
            byte[] contentBuffer = Encoding.UTF8.GetBytes(content);
            byte[] header = BitConverter.GetBytes(contentBuffer.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(header);
            byte[] sendBuffer = new byte[header.Length + contentBuffer.Length];
            System.Buffer.BlockCopy(header, 0, sendBuffer, 0, header.Length);
            System.Buffer.BlockCopy(contentBuffer, 0, sendBuffer, header.Length, contentBuffer.Length);

            if (Client.Connected)
                Client.BeginSend(sendBuffer, 0, sendBuffer.Length, 0, new AsyncCallback(SendCallback), Client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                int bytesSent = Client.EndSend(ar);
                AsyncOnMessageRecived?.Invoke(string.Format("Sent {0} bytes to client {0}.", bytesSent, IPAddress));
            }
            catch (Exception e)
            {
                AsyncOnErrMessageRecived?.Invoke(e.ToString());
            }
        }

        public void Dispose()
        {
            AsyncOnClientDisConnect?.Invoke(this);
            if (Client.Connected)
                Client.Shutdown(SocketShutdown.Both);
            Client.Close();
            Client.Dispose();
            AsyncOnClientDisConnected?.Invoke(this);
        }

        public bool Equals(ClientSocket other)
        {
            return EqualityComparer<IPAddress>.Default.Equals(IPAddress, other.IPAddress) &&
                   EqualityComparer<Socket>.Default.Equals(Client, other.Client);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientSocket socket &&
                   Equals(socket);
        }

        public override int GetHashCode()
        {
            int hashCode = 936257235;
            hashCode = hashCode * -1521134295 + EqualityComparer<IPAddress>.Default.GetHashCode(IPAddress);
            hashCode = hashCode * -1521134295 + EqualityComparer<Socket>.Default.GetHashCode(Client);
            return hashCode;
        }

        public static bool operator ==(ClientSocket left, ClientSocket right)
        {
            return EqualityComparer<ClientSocket>.Default.Equals(left, right);
        }

        public static bool operator !=(ClientSocket left, ClientSocket right)
        {
            return !(left == right);
        }
    }
}