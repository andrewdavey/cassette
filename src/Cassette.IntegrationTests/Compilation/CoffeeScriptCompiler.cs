using Cassette.Scripts;
using Should;
using Xunit;

namespace Cassette
{
    public class JurassicCoffeeScriptCompiler_Tests : CoffeeScriptCompiler_Tests<JurassicCoffeeScriptCompiler>
    {
    }

    public class IECoffeeScriptCompiler_Tests : CoffeeScriptCompiler_Tests<IECoffeeScriptCompiler>
    {
    }

    // Same tests apply to both implementations.

    public abstract class CoffeeScriptCompiler_Tests<T>
        where T : ICompiler, new()
    {
        [Fact]
        public void CompileFile_with_valid_CoffeeScript_returns_JavaScript()
        {
            var source = "x = 1";
            var compiler = new T();
            var javaScript = compiler.Compile(source, new CompileContext());
            javaScript.Output.ShouldEqual("(function() {\n  var x;\n\n  x = 1;\n\n}).call(this);\n");
        }

        [Fact]
        public void CompileFile_with_invalid_CoffeeScript_throws_CompileException()
        {
            var source = "'unclosed string";
            var compiler = new T();
            var exception = Assert.Throws<CoffeeScriptCompileException>(delegate
            {
                compiler.Compile(source, new CompileContext { SourceFilePath = "~/test.coffee" });
            });
            exception.Message.ShouldContain("Parse error on line 1: Unexpected ''' in ~/test.coffee");
            exception.SourcePath.ShouldEqual("~/test.coffee");
        }
    }
}