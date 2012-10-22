using System;
using Cassette.Scripts;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class NamedModuleTests
    {
        [Fact]
        public void ModulePathIsAssignedFromConstructorArgument()
        {
            var asset = new StubAsset("~/test.js");
            var bundle = new ScriptBundle("~");
            var module = new NamedModule(asset, bundle, "module/path");

            module.ModulePath.ShouldEqual("module/path");
        }

        [Fact]
        public void ModulePathIsRequired()
        {
            var asset = new StubAsset("~/test.js");
            var bundle = new ScriptBundle("~");
            Assert.Throws<ArgumentNullException>(
                () => new NamedModule(asset, bundle, null)
            );
        }
    }
}