using System;
using System.IO;
using Cassette.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class LessCompiler_Compile
    {
        public LessCompiler_Compile()
        {
            file = new Mock<IFile>();
            directory = new Mock<IDirectory>();
            file.SetupGet(f => f.FullPath).Returns("~/test.less");
            file.SetupGet(f => f.Directory).Returns(directory.Object);
            directory.Setup(d => d.GetDirectory(It.IsAny<string>())).Returns(directory.Object);
            directory.Setup(d => d.GetFile(It.IsAny<string>())).Returns(new NonExistentFile(""));
            directory.Setup(d => d.GetFile("~/test.less")).Returns(file.Object);

            compileContext = new CompileContext
            {
                RootDirectory = directory.Object,
                SourceFilePath = "~/test.less"
            };
            compiler = new LessCompiler();
        }

        readonly Mock<IFile> file;
        readonly Mock<IDirectory> directory;
        readonly CompileContext compileContext;
        readonly LessCompiler compiler;

        [Fact]
        public void Compile_converts_LESS_into_CSS()
        {
            var css = compiler.Compile("@color: #4d926f; #header { color: @color; }", compileContext);
            css.Output.ShouldEqual("#header {\n  color: #4d926f;\n}\n");
        }

        [Fact]
        public void Compile_invalid_LESS_throws_exception()
        {
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile("#unclosed_rule {", compileContext);
            });
            exception.Message.ShouldContain("message: missing closing `}`");
            exception.Message.ShouldContain("filename: ~/test.less");
            exception.Message.ShouldContain("line: 1");
        }

        [Fact]
        public void Compile_LESS_with_unknown_mixin_throws_exception()
        {
            var less = "form { \nmargin-bottom: @baseline; }";
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile(less, compileContext);
            });
            exception.Message.ShouldContain("message: variable @baseline is undefined");
            exception.Message.ShouldContain("filename: ~/test.less");
            exception.Message.ShouldContain("line: 2");
        }

        [Fact]
        public void Compile_LESS_that_fails_parsing_throws_LessCompileException()
        {
            var exception = Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile("#fail { - }", compileContext);
            });
            exception.Message.ShouldContain("message: Unrecognised input");
            exception.Message.ShouldContain("filename: ~/test.less");
            exception.Message.ShouldContain("line: 1");
        }

        [Fact]
        public void Can_Compile_LESS_that_imports_another_LESS_file()
        {
            var fileSystem = new FakeFileSystem
            {
                {"~/lib.less", "@color: white;"},
                "~/test.less"
            };
            compileContext.RootDirectory = fileSystem;
            compileContext.SourceFilePath = "~/test.less";
            var css = compiler.Compile(
                "@import \"lib\";\nbody{ color: @color }",
                compileContext
            );
            css.Output.ShouldEqual("body {\n  color: #ffffff;\n}\n");
        }

        [Fact]
        public void Can_Compile_LESS_that_imports_another_LESS_file_from_different_directory()
        {
            var fileSystem = new FakeFileSystem
            {
                {"~/bundle-b/lib.less", "@color: red;"},
                "~/bundle-a/test.less"
            };
            compileContext.RootDirectory = fileSystem;
            compileContext.SourceFilePath = "~/bundle-a/test.less";

            var css = compiler.Compile(
                "@import \"../bundle-b/lib.less\";\nbody{ color: @color }",
                compileContext
            );
            css.Output.ShouldEqual("body {\n  color: #ff0000;\n}\n");
        }

        [Fact]
        public void Can_Compile_LESS_with_two_levels_of_import()
        {
            var source = "@import \"../bundle-b/_lib.less\";\nbody{ color: @color }";

            var fileSystem = new FakeFileSystem
            {
                { "~/_base.less", "@size: 100px;" },
                { "~/bundle-b/_lib.less", "@import \"../_base.less\";\n@color: red; p { height: @size; }" },
                { "~/bundle-a/test.less", source }
            };
            compileContext.SourceFilePath = "~/bundle-a/test.less";
            compileContext.RootDirectory = fileSystem;

            var css = compiler.Compile(
                source,
                compileContext
            );
            css.Output.ShouldEqual("p {\n  height: 100px;\n}\nbody {\n  color: #ff0000;\n}\n");
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
                compileContext.SourceFilePath = "~/test.less";

                var exception = Record.Exception(delegate
                {
                    compiler.Compile(
                        "@import \"../bundle-b/_MISSING.less\";\nbody{ color: @color }",
                        compileContext
                    );
                });
                // TODO: Patch dotless to include the imported file name!
                // exception.Message.ShouldContain("_MISSING.less");
                exception.Message.ShouldContain("~/test.less");
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

            Assert.Throws<LessCompileException>(delegate
            {
                compiler.Compile(
                    "@import \"lib.css\";\nbody{ .mixin; }",
                    compileContext
                );
            });
        }

        [Fact]
        public void Import_less_file_that_uses_outer_variable()
        {
            var fileSystem = new FakeFileSystem
            {
                {"~/Framework.less", ".object { padding: @objectpadding; }" },
                "~/test.less"
            };
            compileContext.RootDirectory = fileSystem;
            compileContext.SourceFilePath = "~/test.less";

            var result = compiler.Compile(
                "@objectpadding: 20px;\n@import \"Framework.less\";",
                compileContext
            );
            result.Output.ShouldEqual(".object {\n  padding: 20px;\n}\n");
        }

        [Fact]
        public void Variable_defined_by_nested_import_is_replaced_in_CSS_output()
        {
            using (var path = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(path, "main.less"), "@import 'first.less';\np { color: @c }");
                File.WriteAllText(Path.Combine(path, "first.less"), "@import 'second.less';");
                File.WriteAllText(Path.Combine(path, "second.less"), "@c: red;");
                var directory = new FileSystemDirectory(path);
                var file = directory.GetFile("main.less");

                compileContext.RootDirectory = directory;
                compileContext.SourceFilePath = "~/main.less";
                var css = compiler.Compile(file.OpenRead().ReadToEnd(), compileContext);

                css.Output.ShouldContain("color: #ff0000;");
            }
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