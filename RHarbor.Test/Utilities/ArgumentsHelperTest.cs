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
            Assert.AreEqual(true, ArgumentsHelper.HasConnectionSpecified);
            Assert.AreEqual(2, ArgumentsHelper.SpecifiedConnections.Count());

            // SpecifiedConnections
            var conn1 = ArgumentsHelper.SpecifiedConnections.ElementAt(0);
            Assert.AreEqual(ConnectionType.RDP, conn1.Type);
            Assert.AreEqual(13, conn1.ConnectionId);
            var conn2 = ArgumentsHelper.SpecifiedConnections.ElementAt(1);
            Assert.AreEqual(ConnectionType.SSH, conn2.Type);
            Assert.AreEqual(2, conn2.ConnectionId);
        }
    }
}
