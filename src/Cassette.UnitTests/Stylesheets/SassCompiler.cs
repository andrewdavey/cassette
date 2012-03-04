using System.IO;
using System.Linq;
using Cassette.IO;
using Moq;
using Should;
using Xunit;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    public class SassCompiler_Tests
    {
        [Fact]
        public void WhenCompileScss_ThenReturnCss()
        {
            var compiler = new SassCompiler();
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath).Returns("~/test.scss");

            var css = compiler.Compile("$x: red; p { color: $x; }", file.Object);

            css.ShouldEqual("p {\n  color: red; }\n");
        }

        [Fact]
        public void WhenCompileScss_ThenReferencedFilesContainsTheSourceFile()
        {
            var compiler = new SassCompiler();
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath).Returns("~/test.scss");

            compiler.Compile("$x: red; p { color: $x; }", file.Object);

            compiler.ReferencedFiles.ShouldEqual(new[] { "~/test.scss" });
        }

        [Fact]
        public void GivenScssThatImportsOtherScssFile_WhenCompile_ThenCssReturned()
        {
            var compiler = new SassCompiler();
            
            var file = new Mock<IFile>();
            var other = new Mock<IFile>();
            var directory = new Mock<IDirectory>();
            other.SetupGet(f => f.Exists).Returns(true);
            other.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
                .Returns(() => "p { color: red; }".AsStream());
            directory.Setup(d => d.GetFile(It.IsAny<string>()))
                .Returns(new NonExistentFile("~"));
            directory.Setup(d => d.GetFile("other.scss"))
                .Returns(other.Object);
            file.SetupGet(f => f.FullPath).Returns("~/test.scss");
            file.SetupGet(f => f.Directory).Returns(directory.Object);

            var css = compiler.Compile("@import \"other.scss\";", file.Object);

            css.ShouldEqual("p {\n  color: red; }\n");
        }

        [Fact]
        public void GivenScssThatImportsOtherScssFile_WhenCompile_ThenReferencedFilesContainsBothPaths()
        {
            var compiler = new SassCompiler();

            var file = new Mock<IFile>();
            var other = new Mock<IFile>();
            var directory = new Mock<IDirectory>();
            other.SetupGet(f => f.Exists).Returns(true);
            other.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
                .Returns(() => "p { color: red; }".AsStream());
            directory.Setup(d => d.GetFile(It.IsAny<string>()))
                .Returns(new NonExistentFile("~"));
            directory.Setup(d => d.GetFile("other.scss"))
                .Returns(other.Object);
            file.SetupGet(f => f.FullPath).Returns("~/test.scss");
            file.SetupGet(f => f.Directory).Returns(directory.Object);

            compiler.Compile("@import \"other.scss\";", file.Object);

            compiler.ReferencedFiles.ToArray().ShouldEqual(new[]
            {
                "~/test.scss",
                "./other.scss"
            });
        }
    }
}