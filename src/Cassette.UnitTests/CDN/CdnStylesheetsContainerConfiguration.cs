using System;
using System.IO;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Stylesheets;
using Cassette.TinyIoC;
using Moq;
using Should;
using Xunit;

namespace Cassette.CDN
{
    public class CdnStylesheetsContainerConfiguration_Tests
    {
        readonly TinyIoCContainer container;
        readonly CdnStylesheetsContainerConfiguration configuration;
        readonly FileSearch fileSearch;

        public CdnStylesheetsContainerConfiguration_Tests()
        {
            container = new TinyIoCContainer();
            container.Register<IStylesheetMinifier, MicrosoftStylesheetMinifier>();
            container.Register(typeof(IUrlModifier), Mock.Of<IUrlModifier>());
            container.Register<IUrlGenerator>((c, x) => new UrlGenerator(c.Resolve<IUrlModifier>(), new FakeFileSystem(), "cassette.axd/"));

            configuration = new CdnStylesheetsContainerConfiguration(type => new Type[0]);
            configuration.Configure(container);

            fileSearch = (FileSearch)container.Resolve<IFileSearch>(HostBase.FileSearchComponentName(typeof(CdnStylesheetBundle)));
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
            container.Resolve<IBundleFactory<CdnStylesheetBundle>>().ShouldBeType<CdnStylesheetBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsStylesheetPipeline()
        {
            container.Resolve<IBundlePipeline<StylesheetBundle>>().ShouldBeType<StylesheetPipeline>();
        }

        [Fact]
        public void CreatedBundlesMustHaveTheirOwnPipelines()
        {
            var factory = container.Resolve<IBundleFactory<CdnStylesheetBundle>>();
            var a = factory.CreateBundle("~/1", Enumerable.Empty<IFile>(), new BundleDescriptor());
            var b = factory.CreateBundle("~/2", Enumerable.Empty<IFile>(), new BundleDescriptor());
            a.Pipeline.ShouldNotBeSameAs(b.Pipeline);
        }
    }
}