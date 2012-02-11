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
            directory.Setup(d => d.GetDirectory(It.IsAny<string>()))
                     .Returns(directory.Object);

            directory.Setup(d => d.GetFile(It.IsAny<string>()))
                     .Returns(new NonExistentFile(""));
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
            exception.Message.ShouldStartWith("Missing closing '}' on line 1 in file 'test.less':");
        }

        [Fact]
        public void Compile_LESS_with_unknown_mixin_throws_exception()
        {
            var less = "form { \nmargin-bottom: @baseline; }";
            var compiler = new LessCompiler();
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile(less, file.Object);
            });
            exception.Message.ShouldStartWith("variable @baseline is undefined on line 2 in file 'test.less':");
        }

        [Fact]
        public void Compile_LESS_that_fails_parsing_throws_LessCompileException()
        {
            var compiler = new LessCompiler();
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile("#fail { - }", file.Object);
            });
            exception.Message.ShouldStartWith("Expected '}' on line 1 in file 'test.less':");
        }

        [Fact]
        public void Can_Compile_LESS_that_imports_another_LESS_file()
        {
            StubFile("lib.less", "@color: white;");

            var compiler = new LessCompiler();
            var css = compiler.Compile(
                "@import \"lib\";\nbody{ color: @color }",
                file.Object
            );
            css.ShouldEqual("body {\n  color: white;\n}\n");
        }

        [Fact]
        public void Can_Compile_LESS_that_imports_another_LESS_file_from_different_directory()
        {
            StubFile("../bundle-b/lib.less", "@color: red;");

            var compiler = new LessCompiler();
            var css = compiler.Compile(
                "@import \"../bundle-b/lib.less\";\nbody{ color: @color }",
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
            var bundleA = root.CreateSubdirectory("bundle-a");
            var bundleB = root.CreateSubdirectory("bundle-b");
            file.SetupGet(f => f.Directory)
                .Returns(new FileSystemDirectory(bundleA.FullName));

            try
            {
                File.WriteAllText(
                    Path.Combine(root.FullName, "_base.less"),
                    "@size: 100px;"
                );
                File.WriteAllText(
                    Path.Combine(bundleB.FullName, "_lib.less"),
                    "@import \"../_base.less\";\n@color: red; p { height: @size; }"
                );

                var compiler = new LessCompiler();
                var css = compiler.Compile(
                    "@import \"../bundle-b/_lib.less\";\nbody{ color: @color }",
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
                var bundleA = root.CreateSubdirectory("bundle-a");
                file.SetupGet(f => f.Directory)
                    .Returns(new FileSystemDirectory(bundleA.FullName));
                root.CreateSubdirectory("bundle-b");

                var compiler = new LessCompiler();
                var exception = Assert.Throws<LessCompileException>(delegate
                {
                    compiler.Compile(
                        "@import \"../bundle-b/_MISSING.less\";\nbody{ color: @color }",
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
        public void Using_mixin_from_imported_css_file_throws_exception()
        {
            StubFile("lib.css", ".mixin { color: red; }");

            var compiler = new LessCompiler();
            Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile(
                    "@import \"lib.css\";\nbody{ .mixin; }",
                    file.Object
                );
            });
        }

        [Fact]
        public void Import_less_file_that_uses_outer_variable()
        {
            StubFile("Framework.less", ".object { padding: @objectpadding; }");

            var compiler = new LessCompiler();
            var result = compiler.Compile(
                "@objectpadding: 20px;\n@import \"Framework.less\";",
                file.Object
            );
            result.ShouldEqual(".object {\n  padding: 20px;\n}\n");
        }

        [Fact]
        public void Variable_defined_by_nested_import_is_replaced_in_CSS_output()
        {
            var directory = new FileSystemDirectory(Path.GetFullPath(@"..\..\assets\less"));
            var file = directory.GetFile("Main.less");
            var compiler = new LessCompiler();
            var css = compiler.Compile(file.OpenRead().ReadToEnd(), file);
            css.ShouldContain("color: #404040;");
        }

        void StubFile(string path, string content)
        {
            var otherFile = new Mock<IFile>();
            directory.Setup(d => d.GetFile(path)).Returns(otherFile.Object);
            otherFile.SetupGet(f => f.Exists).Returns(true);
            otherFile.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                     .Returns(() => content.AsStream());
        }
    }
}