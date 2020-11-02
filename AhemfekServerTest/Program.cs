using System;
using System.Collections;
using System.Threading.Tasks;
using AhemfekServer.Server;
using AhemfekServer.Storage;

namespace AhemfekServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            AhemServer ahemServer = new AhemServer();
            ahemServer.OnMessageRecived += ahemServer_OnMessageRecived;
            ahemServer.OnErrMessageRecived += ahemServer_OnErrMessageRecived;
            ahemServer.OnClientPacketRecived += AhemServer_OnClientJsonDataRecived; ;
            ahemServer.Start();
            //IEnumerator enumerator = ahemServer.GetSyncRoutine();
            //for (int i = 0; i < 10000000; i++)
            //{
            //    enumerator.MoveNext();

            //    if ((int)enumerator.Current == 0)
            //        break;

            //    Task.Delay(1).Wait();
            //}

            StorageFileList storageFileList = new StorageFileList("testFileList");
            storageFileList.AddNOpen("1").Close();
            storageFileList.AddNOpen("2").Close();
            storageFileList.AddNOpen("3").Close();
            storageFileList.AddNOpen("4").Close();
            storageFileList.AddNOpen("5").Close();
            storageFileList.Clear();
            storageFileList.AddNOpen("4").Close();
            storageFileList.AddNOpen("5").Close();
            storageFileList.AddNOpen("6").Close();
            storageFileList.AddNOpen("7").Close();
            storageFileList.AddNOpen("8").Close();
            storageFileList.Remove("8");

            StorageDirectoryList storageDirectoryList = new StorageDirectoryList("testDirlist");
            storageDirectoryList.Add("1");
            StorageFileList storageFileList2 = new StorageFileList(storageDirectoryList["1"].FullName);
            storageFileList2.AddNOpen("jk").Close();

            ahemServer.RunSyncRoutine(1);
        }

        private static void AhemServer_OnClientJsonDataRecived(AhemClient client, AhemfekServer.Model.Packet packet)
        {
            throw new NotImplementedException();
        }

        private static void ahemServer_OnErrMessageRecived(string msg) => Console.WriteLine(msg);

        private static void ahemServer_OnMessageRecived(string msg) => Console.WriteLine(msg);
    }
}