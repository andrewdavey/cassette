using Cassette.Scripts;
using Should;
using Xunit;

namespace Cassette.Compilation
{
    public class JurassicCoffeeScriptCompiler_Tests
    {
        [Fact]
        public void CompileFile_with_valid_CoffeeScript_returns_JavaScript()
        {
            var source = "x = 1";
            var compiler = new JurassicCoffeeScriptCompiler();
            var javaScript = compiler.Compile(source, new CompileContext());
            javaScript.Output.ShouldEqual("(function() {\n  var x;\n\n  x = 1;\n\n}).call(this);\n");
        }

        [Fact]
        public void CompileFile_with_invalid_CoffeeScript_throws_CompileException()
        {
            var source = "'unclosed string";
            var compiler = new JurassicCoffeeScriptCompiler();
            var exception = Assert.Throws<CoffeeScriptCompileException>(delegate
            {
                compiler.Compile(source, new CompileContext { SourceFilePath = "~/test.coffee" });
            });
            exception.Message.ShouldContain("Parse error on line 1: Unexpected ''' in ~/test.coffee");
            exception.SourcePath.ShouldEqual("~/test.coffee");
        }
    }

#if !NET35
    public class IECoffeeScriptCompiler_Tests
    {
        [Fact]
        public void CompileFile_with_valid_CoffeeScript_returns_JavaScript()
        {
            using (var queue = new IECoffeeScriptCompilationQueue())
            {
                queue.Start();

                var source = "x = 1";
                var compiler = new IECoffeeScriptCompiler(queue);
                var javaScript = compiler.Compile(source, new CompileContext());
                javaScript.Output.ShouldEqual("(function() {\n  var x;\n\n  x = 1;\n\n}).call(this);\n");
            }
        }

        [Fact]
        public void CompileFile_with_invalid_CoffeeScript_throws_CompileException()
        {
            using (var queue = new IECoffeeScriptCompilationQueue())
            {
                queue.Start();

                var source = "'unclosed string";
                var compiler = new IECoffeeScriptCompiler(queue);
                var exception = Assert.Throws<CoffeeScriptCompileException>(delegate
                {
                    compiler.Compile(source, new CompileContext { SourceFilePath = "~/test.coffee" });
                });
                exception.Message.ShouldContain("Parse error on line 1: Unexpected ''' in ~/test.coffee");
                exception.SourcePath.ShouldEqual("~/test.coffee");
            }
        }
    }
#endif
}