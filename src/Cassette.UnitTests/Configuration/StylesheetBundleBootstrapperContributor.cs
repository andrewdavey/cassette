using System.IO;
using Cassette.BundleProcessing;
using Cassette.Stylesheets;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class StylesheetBundleServiceRegistry_Tests
    {
        readonly FileSearch fileSearch;
        readonly StylesheetBundleServiceRegistry contributor;

        public StylesheetBundleServiceRegistry_Tests()
        {
            contributor = new StylesheetBundleServiceRegistry();
            fileSearch = (FileSearch)contributor.GetInstance<IFileSearch>();
        }

        [Fact]
        public void FileSearchPatternIsHtmAndHtml()
        {
            fileSearch.Pattern.ShouldEqual("*.css");
        }

        [Fact]
        public void FileSearchSearchOptionIsAllDirectories()
        {
            fileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void BundleFactoryIsStylesheetBundleFactory()
        {
            contributor.ShouldHaveTypeRegistration<IBundleFactory<StylesheetBundle>, StylesheetBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsStylesheetPipeline()
        {
            contributor.ShouldHaveTypeRegistration<IBundlePipeline<StylesheetBundle>, StylesheetPipeline>();
        }
    }

}