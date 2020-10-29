using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Net;
using System.Threading;
using TinyChatServer.ChatServer;
using TinyChatServer.Model;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ChatServer chatServer = new ChatServer();
            chatServer.OnMessageRecived += ChatServer_OnMessageRecived;
            chatServer.OnErrMessageRecived += ChatServer_OnErrMessageRecived;
            chatServer.Start(); 
            IEnumerator enumerator = chatServer.GetSyncRoutine();
            for (int i = 0; i < 10000000; i++)
            {
                enumerator.MoveNext();

                if ((int)enumerator.Current == 0)
                    break;
            }
            Thread.Sleep(1000);
            chatServer.Stop();
            chatServer.Start();

            chatServer.RunSyncRoutine();
        }

        private static void ChatServer_OnErrMessageRecived(string msg) => Console.WriteLine(msg);

        private static void ChatServer_OnMessageRecived(string msg) => Console.WriteLine(msg);
    }
}