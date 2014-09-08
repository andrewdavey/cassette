using System.Linq;
using Cassette.BundleProcessing;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.CDN
{
    public class CdnStylesheetBundleFactory_Tests
    {
        readonly CdnStylesheetBundleFactory factory;
        readonly IBundlePipeline<CdnStylesheetBundle> pipeline;

        public CdnStylesheetBundleFactory_Tests()
        {
            pipeline = Mock.Of<IBundlePipeline<CdnStylesheetBundle>>();
            factory = new CdnStylesheetBundleFactory(() => pipeline);
        }

        [Fact]
        public void CreateBundleReturnsStylesheetBundleWithDirectorySet()
        {
            var bundle = factory.CreateBundle(
                "~/test",
                Enumerable.Empty<IFile>(),
                new BundleDescriptor { AssetFilenames = { "*" } }
            );
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void CreateBundleWithUrlCreatesExternalBundle()
        {
            var bundle = factory.CreateExternalBundle("http://test.com/test.css");
            bundle.ShouldBeType<CdnStylesheetBundle>();
        }

        [Fact]
        public void CreateBundleAssignsSettingsDefaultProcessor()
        {
            var bundle = factory.CreateBundle("~", Enumerable.Empty<IFile>(), new BundleDescriptor { AssetFilenames = { "*" } });
            bundle.Pipeline.ShouldBeSameAs(pipeline);
        }

        [Fact]
        public void ShowFriendlyExceptionWhenTryingToCreateMinCssFileWhereNonMinCssExists()
        {
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath).Returns("~/test.css");
            var files = new[] { file.Object };
            var exception = Record.Exception(
                () => factory.CreateBundle("~", files, new BundleDescriptor { AssetFilenames = { "~/test.min.css" } })
            );
            exception.Message.ShouldEqual("Bundle \"~\" references \"~/test.min.css\" when it should reference \"~/test.css\".");
        }

        [Fact]
        public void ShowFriendlyExceptionWhenTryingToCreateCssFileWhereDashDebugCssExists()
        {
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath).Returns("~/test-debug.css");
            var files = new[] { file.Object };
            var exception = Record.Exception(
                () => factory.CreateBundle("~", files, new BundleDescriptor { AssetFilenames = { "~/test.css" } })
            );
            exception.Message.ShouldEqual("Bundle \"~\" references \"~/test.css\" when it should reference \"~/test-debug.css\".");
        }

        [Fact]
        public void ShowFriendlyExceptionWhenTryingToCreateCssFileWhereDotDebugCssExists()
        {
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath).Returns("~/test.debug.css");
            var files = new[] { file.Object };
            var exception = Record.Exception(
                () => factory.CreateBundle("~", files, new BundleDescriptor { AssetFilenames = { "~/test.css" } })
            );
            exception.Message.ShouldEqual("Bundle \"~\" references \"~/test.css\" when it should reference \"~/test.debug.css\".");
        }
    }
}