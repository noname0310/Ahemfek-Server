using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AhemfekServer.Storage.Document;

namespace AhemfekServer.Storage.ClientUser
{
    [Serializable]
    class StoreUser : User, IEquatable<StoreUser>
    {
        public SortedSet<string> Follower { get; private set; }
        public SortedSet<string> Following { get; private set; }
        public List<string> Docs { get; private set; }
        public List<Doc> PrivateDocs { get; private set; }

        public StoreUser(string id) : base(id) { }

        public StoreUser(User user) : this(user.Id)
        {
            Follower = new SortedSet<string>();
            Following = new SortedSet<string>();
            Docs = new List<string>();
            PrivateDocs = new List<Doc>();
        }

        public List<DocThumbnail> GetDocThumbList(List<Doc> reference, int startIndex, int count)
        {
            List<string> SearchList = new List<string>();
            for (int i = startIndex; i < startIndex + count; i++)
            {
            }
                List<DocThumbnail> pagedDocsThum = new List<DocThumbnail>();
            switch (docOrder)
            {
                case DocOrder.Newest:
                    for (int i = startIndex; i < startIndex + count; i++)
                    {
                        if (_newestDocuments.Count < i + 1)
                            break;
                        pagedDocsThum.Add(new DocThumbnail(_newestDocuments.Values[i]));
                    }
                    break;
                case DocOrder.Popular:
                    for (int i = startIndex; i < startIndex + count; i++)
                    {
                        if (_popularDocuments.Count < i + 1)
                            break;
                        pagedDocsThum.Add(new DocThumbnail(_newestDocuments.Values[i]));
                    }
                    break;
                default:
                    break;
            }
            return pagedDocsThum;
        }

        public override bool Equals(object obj) => obj is StoreUser user && Id == user.Id;

        public override int GetHashCode() => HashCode.Combine(Id);

        public bool Equals([AllowNull] StoreUser other) => Id == other.Id;

        public static bool operator ==(StoreUser left, StoreUser right) => EqualityComparer<StoreUser>.Default.Equals(left, right);

        public static bool operator !=(StoreUser left, StoreUser right) => !(left == right);
    }
}
