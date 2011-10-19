using System.IO;
using Should;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundle_DefaultAssetSource_Tests
    {
        readonly BundleDirectoryInitializer initializer;

        public HtmlTemplateBundle_DefaultAssetSource_Tests()
        {
            var bundle = new HtmlTemplateBundle("~/test");
            initializer = bundle.BundleInitializers[0] as BundleDirectoryInitializer;
        }

        [Fact]
        public void PathIsBundlePath()
        {
            initializer.Path.ShouldEqual("~/test");
        }

        [Fact]
        public void FilePatternIsHtmAndHtml()
        {
            initializer.FilePattern.ShouldEqual("*.htm;*.html");
        }

        [Fact]
        public void SearchOptionIsAllDirectories()
        {
            initializer.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }
    }
}