using Should;
using Xunit;

namespace Cassette.CoffeeScript
{
    public class CoffeeScriptCompiler_tests
    {
        [Fact]
        public void CompileFile_with_valid_CoffeeScript_returns_JavaScript()
        {
            var source = "x = 1";
            var compiler = new CoffeeScriptCompiler(path => source);
            var javaScript = compiler.CompileFile("test.coffee");
            javaScript.ShouldEqual("(function() {\n  var x;\n  x = 1;\n}).call(this);\n");
        }

        [Fact]
        public void CompileFile_with_invalid_CoffeeScript_throws_CompileException()
        {
            var source = "'unclosed string";
            var compiler = new CoffeeScriptCompiler(path => source);
            var exception = Assert.Throws<CompileException>(delegate
            {
                compiler.CompileFile("test.coffee");
            });
            exception.SourcePath.ShouldEqual("test.coffee");
        }
    }
}
