using System;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class HtmlTemplateModule_Render_Tests
    {
        [Fact]
        public void GivenApplicationIsNotOutputOptimized_WhenRender_ThenEachAssetIsOutputAsIs()
        {
            var application = new Mock<ICassetteApplication>();
            var module = new HtmlTemplateModule("test");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.SetupGet(a => a.SourceFilename).Returns("template-1.htm");
            asset1.Setup(a => a.OpenStream()).Returns(() => "<p>template 1</p>".AsStream());
            asset2.SetupGet(a => a.SourceFilename).Returns("template-2.htm");
            asset2.Setup(a => a.OpenStream()).Returns(() => "<p>template 2</p>".AsStream());
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);

            module.Render(application.Object).ToHtmlString().ShouldEqual(
                "<p>template 1</p>" + Environment.NewLine +
                "<p>template 2</p>"
            );
        }

        [Fact]
        public void WhenAssetInSubDirectories_ThenRenderTemplateReturnsIdBackSlashesReplacedWithDashes()
        {
            var application = new Mock<ICassetteApplication>();
            var module = new HtmlTemplateModule("test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("sub\\template-1.htm");

            module.RenderTemplate(() => "<p>test</p>".AsStream(), asset.Object).ShouldEqual(
                "<script id=\"sub-template-1\" type=\"text/html\">" + Environment.NewLine +
                "<p>test</p>" + Environment.NewLine +
                "</script>"
            );
        }

        [Fact]
        public void WhenAssetInSubDirectories_ThenRenderTemplateReturnsIdForwardSlashesReplacedWithDashes()
        {
            var application = new Mock<ICassetteApplication>();
            var module = new HtmlTemplateModule("test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("sub/template-2.htm");

            module.RenderTemplate(() => "<p>test</p>".AsStream(), asset.Object).ShouldEqual(
                "<script id=\"sub-template-2\" type=\"text/html\">" + Environment.NewLine +
                "<p>test</p>" + Environment.NewLine +
                "</script>"
            );
        }

        [Fact]
        public void GivenApplicationIsOutputOptimized_WhenRender_ThenOutputIsDirectFromSingleAsset()
        {
            var application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsOutputOptimized).Returns(true);
            var module = new HtmlTemplateModule("test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.OpenStream()).Returns(() => "output".AsStream());
            module.Assets.Add(asset.Object);

            module.Render(application.Object).ToHtmlString().ShouldEqual("output");
        }
    }
}
