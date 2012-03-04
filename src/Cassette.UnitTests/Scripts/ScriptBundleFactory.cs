using System.Linq;
using Cassette.IO;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundleFactory_Tests
    {
        [Fact]
        public void CreateBundleReturnsScriptBundle()
        {
            var factory = new ScriptBundleFactory();
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
            var factory = new ScriptBundleFactory();
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
            new ScriptBundleFactory().CreateExternalBundle("http://test.com/api.js").ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void GivenDescriptorIsFromFile_WhenCreateBundle_ThenBundleIsFromDescriptorFileEqualsTrue()
        {
            var factory = new ScriptBundleFactory();
            var descriptor = new BundleDescriptor
            {
                IsFromFile = true,
                AssetFilenames = { "*" }
            };
            var bundle = factory.CreateBundle(
                "~",
                Enumerable.Empty<IFile>(),
                descriptor
            );
            bundle.IsFromDescriptorFile.ShouldBeTrue();
        }
    }
}