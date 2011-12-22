using Cassette.IO;
using Moq;
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
            var output = compiler.Compile("<div id='a'></div>", Mock.Of<IFile>());
            output.ShouldEqual(
                @"function(jQuery, $item) {var $=jQuery,call,__=[],$data=$item.data;with($data){__.push('<div id=\'a\'></div>');}return __;}"
            );
        }
    }
}
