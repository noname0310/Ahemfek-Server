using System.Text.Json.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using AhemfekServer.Storage.ClientUser;
using AhemfekServer.Storage.Document;

namespace AhemfekServer.Model
{
    public class Packet
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PacketType PacketType { get; protected set; }
    }

    public class ClientConnected : Packet
    {
        public bool Register { get; set; }
        public User User { get; set; }

        public ClientConnected() => PacketType = PacketType.ClientConnected;

        public ClientConnected(bool register, User user) : this()
        {
            Register = register;
            User = user;
        }
    }

    public class ClientDisConnect : Packet
    {
        public ClientDisConnect() => PacketType = PacketType.ClientDisConnect;
    }
    
    public class ClientDisConnected : Packet
    {
        public ClientDisConnected() => PacketType = PacketType.ClientDisConnected;
    }

    public class UploadDocument : Packet
    {
        public Doc Doc { get; set; }

        public UploadDocument() => PacketType = PacketType.UploadDocument;

        public UploadDocument(Doc doc) : this() => Doc = doc;
    }

    public class RemoveDocument : Packet
    {
        public string Theme { get; set; }

        public string DocName { get; set; }

        public RemoveDocument() => PacketType = PacketType.RemoveDocument;

        public RemoveDocument(string theme, string docName) : this()
        {
            Theme = theme;
            DocName = docName;
        }
    }

    public class UploadPrivateDocument : Packet
    {
        public Doc Doc { get; set; }

        public UploadPrivateDocument() => PacketType = PacketType.UploadPrivateDocument;

        public UploadPrivateDocument(Doc doc) : this() => Doc = doc;
    }

    public class RemovePrivateDocument : Packet
    {
        public string DocName { get; set; }

        public RemovePrivateDocument() => PacketType = PacketType.RemovePrivateDocument;

        public RemovePrivateDocument(string docName) : this() => DocName = docName;
    }

    public class Like : Packet
    {
        public string Theme { get; set; }
        public string DocName { get; set; }

        public Like() => PacketType = PacketType.Like;

        public Like(string theme, string docName) : this()
        {
            Theme = theme;
            DocName = docName;
        }
    }

    public class UnLike : Packet
    {
        public string Theme { get; set; }
        public string DocName { get; set; }

        public UnLike() => PacketType = PacketType.Unlike;

        public UnLike(string theme, string docName) : this()
        {
            Theme = theme;
            DocName = docName;
        }
    }

    public class Follow : Packet
    {
        public User Target { get; set; }

        public Follow() => PacketType = PacketType.Follow;

        public Follow(User target) : this() => Target = target;
    }

    public class UnFollow : Packet
    {
        public User Target { get; set; }

        public UnFollow() => PacketType = PacketType.UnFollow;

        public UnFollow(User target) : this() => Target = target;
    }

    public class ReqPageData : Packet
    {
        public string Theme { get; set; }
        public int StartIndex { get; set; }
        public int Count { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DocOrder DocOrder { get; set; }

        public ReqPageData() => PacketType = PacketType.ReqPageData;

        public ReqPageData(string theme, int startIndex, int count, DocOrder docOrder) : this()
        {
            Theme = theme;
            StartIndex = startIndex;
            Count = count;
            DocOrder = docOrder;
        }
    }

    public class PageData : Packet
    {
        public List<DocThumbnail> DocThumbnailPage { get; set; }

        public PageData() => PacketType = PacketType.PageData;

        public PageData(List<DocThumbnail> docThumbnailPage) : this() => DocThumbnailPage = docThumbnailPage;
    }

    public class ReqPrivatePageData : Packet
    {
        public int StartIndex { get; set; }
        public int Count { get; set; }

        public ReqPrivatePageData() => PacketType = PacketType.ReqPrivatePageData;

        public ReqPrivatePageData(int startIndex, int count) : this()
        {
            StartIndex = startIndex;
            Count = count;
        }
    }

    public class PrivatePageData : Packet
    {
        public List<DocThumbnail> DocThumbnailPage { get; set; }

        public PrivatePageData() => PacketType = PacketType.PrivatePageData;

        public PrivatePageData(List<DocThumbnail> docThumbnailPage) : this() => DocThumbnailPage = docThumbnailPage;
    }

    public class ReqDoc : Packet
    {
        public bool IsPrivate { get; set; }
        public User Author { get; set; }
        public string DocName { get; set; }

        public ReqDoc() => PacketType = PacketType.ReqDoc;

        public ReqDoc(bool isPrivate, User author, string docName) : this()
        {
            IsPrivate = isPrivate;
            Author = author;
            DocName = docName;
        }
    }

    public class PDoc : Packet
    {
        public Doc Doc { get; set; }

        public PDoc() => PacketType = PacketType.Doc;

        public PDoc(Doc doc) : this() => Doc = doc;
    }

    public class ReqThemes : Packet
    {
        public ReqThemes() => PacketType = PacketType.ReqThemes;
    }

    public class Themes : Packet
    {
        public List<string> ThemesList { get; set; }

        public Themes() => PacketType = PacketType.Themes;

        public Themes(List<string> themesList) : this() => ThemesList = themesList;
    }

    public class StreamHeader : Packet
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
        UploadDocument,
        RemoveDocument,
        UploadPrivateDocument,
        RemovePrivateDocument,
        Like,
        Unlike,
        Follow,
        UnFollow,
        ReqPageData,
        PageData,
        ReqPrivatePageData,
        PrivatePageData,
        ReqDoc,
        Doc,
        ReqThemes,
        Themes,
        StreamHeader
    }

    public enum StreamPacketType
    {
        Image
    }
}
