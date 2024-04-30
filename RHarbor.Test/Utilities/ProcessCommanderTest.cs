using kenzauros.RHarbor.IPC;
using NUnit.Framework;

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
            Assert.That(ConnectionRequest.Singleton.Queue.Count, Is.EqualTo(2));
            var (type, id) = ConnectionRequest.Singleton.Queue.Dequeue();
            Assert.That(type, Is.EqualTo(Models.ConnectionType.RDP));
            Assert.That(id, Is.EqualTo(800));
            (type, id) = ConnectionRequest.Singleton.Queue.Dequeue();
            Assert.That(type, Is.EqualTo(Models.ConnectionType.SSH));
            Assert.That(id, Is.EqualTo(900));
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
