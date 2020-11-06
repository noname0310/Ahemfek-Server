using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace AhemfekServer.Storage.ClientUser
{
    [Serializable]
    public class User : IEquatable<User>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public User() { }

        public User(string id) : this()
        {
            Id = id;
        }

        public override bool Equals(object obj) => obj is User user && Id == user.Id;

        public override int GetHashCode() => HashCode.Combine(Id);

        public bool Equals([AllowNull] User other) => Id == other.Id;

        public static bool operator ==(User left, User right) => EqualityComparer<User>.Default.Equals(left, right);

        public static bool operator !=(User left, User right) => !(left == right);
    }
}
