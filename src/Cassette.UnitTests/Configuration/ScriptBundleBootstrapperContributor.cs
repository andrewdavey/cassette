using System.IO;
using Cassette.BundleProcessing;
using Cassette.Scripts;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class ScriptBundleBootstrapperContributor_Tests
    {
        readonly FileSearch fileSearch;
        readonly ScriptBundleBootstrapperContributor contributor;

        public ScriptBundleBootstrapperContributor_Tests()
        {
            contributor = new ScriptBundleBootstrapperContributor();
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