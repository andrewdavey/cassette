using System;
using System.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptModuleHtmlRenderer_Tests
    {
        [Fact]
        public void WhenRenderExternalScriptModule_ThenHtmlIsScriptElement()
        {
            var module = new ExternalScriptModule("http://test.com/");
            var fallbackRenderer = new Mock<IModuleHtmlRenderer<ScriptModule>>();

            var renderer = new ExternalScriptModuleHtmlRenderer(fallbackRenderer.Object);
            var html = renderer.Render(module).ToHtmlString();

            html.ShouldEqual("<script src=\"http://test.com/\" type=\"text/javascript\"></script>");
        }

        [Fact]
        public void WhenRenderExternalScriptModuleWithFallbackAsset_ThenHtmlContainsFallbackScript()
        {
            var module = new ExternalScriptModule("http://test.com/");
            var asset = new Mock<IAsset>();
            module.AddFallback("CONDITION", new[] { asset.Object });

            var fallbackRenderer = new Mock<IModuleHtmlRenderer<ScriptModule>>();
            fallbackRenderer.Setup(r => r.Render(module))
                            .Returns(new HtmlString("FALLBACK"));

            var renderer = new ExternalScriptModuleHtmlRenderer(fallbackRenderer.Object);
            var html = renderer.Render(module).ToHtmlString();

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
        public void WhenRenderExternalScriptModuleWithFallbackAsset_ThenHtmlEscapesFallbackScriptTags()
        {
            var fallbackRenderer = new Mock<IModuleHtmlRenderer<ScriptModule>>();
            var renderer = new ExternalScriptModuleHtmlRenderer(fallbackRenderer.Object);
            var module = new ExternalScriptModule("http://test.com/");
            var asset = new Mock<IAsset>();
            module.AddFallback("CONDITION", new[] { asset.Object });

            fallbackRenderer.Setup(r => r.Render(module))
                            .Returns(new HtmlString("<script></script>"));

            var html = renderer.Render(module).ToHtmlString();

            html.ShouldContain("%3Cscript%3E%3C/script%3E");
        }
    }
}