using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class KnockoutJQueryTmplCompiler_Tests
    {
        [Fact]
        public void WhenCompileHtmlWithSingleQuotesInDataBindAttribute_ThenSingleQuotesAreNotEscaped()
        {
            var compiler = new KnockoutJQueryTmplCompiler();
            var result = compiler.Compile("<div data-bind=\"test: 'a'\"></div>", new CompileContext());
            result.Output.ShouldEqual(@"function(jQuery, $item) {var $=jQuery,call,__=[],$data=$item.data;with($data){__.push('');__.push(((function() { return ko.templateRewriting.applyMemoizedBindingsToNextSibling(function() {                     return (function() { return { test: 'a' } })()                 }) })()) || '');__.push('<div ></div>');}return __;}");
        }
    }
}