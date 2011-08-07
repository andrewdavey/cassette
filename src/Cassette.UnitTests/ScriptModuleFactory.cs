using Should;
using Xunit;

namespace Cassette
{
    public class ScriptModuleFactory_Tests
    {
        [Fact]
        public void CreateModuleReturnsScriptModule()
        {
            var factory = new ScriptModuleFactory(_ => null);
            var module = factory.CreateModule("test");
            module.ShouldBeType<ScriptModule>();
        }

        [Fact]
        public void CreateModuleAssignsScriptModuleDirectory()
        {
            var factory = new ScriptModuleFactory(_ => null);
            var module = factory.CreateModule("test");
            module.Directory.ShouldEqual("test");
        }
    }
}
