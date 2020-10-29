using System;
using System.Collections.Generic;

namespace TinyChatServer.Model
{
    public enum PacketType
    {
        ClientConnected,
        ClientDisConnect,
        Message,
        GPS,
        LinkInfo
    }

    public class Packet
    {
        public string PacketType;
    }

    public class Message : Packet, IEquatable<Message>
    {
        public ChatClient ChatClient;
        public string Msg;

        public Message()
        {
            PacketType = Model.PacketType.Message.ToString("G");
        }

        public Message(ChatClient chatClient, string msg) : this()
        {
            ChatClient = chatClient;
            Msg = msg;
        }

        public bool Equals(Message other)
        {
            return EqualityComparer<ChatClient>.Default.Equals(ChatClient, other.ChatClient) &&
                   Msg == other.Msg;
        }

        public override bool Equals(object obj)
        {
            return obj is Message message &&
                   Equals(message);
        }

        public override int GetHashCode()
        {
            int hashCode = -943457266;
            hashCode = hashCode * -1521134295 + EqualityComparer<ChatClient>.Default.GetHashCode(ChatClient);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Msg);
            return hashCode;
        }

        public static bool operator ==(Message left, Message right)
        {
            return EqualityComparer<Message>.Default.Equals(left, right);
        }

        public static bool operator !=(Message left, Message right)
        {
            return !(left == right);
        }
    }

    class GPS : Packet
    {
        public GPSdata GPSdata;

        public GPS()
        {
            PacketType = Model.PacketType.GPS.ToString("G");
        }

        public GPS(GPSdata gPSdata) : this()
        {
            GPSdata = gPSdata;
        }
    }

    class ClientConnected : Packet
    {
        public ChatClient ChatClient;
        public GPSdata GPSdata;

        public ClientConnected()
        {
            PacketType = Model.PacketType.ClientConnected.ToString("G");
        }

        public ClientConnected(ChatClient chatClient, GPSdata gPSdata) : this()
        {
            ChatClient = chatClient;
            GPSdata = gPSdata;
        }
    }

    class ClientDisConnect : Packet
    {
        public ClientDisConnect()
        {
            PacketType = Model.PacketType.ClientDisConnect.ToString("G");
        }
    }

    class LinkInfo : Packet
    {
        public int LinkedClients;
        public int SearchRange;

        public LinkInfo()
        {
            PacketType = Model.PacketType.LinkInfo.ToString("G");
        }

        public LinkInfo(int linkedClients, int searchRange) : this()
        {
            LinkedClients = linkedClients;
            SearchRange = searchRange;
        }
    }

    public class ChatClient
    {
        public string Id;
        public string Name;
        public string UserEmail;

        public ChatClient()
        {

        }

        public ChatClient(string id, string name, string userEmail)
        {
            Id = id;
            Name = name;
            UserEmail = userEmail;
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
    }
}