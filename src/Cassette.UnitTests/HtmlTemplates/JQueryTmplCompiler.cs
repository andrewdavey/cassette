using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplCompiler_Tests
    {
        [Fact]
        public void WhenCompileHtmlWithSingleQuotes_ThenSingleQuotesAreEscaped()
        {
            var compiler = new KnockoutJQueryTmplCompiler();
            var result = compiler.Compile("<div id='a'></div>", new CompileContext());
            result.Output.ShouldEqual(
                @"function(jQuery, $item) {var $=jQuery,call,__=[],$data=$item.data;with($data){__.push('<div id=\'a\'></div>');}return __;}"
            );
        }
    }
}