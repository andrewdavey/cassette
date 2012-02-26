using System;
using Cassette.Configuration;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptBundleHtmlRenderer_Tests
    {
        public ExternalScriptBundleHtmlRenderer_Tests()
        {
            settings = new CassetteSettings("")
            {
                IsDebuggingEnabled = false
            };
        }

        readonly CassetteSettings settings;

        [Fact]
        public void WhenRenderExternalScriptBundle_ThenHtmlIsScriptElement()
        {
            var bundle = new ExternalScriptBundle("http://test.com/");
            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, settings);
            var html = renderer.Render(bundle);

            html.ShouldEqual("<script src=\"http://test.com/\" type=\"text/javascript\"></script>");
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithCondition_ThenHtmlIsScriptElementWithConditional()
        {
            var bundle = new ExternalScriptBundle("http://test.com/") {Condition = "CONDITION"};
            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, settings);
            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<!--[if CONDITION]>" + Environment.NewLine +
                "<script src=\"http://test.com/\" type=\"text/javascript\"></script>" + Environment.NewLine +
                "<![endif]-->");
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithHtmlAttributes_ThenHtmlIsScriptElementWithExtraAttributes()
        {
            var bundle = new ExternalScriptBundle("http://test.com/");
            bundle.HtmlAttributes["class"] = "foo";
            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, settings);
            var html = renderer.Render(bundle);

            html.ShouldEqual("<script src=\"http://test.com/\" type=\"text/javascript\" class=\"foo\"></script>");
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithLocalAssetsAndIsDebugMode_ThenFallbackRendererUsed()
        {
            var bundle = new ExternalScriptBundle("http://test.com/", "test");
            bundle.Assets.Add(Mock.Of<IAsset>());
            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();
            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns(("FALLBACK"));
            settings.IsDebuggingEnabled = true;

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, settings);
            var html = renderer.Render(bundle);

            html.ShouldEqual("FALLBACK");
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithFallbackAsset_ThenHtmlContainsFallbackScript()
        {
            var bundle = new ExternalScriptBundle("http://test.com/", "test", "CONDITION");
            var asset = new Mock<IAsset>();
            bundle.Assets.Add(asset.Object);

            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();
            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns(("FALLBACK"));

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, settings);
            var html = renderer.Render(bundle);

            html.ShouldEqual(
                "<script src=\"http://test.com/\" type=\"text/javascript\"></script>" + Environment.NewLine +
                "<script type=\"text/javascript\">" + Environment.NewLine +
                "if(CONDITION){" + Environment.NewLine +
                "document.write(unescape('FALLBACK'));" + Environment.NewLine +
                "}" + Environment.NewLine +
                "</script>"
            );
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithFallbackAsset_ThenHtmlEscapesFallbackScriptTags()
        {
            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();
            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, settings);
            var bundle = new ExternalScriptBundle("http://test.com/", "test", "CONDITION");
            var asset = new Mock<IAsset>();
            bundle.Assets.Add(asset.Object);

            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns(("<script></script>"));

            var html = renderer.Render(bundle);

            html.ShouldContain("%3Cscript%3E%3C/script%3E");
        }

        [Fact]
        public void GivenExternalScriptBundleWithFallbackAssetsAndDebugMode_WhenRender_ThenOnlyOutputFallbackScripts()
        {
            settings.IsDebuggingEnabled = true;

            var bundle = new ExternalScriptBundle("http://test.com/", "test", "CONDITION");
            var asset = new Mock<IAsset>();
            bundle.Assets.Add(asset.Object);

            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();
            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns(("<script></script>"));

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, settings);
            var html = renderer.Render(bundle);

            html.ShouldEqual("<script></script>");
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithNoLocalAssetsAndIsDebugMode_ThenNormalScriptElementIsReturned()
        {
            var bundle = new ExternalScriptBundle("http://test.com/", "test");
            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();
            settings.IsDebuggingEnabled = true;

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, settings);
            var html = renderer.Render(bundle);

            html.ShouldEqual("<script src=\"http://test.com/\" type=\"text/javascript\"></script>");
        }
    }
}