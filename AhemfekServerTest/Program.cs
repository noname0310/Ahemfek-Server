using System;
using System.Collections;
using System.Threading;
using AhemfekServer.Server;

namespace AhemfekServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            AhemServer chatServer = new AhemServer();
            chatServer.OnMessageRecived += ChatServer_OnMessageRecived;
            chatServer.OnErrMessageRecived += ChatServer_OnErrMessageRecived;
            chatServer.Start(); 
            //IEnumerator enumerator = chatServer.GetSyncRoutine();
            //for (int i = 0; i < 10000000; i++)
            //{
            //    enumerator.MoveNext();

            //    if ((int)enumerator.Current == 0)
            //        break;
            //}

            chatServer.RunSyncRoutine(1);
        }

        private static void ChatServer_OnErrMessageRecived(string msg) => Console.WriteLine(msg);

        private static void ChatServer_OnMessageRecived(string msg) => Console.WriteLine(msg);
    }
}