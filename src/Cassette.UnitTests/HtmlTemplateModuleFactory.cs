using Should;
using Xunit;

namespace Cassette
{
    public class HtmlTemplateModuleFactory_Test
    {
        [Fact]
        public void CreateModule_ReturnsHtmlTemplateModuleWithDirectorySet()
        {
            var factory = new HtmlTemplateModuleFactory(_ => "");
            var module = factory.CreateModule("test");
            module.Directory.ShouldEqual("test");
        }
    }
}
