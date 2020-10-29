using System.Collections.Generic;
using TinyChatServer.Model;
using TinyChatServer.Server.ClientProcess;
using Newtonsoft.Json.Linq;

namespace TinyChatServer.ChatServer
{
    public class ChatClient
    {
        public delegate void GPSUpdateHandler(ChatClient chatClient);
        public event GPSUpdateHandler OnGPSUpdated;
        
        public readonly ClientSocket ClientSocket;
        public readonly string UserEmail;

        public string Id { get; set; }
        public string Name { get; set; }
        public GPSdata GPSdata {
            get 
            {
                return gPSdata;
            } 
            set 
            {
                gPSdata = value;
                OnGPSUpdated?.Invoke(this);
            } 
        }
        public GPSdata gPSdata;
        public List<ChatClient> LinkedClients { get; private set; }

        private Message PrevMessage;

        public ChatClient(ClientSocket clientSocket, string userEmail, string id, string name, GPSdata gPSdata)
        {
            ClientSocket = clientSocket;
            UserEmail = userEmail;

            Id = id;
            Name = name;
            GPSdata = gPSdata;
            LinkedClients = new List<ChatClient>();
        }

        public void OnRootMessageRecived(Message message)
        {
            //SendData(message);

            foreach (var item in LinkedClients)
            {
                item.OnClientMessageRecived(message);
            }
        }

        public void OnClientMessageRecived(Message message)
        {
            if (UserEmail == message.ChatClient.UserEmail)
                return;

            if (PrevMessage == message)
                return;

            PrevMessage = message;

            SendData(message);

            foreach (var item in LinkedClients)
            {
                item.OnClientMessageRecived(message);
            }
        }

        public void SendData(Packet packet)
        {
            JObject jObject = JObject.FromObject(packet);
            ClientSocket.AsyncSend(jObject.ToString());
        }
    }

    public struct GPSdata
    {
        public double Longitude;
        public double Latitude;

        public GPSdata(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public GPSdata(Model.GPSdata gPSdata)
        {
            Longitude = gPSdata.Longitude;
            Latitude = gPSdata.Latitude;
        }
    }
}