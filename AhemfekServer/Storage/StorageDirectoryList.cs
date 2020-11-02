using System.Collections.Generic;
using System.IO;

namespace AhemfekServer.Storage
{
    public class StorageDirectoryList : StorageList<DirectoryInfo>
    {
        public override DirectoryInfo this[string name]
        { 
            get {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DirectoryInfo.FullName, name));
                if (directoryInfo.Exists)
                    return directoryInfo;
                else
                    throw new KeyNotFoundException();
            } 
        }

        public StorageDirectoryList(string path) : base(path) { }

        public DirectoryInfo Add(string key)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DirectoryInfo.FullName, key));
            if (!directoryInfo.Exists)
            {
            createTry:
                try
                {
                    directoryInfo.Create();
                }
                catch (IOException)
                {
                    goto createTry;
                }
            }
            return directoryInfo;
        }

        public override bool Contains(string key)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DirectoryInfo.FullName, key));
            return directoryInfo.Exists;
        }

        public override bool Remove(string key)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(DirectoryInfo.FullName, key));
            if (directoryInfo.Exists)
            {
            deleteTry:
                try
                {
                    directoryInfo.Delete(true);
                }
                catch (IOException)
                {
                    goto deleteTry;
                }
                return true;
            }
            else
                return false;
        }

        public override bool Clear()
        {
            if (DirectoryInfo.Exists)
            {
            deleteTry:
                try
                {
                    DirectoryInfo.Delete(true);
                }
                catch (IOException)
                {
                    goto deleteTry;
                }
            createTry:
                try
                {
                    DirectoryInfo.Create();
                }
                catch (IOException)
                {
                    goto createTry;
                }
                return true;
            }
            else
                return false;
        }
    }
}
