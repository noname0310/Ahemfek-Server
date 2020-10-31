using System.Collections.Generic;
using AhemfekServer.Model;
using TinyTCPServer.ClientProcess;
using Newtonsoft.Json.Linq;

namespace AhemfekServer.Server
{
    public class AhemClient
    {
        public readonly ClientSocket ClientSocket;

        public string Id { get; set; }

        public AhemClient(ClientSocket clientSocket, string id)
        {
            ClientSocket = clientSocket;
            Id = id;
        }

        public void SendData(Packet packet)
        {
            JObject jObject = JObject.FromObject(packet);
            ClientSocket.AsyncSend(TinyTCPServer.ClientProcess.PacketType.UTF8Json, jObject.ToString());
        }
    }
}
