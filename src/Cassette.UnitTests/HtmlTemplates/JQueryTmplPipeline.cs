using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline_Tests
    {
        readonly JQueryTmplPipeline pipeline;

        public JQueryTmplPipeline_Tests()
        {
            var container = new TinyIoC.TinyIoCContainer();
            container.Register(Mock.Of<IUrlGenerator>());
            container.Register(new CassetteSettings());
            container.Register<IHtmlTemplateIdStrategy>(new HtmlTemplateIdBuilder());
            pipeline = new JQueryTmplPipeline(container);
        }

        [Fact]
        public void WhenProcessBundle_ThenBundleContentTypeIsTextJavascript()
        {
            var bundle = new HtmlTemplateBundle("~/");

            pipeline.Process(bundle);

            bundle.ContentType.ShouldEqual("text/javascript");
        }

        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            var bundle = new HtmlTemplateBundle("~");

            pipeline.Process(bundle);

            bundle.Hash.ShouldNotBeNull();
        }
    }
}