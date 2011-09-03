using System;
using System.IO;
using Cassette.IO;
using Cassette.Stylesheets;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class LessCompiler_Compile
    {
        public LessCompiler_Compile()
        {
            file = new Mock<IFile>();
            directory = new Mock<IDirectory>();
            file.SetupGet(f => f.FullPath)
                .Returns("test.less");
            file.SetupGet(f => f.Directory)
                .Returns(directory.Object);
            directory.Setup(d => d.NavigateTo(It.IsAny<string>(), false))
                     .Returns(directory.Object);
        }

        readonly Mock<IFile> file;
        readonly Mock<IDirectory> directory;

        [Fact]
        public void Compile_converts_LESS_into_CSS()
        {
            var compiler = new LessCompiler();
            var css = compiler.Compile("@color: #4d926f; #header { color: @color; }", file.Object);
            css.ShouldEqual("#header {\n  color: #4d926f;\n}\n");
        }

        [Fact]
        public void Compile_invalid_LESS_throws_exception()
        {
            var compiler = new LessCompiler();
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile("#unclosed_rule {", file.Object);
            });
            exception.Message.ShouldEqual("Less compile error in test.less:\r\nMissing closing `}`");
        }

        [Fact]
        public void Compile_LESS_that_fails_parsing_throws_LessCompileException()
        {
            var compiler = new LessCompiler();
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile("#fail { - }", file.Object);
            });
            exception.Message.ShouldEqual("Less compile error in test.less:\r\nSyntax Error on line 1");
        }

        [Fact]
        public void Can_Compile_LESS_that_imports_another_LESS_file()
        {
            var otherFile = new Mock<IFile>();
            directory.Setup(d => d.GetFile("lib.less"))
                     .Returns(otherFile.Object);
            otherFile.Setup(f => f.Open(FileMode.Open, FileAccess.Read))
                     .Returns(() => "@color: red;".AsStream());

            var compiler = new LessCompiler();
            var css = compiler.Compile(
                "@import \"lib\";\nbody{ color: @color }",
                file.Object
            );
            css.ShouldEqual("body {\n  color: red;\n}\n");
        }

        [Fact]
        public void Can_Compile_LESS_that_imports_another_LESS_file_from_different_directory()
        {
            var otherFile = new Mock<IFile>();
            directory.Setup(d => d.GetFile("../module-b/lib.less"))
                     .Returns(otherFile.Object);
            otherFile.Setup(f => f.Open(FileMode.Open, FileAccess.Read))
                .Returns(() => "@color: red;".AsStream());

            var compiler = new LessCompiler();
            var css = compiler.Compile(
                "@import \"../module-b/lib.less\";\nbody{ color: @color }",
                file.Object
            );
            css.ShouldEqual("body {\n  color: red;\n}\n");
        }

        [Fact]
        public void Can_Compile_LESS_with_two_levels_of_import()
        {
            // Mocking out IFileSystem here would be lots of work, given the directory navigations
            // that are required. So it's easier to use a temp directory and a real FileSystemDirectory object.
            var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            var moduleA = root.CreateSubdirectory("module-a");
            var moduleB = root.CreateSubdirectory("module-b");
            file.SetupGet(f => f.Directory)
                .Returns(new FileSystemDirectory(moduleA.FullName));

            try
            {
                File.WriteAllText(
                    Path.Combine(root.FullName, "_base.less"),
                    "@size: 100px;"
                );
                File.WriteAllText(
                    Path.Combine(moduleB.FullName, "_lib.less"),
                    "@import \"../_base.less\";\n@color: red; p { height: @size; }"
                );

                var compiler = new LessCompiler();
                var css = compiler.Compile(
                    "@import \"../module-b/_lib.less\";\nbody{ color: @color }",
                    file.Object
                );
                css.ShouldEqual("p {\n  height: 100px;\n}\nbody {\n  color: red;\n}\n");
            }
            finally
            {
                root.Delete(true);
            }
        }

        [Fact]
        public void Importing_less_file_not_found_throws_useful_exception()
        {
            var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            try
            {
                var moduleA = root.CreateSubdirectory("module-a");
                file.SetupGet(f => f.Directory)
                    .Returns(new FileSystemDirectory(moduleA.FullName));
                root.CreateSubdirectory("module-b");

                var compiler = new LessCompiler();
                var exception = Assert.Throws<FileNotFoundException>(delegate
                {
                    compiler.Compile(
                        "@import \"../module-b/_MISSING.less\";\nbody{ color: @color }",
                        file.Object
                    );
                });
                exception.Message.ShouldContain("_MISSING.less");
                exception.Message.ShouldContain("test.less");
            }
            finally
            {
                root.Delete(true);
            }
        }

        [Fact]
        public void Importing_less_file_when_directory_not_found_throws_useful_exception()
        {
            var root = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            try
            {
                var moduleA = root.CreateSubdirectory("module-a");
                file.SetupGet(f => f.Directory)
                    .Returns(new FileSystemDirectory(moduleA.FullName));

                var compiler = new LessCompiler();
                var exception = Assert.Throws<DirectoryNotFoundException>(delegate
                {
                    compiler.Compile(
                        "@import \"../MISSING/_file.less\";\nbody{ color: @color }",
                        file.Object
                    );
                });
                exception.Message.ShouldContain("test.less");
            }
            finally
            {
                root.Delete(true);
            }
        }
    }
}
