using NUnit.Framework;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace kenzauros.RHarbor.Test
{
    [TestFixture]
    public class FirstTest
    {
        const string LOCAL_HOST = "127.0.0.1";
        const string FIRST_HOP_HOST = "192.168.10.112"; // start
        const int FIRST_HOP_PORT = 22;
        const string SECOND_HOP_HOST = "192.168.10.114"; // f-admin
        const int SECOND_HOP_PORT = 22;
        const string REMOTE_IP = "192.168.10.10"; // remote1
        const int REMOTE_PORT = 3389; // RDP
        const string SSH_USERNAME1 = "";
        const string SSH_PASSWORD1 = "";

        [TestCase]
        public void A01_FirstTest()
        {
            var connectionInfo = new PasswordConnectionInfo(FIRST_HOP_HOST, FIRST_HOP_PORT, SSH_USERNAME1, SSH_PASSWORD1);
            using var client = new SshClient(connectionInfo);
            client.Connect();
            using var forward = new ForwardedPortLocal(LOCAL_HOST, SECOND_HOP_HOST, SECOND_HOP_PORT);
            client.AddForwardedPort(forward);
            forward.Start();
            var connectionInfo2 = new PasswordConnectionInfo(forward.BoundHost, (int)forward.BoundPort, SSH_USERNAME1, SSH_PASSWORD1);
            using var client2 = new SshClient(connectionInfo2);
            client2.Connect();
            var command = client2.RunCommand("ls -la");
            Assert.That(command.Result, Is.Not.Empty);
            command.Dispose();
            using var forward2 = new ForwardedPortLocal(LOCAL_HOST, REMOTE_IP, REMOTE_PORT);
            client2.AddForwardedPort(forward2);
            forward2.Start();
            Assert.That(forward2.IsStarted, Is.True);
            Assert.That(forward2.BoundHost, Is.EqualTo(LOCAL_HOST));
            Assert.That(forward2.BoundPort, Is.GreaterThan(0));
        }
    }
}
