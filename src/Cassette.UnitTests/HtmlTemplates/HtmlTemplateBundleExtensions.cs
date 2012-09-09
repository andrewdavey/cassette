using Cassette.BundleProcessing;
using Moq;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundleExtensionsTests
    {
        [Fact]
        public void AsJavaScriptReplacesPipelineWithJavaScriptHtmlTemplatePipeline()
        {
            var currentPipeline = new Mock<IBundlePipeline<HtmlTemplateBundle>>();
            var bundle = new HtmlTemplateBundle("~")
            {
                Pipeline = currentPipeline.Object
            };

            bundle.AsJavaScript();

            currentPipeline.Verify(p => p.ReplaceWith<JavaScriptHtmlTemplatePipeline>());
        }
    }
}