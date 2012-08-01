using System.Linq;
using Cassette.BundleProcessing;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundleFactory_Tests
    {
        readonly IBundlePipeline<ScriptBundle> pipeline = Mock.Of<IBundlePipeline<ScriptBundle>>();

        [Fact]
        public void CreateBundleReturnsScriptBundle()
        {
            var factory = new ScriptBundleFactory(() => pipeline);
            var bundle = factory.CreateBundle(
                "~/test",
                Enumerable.Empty<IFile>(),
                new BundleDescriptor { AssetFilenames = { "*" } }
            );
            bundle.ShouldBeType<ScriptBundle>();
        }

        [Fact]
        public void CreateBundleAssignsScriptBundleDirectory()
        {
            var factory = new ScriptBundleFactory(() => pipeline);
            var bundle = factory.CreateBundle(
                "~/test",
                Enumerable.Empty<IFile>(),
                new BundleDescriptor { AssetFilenames = { "*" } }
            );
            bundle.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void CreateBundleWithUrlCreatesExternalScriptBundle()
        {
            new ScriptBundleFactory(() => pipeline).CreateExternalBundle("http://test.com/api.js").ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void GivenDescriptorIsFromFile_WhenCreateBundle_ThenBundleIsFromDescriptorFileEqualsTrue()
        {
            var factory = new ScriptBundleFactory(() => pipeline);
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath).Returns("~/bundle.txt");
            var descriptor = new BundleDescriptor
            {
                File = file.Object,
                AssetFilenames = { "*" }
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
            var factory = new ScriptBundleFactory(() => pipeline);
            var bundle = factory.CreateBundle("~", Enumerable.Empty<IFile>(), new BundleDescriptor { AssetFilenames = { "*" } });
            bundle.Pipeline.ShouldBeSameAs(pipeline);
        }
    }
}