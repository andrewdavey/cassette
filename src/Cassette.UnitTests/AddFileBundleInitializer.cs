using System.IO;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class AddFileBundleInitializer_Tests
    {
        readonly Mock<ICassetteApplication> application;
        readonly Mock<IDirectory> directory;
        readonly Mock<IFile> file;

        public AddFileBundleInitializer_Tests()
        {
            application = new Mock<ICassetteApplication>();
            directory = new Mock<IDirectory>();
            file = new Mock<IFile>();
            application.SetupGet(a => a.SourceDirectory)
                .Returns(directory.Object);
            directory.Setup(d => d.GetDirectory("~"))
                .Returns(directory.Object);
            directory.Setup(d => d.GetFile("asset.js"))
                .Returns(file.Object);
            file.SetupGet(f => f.Exists)
                .Returns(true);
        }

        [Fact]
        public void GivenFileExists_WhenInitializeBundle_ThenAssetAddedForTheFile()
        {
            var initializer = new AddFileBundleInitializer("asset.js");
            var bundle = new TestableBundle("~");

            initializer.InitializeBundle(bundle, application.Object);

            bundle.Assets[0].SourceFile.ShouldBeSameAs(file.Object);
        }

        [Fact]
        public void GivenFilePathIsApplicationRelative_WhenInitializeBundle_ThenAssetAddedForTheFile()
        {
            directory.Setup(d => d.GetFile("~/test/asset.js"))
                .Returns(file.Object);
            
            var initializer = new AddFileBundleInitializer("~/test/asset.js");
            var bundle = new TestableBundle("~");

            initializer.InitializeBundle(bundle, application.Object);

            bundle.Assets[0].SourceFile.ShouldBeSameAs(file.Object);
        }

        [Fact]
        public void GivenFileDoesNotExist_WhenInitializeBundle_ThenFileNotFoundExceptionThrown()
        {
            file.SetupGet(f => f.Exists)
                .Returns(false);

            var initializer = new AddFileBundleInitializer("asset.js");
            var bundle = new TestableBundle("~");

            Assert.Throws<FileNotFoundException>(
                () => initializer.InitializeBundle(bundle, application.Object)
            );
        }
    }
}