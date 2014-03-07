using System;
using System.IO;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.TinyIoC;
using Moq;
using Should;
using Xunit;

namespace Cassette.CDN
{
    public class CdnScriptsContainerConfiguration_Tests
    {
        const string TestUrl = "http://test.com";
        
        readonly TinyIoCContainer container;
        readonly CdnScriptContainerConfiguration configuration;
        readonly FileSearch fileSearch;

        public CdnScriptsContainerConfiguration_Tests()
        {
            container = new TinyIoCContainer();
            container.Register<IJavaScriptMinifier, MicrosoftJavaScriptMinifier>();
            container.Register(typeof(IUrlModifier), Mock.Of<IUrlModifier>());
            container.Register<IUrlGenerator>((c, x) => new UrlGenerator(c.Resolve<IUrlModifier>(), new FakeFileSystem(), "cassette.axd/"));

            configuration = new CdnScriptContainerConfiguration(type => new Type[0]);
            configuration.Configure(container);

            fileSearch = (FileSearch)container.Resolve<IFileSearch>(HostBase.FileSearchComponentName(typeof(CdnScriptBundle)));
        }

        [Fact]
        public void FileSearchPatternIsJs()
        {
            fileSearch.Pattern.ShouldContain("*.js");
        }

        [Fact]
        public void FileSearchExcludesVsDocFiles()
        {
            fileSearch.Exclude.IsMatch("jquery-vsdoc.js").ShouldBeTrue();
        }

        [Fact]
        public void FileSearchSearchOptionIsAllDirectories()
        {
            fileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void BundleFactoryIsScriptBundleFactory()
        {
            container.Resolve<IBundleFactory<CdnScriptBundle>>().ShouldBeType<CdnScriptBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsScriptPipeline()
        {
            container.Resolve<IBundlePipeline<CdnScriptBundle>>().ShouldBeType<CdnScriptPipeline>();
        }

        [Fact]
        public void CreatedBundlesMustHaveTheirOwnPipelines()
        {
            var factory = container.Resolve<IBundleFactory<CdnScriptBundle>>();
            var a = factory.CreateBundle("~/1", Enumerable.Empty<IFile>(), new BundleDescriptor {ExternalUrl = TestUrl});
            var b = factory.CreateBundle("~/2", Enumerable.Empty<IFile>(), new BundleDescriptor { ExternalUrl = TestUrl });
            a.Pipeline.ShouldNotBeSameAs(b.Pipeline);
        }
    }

    public class CdnScriptBundleContainerModuleWithFileSearchModifierTests
    {
        readonly TinyIoCContainer container;
        readonly CdnScriptContainerConfiguration configuration;
        readonly FileSearch fileSearch;

        public CdnScriptBundleContainerModuleWithFileSearchModifierTests()
        {
            container = new TinyIoCContainer();
            container.Register<IJavaScriptMinifier, MicrosoftJavaScriptMinifier>();
            container.Register<IUrlGenerator, UrlGenerator>();
            container.Register(typeof(IUrlModifier), Mock.Of<IUrlModifier>());

            var modifier = new Mock<IFileSearchModifier<CdnScriptBundle>>();
            modifier
                .Setup(m => m.Modify(It.IsAny<FileSearch>()))
                .Callback<FileSearch>(fs => fs.Pattern += ";*.other");

            container.Register(typeof(IFileSearchModifier<CdnScriptBundle>), modifier.Object);

            configuration = new CdnScriptContainerConfiguration(type => new Type[0]);
            configuration.Configure(container);

            fileSearch = (FileSearch)container.Resolve<IFileSearch>(HostBase.FileSearchComponentName(typeof(CdnScriptBundle)));
        }

        [Fact]
        public void FileSearchModifierModifiesTheFileSearchPattern()
        {
            fileSearch.Pattern.ShouldContain("*.other");
        }
    }
}