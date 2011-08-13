using Should;
using Xunit;
using Moq;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateModuleFactory_Test
    {
        [Fact]
        public void CreateModule_ReturnsHtmlTemplateModuleWithDirectorySet()
        {
            var factory = new HtmlTemplateModuleFactory(Mock.Of<IFileSystem>());
            var module = factory.CreateModule("test");
            module.Directory.ShouldEqual("test");
        }
    }
}
