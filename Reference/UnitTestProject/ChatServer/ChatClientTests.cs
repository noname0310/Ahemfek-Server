using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyChatServer.ChatServer;
using System;
using System.Collections.Generic;
using System.Text;
using TinyChatServer.Model;
using Newtonsoft.Json.Linq;

namespace TinyChatServer.ChatServer.Tests
{
    [TestClass()]
    public class ChatClientTests
    {
        [TestMethod()]
        public void SendDataTest()
        {
            Packet a = new Message(new TinyChatServer.Model.ChatClient(), "asasas");
            throw new Exception(JObject.FromObject(a).ToString());
        }
    }
}