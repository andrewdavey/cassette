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

using Should;
using Xunit;
using Moq;

namespace Cassette.Stylesheets
{
    public class StylesheetHtmlRenderer_Tests
    {
        [Fact]
        public void GivenBundle_WhenRender_ThenHtmlLinkReturned()
        {
            var bundle = new StylesheetBundle("~/tests");
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                .Returns("URL")
                .Verifiable();

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\"/>");

            urlGenerator.VerifyAll();
        }

        [Fact]
        public void GivenBundleWithMedia_WhenRender_ThenHtmlLinkWithMediaAttributeReturned()
        {
            var bundle = new StylesheetBundle("~/tests")
            {
                Media = "MEDIA"
            };
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle))
                .Returns("URL");

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(bundle);

            html.ShouldEqual("<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\" media=\"MEDIA\"/>");
        }
    }
}