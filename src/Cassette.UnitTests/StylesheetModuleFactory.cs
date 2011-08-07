using Should;
using Xunit;

namespace Cassette
{
    public class StylesheetModuleFactory_Tests
    {
        [Fact]
        public void CreateModule_ReturnsStylesheetModuleWithDirectorySet()
        {
            var factory = new ScriptModuleFactory(_ => "");
            var module = factory.CreateModule("test");
            module.Directory.ShouldEqual("test");
        }
    }
}
