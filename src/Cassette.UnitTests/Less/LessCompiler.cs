using Should;
using Xunit;

namespace Cassette.Less
{
    public class LessCompiler_Compile
    {
        [Fact]
        public void Compile_converts_LESS_into_CSS()
        {
            var compiler = new LessCompiler();
            var css = compiler.Compile(@"@color: #4d926f; #header { color: @color; }");
            css.ShouldEqual("#header {\n  color: #4d926f;\n}\n");
        }

        [Fact]
        public void Compile_invalid_LESS_throws_exception()
        {
            var compiler = new LessCompiler();
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile("#unclosed_rule {");
            });
            exception.Message.ShouldEqual("Missing closing `}`");
        }

        [Fact]
        public void Compile_LESS_that_fails_parsing_throws_LessCompileException()
        {
            var compiler = new LessCompiler();
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile("#fail { - }");
            });
            exception.Message.ShouldEqual("Syntax Error on line 1");
        }
    }
}
