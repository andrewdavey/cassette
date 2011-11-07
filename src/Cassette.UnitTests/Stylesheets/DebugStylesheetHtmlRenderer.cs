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
            var html = renderer.Render(module);

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
            var html = renderer.Render(module);

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
            var html = renderer.Render(module);

            html.ShouldEqual(
                "<link href=\"asset1\" type=\"text/css\" rel=\"stylesheet\" media=\"MEDIA\"/>" +
                Environment.NewLine +
                "<link href=\"asset2\" type=\"text/css\" rel=\"stylesheet\" media=\"MEDIA\"/>"
            );
        }
    }
}

