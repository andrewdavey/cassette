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

using Cassette.Configuration;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetHtmlRenderer_Tests
    {
        readonly CassetteSettings settings;
        readonly Mock<IBundleHtmlRenderer<StylesheetBundle>> fallbackRenderer;
        readonly ExternalStylesheetHtmlRenderer renderer;
        readonly ExternalStylesheetBundle bundle;

        public ExternalStylesheetHtmlRenderer_Tests()
        {
            settings = new CassetteSettings("");
            fallbackRenderer = new Mock<IBundleHtmlRenderer<StylesheetBundle>>(); 
            renderer = new ExternalStylesheetHtmlRenderer(fallbackRenderer.Object, settings);
            bundle = new ExternalStylesheetBundle("http://test.com/");
        }

        [Fact]
        public void GivenApplicationInProduction_WhenRender_ThenLinkElementReturnedWithBundleUrlAsHref()
        {
            settings.IsDebuggingEnabled = false;
            
            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"http://test.com/\" type=\"text/css\" rel=\"stylesheet\"/>");
        }

        [Fact]
        public void GivenApplicationInProduction_WhenRenderBundleWithMedia_ThenLinkElementReturnedWithMediaAttribute()
        {
            settings.IsDebuggingEnabled = false;
            bundle.Media = "print";

            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"http://test.com/\" type=\"text/css\" rel=\"stylesheet\" media=\"print\"/>");
        }

        [Fact]
        public void GivenApplicationInDebugMode_WhenRenderBundleWithAssets_ThenFallbackRendererIsUsed()
        {
            settings.IsDebuggingEnabled = true;
            bundle.Assets.Add(Mock.Of<IAsset>());

            renderer.Render(bundle);

            fallbackRenderer.Verify(r => r.Render(bundle));
        }
    }
}
