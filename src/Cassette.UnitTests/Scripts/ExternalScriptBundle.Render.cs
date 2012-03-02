using System;
using System.IO;
using Cassette.Configuration;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptBundleRender_Tests
    {
        public ExternalScriptBundleRender_Tests()
        {
            settings = new CassetteSettings("")
            {
                IsDebuggingEnabled = false,
                UrlGenerator = Mock.Of<IUrlGenerator>()
            };
            fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();
        }

        readonly CassetteSettings settings;
        readonly Mock<IBundleHtmlRenderer<ScriptBundle>> fallbackRenderer;

        [Fact]
        public void WhenRenderExternalScriptBundle_ThenHtmlIsScriptElement()
        {
            var bundle = new ExternalScriptBundle("http://test.com/");
            var html = Render(bundle);
            html.ShouldEqual("<script src=\"http://test.com/\" type=\"text/javascript\"></script>");
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithCondition_ThenHtmlIsScriptElementWithConditional()
        {
            var bundle = new ExternalScriptBundle("http://test.com/") {Condition = "CONDITION"};
            var html = Render(bundle);

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

            var html = Render(bundle);

            html.ShouldEqual("<script src=\"http://test.com/\" type=\"text/javascript\" class=\"foo\"></script>");
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithLocalAssetsAndIsDebugMode_ThenFallbackRendererUsed()
        {
            var bundle = new ExternalScriptBundle("http://test.com/", "test");
            bundle.Assets.Add(StubAsset());
            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns("FALLBACK");
            settings.IsDebuggingEnabled = true;

            var html = Render(bundle);

            html.ShouldEqual("FALLBACK");
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithFallbackAsset_ThenHtmlContainsFallbackScript()
        {
            var bundle = new ExternalScriptBundle("http://test.com/", "test", "CONDITION");
            bundle.Assets.Add(StubAsset());

            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns("FALLBACK");

            var html = Render(bundle);

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
            var bundle = new ExternalScriptBundle("http://test.com/", "test", "CONDITION");
            bundle.Assets.Add(StubAsset());

            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns("<script></script>");

            var html = Render(bundle);

            html.ShouldContain("%3Cscript%3E%3C/script%3E");
        }

        [Fact]
        public void GivenExternalScriptBundleWithFallbackAssetsAndDebugMode_WhenRender_ThenOnlyOutputFallbackScripts()
        {
            settings.IsDebuggingEnabled = true;

            var bundle = new ExternalScriptBundle("http://test.com/", "test", "CONDITION");
            bundle.Assets.Add(StubAsset());

            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns("<script></script>");

            var html = Render(bundle);

            html.ShouldEqual("<script></script>");
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithNoLocalAssetsAndIsDebugMode_ThenNormalScriptElementIsReturned()
        {
            var bundle = new ExternalScriptBundle("http://test.com/", "test");
            settings.IsDebuggingEnabled = true;

            var html = Render(bundle);

            html.ShouldEqual("<script src=\"http://test.com/\" type=\"text/javascript\"></script>");
        }

        string Render(ExternalScriptBundle bundle)
        {
            bundle.Process(settings);
            bundle.FallbackRenderer = fallbackRenderer.Object;
            return bundle.Render();
        }

        IAsset StubAsset()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset.js");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            return asset.Object;
        }
    }
}