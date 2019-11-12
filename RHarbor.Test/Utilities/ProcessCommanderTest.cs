using kenzauros.RHarbor.IPC;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.Utilities.Test
{
    [TestFixture]
    public class ProcessCommanderTest
    {
        [TestCase]
        public void Invoke_ShouldEnqueueConnectionInfoToConnectioRequest()
        {
            var processCommander = new ProcessCommander
            {
                ConnectionRequest = ConnectionRequest.Singleton
            };
            processCommander.Invoke(Models.ConnectionType.RDP, 800);
            processCommander.Invoke(Models.ConnectionType.SSH, 900);
            Assert.AreEqual(2, ConnectionRequest.Singleton.Queue.Count);
            var (type, id) = ConnectionRequest.Singleton.Queue.Dequeue();
            Assert.AreEqual(Models.ConnectionType.RDP, type);
            Assert.AreEqual(800, id);
            (type, id) = ConnectionRequest.Singleton.Queue.Dequeue();
            Assert.AreEqual(Models.ConnectionType.SSH, type);
            Assert.AreEqual(900, id);
        }

        [TestCase]
        public void Activate_ShouldNotThrow()
        {
            var processCommander = new ProcessCommander
            {
                ConnectionRequest = ConnectionRequest.Singleton
            };
            Assert.DoesNotThrow(() => processCommander.Activate());
        }


    }
}
