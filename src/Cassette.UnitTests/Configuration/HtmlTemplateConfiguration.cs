using System;
using System.IO;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class HtmlTemplateConfiguration_Tests
    {
        readonly FileSearch fileSearch;
        readonly HtmlTemplateBundleBootstrapperContributor contributor;

        public HtmlTemplateConfiguration_Tests()
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

    static class BootstrapperContributorExtensions
    {
        public static T GetInstance<T>(this BootstrapperContributor contributor)
        {
            return (T)contributor.InstanceRegistrations.First(i => i.RegistrationType == typeof(T)).Instance;
        }

        public static void ShouldHaveTypeRegistration<TR,TI>(this BootstrapperContributor contributor)
        {
            contributor
                .TypeRegistrations
                .First(t => t.RegistrationType == typeof(TR))
                .ImplementationType
                .ShouldEqual(typeof(TI));
        }
    }
}