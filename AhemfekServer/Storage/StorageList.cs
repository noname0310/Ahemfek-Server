using System.IO;

namespace AhemfekServer.Storage
{
    public abstract class StorageList<T>
    {
        public abstract T this[string name] { get; }
        public readonly DirectoryInfo DirectoryInfo;

        public StorageList(string path)
        {
            DirectoryInfo = new DirectoryInfo(path);
            if (!DirectoryInfo.Exists)
                DirectoryInfo.Create();
        }

        public abstract bool Contains(string key);

        public abstract bool Remove(string key);

        public abstract bool Clear();
    }
}
