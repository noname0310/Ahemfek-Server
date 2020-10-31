using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using AhemfekServer.Model;
using TinyTCPServer.ClientProcess;

namespace AhemfekServer.Server
{
    public interface IChatClientManager
    {
        IReadOnlyDictionary<IPAddress, AhemClient> ReadOnlyChatClients { get; set; }
        int SearchRange { get; set; }
    }

    class AhemClientManager : IChatClientManager
    {
        public delegate void MessageHandler(string msg);
        //public event MessageHandler OnMessageRecived;
        public event MessageHandler OnErrMessageRecived;

        public IReadOnlyDictionary<IPAddress, AhemClient> ReadOnlyChatClients { get; set; }
        public int SearchRange { get; set; }

        private Dictionary<IPAddress, AhemClient> ChatClients;

        private LinkingHelper LinkingHelper;

        public AhemClientManager()
        {
            SearchRange = 30;
            ChatClients = new Dictionary<IPAddress, AhemClient>();
            ReadOnlyChatClients = ChatClients;

            LinkingHelper = new LinkingHelper(ChatClients);
        }

        public void Dispose()
        {
            foreach (var item in ChatClients)
            {
                item.Value.OnGPSUpdated -= ChatClient_OnGPSUpdated;
            }
            ChatClients.Clear();
        }

        public AhemClient AddClient(ClientSocket clientSocket, ClientConnected clientConnectedinfo)
        {
            AhemClient searched = null;

            foreach (var item in ChatClients)
            {
                if (item.Value.ClientSocket == clientSocket)
                {
                    searched = item.Value;
                    break;
                }
            }

            if (searched != null)
            {
                OnErrMessageRecived?.Invoke(
                       string.Format("ClientSocket {0} is already exist while trying AddClient", clientSocket.IPAddress.ToString())
                       );
                return searched;
            }

            AhemClient chatClient = new AhemClient(
                clientSocket,
                clientConnectedinfo.ChatClient.UserEmail,
                clientConnectedinfo.ChatClient.Id,
                clientConnectedinfo.ChatClient.Name,
                new GPSdata(clientConnectedinfo.GPSdata)
                );
            LinkingHelper.LinkClient(chatClient, SearchRange);
            chatClient.SendData(new LinkInfo(chatClient.LinkedClients.Count, SearchRange));
            chatClient.OnGPSUpdated += ChatClient_OnGPSUpdated;

            ChatClients.Add(clientSocket.IPAddress, chatClient);
            return chatClient;
        }

        private void ChatClient_OnGPSUpdated(AhemClient chatClient)
        {
            LinkingHelper.UpdateLink(chatClient, SearchRange);
            chatClient.SendData(new LinkInfo(chatClient.LinkedClients.Count, SearchRange));
        }

        public void RemoveClient(ClientSocket clientSocket)
        {
            AhemClient searched = null;

            foreach (var item in ChatClients)
            {
                if (item.Value.ClientSocket == clientSocket)
                {
                    searched = item.Value;
                    break;
                }
            }

            if (searched == null)
            {
                OnErrMessageRecived?.Invoke(
                    string.Format("ClientSocket {0} is not exist while trying RemoveClient", clientSocket.IPAddress.ToString())
                    );
                return;
            }

            searched.SendData(new ClientDisConnect());
            ChatClients.Remove(searched.ClientSocket.IPAddress);
            searched.OnGPSUpdated -= ChatClient_OnGPSUpdated;
        }
    }
}