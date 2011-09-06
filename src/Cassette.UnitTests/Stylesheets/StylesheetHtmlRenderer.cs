using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Should;
using Xunit;
using Moq;

namespace Cassette.Stylesheets
{
    public class StylesheetHtmlRenderer_Tests
    {
        [Fact]
        public void GivenModule_WhenRender_ThenHtmlLinkReturned()
        {
            var module = new StylesheetModule("~/tests");
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateModuleUrl(module))
                        .Returns("URL")
                        .Verifiable();

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(module).ToHtmlString();

            html.ShouldEqual("<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\"/>");

            urlGenerator.VerifyAll();
        }

        [Fact]
        public void GivenModuleWithMedia_WhenRender_ThenHtmlLinkWithMediaAttributeReturned()
        {
            var module = new StylesheetModule("~/tests")
            {
                Media = "MEDIA"
            };
            var urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateModuleUrl(module))
                        .Returns("URL");

            var renderer = new StylesheetHtmlRenderer(urlGenerator.Object);
            var html = renderer.Render(module).ToHtmlString();

            html.ShouldEqual("<link href=\"URL\" type=\"text/css\" rel=\"stylesheet\" media=\"MEDIA\"/>");
        }
    }
}
