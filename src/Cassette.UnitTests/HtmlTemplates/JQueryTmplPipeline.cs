using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline_Tests
    {
        [Fact]
        public void WhenProcessModule_ThenModuleContentTypeIsTextJavascript()
        {
            var pipeline = new JQueryTmplPipeline();
            var module = new HtmlTemplateModule("~/");

            pipeline.Process(module, Mock.Of<ICassetteApplication>());

            module.ContentType.ShouldEqual("text/javascript");
        }
    }
}