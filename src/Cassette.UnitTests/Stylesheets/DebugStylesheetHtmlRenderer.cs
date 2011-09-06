using System;
using System.Collections.Generic;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class DebugStylesheetHtmlRenderer_Tests
    {
        [Fact]
        public void GivenModuleWithAssets_WhenRender_ThenLinkForEachAssetIsReturned()
        {
            var module = new StylesheetModule("~/test");
            module.Assets.Add(Mock.Of<IAsset>());
            module.Assets.Add(Mock.Of<IAsset>());

            var urlGenerator = new Mock<IUrlGenerator>();
            var assetUrls = new Queue<string>(new[] { "asset1", "asset2" });
            urlGenerator.Setup(g => g.CreateAssetUrl(It.IsAny<IAsset>()))
                        .Returns(assetUrls.Dequeue);

            var renderer = new DebugStylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(module).ToHtmlString();

            html.ShouldEqual(
                "<link href=\"asset1\" type=\"text/css\" rel=\"stylesheet\"/>" + 
                Environment.NewLine + 
                "<link href=\"asset2\" type=\"text/css\" rel=\"stylesheet\"/>"
            );
        }

        [Fact]
        public void GivenModuleWithAssetsThatIsTransformed_WhenRender_ThenLinkHtmlHasTransformUrlReturned()
        {
            var module = new StylesheetModule("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.HasTransformers)
                 .Returns(true);
            module.Assets.Add(asset.Object);

            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateAssetCompileUrl(module, It.IsAny<IAsset>()))
                        .Returns("URL");

            var renderer = new DebugStylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(module).ToHtmlString();

            html.ShouldEqual(
                "<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\"/>"
            );
        }

        [Fact]
        public void GivenModuleWithMediaAndAssets_WhenRender_ThenLinkForEachAssetIsReturned()
        {
            var module = new StylesheetModule("~/test")
            {
                Media = "MEDIA"
            };
            module.Assets.Add(Mock.Of<IAsset>());
            module.Assets.Add(Mock.Of<IAsset>());

            var urlGenerator = new Mock<IUrlGenerator>();
            var assetUrls = new Queue<string>(new[] { "asset1", "asset2" });
            urlGenerator.Setup(g => g.CreateAssetUrl(It.IsAny<IAsset>()))
                        .Returns(assetUrls.Dequeue);

            var renderer = new DebugStylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(module).ToHtmlString();

            html.ShouldEqual(
                "<link href=\"asset1\" type=\"text/css\" rel=\"stylesheet\" media=\"MEDIA\"/>" +
                Environment.NewLine +
                "<link href=\"asset2\" type=\"text/css\" rel=\"stylesheet\" media=\"MEDIA\"/>"
            );
        }
    }
}
