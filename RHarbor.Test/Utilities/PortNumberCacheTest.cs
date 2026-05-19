using NUnit.Framework;
using System.Net;
using System.Net.Sockets;

namespace kenzauros.RHarbor.Utilities.Test
{
    [TestFixture]
    public class PortNumberCacheTest
    {
        [TestCase]
        public void IsAvailablePort_ShouldReturnTrue_WhenPortIsNotInUse()
        {
            // Find a free port by letting the OS pick one, then immediately release it.
            // There is a very small inherent race window between Stop() and IsAvailablePort(),
            // but in practice the chance of another process grabbing the same ephemeral port
            // within microseconds is negligible.
            int freePort;
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            freePort = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            Assert.That(PortNumberCache.IsAvailablePort(freePort), Is.True);
        }

        [TestCase]
        public void IsAvailablePort_ShouldReturnFalse_WhenPortIsInUse()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var usedPort = ((IPEndPoint)listener.LocalEndpoint).Port;

            // The port is currently bound, so it should not be available.
            Assert.That(PortNumberCache.IsAvailablePort(usedPort), Is.False);

            listener.Stop();
        }
    }
}
