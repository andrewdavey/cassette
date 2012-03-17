#if !NET35
using System.IO;
using System.Linq;
using Cassette.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class SassCompiler_Tests
    {
        readonly SassCompiler compiler;
        readonly Mock<IFile> file;
        readonly Mock<IDirectory> directory;
        readonly CompileContext compileContext;

        public SassCompiler_Tests()
        {
            directory = new Mock<IDirectory>();
            file = new Mock<IFile>();
            directory.Setup(d => d.GetFile(It.IsAny<string>())).Returns(new NonExistentFile("~"));
            directory.Setup(d => d.GetFile("~/test.scss")).Returns(file.Object);
            file.SetupGet(f => f.FullPath).Returns("~/test.scss");
            file.SetupGet(f => f.Directory).Returns(directory.Object);

            compileContext = new CompileContext { RootDirectory = directory.Object, SourceFilePath = "~/test.scss" };
            compiler = new SassCompiler();
        }

        [Fact]
        public void WhenCompileScss_ThenReturnCss()
        {
            var css = compiler.Compile(
                "$x: red; p { color: $x; }",
                compileContext
            );

            css.Output.ShouldEqual("p {\n  color: red; }\n");
        }

        [Fact]
        public void WhenCompileScss_ThenReferencedFilesContainsTheSourceFile()
        {
            var result = compiler.Compile("$x: red; p { color: $x; }", compileContext);
            result.ImportedFilePaths.ShouldEqual(new[] { "~/test.scss" });
        }

        [Fact]
        public void GivenScssThatImportsOtherScssFile_WhenCompile_ThenCssReturned()
        {
            var other = new Mock<IFile>();
            other.SetupGet(f => f.Exists).Returns(true);
            other.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
                .Returns(() => "p { color: red; }".AsStream());
            directory.Setup(d => d.GetFile("other.scss"))
                .Returns(other.Object);
            
            var css = compiler.Compile("@import \"other.scss\";", compileContext);

            css.Output.ShouldEqual("p {\n  color: red; }\n");
        }

        [Fact]
        public void GivenScssThatImportsOtherScssFile_WhenCompile_ThenReferencedFilesContainsBothPaths()
        {
            var other = new Mock<IFile>();
            other.SetupGet(f => f.Exists).Returns(true);
            other.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
                .Returns(() => "p { color: red; }".AsStream());
            directory.Setup(d => d.GetFile("other.scss"))
                .Returns(other.Object);

            var result = compiler.Compile("@import \"other.scss\";", compileContext);

            result.ImportedFilePaths.ToArray().ShouldEqual(new[]
            {
                "~/test.scss",
                "./other.scss"
            });
        }
    }
}
#endif