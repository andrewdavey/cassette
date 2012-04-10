using System.IO;
using Cassette.BundleProcessing;
using Cassette.Scripts;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class ScriptBundleServiceRegistry_Tests
    {
        readonly FileSearch fileSearch;
        readonly ScriptBundleServiceRegistry contributor;

        public ScriptBundleServiceRegistry_Tests()
        {
            contributor = new ScriptBundleServiceRegistry();
            fileSearch = (FileSearch)contributor.GetInstance<IFileSearch>();
        }

        [Fact]
        public void FileSearchPatternIsHtmAndHtml()
        {
            fileSearch.Pattern.ShouldEqual("*.js");
        }

        [Fact]
        public void FileSearchSearchOptionIsAllDirectories()
        {
            fileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void FileSearchExcludesVsDocFiles()
        {
            fileSearch.Exclude.IsMatch("jquery-vsdoc.js").ShouldBeTrue();
        }

        [Fact]
        public void BundleFactoryIsScriptBundleFactory()
        {
            contributor.ShouldHaveTypeRegistration<IBundleFactory<ScriptBundle>, ScriptBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsScriptPipeline()
        {
            contributor.ShouldHaveTypeRegistration<IBundlePipeline<ScriptBundle>, ScriptPipeline>();
        }
    }
}