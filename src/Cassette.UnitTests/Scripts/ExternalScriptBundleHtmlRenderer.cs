#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptBundleHtmlRenderer_Tests
    {
        public ExternalScriptBundleHtmlRenderer_Tests()
        {
            application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsDebuggingEnabled)
                       .Returns(false);
        }

        readonly Mock<ICassetteApplication> application;

        [Fact]
        public void WhenRenderExternalScriptBundle_ThenHtmlIsScriptElement()
        {
            var bundle = new ExternalScriptBundle("http://test.com/");
            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, application.Object);
            var html = renderer.Render(bundle).ToHtmlString();

            html.ShouldEqual("<script src=\"http://test.com/\" type=\"text/javascript\"></script>");
        }

        [Fact]
        public void WhenRenderExternalScriptBundleWithLocalAssetsAndIsDebugMode_ThenFallbackRendererUsed()
        {
            var bundle = new ExternalScriptBundle("http://test.com/", "test");
            bundle.Assets.Add(Mock.Of<IAsset>());
            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();
            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns(new HtmlString("FALLBACK"));
            application.SetupGet(a => a.IsDebuggingEnabled)
                       .Returns(true);

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, application.Object);
            var html = renderer.Render(bundle).ToHtmlString();

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
                            .Returns(new HtmlString("FALLBACK"));

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, application.Object);
            var html = renderer.Render(bundle).ToHtmlString();

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
            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, application.Object);
            var bundle = new ExternalScriptBundle("http://test.com/", "test", "CONDITION");
            var asset = new Mock<IAsset>();
            bundle.Assets.Add(asset.Object);

            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns(new HtmlString("<script></script>"));

            var html = renderer.Render(bundle).ToHtmlString();

            html.ShouldContain("%3Cscript%3E%3C/script%3E");
        }

        [Fact]
        public void GivenExternalScriptBundleWithFallbackAssetsAndDebugMode_WhenRender_ThenOnlyOutputFallbackScripts()
        {
            application.SetupGet(a => a.IsDebuggingEnabled).Returns(true);

            var bundle = new ExternalScriptBundle("http://test.com/", "test", "CONDITION");
            var asset = new Mock<IAsset>();
            bundle.Assets.Add(asset.Object);

            var fallbackRenderer = new Mock<IBundleHtmlRenderer<ScriptBundle>>();
            fallbackRenderer.Setup(r => r.Render(bundle))
                            .Returns(new HtmlString("<script></script>"));

            var renderer = new ExternalScriptBundleHtmlRenderer(fallbackRenderer.Object, application.Object);
            var html = renderer.Render(bundle).ToHtmlString();

            html.ShouldEqual("<script></script>");
        }
    }
}