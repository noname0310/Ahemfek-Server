using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TinyTCPServer.ClientProcess;
using AhemfekServer.Model;
using AhemfekServer.Storage.ClientUser;

namespace AhemfekServer.Server
{
    public class AhemClient
    {
        public readonly ClientSocket ClientSocket;
        public User User { get; set; }
        public int StreamHeaderQueueCount => _streamHeaderQueue.Count;

        private Queue<StreamHeader> _streamHeaderQueue;

        public AhemClient(ClientSocket clientSocket, User user)
        {
            ClientSocket = clientSocket;
            User = user;
            _streamHeaderQueue = new Queue<StreamHeader>();
        }

        public void SendData(Packet packet)
        {
            JObject jObject = JObject.FromObject(packet);
            ClientSocket.AsyncSend(TinyTCPServer.ClientProcess.PacketType.UTF8Json, jObject.ToString());
        }
        public void StreamEnqueue(StreamHeader streamHeader) => _streamHeaderQueue.Enqueue(streamHeader);

        public void StreamDequeue(ClientSocket clientSocket, byte[] content) => OnStreamRecived(_streamHeaderQueue.Dequeue(), content);

        private void OnStreamRecived(StreamHeader streamHeader, byte[] content)
        {
            throw new NotImplementedException();
        }
    }
}
