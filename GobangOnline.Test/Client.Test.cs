using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GobangOnline.Client;

namespace GobangOnline.Test
{
    [TestClass]
    public class NumberTest 
    {
        [TestMethod]
        public async Task MemberNumberTest1()
        {
            OnlineClient client = new OnlineClient("123");
            client.Start();
            var num = await OnlineClient.GetMemberNum("123");
            Assert.AreEqual(1, num);
        }
        [TestMethod]
        public async Task MemberNumberTest2()
        {
            OnlineClient client1 = new OnlineClient("123");
            OnlineClient client2 = new OnlineClient("123");
            OnlineClient client3 = new OnlineClient("123");
            client1.Start();
            client2.Start();
            client3.Start();
            var num = await OnlineClient.GetMemberNum("123");
            Assert.AreEqual(3, num);
        }
    }
}