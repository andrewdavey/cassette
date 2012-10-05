using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class GlobalVarHtmlTemplateScriptStrategy_Tests
    {
        readonly GlobalVarHtmlTemplateScriptStrategy strategy;

        public GlobalVarHtmlTemplateScriptStrategy_Tests()
        {
            strategy = new GlobalVarHtmlTemplateScriptStrategy("JST", new SimpleJsonSerializer());
        }

        [Fact]
        public void AddsTemplateToAliasedGlobalVar()
        {
            var javaScript = strategy.DefineTemplate("template-id", "template-content");
            javaScript.ShouldEqual("t[\"template-id\"]=\"template-content\";");
        }

        [Fact]
        public void EscapesJavaScriptStringContent()
        {
            var javaScript = strategy.DefineTemplate("template\"id", "template\ncontent");
            javaScript.ShouldEqual("t[\"template\\\"id\"]=\"template\\ncontent\";");
        }

        [Fact]
        public void WrapsTemplateDefinitionsInIifeThatCreatesJSTIfUndefined()
        {
            var javaScript = strategy.WrapTemplates("TEMPLATES;");
            javaScript.ShouldEqual(@"(function(t){
TEMPLATES;
}(window.JST||(window.JST={})));");
        }
    }
}