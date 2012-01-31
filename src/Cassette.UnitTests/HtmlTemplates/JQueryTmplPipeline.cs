using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline_Tests
    {
        [Fact]
        public void WhenProcessBundle_ThenBundleContentTypeIsTextJavascript()
        {
            var pipeline = new JQueryTmplPipeline();
            var bundle = new HtmlTemplateBundle("~/");

            pipeline.Process(bundle, new CassetteSettings(""));

            bundle.ContentType.ShouldEqual("text/javascript");
        }

        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            var pipeline = new JQueryTmplPipeline();
            var bundle = new HtmlTemplateBundle("~");

            pipeline.Process(bundle, new CassetteSettings(""));

            bundle.Hash.ShouldNotBeNull();
        }

        [Fact]
        public void GivenBundleIsFromCache_WhenProcessBundle_ThenRendererStillAssigned()
        {
            var pipeline = new JQueryTmplPipeline();
            var bundle = new HtmlTemplateBundle("~") { IsFromCache = true };

            pipeline.Process(bundle, new CassetteSettings(""));

            bundle.Renderer.ShouldNotBeNull();
        }
    }
}