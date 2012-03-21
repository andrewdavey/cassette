using System.Linq;
using Cassette.IO;
using Should;
using Xunit;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class ScriptBundleFactory_Tests
    {
        readonly CassetteSettings settings;

        public ScriptBundleFactory_Tests()
        {
            settings = new CassetteSettings("");
        }

        [Fact]
        public void CreateBundleReturnsScriptBundle()
        {
            var factory = new ScriptBundleFactory(settings);
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
            var factory = new ScriptBundleFactory(settings);
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
            new ScriptBundleFactory(settings).CreateExternalBundle("http://test.com/api.js").ShouldBeType<ExternalScriptBundle>();
        }

        [Fact]
        public void GivenDescriptorIsFromFile_WhenCreateBundle_ThenBundleIsFromDescriptorFileEqualsTrue()
        {
            var factory = new ScriptBundleFactory(settings);
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

        [Fact]
        public void CreateBundleAssignsSettingsDefaultProcessor()
        {
            var processor = new ScriptPipeline();
            settings.ModifyDefaults<ScriptBundle>(defaults => defaults.BundlePipeline = processor);
            var factory = new ScriptBundleFactory(settings);
            var bundle = factory.CreateBundle("~", Enumerable.Empty<IFile>(), new BundleDescriptor { AssetFilenames = { "*" } });
            bundle.Processor.ShouldBeSameAs(processor);
        }
    }
}