using System.Linq;
using Cassette.Configuration;
using Cassette.Stylesheets;
using Should;
using Xunit;
using Cassette.HtmlTemplates;

namespace Cassette.Scripts
{
    public abstract class ExtensionConfigurationTestsBase
    {
        protected readonly CassetteSettings Settings = new CassetteSettings("");

        protected void AssertFileIsMatchedByDefaultFileSearch<T>(string filename)
            where T : Bundle
        {
            var search = Settings.GetDefaultFileSearch<T>();
            var directory = new FakeFileSystem { filename };
            var files = search.FindFiles(directory);
            files.Count().ShouldEqual(1);
        }
    }

    public class CoffeeScriptConfiguration_Tests : ExtensionConfigurationTestsBase
    {
        [Fact]
        public void WhenConfigure_ThenCoffeeScriptFilesAreMatchedByDefaultFileSearch()
        {
            var config = new CoffeeScriptConfiguration();
            config.Configure(new BundleCollection(Settings), Settings);
            AssertFileIsMatchedByDefaultFileSearch<ScriptBundle>("~/test.coffee");
        }
    }

    public class LessConfiguration_Tests : ExtensionConfigurationTestsBase
    {
        [Fact]
        public void WhenConfigure_ThenLessFilesAreMatchedByDefaultFileSearch()
        {
            var config = new LessConfiguration();
            config.Configure(new BundleCollection(Settings), Settings);
            AssertFileIsMatchedByDefaultFileSearch<StylesheetBundle>("~/test.less");
        }
    }

#if !NET35
    public class SassConfiguration_Tests : ExtensionConfigurationTestsBase
    {
        [Fact]
        public void WhenConfigure_ThenScssFilesAreMatchedByDefaultFileSearch()
        {
            var config = new SassConfiguration();
            config.Configure(new BundleCollection(Settings), Settings);
            AssertFileIsMatchedByDefaultFileSearch<StylesheetBundle>("~/test.scss");
        }

        [Fact]
        public void WhenConfigure_ThenSassFilesAreMatchedByDefaultFileSearch()
        {
            var config = new SassConfiguration();
            config.Configure(new BundleCollection(Settings), Settings);
            AssertFileIsMatchedByDefaultFileSearch<StylesheetBundle>("~/test.sass");
        }
    }
#endif

    public class HoganConfiguration_Tests : ExtensionConfigurationTestsBase
    {
        [Fact]
        public void WhenConfigure_ThenMustacheFilesAreMatchedByDefaultFileSearch()
        {
            var config = new HoganConfiguration();
            config.Configure(new BundleCollection(Settings), Settings);
            AssertFileIsMatchedByDefaultFileSearch<HtmlTemplateBundle>("~/test.mustache");
        }

        [Fact]
        public void WhenConfigure_ThenJstFilesAreMatchedByDefaultFileSearch()
        {
            var config = new HoganConfiguration();
            config.Configure(new BundleCollection(Settings), Settings);
            AssertFileIsMatchedByDefaultFileSearch<HtmlTemplateBundle>("~/test.jst");
        }

        [Fact]
        public void WhenConfigure_ThenTmplFilesAreMatchedByDefaultFileSearch()
        {
            var config = new HoganConfiguration();
            config.Configure(new BundleCollection(Settings), Settings);
            AssertFileIsMatchedByDefaultFileSearch<HtmlTemplateBundle>("~/test.tmpl");
        }
    }
}