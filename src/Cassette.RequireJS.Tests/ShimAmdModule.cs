using Cassette.Scripts;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class ShimAmdModuleTests
    {
        [Fact]
        public void TransformScriptAppendsDefineCallThatReturnsModuleExpression()
        {
            AssertTransform(
                "var test = {};",
                "MODULE_RESULT",
                "var test = {};\r\ndefine(\"test\",[],function(){return MODULE_RESULT;});"
            );
        }

        [Fact]
        public void GeneratedDefineCallIncludesDependencies()
        {
            GivenDependencies("dependency1", "dependency2");
            AssertTransform(
                "var test = {};",
                "MODULE_RESULT",
                "var test = {};\r\ndefine(\"test\",[\"dependency1\",\"dependency2\"],function(){return MODULE_RESULT;});"
            );
        }

        [Fact]
        public void AliasDefaultsToFilenameWithoutExtension()
        {
            var module = new ShimAmdModule(new StubAsset("~/test.js"), new ScriptBundle("~"), "MODULE_RESULT", new string[0], new SimpleJsonSerializer());
            module.Alias.ShouldEqual("test");
        }

        string[] dependencies = new string[0];

        void GivenDependencies(params string[] dependencies)
        {
            this.dependencies = dependencies;
        }

        void AssertTransform(string input, string moduleResult, string expectedOutput)
        {
            var module = new ShimAmdModule(
                new StubAsset("~/test.js"),
                new ScriptBundle("~"),
                moduleResult,
                dependencies,
                new SimpleJsonSerializer()
            );

            var outputStreamFactory = module.Transform(() => input.AsStream(), null);
            var output = outputStreamFactory().ReadToEnd();

            output.ShouldEqual(expectedOutput);
        }
    }
}