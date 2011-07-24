using Should;
using Xunit;
using System.Collections.Generic;

namespace Cassette.Less
{
    public class LessCompiler_Compile
    {
        [Fact]
        public void Compile_converts_LESS_into_CSS()
        {
            var compiler = new LessCompiler(_ => @"@color: #4d926f; #header { color: @color; }");
            var css = compiler.CompileFile("test.less");
            css.ShouldEqual("#header {\n  color: #4d926f;\n}\n");
        }

        [Fact]
        public void Compile_invalid_LESS_throws_exception()
        {
            var compiler = new LessCompiler(_ => "#unclosed_rule {");
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.CompileFile("test.less");
            });
            exception.Message.ShouldEqual("Less compile error in test.less:\r\nMissing closing `}`");
        }

        [Fact]
        public void Compile_LESS_that_fails_parsing_throws_LessCompileException()
        {
            var compiler = new LessCompiler(_ => "#fail { - }");
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.CompileFile("test.less");
            });
            exception.Message.ShouldEqual("Less compile error in test.less:\r\nSyntax Error on line 1");
        }

        [Fact]
        public void Can_Compile_LESS_that_imports_another_LESS_file()
        {
            var files = new Dictionary<string, string>
            {
                { @"c:\test.less", "@import \"lib\";\nbody{ color: @color }"},
                { @"c:\lib.less", "@color: red;" }
            };

            var compiler = new LessCompiler(filename => files[filename]);
            var css = compiler.CompileFile(@"c:\test.less");
            css.ShouldEqual("body {\n  color: red;\n}\n");
        }

        [Fact]
        public void Can_Compile_LESS_that_imports_another_LESS_file_from_different_directory()
        {
            var files = new Dictionary<string, string>
            {
                { @"c:\module-a\test.less", "@import \"../module-b/lib.less\";\nbody{ color: @color }"},
                { @"c:\module-b\lib.less", "@color: red;" }
            };

            var compiler = new LessCompiler(filename => files[filename]);
            var css = compiler.CompileFile(@"c:\module-a\test.less");
            css.ShouldEqual("body {\n  color: red;\n}\n");
        }
    }
}
