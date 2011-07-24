using Should;
using Xunit;

namespace Cassette.Less
{
    public class LessCompiler_Compile
    {
        [Fact]
        public void Compile_converts_LESS_into_CSS()
        {
            var compiler = new LessCompiler(_ => @"@color: #4d926f; #header { color: @color; }");
            var css = compiler.CompileFile("");
            css.ShouldEqual("#header {\n  color: #4d926f;\n}\n");
        }

        [Fact]
        public void Compile_invalid_LESS_throws_exception()
        {
            var compiler = new LessCompiler(_ => "#unclosed_rule {");
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.CompileFile("");
            });
            exception.Message.ShouldEqual("Missing closing `}`");
        }

        [Fact]
        public void Compile_LESS_that_fails_parsing_throws_LessCompileException()
        {
            var compiler = new LessCompiler(_ => "#fail { - }");
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.CompileFile("");
            });
            exception.Message.ShouldEqual("Syntax Error on line 1");
        }
    }
}
