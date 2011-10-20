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
            bundle.AddAssets(new[] { Mock.Of<IAsset>(), Mock.Of<IAsset>() }, true);

            var urlGenerator = new Mock<IUrlGenerator>();
            var assetUrls = new Queue<string>(new[] { "asset1", "asset2" });
            urlGenerator.Setup(g => g.CreateAssetUrl(It.IsAny<IAsset>()))
                        .Returns(assetUrls.Dequeue);

            var renderer = new DebugScriptBundleHtmlRenderer(urlGenerator.Object);

            var html = renderer.Render(bundle).ToHtmlString();

            html.ShouldEqual(
                "<script src=\"asset1\" type=\"text/javascript\"></script>" + 
                Environment.NewLine +
                "<script src=\"asset2\" type=\"text/javascript\"></script>"
            );
        }

        [Fact]
        public void GivenBundleWithTransformedAsset_WhenRenderBundle_ThenScriptElementHasCompiledUrl()
        {
            var bundle = new ScriptBundle("~/test");
            var asset = new Mock<IAsset>();
            bundle.Assets.Add(asset.Object);
            asset.SetupGet(a => a.HasTransformers)
                 .Returns(true);

            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateAssetCompileUrl(asset.Object))
                        .Returns("COMPILED-URL");

            var renderer = new DebugScriptBundleHtmlRenderer(urlGenerator.Object);

            var html = renderer.Render(bundle).ToHtmlString();
            html.ShouldEqual("<script src=\"COMPILED-URL\" type=\"text/javascript\"></script>");
        }
    }
}

