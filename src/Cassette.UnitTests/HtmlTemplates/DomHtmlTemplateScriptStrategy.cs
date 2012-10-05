using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class DomHtmlTemplateScriptStrategy_Tests
    {
        readonly DomHtmlTemplateScriptStrategy strategy;

        public DomHtmlTemplateScriptStrategy_Tests()
        {
            strategy = new DomHtmlTemplateScriptStrategy(new SimpleJsonSerializer());
        }

        [Fact]
        public void GeneratesCallsToAddTemplateFunction()
        {
            var javaScript = strategy.DefineTemplate("template-id", "template-content");
            javaScript.ShouldEqual("addTemplate(\"template-id\",\"template-content\");");
        }

        [Fact]
        public void DoubleQuotesInStringsAreEscaped()
        {
            var javaScript = strategy.DefineTemplate("template-id", "template\"content");
            javaScript.ShouldEqual("addTemplate(\"template-id\",\"template\\\"content\");");
        }

        [Fact]
        public void WrapsTemplateDefinitionsInIifeThatContainsAddTemplateFunction()
        {
            var javaScript = strategy.WrapTemplates("TEMPLATES;");
            javaScript.ShouldEqual(@"(function(d){
var addTemplate = function(id, content) {{
    var script = d.createElement('script');
    script.type = 'text/html';
    script.id = id;
    if (typeof script.textContent !== 'undefined') {{
        script.textContent = content;
    }} else {{
        script.innerText = content;
    }}
    var x = d.getElementsByTagName('script')[0];
    x.parentNode.insertBefore(script, x);
}};
TEMPLATES;
}(document));");
        }
    }
}