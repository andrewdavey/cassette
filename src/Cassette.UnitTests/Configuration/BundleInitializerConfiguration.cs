using System.IO;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public abstract class BundleInitializerConfiguration_Tests<T>
        where T : Bundle
    {
        internal readonly AssetSource Initializer;

        protected BundleInitializerConfiguration_Tests()
        {
            Initializer = GetDefaultInitializer();
        }

        static AssetSource GetDefaultInitializer()
        {
            var configuration = new BundleInitializerConfiguration();
            var settings = new CassetteSettings();
            configuration.Configure(new BundleCollection(settings), settings);

            return settings.DefaultAssetSources[typeof(T)] as AssetSource;
        }
    }

    public class BundleInitializerConfiguration_ScriptBundle_Tests : BundleInitializerConfiguration_Tests<Scripts.ScriptBundle>
    {
        [Fact]
        public void FilePatternIsJsAndCoffee()
        {
            Initializer.FilePattern.ShouldEqual("*.js;*.coffee");
        }

        [Fact]
        public void ExcludeMatchesVSDocFiles()
        {
            Initializer.ExcludeFilePath.IsMatch("foo-vsdoc.js").ShouldBeTrue();
            Initializer.ExcludeFilePath.IsMatch("foo.js").ShouldBeFalse();
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            Initializer.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }

    public class BundleInitializerConfiguration_StylesheetBundle_Tests : BundleInitializerConfiguration_Tests<Stylesheets.StylesheetBundle>
    {
        [Fact]
        public void FilePatternIsCssAndLess()
        {
            Initializer.FilePattern.ShouldEqual("*.css;*.less");
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            Initializer.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }

    public class BundleInitializerConfiguration_HtmlTemplateBundle_Tests : BundleInitializerConfiguration_Tests<HtmlTemplates.HtmlTemplateBundle>
    {
        [Fact]
        public void FilePatternIsHtmAndHtml()
        {
            Initializer.FilePattern.ShouldEqual("*.htm;*.html");
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            Initializer.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }
}