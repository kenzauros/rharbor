using NUnit.Framework;
using System.Collections.Generic;

namespace kenzauros.RHarbor.Models
{
    public class ExternalProgramDefinitionTests
    {
        readonly Dictionary<string, string> SampleParameters = new Dictionary<string, string>()
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
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters("This is an {extra1} param", SampleParameters),
                Is.EqualTo("This is an EXTRA param"));
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a {camelCase} param", SampleParameters),
                Is.EqualTo("This is a camel case param"));
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a {snake_case} param", SampleParameters),
                Is.EqualTo("This is a snake case param"));
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a param {withBrackets}", SampleParameters),
                Is.EqualTo("This is a param {with brackets}"));
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a param {conditionalLike}", SampleParameters),
                Is.EqualTo("This is a param {with?hoge:fuga}"));
        }

        [TestCase]
        public void ReplaceConnectionParameters_FallbackValue()
        {
            // Fallback
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a {extra2:fallback} param", SampleParameters),
                Is.EqualTo("This is a fallback param"));
            // No fallback specified
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters("This is a param{ with no fallback value}", SampleParameters),
                Is.EqualTo("This is a param"));
        }

        [TestCase]
        public void ReplaceConnectionParameters_BracketEscape()
        {
            // Escape brackets
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {{with escaped bracket}}", SampleParameters),
                Is.EqualTo("This is a param {with escaped bracket}"));
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {{{{with escaped bracket}}}}", SampleParameters),
                Is.EqualTo("This is a param {{with escaped bracket}}"));
        }

        [TestCase]
        public void ReplaceConnectionParameters_ConditionalParameters()
        {
            // Conditional
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {extra1=EXTRA?hoge:fuga}", SampleParameters),
                Is.EqualTo("This is a param hoge"));
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {extra1!=EXTRA?hoge:fuga}", SampleParameters),
                Is.EqualTo("This is a param fuga"));
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {extra1?hoge:fuga}", SampleParameters),
                Is.EqualTo("This is a param hoge"));
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {extra2?hoge:fuga}", SampleParameters),
                Is.EqualTo("This is a param fuga"));
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {!extra1?hoge:fuga}", SampleParameters),
                Is.EqualTo("This is a param fuga"));
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(@"This is a param {!extra2?hoge:fuga}", SampleParameters),
                Is.EqualTo("This is a param hoge"));
        }

        [TestCase]
        public void ReplaceConnectionParameters_Complex()
        {
            // Complex
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(
                    @"{extra1} {camelCase} is not {snake_case} {withBrackets} {extra2:fallback}ed{ with no fallback value} {{with escaped bracket}}",
                    SampleParameters),
                Is.EqualTo("EXTRA camel case is not snake case {with brackets} fallbacked {with escaped bracket}")
            );

            // Conditional & Nested parameters
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(@"{extra1} {extra1=EXTRA?{camelCase}:{snake_case}}", SampleParameters),
                Is.EqualTo("EXTRA camel case")
            );

            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(@"{!stage?{host}:{host}-{stage}}", SampleParameters),
                Is.EqualTo("my-host-staging")
            );

            // Practical paramter
            Assert.That(
                ExternalProgramDefinition.ReplaceConnectionParameters(
                    @"{host}:{port} /ssh2 /user=""{username}"" /passwd=""{password}"" /keyfile=""{keyfile}"" " +
                    "/auth={keyfile?publickey:password} /bg={stage=staging?40,88,61:65,66,110} /size=120,32",
                    SampleParameters
                    ),
                Is.EqualTo(@"my-host:99 /ssh2 /user=""user1"" /passwd=""passwd"" /keyfile=""C:\tmp\hogehoge.key"" /auth=publickey /bg=40,88,61 /size=120,32")
            );
        }
    }
}
