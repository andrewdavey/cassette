using System.IO;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleContainerModule_Tests
    {
        readonly TinyIoCContainer container;
        readonly StylesheetBundleContainerModule module;
        readonly FileSearch fileSearch;

        public StylesheetBundleContainerModule_Tests()
        {
            container = new TinyIoCContainer();
            container.Register<IStylesheetMinifier, MicrosoftStylesheetMinifier>();
            container.Register<IUrlGenerator, UrlGenerator>();
            container.Register(typeof(IUrlModifier), Mock.Of<IUrlModifier>());

            module = new StylesheetBundleContainerModule();
            module.Load(container);

            fileSearch = (FileSearch)container.Resolve<IFileSearch>(HostBase.FileSearchComponentName(typeof(StylesheetBundle)));
        }

        [Fact]
        public void FileSearchPatternIsCss()
        {
            fileSearch.Pattern.ShouldEqual("*.css");
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