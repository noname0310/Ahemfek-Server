using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

namespace TinyTCPServer.ClientProcess
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

        private readonly uint _packetSize;
        private byte[] _buffer;

        private readonly Socket _client;

        private bool _headerParsed;
        private byte[] _contentFullBuffer;
        private int _leftContentByte;
        private int _contentOffSet;
        private ParseResult _parseResult;

        public ClientSocket(uint packetSize, Socket client)
        {
            _packetSize = packetSize;
            _client = client;
            IPAddress = (_client.RemoteEndPoint as IPEndPoint).Address;
            _headerParsed = false;
        }

        public void StartProcess()
        {
            _buffer = new byte[_packetSize];
            Process(PacketParser.HeaderSize);
        }

        private void Process(int ReceiveLength)
        {
            for (int i = 0; i < _buffer.Length; i++)
                _buffer[i] = 0;

            _client.BeginReceive(_buffer, 0, (_buffer.Length < ReceiveLength) ? _buffer.Length : ReceiveLength, SocketFlags.None, Receivecallback, _client);
        }

        private void Receivecallback(IAsyncResult ar)
        {
            try
            {
                int receivedByte = _client.EndReceive(ar);
                if (receivedByte == 0)
                {
                    AsyncOnMessageRecived?.Invoke("Error Handled, Client force disconnected");
                    Dispose();
                }

                if (!_headerParsed)
                {
                    _parseResult = PacketParser.HeaderParse(_buffer, (uint)receivedByte);

                    if (_parseResult.HeaderParsed == false)
                    {
                        Process(PacketParser.HeaderSize - receivedByte);
                    }
                    else if (_parseResult.ContentLength == _parseResult.ReceivedContentLength)
                    {
                        byte[] bufferClone = new byte[_buffer.Length];
                        Buffer.BlockCopy(_buffer, 0, bufferClone, 0, PacketParser.HeaderSize + _parseResult.ContentLength);

                        _headerParsed = false;
                        AsyncOnClientDataRecived?.Invoke(this, bufferClone, _parseResult);
                        Process(PacketParser.HeaderSize);
                    }
                    else
                    {
                        _leftContentByte = _parseResult.ContentLength - _parseResult.ReceivedContentLength;
                        _contentOffSet = PacketParser.HeaderSize + _parseResult.ReceivedContentLength;

                        _contentFullBuffer = new byte[PacketParser.HeaderSize + _parseResult.ContentLength];
                        Buffer.BlockCopy(_buffer, 0, _contentFullBuffer, 0, receivedByte);

                        _headerParsed = true;
                        Process(_leftContentByte);
                    }
                }
                else
                {
                    Buffer.BlockCopy(_buffer, 0, _contentFullBuffer, _contentOffSet, receivedByte);

                    _leftContentByte -= receivedByte;
                    _contentOffSet += receivedByte;

                    if (_leftContentByte <= 0)
                    {
                        byte[] bufferClone = new byte[_contentFullBuffer.Length];
                        Buffer.BlockCopy(_contentFullBuffer, 0, bufferClone, 0, PacketParser.HeaderSize + _parseResult.ContentLength);

                        _headerParsed = false;
                        AsyncOnClientDataRecived?.Invoke(this, bufferClone, _parseResult);
                        Process(PacketParser.HeaderSize);
                    }
                    else
                        Process(_leftContentByte);
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

        public void AsyncSend(PacketType packetType, string content)
        {
            byte[] contentBuffer = Encoding.UTF8.GetBytes(content);
            AsyncSend(packetType, contentBuffer);
        }

        public void AsyncSend(PacketType packetType, byte[] content)
        {
            byte[] header = PacketParser.CreateHeader(packetType, content.Length);
            byte[] sendBuffer = new byte[header.Length + content.Length];
            Buffer.BlockCopy(header, 0, sendBuffer, 0, header.Length);
            Buffer.BlockCopy(content, 0, sendBuffer, header.Length, content.Length);

            if (_client.Connected)
                _client.BeginSend(sendBuffer, 0, sendBuffer.Length, 0, new AsyncCallback(SendCallback), _client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                int bytesSent = _client.EndSend(ar);
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
            if (_client.Connected)
                _client.Shutdown(SocketShutdown.Both);
            _client.Close();
            _client.Dispose();
            AsyncOnClientDisConnected?.Invoke(this);
        }

        public bool Equals(ClientSocket other)
        {
            return EqualityComparer<IPAddress>.Default.Equals(IPAddress, other.IPAddress) &&
                   EqualityComparer<Socket>.Default.Equals(_client, other._client);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientSocket socket &&
                   Equals(socket);
        }

        public override int GetHashCode() => HashCode.Combine(IPAddress, _client);

        public static bool operator ==(ClientSocket left, ClientSocket right) => EqualityComparer<ClientSocket>.Default.Equals(left, right);

        public static bool operator !=(ClientSocket left, ClientSocket right) => !(left == right);
    }
}
