using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AhemfekServer.Storage
{
    public class StorageFileList : StorageList<FileInfo>
    {
        public override FileInfo this[string name]
        {
            get
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(DirectoryInfo.FullName, name));
                if (fileInfo.Exists)
                    return fileInfo;
                else
                    throw new KeyNotFoundException();
            }
        }

        public StorageFileList(string path) : base(path) { }

        public FileStream AddNOpen(string key)
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(DirectoryInfo.FullName, key));
            if (!fileInfo.Exists)
            {
            createTry:
                try
                {
                    return fileInfo.Create();
                }
                catch (IOException)
                {
                    goto createTry;
                }
            }
            return fileInfo.OpenWrite();
        }

        public override bool Contains(string key)
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(DirectoryInfo.FullName, key));
            return fileInfo.Exists;
        }

        public override bool Remove(string key)
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(DirectoryInfo.FullName, key));
            if (fileInfo.Exists)
            {
            deleteTry:
                try
                {
                    fileInfo.Delete();
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
