using Should;
using Xunit;
using Moq;

namespace Cassette
{
    public class StylesheetModuleFactory_Tests
    {
        [Fact]
        public void CreateModule_ReturnsStylesheetModuleWithDirectorySet()
        {
            var factory = new StylesheetModuleFactory(Mock.Of<IFileSystem>());
            var module = factory.CreateModule("test");
            module.Directory.ShouldEqual("test");
        }
    }
}
