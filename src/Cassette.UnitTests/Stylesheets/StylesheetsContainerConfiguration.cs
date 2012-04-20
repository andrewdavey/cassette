using System;
using System.IO;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetsContainerConfiguration_Tests
    {
        readonly TinyIoCContainer container;
        readonly StylesheetsContainerConfiguration configuration;
        readonly FileSearch fileSearch;

        public StylesheetsContainerConfiguration_Tests()
        {
            container = new TinyIoCContainer();
            container.Register<IStylesheetMinifier, MicrosoftStylesheetMinifier>();
            container.Register<IUrlGenerator, UrlGenerator>();
            container.Register(typeof(IUrlModifier), Mock.Of<IUrlModifier>());

            configuration = new StylesheetsContainerConfiguration(type => new Type[0]);
            configuration.Configure(container);

            fileSearch = (FileSearch)container.Resolve<IFileSearch>(HostBase.FileSearchComponentName(typeof(StylesheetBundle)));
        }

        [Fact]
        public void FileSearchPatternIsCss()
        {
            fileSearch.Pattern.ShouldContain("*.css");
        }

        [Fact]
        public void FileSearchSearchOptionIsAllDirectories()
        {
            fileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void BundleFactoryIsStylesheetBundleFactory()
        {
            container.Resolve<IBundleFactory<StylesheetBundle>>().ShouldBeType<StylesheetBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsStylesheetPipeline()
        {
            container.Resolve<IBundlePipeline<StylesheetBundle>>().ShouldBeType<StylesheetPipeline>();
        }
    }
}