using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class KnockoutJQueryTmplPipeline_Tests
    {
        readonly KnockoutJQueryTmplPipeline pipeline;

        public KnockoutJQueryTmplPipeline_Tests()
        {
            var container = new TinyIoC.TinyIoCContainer();
            container.Register(Mock.Of<IUrlGenerator>());
            container.Register(new CassetteSettings());
            container.Register<IHtmlTemplateIdStrategy>(new HtmlTemplateIdBuilder());
            pipeline = new KnockoutJQueryTmplPipeline(container);
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