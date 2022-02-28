using NUnit.Framework;
using System.Collections.Generic;

namespace kenzauros.RHarbor.Models
{
    public class ExternalProgramDefinitionTests
    {
        [TestCase]
        public void ReplaceConnectionParameters()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>()
            {
                { "extra1", "EXTRA" },
                { "camelCase", "camel case" },
                { "snake_case", "snake case" },
                { "withBrackets", "{with brackets}" },
            };

            Assert.AreEqual("This is an EXTRA param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is an {extra1} param", parameters));
            Assert.AreEqual("This is a camel case param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a {camelCase} param", parameters));
            Assert.AreEqual("This is a snake case param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a {snake_case} param", parameters));
            Assert.AreEqual("This is a param {with brackets}",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a param {withBrackets}", parameters));

            // Fallback
            Assert.AreEqual("This is a fallback param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a {extra2:fallback} param", parameters));
            // No fallback specified
            Assert.AreEqual("This is a param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a param{ with no fallback value}", parameters));
            // Fallback value with colon
            Assert.AreEqual("This is a fall:back param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a {extra2:fall:back} param", parameters));

            // Escape brackets
            Assert.AreEqual("This is a param {with escaped bracket}",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {{with escaped bracket}}", parameters));
            Assert.AreEqual("This is a param {{with escaped bracket}}",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {{{{with escaped bracket}}}}", parameters));

            // Complex
            Assert.AreEqual("EXTRA camel case is not snake case {with brackets} fallbacked {with escaped bracket}",
                ExternalProgramDefinition.ReplaceConnectionParameters(
                    @"{extra1} {camelCase} is not {snake_case} {withBrackets} {extra2:fallback}ed{ with no fallback value} {{with escaped bracket}}",
                    parameters));
        }

        [TestCase("{host} {port} {username} {password} {keyfile}", "host1 80 user1 pass1 key1")]
        public void ReplacePlaceholders(string arguments, string expected)
        {
            SSHConnectionInfo info = new SSHConnectionInfo
            {
                Host = "host1",
                Port = 80,
                Username = "user1",
                RawPassword = "pass1",
                PrivateKeyFilePath = "key1",
            };
            Assert.AreEqual(expected, ExternalProgramDefinition.ReplacePlaceholders(arguments, info));
        }

    }
}
