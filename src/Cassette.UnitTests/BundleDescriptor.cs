using System.IO;
using System.Text.RegularExpressions;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleDescriptor_Tests
    {
        [Fact]
        public void GivenFileExists_WhenGetAssetFilesWithExclusionRegexMatchingFilePath_ThenFileIsNotReturned()
        {
            var directory = new Mock<IDirectory>();
            var file = new Mock<IFile>();
            directory.Setup(d => d.GetFiles("*.js", SearchOption.AllDirectories))
                     .Returns(new[] { file.Object });
            file.SetupGet(f => f.FullPath)
                .Returns("~/test-vsdoc.js");

            var descriptor = new BundleDescriptor(new[] { "*" });
            var files = descriptor.GetAssetFiles(directory.Object, new[] { "*.js" }, new Regex("-vsdoc\\.js"), SearchOption.AllDirectories);

            files.ShouldBeEmpty();
        }
    }
}