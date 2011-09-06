using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class KnockoutJQueryTmplPipeline_Tests
    {
        [Fact]
        public void WhenProcessModule_ThenModuleContentTypeIsTextJavascript()
        {
            var pipeline = new KnockoutJQueryTmplPipeline();
            var module = new HtmlTemplateModule("~/");

            pipeline.Process(module, Mock.Of<ICassetteApplication>());

            module.ContentType.ShouldEqual("text/javascript");
        }
    }
}