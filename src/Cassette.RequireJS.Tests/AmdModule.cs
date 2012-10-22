using Cassette.Scripts;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class AmdModuleTests
    {
        [Fact]
        public void ModulePathArgumentIsInsertedIntoDefineCall()
        {
            AssertTransform(
                "define([],function(){})",
                "~/example.js",
                "define(\"example\",[],function(){})"
            );
        }

        [Fact]
        public void OnlyDefineFunctionCallIsRewritten()
        {
            AssertTransform(
                "other([],function(){})",
                "~/example.js",
                "other([],function(){})"
            );
        }

        [Fact]
        public void NonTopLevelDefineCallIsRewritten()
        {
            AssertTransform(
                "(function(){define([],function(){})})()",
                "~/example.js",
                "(function(){define(\"example\",[],function(){})})()"
            );
        }

        [Fact]
        public void DontInsertModulePathWhenItsAlreadyPresent()
        {
            AssertTransform(
                "define(\"defined/path\",[],function(){})", 
                "example",
                "define(\"defined/path\",[],function(){})"
            );
        }

        void AssertTransform(string input, string path, string expectedOutput)
        {
            var module = new AmdModule(new StubAsset(path), new ScriptBundle("~/"), "alias");
            var outputStreamFactory = module.Transform(() => input.AsStream(), null);
            var output = outputStreamFactory().ReadToEnd();
            output.ShouldEqual(expectedOutput);
        }
    }
}