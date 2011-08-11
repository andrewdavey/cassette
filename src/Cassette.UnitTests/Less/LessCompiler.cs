using Should;
using Xunit;
using System.Collections.Generic;
using Moq;
using System.IO;
using Cassette.Utilities;
using System;

namespace Cassette.Less
{
    public class LessCompiler_Compile
    {
        public LessCompiler_Compile()
        {
            fileSystem = new Mock<IFileSystem>();
        }

        readonly Mock<IFileSystem> fileSystem;

        [Fact]
        public void Compile_converts_LESS_into_CSS()
        {
            var compiler = new LessCompiler();
            var css = compiler.Compile("@color: #4d926f; #header { color: @color; }", "test.less", fileSystem.Object);
            css.ShouldEqual("#header {\n  color: #4d926f;\n}\n");
        }

        [Fact]
        public void Compile_invalid_LESS_throws_exception()
        {
            var compiler = new LessCompiler();
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile("#unclosed_rule {", "test.less", fileSystem.Object);
            });
            exception.Message.ShouldEqual("Less compile error in test.less:\r\nMissing closing `}`");
        }

        [Fact]
        public void Compile_LESS_that_fails_parsing_throws_LessCompileException()
        {
            var compiler = new LessCompiler();
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile("#fail { - }", "test.less", fileSystem.Object);
            });
            exception.Message.ShouldEqual("Less compile error in test.less:\r\nSyntax Error on line 1");
        }

        [Fact]
        public void Can_Compile_LESS_that_imports_another_LESS_file()
        {
            fileSystem.Setup(fs => fs.OpenFile("lib.less", FileMode.Open, FileAccess.Read))
                      .Returns(() => "@color: red;".AsStream());
            var compiler = new LessCompiler();
            var css = compiler.Compile(
                "@import \"lib\";\nbody{ color: @color }",
                "test.less",
                fileSystem.Object
            );
            css.ShouldEqual("body {\n  color: red;\n}\n");
        }

        [Fact]
        public void Can_Compile_LESS_that_imports_another_LESS_file_from_different_directory()
        {
            fileSystem.Setup(fs => fs.OpenFile("../module-b/lib.less", FileMode.Open, FileAccess.Read))
                      .Returns(() => "@color: red;".AsStream());

            var compiler = new LessCompiler();
            var css = compiler.Compile(
                "@import \"../module-b/lib.less\";\nbody{ color: @color }",
                @"c:\module-a\test.less",
                fileSystem.Object
            );
            css.ShouldEqual("body {\n  color: red;\n}\n");
        }

        [Fact]
        public void Can_Compile_LESS_with_two_levels_of_import()
        {
            // Mocking out IFileSystem here would be lots of work, given the directory navigations
            // that are required. So it's easier to use a temp directory and a real FileSystem object.
            var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            try
            {
                var moduleA = root.CreateSubdirectory("module-a");
                var moduleB = root.CreateSubdirectory("module-b");
                File.WriteAllText(
                    Path.Combine(root.FullName, "_base.less"),
                    "@size: 100px;"
                );
                File.WriteAllText(
                    Path.Combine(moduleB.FullName, "_lib.less"),
                    "@import \"../_base.less\";\n@color: red; p { height: @size; }"
                );

                var fileSystem = new FileSystem(moduleA.FullName);

                var compiler = new LessCompiler();
                var css = compiler.Compile(
                    "@import \"../module-b/_lib.less\";\nbody{ color: @color }",
                    @"test.less",
                    fileSystem
                );
                css.ShouldEqual("p {\n  height: 100px;\n}\nbody {\n  color: red;\n}\n");
            }
            finally
            {
                root.Delete(true);
            }
        }
    }
}
