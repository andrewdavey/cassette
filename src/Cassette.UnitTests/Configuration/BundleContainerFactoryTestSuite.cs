using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.IO;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public abstract class BundleContainerFactoryTestSuite
    {
        protected readonly CassetteSettings Settings = new CassetteSettings("")
        {
            SourceDirectory = Mock.Of<IDirectory>(),
            UrlGenerator = Mock.Of<IUrlGenerator>()
        };
        internal abstract IBundleContainerFactory CreateFactory(IEnumerable<Bundle> bundles);

        [Fact]
        public void WhenCreateWithBundles_ThenItReturnsContainerWithBundles()
        {
            var bundle1 = new TestableBundle("~/test1");
            var bundle2 = new TestableBundle("~/test2");
            var bundles = new[] { bundle1, bundle2 };

            var builder = CreateFactory(bundles);
            var container = builder.CreateBundleContainer();

            container.FindBundlesContainingPath("~/test1").ShouldNotBeEmpty();
            container.FindBundlesContainingPath("~/test2").ShouldNotBeEmpty();
        }

        [Fact]
        public void WhenCreateWithBundle_ThenBundleIsProcessed()
        {
            var bundle = new TestableBundle("~/test");

            var builder = CreateFactory(new[] { bundle });
            builder.CreateBundleContainer();

            bundle.WasProcessed.ShouldBeTrue();
        }

        [Fact]
        public void WhenCreateWithBundleHavingExternalReference_ThenAnExternalBundleIsAlsoAddedToContainer()
        {
            var externalBundle = new TestableBundle("http://external.com/api.js");
            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://external.com/api.js");

            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://external.com/api.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            SetBundleFactory(factory);

            var builder = CreateFactory(new[] { bundle });
            var container = builder.CreateBundleContainer();

            container.FindBundlesContainingPath("http://external.com/api.js").First().ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void WhenExternalModuleReferencedTwice_ThenContainerOnlyHasTheExternalModuleOnce()
        {
            var urlModifier = Mock.Of<IUrlModifier>();
            var externalBundle = new ExternalScriptBundle("http://external.com/api.js") { Processor = Mock.Of<IBundleProcessor<ScriptBundle>>() };
            var bundle1 = new ScriptBundle("~/test1") { Processor = Mock.Of<IBundleProcessor<ScriptBundle>>() };
            bundle1.Renderer = new ConstantHtmlRenderer<ScriptBundle>("", urlModifier);
            bundle1.AddReference("http://external.com/api.js");
            var bundle2 = new ScriptBundle("~/test2") { Processor = Mock.Of<IBundleProcessor<ScriptBundle>>() };
            bundle2.Renderer = new ConstantHtmlRenderer<ScriptBundle>("", urlModifier);
            bundle2.AddReference("http://external.com/api.js");
            var bundles = new[] { bundle1, bundle2 };

            var factory = new Mock<IBundleFactory<ScriptBundle>>();
            factory.Setup(f => f.CreateBundle("http://external.com/api.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            SetBundleFactory(factory);

            var builder = CreateFactory(bundles);
            var container = builder.CreateBundleContainer();

            container.Bundles.Count().ShouldEqual(3);
        }

        void SetBundleFactory<T>(Mock<IBundleFactory<T>> factory)
            where T : Bundle
        {
            Settings.ModifyDefaults<T>(defaults => defaults.BundleFactory = factory.Object);
        }

        [Fact]
        public void GivenAssetWithUrlReference_WhenCreate_ThenExternalBundleInContainer()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset");
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            SetBundleFactory(factory);

            var bundle = new TestableBundle("~/test");
            bundle.Assets.Add(asset.Object);

            var containerFactory = CreateFactory(new[] { bundle });
            var container = containerFactory.CreateBundleContainer();

            container.FindBundlesContainingPath("http://test.com/").First().ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void GivenAssetWithUrlReferenceAndSameBundleLevelUrlReference_WhenCreate_ThenExternalBundleInContainer()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            SetBundleFactory(factory);

            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://test.com/");
            bundle.Assets.Add(asset.Object);

            var containerFactory = CreateFactory(new[] { bundle });
            var container = containerFactory.CreateBundleContainer();

            container.FindBundlesContainingPath("http://test.com/").First().ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void GivenBundleWithAdHocUrlReference_WhenCreate_ThenExternalBundleIsCreatedAndProcessed()
        {
            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            SetBundleFactory(factory);

            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://test.com/");

            var containerFactory = CreateFactory(new[] { bundle });
            containerFactory.CreateBundleContainer();

            externalBundle.WasProcessed.ShouldBeTrue();
        }

        [Fact]
        public void GivenAssetWithAdHocUrlReference_WhenCreate_ThenExternalBundleIsCreatedAndProcessed()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset");
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            SetBundleFactory(factory);

            var bundle = new TestableBundle("~/test");
            bundle.Assets.Add(asset.Object);

            var containerFactory = CreateFactory(new[] { bundle });
            containerFactory.CreateBundleContainer();

            externalBundle.WasProcessed.ShouldBeTrue();
        }
    }
}