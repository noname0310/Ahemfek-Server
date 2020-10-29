using System;
using System.Collections.Generic;
using System.Net;

namespace TinyChatServer.ChatServer.ChatLinker
{
    class LinkingHelper
    {
        private Dictionary<IPAddress, ChatClient> _chatClients;

        public LinkingHelper(Dictionary<IPAddress, ChatClient> chatClients)
        {
            _chatClients = chatClients;
        }
        
        public void InitLinks(int searchRange)
        {
            foreach (var item in _chatClients)
            {
                item.Value.LinkedClients.Clear();
                foreach (var other in _chatClients)
                {
                    if (item.Value == other.Value)
                        continue;

                    double distance = GPSDistanceMeter(item.Value.GPSdata, other.Value.GPSdata);

                    if (distance <= searchRange)
                        item.Value.LinkedClients.Add(other.Value);
                }
            }
        }

        public void LinkClient(ChatClient chatClient, int searchRange)
        {
            chatClient.LinkedClients.Clear();

            foreach (var other in _chatClients)
            {
                if (chatClient == other.Value)
                    continue;

                double distance = GPSDistanceMeter(chatClient.GPSdata, other.Value.GPSdata);

                if (distance <= searchRange)
                {
                    chatClient.LinkedClients.Add(other.Value);
                    other.Value.LinkedClients.Add(chatClient);
                }
            }
        }

        public void UpdateLink(ChatClient chatClient, int searchRange)
        {
            foreach (var item in chatClient.LinkedClients)
            {
                item.LinkedClients.Remove(chatClient);
            }
            chatClient.LinkedClients.Clear();

            foreach (var other in _chatClients)
            {
                if (chatClient == other.Value)
                    continue;

                double distance = GPSDistanceMeter(chatClient.GPSdata, other.Value.GPSdata);

                if (distance <= searchRange)
                {
                    chatClient.LinkedClients.Add(other.Value);
                    other.Value.LinkedClients.Add(chatClient);
                }
            }
        }

        public double GPSDistanceMeter(GPSdata some, GPSdata other)
        {
            double theta;
            double distance;
            theta = some.Longitude - other.Longitude;
            distance =
                (
                Math.Sin(ConvertDegreesToRadians(some.Latitude)) *
                Math.Sin(ConvertDegreesToRadians(other.Latitude))
                )
                +
                (
                Math.Cos(ConvertDegreesToRadians(some.Latitude)) *
                Math.Cos(ConvertDegreesToRadians(other.Latitude)) *
                Math.Cos(ConvertDegreesToRadians(theta))
                );
            distance = Math.Acos(distance);
            distance = ConvertRadiansToDegrees(distance);
            distance = distance * 60 * 1.1515;
            distance = distance * 1.609344 * 1000;
            return distance;
        }

        public static double ConvertDegreesToRadians(double degrees)
        {
            return (Math.PI / 180) * degrees;
        }

        public static double ConvertRadiansToDegrees(double radians)
        {
            return (180 / Math.PI) * radians;
        }
    }
}