using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace AhemfekServer.Model
{
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

    public class ClientDisConnect : Packet
    {
        public ClientDisConnect() => PacketType = PacketType.ClientDisConnect;
    }
    
    public class ClientDisConnected : Packet
    {
        public ClientDisConnected() => PacketType = PacketType.ClientDisConnected;
    }

    public abstract class StreamHeader : Packet
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public StreamPacketType StreamPacketType { get; protected set; }

        public StreamHeader() => PacketType = PacketType.StreamHeader;
    }

    public class ImageStream : StreamHeader
    {
        public ImageStream() => StreamPacketType = StreamPacketType.Image;
    }

    public enum PacketType
    {
        ClientConnected,
        ClientDisConnect,
        ClientDisConnected,
        StreamHeader
    }

    public enum StreamPacketType
    {
        Image
    }
}
