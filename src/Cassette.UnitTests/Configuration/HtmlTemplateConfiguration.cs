using System.IO;
using Cassette.HtmlTemplates;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class HtmlTemplateConfiguration_Tests
    {
        readonly BundleDefaults<HtmlTemplateBundle> defaults;

        public HtmlTemplateConfiguration_Tests()
        {
            var configuration = new HtmlTemplateBundleConfiguration();
            var settings = new CassetteSettings("");
            configuration.Configure(null, settings);
            defaults = settings.GetDefaults<HtmlTemplateBundle>();
        }

        [Fact]
        public void FileSearchPatternIsHtmAndHtml()
        {
            defaults.FileSearch.Pattern.ShouldEqual("*.htm;*.html");
        }

        [Fact]
        public void FileSearchSearchOptionIsAllDirectories()
        {
            defaults.FileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void BundleFactoryIsHtmlTemplateBundleFactory()
        {
            defaults.BundleFactory.ShouldBeType<HtmlTemplateBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsHtmlTemplatePipeline()
        {
            defaults.BundlePipeline.ShouldBeType<HtmlTemplatePipeline>();
        }
    }
}