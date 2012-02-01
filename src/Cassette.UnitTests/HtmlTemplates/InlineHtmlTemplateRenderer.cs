using System;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class InlineHtmlTemplateRenderer_Tests
    {
        readonly InlineHtmlTemplateBundleRenderer render;

        public InlineHtmlTemplateRenderer_Tests()
        {
            render = new InlineHtmlTemplateBundleRenderer();            
        }

        [Fact]
        public void GivenHtmlTemplateBundleWithNoAssets_WhenRender_ThenReturnEmptyString()
        {
            var bundle = new HtmlTemplateBundle("~");
            var html = render.Render(bundle);
            html.ShouldEqual("");
        }

        [Fact]
        public void GivenHtmlTemplateBundleWithOneAsset_WhenRender_ThenReturnsAssetsOpenStreamResult()
        {
            var bundle = new HtmlTemplateBundle("~");
            bundle.Assets.Add(StubAsset("content"));

            var html = render.Render(bundle);
            html.ShouldEqual("content");
        }

        [Fact]
        public void GivenHtmlTemplateBundleWithTwoAssets_WhenRender_ThenReturnsEachAssetsOpenStreamResultSeparatedByNewLine()
        {
            var bundle = new HtmlTemplateBundle("~");
            bundle.Assets.Add(StubAsset("content1"));
            bundle.Assets.Add(StubAsset("content2"));

            var html = render.Render(bundle);
            html.ShouldEqual("content1" + Environment.NewLine + "content2");
        }

        IAsset StubAsset(string content)
        {
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.OpenStream()).Returns(content.AsStream());
            return asset.Object;
        }
    }
}