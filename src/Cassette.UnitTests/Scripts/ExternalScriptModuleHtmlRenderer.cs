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
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptModuleHtmlRenderer_Tests
    {
        public ExternalScriptModuleHtmlRenderer_Tests()
        {
            application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsOutputOptimized)
                       .Returns(true);
        }

        readonly Mock<ICassetteApplication> application;

        [Fact]
        public void WhenRenderExternalScriptModule_ThenHtmlIsScriptElement()
        {
            var module = new ExternalScriptModule("http://test.com/");
            var fallbackRenderer = new Mock<IModuleHtmlRenderer<ScriptModule>>();

            var renderer = new ExternalScriptModuleHtmlRenderer(fallbackRenderer.Object, application.Object);
            var html = renderer.Render(module);

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
                            .Returns("FALLBACK");

            var renderer = new ExternalScriptModuleHtmlRenderer(fallbackRenderer.Object, application.Object);
            var html = renderer.Render(module);

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
            var renderer = new ExternalScriptModuleHtmlRenderer(fallbackRenderer.Object, application.Object);
            var module = new ExternalScriptModule("http://test.com/");
            var asset = new Mock<IAsset>();
            module.AddFallback("CONDITION", new[] { asset.Object });

            fallbackRenderer.Setup(r => r.Render(module))
                            .Returns("<script></script>");

            var html = renderer.Render(module);

            html.ShouldContain("%3Cscript%3E%3C/script%3E");
        }

        [Fact]
        public void GivenExternalScriptModuleWithFallbackAssetsAndApplicationNotOptimized_WhenRender_ThenOnlyOutputFallbackScripts()
        {
            application.SetupGet(a => a.IsOutputOptimized).Returns(false);

            var module = new ExternalScriptModule("http://test.com/");
            var asset = new Mock<IAsset>();
            module.AddFallback("CONDITION", new[] { asset.Object });

            var fallbackRenderer = new Mock<IModuleHtmlRenderer<ScriptModule>>();
            fallbackRenderer.Setup(r => r.Render(module))
                            .Returns("<script></script>");

            var renderer = new ExternalScriptModuleHtmlRenderer(fallbackRenderer.Object, application.Object);
            var html = renderer.Render(module);

            html.ShouldEqual("<script></script>");
        }
    }
}
