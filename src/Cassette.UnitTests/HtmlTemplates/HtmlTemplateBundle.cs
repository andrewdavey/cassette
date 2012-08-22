using Cassette.BundleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundle_Tests
    {
        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new HtmlTemplateBundle("~");
            var pipeline = new Mock<IBundlePipeline<HtmlTemplateBundle>>();
            var settings = new CassetteSettings();
            bundle.Pipeline = pipeline.Object;

            bundle.Process(settings);

            pipeline.Verify(p => p.Process(bundle));
        }

        [Fact]
        public void RenderCallsRenderer()
        {
            var bundle = new HtmlTemplateBundle("~");
            var renderer = new Mock<IBundleHtmlRenderer<HtmlTemplateBundle>>();
            bundle.Renderer = renderer.Object;

            bundle.Render();

            renderer.Verify(p => p.Render(bundle));
        }

        [Fact]
        public void GetTemplateIdReturnsAssetFilenameWithoutExtension()
        {
            var bundle = new HtmlTemplateBundle("~");
            var id = bundle.GetTemplateId(new StubAsset("~/test.htm"));
            id.ShouldEqual("test");
        }

        [Fact]
        public void GetTemplateIdReturnsAssetFilenameWithoutExtension_WhenTemplateBundlePerFile()
        {
            var bundle = new HtmlTemplateBundle("~/test.htm");
            var id = bundle.GetTemplateId(new StubAsset("~/test.htm"));
            id.ShouldEqual("test");
        }

        [Fact]
        public void GetTemplateIdIncludesDirectoryAndFilenameSeparatedWithHyphen()
        {
            var bundle = new HtmlTemplateBundle("~");
            var id = bundle.GetTemplateId(new StubAsset("~/dir/test.htm"));
            id.ShouldEqual("dir-test");
        }

        [Fact]
        public void GivenAssetNotSubPathOfBundlePath_ThenGetTemplateIdReturnsJustFilename()
        {
            var bundle = new HtmlTemplateBundle("~/bundle");
            var id = bundle.GetTemplateId(new StubAsset("~/test.htm"));
            id.ShouldEqual("test");
        }
    }
}