using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AhemfekServer.Storage.Document;
using AhemfekServer.Storage.ClientUser;

namespace AhemfekServer.Storage
{
    [Serializable]
    class AhemStorage
    {
        private readonly Dictionary<string, DocFolder> _docFolder;
        private Dictionary<string, StoreUser> _storeUsers;

        private const string _path = "data";

        public AhemStorage() 
        {
            _docFolder = new Dictionary<string, DocFolder>
            {
                { "life", new DocFolder("life") },
                { "love", new DocFolder("love") }
            };
        }

        public void Load()
        {
            if (!File.Exists(_path))
            {
                _storeUsers = new Dictionary<string, StoreUser>();
                return;
            }
            AhemStorage serializeStorage;
            using (FileStream fileStream = File.OpenRead(_path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                serializeStorage = (AhemStorage)bf.Deserialize(fileStream);
            }
        }

        public void Save()
        {
            AhemStorage serializeStorage = this;
            using (FileStream fileStream = File.Open(_path, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fileStream, serializeStorage);
            }
        }

        public bool TryAddDoc(Doc doc)
        {
            if (_docFolder.TryGetValue(doc.Theme, out DocFolder value))
            {
                bool result = value.TryAddDoc(doc);
                if (result == false)
                    return result;

                if (_storeUsers.TryGetValue(doc.User.Id, out StoreUser storeUser))
                {
                    storeUser.Docs.Add(doc.Title);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public bool RemoveDoc(string theme, string userId, string docName)
        {
            if (_docFolder.TryGetValue(theme, out DocFolder value))
            {
                Doc result = value.RemoveDoc(docName);
                if (result == null)
                    return false;

                if (_storeUsers.TryGetValue(userId, out StoreUser storeUser))
                {
                    storeUser.Docs.Remove(docName);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public bool TryAddPrivateDoc(Doc doc)
        {
            if (_storeUsers.TryGetValue(doc.User.Id, out StoreUser storeUser))
            {
                if (storeUser.PrivateDocs.Contains(doc))
                    return false;
                storeUser.PrivateDocs.Add(doc);
                return true;
            }
            else
                return false;
        }

        public bool RemovePrivateDoc(string userId, string docName)
        {
            if (_storeUsers.TryGetValue(userId, out StoreUser storeUser))
            {
                Doc doc = storeUser.PrivateDocs.Find(item => item.Title == docName);
                if (doc == default)
                    return false;
                storeUser.PrivateDocs.Remove(doc);
                return true;
            }
            else
                return false;
        }

        public bool TryAddUser(User user)
        {
            StoreUser storeUser = new StoreUser(user);
            if (_storeUsers.ContainsKey(storeUser.Id))
                return false;
            _storeUsers.Add(storeUser.Id, storeUser);
            return true;
        }

        public bool RemoveUser(User user)
        {
            StoreUser storeUser = new StoreUser(user);
            if (!_storeUsers.ContainsKey(storeUser.Id))
                return false;
            _storeUsers.Remove(storeUser.Id);
            return true;
        }

        public bool TryLike(string theme, string DocName)
        {
            if (_docFolder.TryGetValue(theme, out DocFolder value))
                return value.TryLike(DocName);
            else
                return false;
        }

        public bool TryUnlike(string theme, string DocName)
        {
            if (_docFolder.TryGetValue(theme, out DocFolder value))
                return value.TryUnlike(DocName);
            else
                return false;
        }

        public bool AddFollower(string targetUserId, string sourceUserId)
        {
            if (_storeUsers.TryGetValue(targetUserId, out StoreUser targetUser) &&
                _storeUsers.TryGetValue(sourceUserId, out StoreUser sourceUser))
            {
                if (!targetUser.Follower.Contains(sourceUserId))
                    targetUser.Follower.Add(sourceUserId);

                if (!sourceUser.Following.Contains(targetUserId))
                    sourceUser.Following.Add(targetUserId);

                return true;
            }
            else
                return false;
        }

        public bool RemoveFollower(string targetUserId, string sourceUserId)
        {
            bool contains = false;
            if (_storeUsers.TryGetValue(targetUserId, out StoreUser targetUser))
            {
                contains = true;
                if (targetUser.Follower.Contains(sourceUserId))
                    targetUser.Follower.Remove(sourceUserId);
            }
            if (_storeUsers.TryGetValue(sourceUserId, out StoreUser sourceUser))
            {
                contains = true;
                if (sourceUser.Following.Contains(targetUserId))
                    sourceUser.Following.Remove(targetUserId);
            }
            return contains;
        }

        public List<DocThumbnail> GetPublicDocThumb(string theme, int startIndex, int count, DocOrder docOrder)
        {
            if (_docFolder.TryGetValue(theme, out DocFolder value))
                return value.GetDocThumbList(startIndex, count, docOrder);
            else
                return null;
        }

        public List<DocThumbnail> GetPrivateDocThumb(string userId, int startIndex, int count)
        {
            if (_storeUsers.TryGetValue(userId, out StoreUser storeUser))
            {
                return storeUser.GetDocThumbList(startIndex, count);
            }
            return null;
        }

        public Doc GetUserDoc(string userId, string docName)
        {
            foreach (var item in _docFolder)
            {
                foreach (var doc in item.Value.NewestDocuments)
                {
                    if (doc.Value.User.Id == userId && doc.Value.Title == docName)
                    {
                        return doc.Value;
                    }
                }
            }
            return null;
        }

        public Doc GetUserPrivateDoc(string userId, string docName)
        {
            if (_storeUsers.TryGetValue(userId, out StoreUser value))
            {
                Doc doc = value.PrivateDocs.Find(item => item.Title == docName);
                if (doc == default)
                    return null;
                return doc;
            }
            return null;
        }

        public List<string> GetTheme()
        {
            return _docFolder.Select(item => item.Key).ToList();
        }
    }
}
