using System.IO;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundleContainerModule_Tests
    {
        readonly TinyIoCContainer container;
        readonly ScriptBundleContainerModule module;
        readonly FileSearch fileSearch;

        public ScriptBundleContainerModule_Tests()
        {
            container = new TinyIoCContainer();
            container.Register<IJavaScriptMinifier, MicrosoftJavaScriptMinifier>();
            container.Register<IUrlGenerator, UrlGenerator>();
            container.Register(typeof(IUrlModifier), Mock.Of<IUrlModifier>());

            module = new ScriptBundleContainerModule();
            module.Load(container);

            fileSearch = (FileSearch)container.Resolve<IFileSearch>(HostBase.FileSearchComponentName(typeof(ScriptBundle)));
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
            container.Resolve<IBundleFactory<ScriptBundle>>().ShouldBeType<ScriptBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsScriptPipeline()
        {
            container.Resolve<IBundlePipeline<ScriptBundle>>().ShouldBeType<ScriptPipeline>();
        }
    }

    public class ScriptBundleContainerModuleWithFileSearchModifierTests
    {
        readonly TinyIoCContainer container;
        readonly ScriptBundleContainerModule module;
        readonly FileSearch fileSearch;

        public ScriptBundleContainerModuleWithFileSearchModifierTests()
        {
            container = new TinyIoCContainer();
            container.Register<IJavaScriptMinifier, MicrosoftJavaScriptMinifier>();
            container.Register<IUrlGenerator, UrlGenerator>();
            container.Register(typeof(IUrlModifier), Mock.Of<IUrlModifier>());

            var modifier = new Mock<IFileSearchModifier<ScriptBundle>>();
            modifier
                .Setup(m => m.Modify(It.IsAny<FileSearch>()))
                .Callback<FileSearch>(fs => fs.Pattern += ";*.other");

            container.Register(typeof(IFileSearchModifier<ScriptBundle>), modifier.Object);

            module = new ScriptBundleContainerModule();
            module.Load(container);

            fileSearch = (FileSearch)container.Resolve<IFileSearch>(HostBase.FileSearchComponentName(typeof(ScriptBundle)));
        }

        [Fact]
        public void FileSearchModifierModifiesTheFileSearchPattern()
        {
            fileSearch.Pattern.ShouldContain("*.other");
        }
    }
}