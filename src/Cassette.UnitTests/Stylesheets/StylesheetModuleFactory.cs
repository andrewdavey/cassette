using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetModuleFactory_Tests
    {
        [Fact]
        public void CreateModuleReturnsStylesheetModuleWithDirectorySet()
        {
            var factory = new StylesheetModuleFactory();
            var module = factory.CreateModule("test");
            module.Directory.ShouldEqual("test");
        }

        [Fact]
        public void CreateExternalModuleReturnsExternalsStylesheetModule()
        {
            var factory = new StylesheetModuleFactory();
            var module = factory.CreateExternalModule("http://test.com/test.css");
            module.ShouldBeType<ExternalStylesheetModule>();
        }
    }
}