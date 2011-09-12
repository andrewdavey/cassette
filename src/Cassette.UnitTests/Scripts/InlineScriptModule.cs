using System;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class InlineScriptModule_Tests
    {
        [Fact]
        public void GivenInlineScriptModuleWithContent_WhenRender_ThenScriptElementCreatedWithContent()
        {
            var module = new InlineScriptModule("var x = 1;");
            var html = module.Render().ToHtmlString();
            html.ShouldEqual(
                "<script type=\"text/javascript\">" + Environment.NewLine + 
                "var x = 1;" + Environment.NewLine + 
                "</script>"
            );
        }
    }
}
