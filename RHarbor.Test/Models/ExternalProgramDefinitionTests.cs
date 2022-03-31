using NUnit.Framework;
using System.Collections.Generic;

namespace kenzauros.RHarbor.Models
{
    public class ExternalProgramDefinitionTests
    {
        Dictionary<string, string> SampleParameters = new Dictionary<string, string>()
            {
                { "extra1", "EXTRA" },
                { "camelCase", "camel case" },
                { "snake_case", "snake case" },
                { "withBrackets", "{with brackets}" },
                { "conditionalLike", "{with?hoge:fuga}" },
                { "host", "my-host" },
                { "port", "99" },
                { "username", "user1" },
                { "password", "passwd" },
                { "keyfile", @"C:\tmp\hogehoge.key" },
                { "stage", @"staging" },
            };

        [TestCase]
        public void ReplaceConnectionParameters_SimpleParameters()
        {
            Assert.AreEqual("This is an EXTRA param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is an {extra1} param", SampleParameters));
            Assert.AreEqual("This is a camel case param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a {camelCase} param", SampleParameters));
            Assert.AreEqual("This is a snake case param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a {snake_case} param", SampleParameters));
            Assert.AreEqual("This is a param {with brackets}",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a param {withBrackets}", SampleParameters));
            Assert.AreEqual("This is a param {with?hoge:fuga}",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a param {conditionalLike}", SampleParameters));
        }

        [TestCase]
        public void ReplaceConnectionParameters_FallbackValue()
        {
            // Fallback
            Assert.AreEqual("This is a fallback param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a {extra2:fallback} param", SampleParameters));
            // No fallback specified
            Assert.AreEqual("This is a param",
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a param{ with no fallback value}", SampleParameters));
        }

        [TestCase]
        public void ReplaceConnectionParameters_BracketEscape()
        {
            // Escape brackets
            Assert.AreEqual("This is a param {with escaped bracket}",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {{with escaped bracket}}", SampleParameters));
            Assert.AreEqual("This is a param {{with escaped bracket}}",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {{{{with escaped bracket}}}}", SampleParameters));
        }

        [TestCase]
        public void ReplaceConnectionParameters_ConditionalParameters()
        {
            // Conditional
            Assert.AreEqual("This is a param hoge",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {extra1=EXTRA?hoge:fuga}", SampleParameters));
            Assert.AreEqual("This is a param fuga",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {extra1!=EXTRA?hoge:fuga}", SampleParameters));
            Assert.AreEqual("This is a param hoge",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {extra1?hoge:fuga}", SampleParameters));
            Assert.AreEqual("This is a param fuga",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {extra2?hoge:fuga}", SampleParameters));
            Assert.AreEqual("This is a param fuga",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {!extra1?hoge:fuga}", SampleParameters));
            Assert.AreEqual("This is a param hoge",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {!extra2?hoge:fuga}", SampleParameters));
        }

        [TestCase]
        public void ReplaceConnectionParameters_Complex()
        {
            // Complex
            Assert.AreEqual("EXTRA camel case is not snake case {with brackets} fallbacked {with escaped bracket}",
                ExternalProgramDefinition.ReplaceConnectionParameters(
                    @"{extra1} {camelCase} is not {snake_case} {withBrackets} {extra2:fallback}ed{ with no fallback value} {{with escaped bracket}}",
                    SampleParameters));

            // Conditional & Nested parameters
            Assert.AreEqual("EXTRA camel case",
                ExternalProgramDefinition.ReplaceConnectionParameters(@"{extra1} {extra1=EXTRA?{camelCase}:{snake_case}}", SampleParameters));

            // Practical paramter
            Assert.AreEqual(@"my-host:99 /ssh2 /user=""user1"" /passwd=""passwd"" /keyfile=""C:\tmp\hogehoge.key"" /auth=publickey /bg=40,88,61 /size=120,32",
                ExternalProgramDefinition.ReplaceConnectionParameters(
                    @"{host}:{port} /ssh2 /user=""{username}"" /passwd=""{password}"" /keyfile=""{keyfile}"" " +
                    "/auth={keyfile?publickey:password} /bg={stage=staging?40,88,61:65,66,110} /size=120,32",
                    SampleParameters
                    )
                );
        }
    }
}
