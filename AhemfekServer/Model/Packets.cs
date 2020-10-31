using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace AhemfekServer.Model
{
    public enum PacketType
    {
        ClientConnected,
        ClientDisConnect,
        StreamHeader
    }

    public abstract class Packet
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PacketType PacketType { get; protected set; }
    }

    public class ClientConnected : Packet
    {
        public readonly string ClientId;

        /// <summary>
        /// for json serialize
        /// </summary>
        public ClientConnected() => PacketType = PacketType.ClientConnected;

        public ClientConnected(string clientId) : this() => ClientId = clientId;
    }

    public class ClientDisConnected : Packet
    {
        public ClientDisConnected() => PacketType = PacketType.ClientDisConnect;
    }

    public class StreamHeader : Packet
    {
        public StreamHeader() => PacketType = PacketType.StreamHeader;
    }
}