using System.IO;
using Cassette.Stylesheets;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class StylesheetBundleConfiguration_Tests
    {
        readonly BundleDefaults<StylesheetBundle> defaults;

        public StylesheetBundleConfiguration_Tests()
        {
            var configuration = new StylesheetBundleConfiguration();
            var settings = new CassetteSettings("");
            configuration.Configure(null, settings);
            defaults = settings.GetDefaults<StylesheetBundle>();
        }

        [Fact]
        public void FileSearchPatternIsCss()
        {
            defaults.FileSearch.Pattern.ShouldEqual("*.css");
        }

        [Fact]
        public void FileSearchSearchOptionIsAllDirectories()
        {
            defaults.FileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void BundleFactoryIsStylesheetBundleFactory()
        {
            defaults.BundleFactory.ShouldBeType<StylesheetBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsStylesheetPipeline()
        {
            defaults.BundlePipeline.ShouldBeType<StylesheetPipeline>();
        }
    }
}