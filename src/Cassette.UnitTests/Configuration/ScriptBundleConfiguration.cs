using System.IO;
using Cassette.Scripts;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class ScriptBundleConfiguration_Tests
    {
        readonly BundleDefaults<ScriptBundle> defaults;

        public ScriptBundleConfiguration_Tests()
        {
            var configuration = new ScriptBundleConfiguration();
            var settings = new CassetteSettings("");
            configuration.Configure(null, settings);
            defaults = settings.GetDefaults<ScriptBundle>();
        }

        [Fact]
        public void FileSearchPatternIsJs()
        {
            defaults.FileSearch.Pattern.ShouldEqual("*.js");
        }

        [Fact]
        public void FileSearchSearchOptionIsAllDirectories()
        {
            defaults.FileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void FileSearchExcludesVsDocFiles()
        {
            defaults.FileSearch.Exclude.IsMatch("jquery-vsdoc.js").ShouldBeTrue();
        }

        [Fact]
        public void BundleFactoryIsScriptBundleFactory()
        {
            defaults.BundleFactory.ShouldBeType<ScriptBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsScriptPipeline()
        {
            defaults.BundlePipeline.ShouldBeType<ScriptPipeline>();
        }
    }
}