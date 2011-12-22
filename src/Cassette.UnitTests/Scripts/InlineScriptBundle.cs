using System;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class InlineScriptBundle_Tests
    {
        [Fact]
        public void GivenInlineScriptBundleWithContent_WhenRender_ThenScriptElementCreatedWithContent()
        {
            var bundle = new InlineScriptBundle("var x = 1;");
            var html = bundle.Render();
            html.ShouldEqual(
                "<script type=\"text/javascript\">" + Environment.NewLine + 
                "var x = 1;" + Environment.NewLine + 
                "</script>"
            );
        }
    }
}

