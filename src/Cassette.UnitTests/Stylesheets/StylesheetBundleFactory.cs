using System.Linq;
using Cassette.Configuration;
using Cassette.IO;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleFactory_Tests
    {
        readonly StylesheetBundleFactory factory;
        readonly CassetteSettings settings;

        public StylesheetBundleFactory_Tests()
        {
            settings = new CassetteSettings("");
            factory = new StylesheetBundleFactory(settings);
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
            bundle.ShouldBeType<ExternalStylesheetBundle>();
        }

        [Fact]
        public void CreateBundleAssignsSettingsDefaultProcessor()
        {
            var processor = new StylesheetPipeline();
            settings.ModifyDefaults<StylesheetBundle>(defaults => defaults.BundlePipeline = processor);
            var bundle = factory.CreateBundle("~", Enumerable.Empty<IFile>(), new BundleDescriptor { AssetFilenames = { "*" } });
            bundle.Processor.ShouldBeSameAs(processor);
        }
    }
}