using System;
using System.Collections.Generic;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class DebugScriptBundleHtmlRenderer_Tests
    {
        [Fact]
        public void GivenBundleWithTwoAssets_WhenRenderBundle_ThenScriptsElementReturnedForEachAsset()
        {
            var bundle = new ScriptBundle("~/test");
            bundle.Assets.Add(new StubAsset());
            bundle.Assets.Add(new StubAsset());

            var urlGenerator = new Mock<IUrlGenerator>();
            var assetUrls = new Queue<string>(new[] { "asset1", "asset2" });
            urlGenerator.Setup(g => g.CreateAssetUrl(It.IsAny<IAsset>()))
                        .Returns(assetUrls.Dequeue);

            var renderer = new DebugScriptBundleHtmlRenderer(urlGenerator.Object);

            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<script src=\"asset1\" type=\"text/javascript\"></script>" + 
                Environment.NewLine +
                "<script src=\"asset2\" type=\"text/javascript\"></script>"
            );
        }

        [Fact]
        public void GivenScriptCondition_WhenRender_ThenConditionalCommentWrapsScripts()
        {
            var bundle = new ScriptBundle("~/test") {Condition = "CONDITION"};
            bundle.Assets.Add(new StubAsset());
            bundle.Assets.Add(new StubAsset());

            var urlGenerator = new Mock<IUrlGenerator>();
            var assetUrls = new Queue<string>(new[] { "asset1", "asset2" });
            urlGenerator.Setup(g => g.CreateAssetUrl(It.IsAny<IAsset>()))
                        .Returns(assetUrls.Dequeue);

            var renderer = new DebugScriptBundleHtmlRenderer(urlGenerator.Object);

            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<!--[if CONDITION]>" + Environment.NewLine +
                "<script src=\"asset1\" type=\"text/javascript\"></script>" + Environment.NewLine +
                "<script src=\"asset2\" type=\"text/javascript\"></script>" + Environment.NewLine +
                "<![endif]-->"
            );
        }
    }
}