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

        public List<DocThumbnail> GetDocThumbList(int startIndex, int count)
        {
            List<DocThumbnail> pagedDocsThum = new List<DocThumbnail>();
            for (int i = startIndex; i < startIndex + count; i++)
            {
                if (PrivateDocs.Count < i + 1)
                    break;
                pagedDocsThum.Add(new DocThumbnail(PrivateDocs[i]));
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
