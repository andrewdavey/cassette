using Cassette.TinyIoC;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplatePipeline_Tests
    {
        readonly HtmlTemplatePipeline pipeline;
        readonly HtmlTemplateBundle bundle;

        public HtmlTemplatePipeline_Tests()
        {
            bundle = new HtmlTemplateBundle("~");
            var container = new TinyIoCContainer();
            container.Register<IHtmlTemplateIdStrategy>(new DefaultHtmlTemplateIdStrategy());
            pipeline = new HtmlTemplatePipeline(container);   
        }

        [Fact]
        public void WhenProcess_ThenItAssignsInlineHtmlTemplateBundleRenderer()
        {
            pipeline.Process(bundle);

            bundle.Renderer.ShouldBeType<InlineHtmlTemplateBundleRenderer>();
        }
        
        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            pipeline.Process(bundle);

            bundle.Hash.ShouldNotBeNull();
        }
    }
}