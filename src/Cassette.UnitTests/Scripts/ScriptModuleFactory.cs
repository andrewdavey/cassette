using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptModuleFactory_Tests
    {
        [Fact]
        public void CreateModuleReturnsScriptModule()
        {
            var factory = new ScriptModuleFactory();
            var module = factory.CreateModule("~/test");
            module.ShouldBeType<ScriptModule>();
        }

        [Fact]
        public void CreateModuleAssignsScriptModuleDirectory()
        {
            var factory = new ScriptModuleFactory();
            var module = factory.CreateModule("~/test");
            module.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void CreateExternalModuleReturnsExternalScriptModule()
        {
            new ScriptModuleFactory().CreateExternalModule("http://test.com/api.js").ShouldBeType<ExternalScriptModule>();
        }
    }
}
