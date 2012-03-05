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
        public void GivenBundleWithAssets_WhenRender_ThenLinkForEachAssetIsReturned()
        {
            var bundle = new StylesheetBundle("~/test");
            bundle.Assets.Add(new StubAsset());
            bundle.Assets.Add(new StubAsset());

            var urlGenerator = new Mock<IUrlGenerator>();
            var assetUrls = new Queue<string>(new[] { "asset1", "asset2" });
            urlGenerator.Setup(g => g.CreateAssetUrl(It.IsAny<IAsset>()))
                        .Returns(assetUrls.Dequeue);

            var renderer = new DebugStylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<link href=\"asset1\" type=\"text/css\" rel=\"stylesheet\"/>" + 
                Environment.NewLine + 
                "<link href=\"asset2\" type=\"text/css\" rel=\"stylesheet\"/>"
            );
        }

        [Fact]
        public void GivenBundleWithMediaAndAssets_WhenRender_ThenLinkForEachAssetIsReturned()
        {
            var bundle = new StylesheetBundle("~/test")
            {
                Media = "MEDIA"
            };
            bundle.Assets.Add(new StubAsset());
            bundle.Assets.Add(new StubAsset());

            var urlGenerator = new Mock<IUrlGenerator>();
            var assetUrls = new Queue<string>(new[] { "asset1", "asset2" });
            urlGenerator.Setup(g => g.CreateAssetUrl(It.IsAny<IAsset>()))
                        .Returns(assetUrls.Dequeue);

            var renderer = new DebugStylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<link href=\"asset1\" type=\"text/css\" rel=\"stylesheet\" media=\"MEDIA\"/>" +
                Environment.NewLine +
                "<link href=\"asset2\" type=\"text/css\" rel=\"stylesheet\" media=\"MEDIA\"/>"
            );
        }

        [Fact]
        public void GivenStylesheetCondition_WhenRender_ThenConditionalCommentWrapsLinks()
        {
            var bundle = new StylesheetBundle("~/test")
            {
                Condition = "CONDITION"
            };
            bundle.Assets.Add(new StubAsset());
            bundle.Assets.Add(new StubAsset());

            var urlGenerator = new Mock<IUrlGenerator>();
            var assetUrls = new Queue<string>(new[] { "asset1", "asset2" });
            urlGenerator.Setup(g => g.CreateAssetUrl(It.IsAny<IAsset>()))
                        .Returns(assetUrls.Dequeue);

            var renderer = new DebugStylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<!--[if CONDITION]>" + Environment.NewLine +
                "<link href=\"asset1\" type=\"text/css\" rel=\"stylesheet\"/>" +
                Environment.NewLine +
                "<link href=\"asset2\" type=\"text/css\" rel=\"stylesheet\"/>" + Environment.NewLine +
                "<![endif]-->"
            );
        }
    }
}