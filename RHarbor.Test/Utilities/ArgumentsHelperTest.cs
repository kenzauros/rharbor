using kenzauros.RHarbor.Models;
using NUnit.Framework;
using System.Linq;

namespace kenzauros.RHarbor.Utilities.Test
{
    [TestFixture]
    public class ArgumentsHelperTest
    {
        const string ValidArg1 = "/open=rdp:13,ssh:2";
        const string ValidArg2 = "--open=rdp:13|ssh:2";

        [TestCase(ValidArg1)]
        [TestCase(ValidArg2)]
        public void SetArgs_ShouldUnderstandOpenArg(string arg)
        {
            ArgumentsHelper.SetArgs(arg);
            Assert.That(ArgumentsHelper.HasConnectionSpecified, Is.True);
            Assert.That(ArgumentsHelper.SpecifiedConnections.Count(), Is.EqualTo(2));

            // SpecifiedConnections
            var conn1 = ArgumentsHelper.SpecifiedConnections.ElementAt(0);
            Assert.That(conn1.Type, Is.EqualTo(ConnectionType.RDP));
            Assert.That(conn1.ConnectionId, Is.EqualTo(13));
            var conn2 = ArgumentsHelper.SpecifiedConnections.ElementAt(1);
            Assert.That(conn2.Type, Is.EqualTo(ConnectionType.SSH));
            Assert.That(conn2.ConnectionId, Is.EqualTo(2));
        }
    }
}
