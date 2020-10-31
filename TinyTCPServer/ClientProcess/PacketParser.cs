using System;

namespace TinyTCPServer.ClientProcess
{
    static class PacketParser
    {
        public const int HeaderSize = HeaderItemSize * HeaderItemsCount;
        public const int HeaderItemSize = sizeof(int);
        public const int HeaderItemsCount = 2;

        public static ParseResult HeaderParse(byte[] buffer, uint recivedbyte)
        {
            if (recivedbyte < HeaderSize)
                return new ParseResult(false, 0, 0, 0);

            byte[] header = new byte[HeaderSize];
            System.Buffer.BlockCopy(buffer, 0, header, 0, header.Length);
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < HeaderItemsCount; i++)
                    Array.Reverse(header, i * HeaderItemSize, HeaderItemSize);
            }

            return new ParseResult(
                true,
                (PacketType)BitConverter.ToInt32(header, HeaderItemSize * 0),
                BitConverter.ToInt32(header, HeaderItemSize * 1),
                (int)recivedbyte - sizeof(int)
            );
        }

        public static byte[] CreateHeader(PacketType packetType, int length)
        {
            byte[] header = new byte[HeaderSize];

            byte[] packetTypebyte = BitConverter.GetBytes((int)packetType);
            byte[] lengthbyte = BitConverter.GetBytes(length);

            Buffer.BlockCopy(packetTypebyte, 0, header, HeaderItemSize * 0, packetTypebyte.Length);
            Buffer.BlockCopy(lengthbyte, 0, header, HeaderItemSize * 1, lengthbyte.Length);

            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < HeaderItemsCount; i++)
                    Array.Reverse(header, i * HeaderItemSize, HeaderItemSize);
            }

            return header;
        }
    }

    public enum PacketType : int
    {
        UTF8Json = 0,
        ByteStream = 1
    }

    public struct ParseResult
    {
        public bool HeaderParsed { get; set; }
        public PacketType PacketType { get; set; }
        public int ContentLength { get; set; }
        public int ReceivedContentLength { get; set; }

        public ParseResult(bool headerParsed, PacketType packetType, int contentLength, int receivedContentLength)
        {
            HeaderParsed = headerParsed;
            PacketType = packetType;
            ContentLength = contentLength;
            ReceivedContentLength = receivedContentLength;
        }
    }
}
