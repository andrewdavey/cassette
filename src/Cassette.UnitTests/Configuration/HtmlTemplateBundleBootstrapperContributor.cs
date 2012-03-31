using System.IO;
using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class HtmlTemplateBundleBootstrapperContributor_Tests
    {
        readonly FileSearch fileSearch;
        readonly HtmlTemplateBundleBootstrapperContributor contributor;

        public HtmlTemplateBundleBootstrapperContributor_Tests()
        {
            contributor = new HtmlTemplateBundleBootstrapperContributor();
            fileSearch = (FileSearch)contributor.GetInstance<IFileSearch>();
        }

        [Fact]
        public void FileSearchPatternIsHtmAndHtml()
        {
            fileSearch.Pattern.ShouldEqual("*.htm;*.html");
        }

        [Fact]
        public void FileSearchSearchOptionIsAllDirectories()
        {
            fileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void BundleFactoryIsHtmlTemplateBundleFactory()
        {
            contributor.ShouldHaveTypeRegistration<IBundleFactory<HtmlTemplateBundle>, HtmlTemplateBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsHtmlTemplatePipeline()
        {
            contributor.ShouldHaveTypeRegistration<IBundlePipeline<HtmlTemplateBundle>, HtmlTemplatePipeline>();
        }
    }
}