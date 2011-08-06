using Should;
using Xunit;

namespace Cassette
{
    public class ScriptModuleFactory_Tests
    {
        [Fact]
        public void CreateModuleReturnsScriptModule()
        {
            var factory = new ScriptModuleFactory();
            var module = factory.CreateModule("c:\\test");
            module.ShouldBeType<ScriptModule>();
        }

        [Fact]
        public void CreateModuleAssignsScriptModuleDirectory()
        {
            var factory = new ScriptModuleFactory();
            var module = factory.CreateModule("c:\\test");
            module.Directory.ShouldEqual("c:\\test");
        }
    }
}
