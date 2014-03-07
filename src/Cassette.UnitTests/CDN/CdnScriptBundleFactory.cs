using System.Linq;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.CDN
{
    public class CdnScriptBundleFactory_Tests
    {
        const string CDN_TEST_URL = "http://cdn.test.com";

        readonly IBundlePipeline<CdnScriptBundle> pipeline = Mock.Of<IBundlePipeline<CdnScriptBundle>>();
        readonly BundleDescriptor testBundleDescriptor;

        public CdnScriptBundleFactory_Tests()
        {
            testBundleDescriptor = new BundleDescriptor
            {
                AssetFilenames = { "*" },
                ExternalUrl = CDN_TEST_URL
            };
        }

        [Fact]
        public void CreateBundleReturnsScriptBundle()
        {
            var factory = new CdnScriptBundleFactory(() => pipeline);
            var bundle = factory.CreateBundle(
                "~/test",
                Enumerable.Empty<IFile>(),
                testBundleDescriptor
            );
            bundle.ShouldBeType<CdnScriptBundle>();
        }

        [Fact]
        public void CreateBundleAssignsScriptBundleDirectory()
        {
            var factory = new CdnScriptBundleFactory(() => pipeline);
            var bundle = factory.CreateBundle(
                "~/test",
                Enumerable.Empty<IFile>(),
                testBundleDescriptor
            );
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void CreateBundleWithUrlCreatesExternalScriptBundle()
        {
            new CdnScriptBundleFactory(() => pipeline).CreateExternalBundle("http://test.com/api.js").ShouldBeType<CdnScriptBundle>();
        }

        [Fact]
        public void GivenDescriptorIsFromFile_WhenCreateBundle_ThenBundleIsFromDescriptorFileEqualsTrue()
        {
            var factory = new CdnScriptBundleFactory(() => pipeline);
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath).Returns("~/bundle.txt");
            var descriptor = new BundleDescriptor
            {
                File = file.Object,
                AssetFilenames = { "*" },
                ExternalUrl = CDN_TEST_URL

            };
            var bundle = factory.CreateBundle(
                "~",
                Enumerable.Empty<IFile>(),
                descriptor
            );
            bundle.IsFromDescriptorFile.ShouldBeTrue();
        }

        [Fact]
        public void CreateBundleAssignsPipelineToBundleProcessor()
        {
            var factory = new CdnScriptBundleFactory(() => pipeline);
            var bundle = factory.CreateBundle("~", Enumerable.Empty<IFile>(), testBundleDescriptor );
            bundle.Pipeline.ShouldBeSameAs(pipeline);
        }

        [Fact]
        public void ShowFriendlyExceptionWhenTryingToCreateMinJsFileWhereNonMinJsExists()
        {
            var factory = new CdnScriptBundleFactory(() => pipeline);
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath).Returns("~/test.js");
            var files = new[] { file.Object };
            var exception = Record.Exception(
                () => factory.CreateBundle("~", files, new BundleDescriptor { AssetFilenames = { "~/test.min.js" }, ExternalUrl = CDN_TEST_URL})
            );
            exception.Message.ShouldEqual("Bundle \"~\" references \"~/test.min.js\" when it should reference \"~/test.js\".");
        }
    }
}