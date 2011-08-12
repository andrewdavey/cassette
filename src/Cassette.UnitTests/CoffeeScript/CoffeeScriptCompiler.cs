using Should;
using Xunit;
using Moq;

namespace Cassette.CoffeeScript
{
    public class CoffeeScriptCompiler_tests
    {
        [Fact]
        public void CompileFile_with_valid_CoffeeScript_returns_JavaScript()
        {
            var source = "x = 1";
            var compiler = new CoffeeScriptCompiler();
            var javaScript = compiler.Compile(source, "test.coffee", Mock.Of<IFileSystem>());
            javaScript.ShouldEqual("(function() {\n  var x;\n  x = 1;\n}).call(this);\n");
        }

        [Fact]
        public void CompileFile_with_invalid_CoffeeScript_throws_CompileException()
        {
            var source = "'unclosed string";
            var compiler = new CoffeeScriptCompiler();
            var exception = Assert.Throws<CompileException>(delegate
            {
                compiler.Compile(source, "test.coffee", Mock.Of<IFileSystem>());
            });
            exception.SourcePath.ShouldEqual("test.coffee");
        }

        [Fact]
        public void OutputContentTypeIsTextJavascript()
        {
            new CoffeeScriptCompiler().OutputContentType.ShouldEqual("text/javascript");
        }
    }
}
