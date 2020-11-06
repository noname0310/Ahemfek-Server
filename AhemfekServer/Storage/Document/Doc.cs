using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using AhemfekServer.Storage.ClientUser;
using System.Diagnostics.CodeAnalysis;

namespace AhemfekServer.Storage.Document
{
    [Serializable]
    public class Doc : IEquatable<Doc>
    {
        [JsonProperty("theme")]
        public string Theme { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("author")]
        public User User { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("likes")]
        public int Likes { get; set; }

        [JsonProperty("content")]
        public List<string> Content { get; set; }

        public Doc()
        {

        }

        public override bool Equals(object obj) => obj is Doc doc && Title == doc.Title && EqualityComparer<User>.Default.Equals(User, doc.User);

        public override int GetHashCode() => HashCode.Combine(Title, User);

        public bool Equals([AllowNull] Doc other) => Title == other.Title && EqualityComparer<User>.Default.Equals(User, other.User);

        public static bool operator ==(Doc left, Doc right) => EqualityComparer<Doc>.Default.Equals(left, right);

        public static bool operator !=(Doc left, Doc right) => !(left == right);
    }
}
