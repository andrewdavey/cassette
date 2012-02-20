using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            SpriteDirectory = Mock.Of<IDirectory>()
        };
        internal abstract IBundleContainerFactory CreateFactory();

        [Fact]
        public void WhenCreateWithBundles_ThenItReturnsContainerWithBundles()
        {
            var bundle1 = new TestableBundle("~/test1");
            var bundle2 = new TestableBundle("~/test2");
            var bundles = new[] { bundle1, bundle2 };

            var builder = CreateFactory();
            var container = builder.Create(bundles);

            container.FindBundlesContainingPath("~/test1").ShouldNotBeEmpty();
            container.FindBundlesContainingPath("~/test2").ShouldNotBeEmpty();
        }

        [Fact]
        public void WhenCreateWithBundle_ThenBundleIsProcessed()
        {
            var bundle = new TestableBundle("~/test");

            var builder = CreateFactory();
            builder.Create(new[] { bundle });

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
            Settings.BundleFactories[typeof(TestableBundle)] = factory.Object;

            var builder = CreateFactory();
            var container = builder.Create(new[] { bundle });

            container.FindBundlesContainingPath("http://external.com/api.js").First().ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void WhenExternalModuleReferencedTwice_ThenContainerOnlyHasTheExternalModuleOnce()
        {
            var externalBundle = new ExternalScriptBundle("http://external.com/api.js");
            var bundle1 = new ScriptBundle("~/test1");
            bundle1.AddReference("http://external.com/api.js");
            var bundle2 = new ScriptBundle("~/test2");
            bundle2.AddReference("http://external.com/api.js");
            var bundles = new[] { bundle1, bundle2 };

            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://external.com/api.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            Settings.BundleFactories[typeof(ScriptBundle)] = factory.Object;

            var builder = CreateFactory();
            var container = builder.Create(bundles);

            container.Bundles.Count().ShouldEqual(3);
        }

        [Fact]
        public void GivenAssetWithUrlReference_WhenCreate_ThenExternalBundleInContainer()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset");
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            Settings.BundleFactories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory();

            var bundle = new TestableBundle("~/test");
            bundle.Assets.Add(asset.Object);
            var container = containerFactory.Create(new[] { bundle });

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
            Settings.BundleFactories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory();

            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://test.com/");
            bundle.Assets.Add(asset.Object);
            var container = containerFactory.Create(new[] { bundle });

            container.FindBundlesContainingPath("http://test.com/").First().ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void GivenBundleWithAdHocUrlReference_WhenCreate_ThenExternalBundleIsCreatedAndProcessed()
        {
            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            Settings.BundleFactories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory();

            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://test.com/");

            containerFactory.Create(new[] { bundle });

            externalBundle.WasProcessed.ShouldBeTrue();
        }

        [Fact]
        public void GivenAssetWithAdHocUrlReference_WhenCreate_ThenExternalBundleIsCreatedAndProcessed()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset");
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            Settings.BundleFactories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory();

            var bundle = new TestableBundle("~/test");
            bundle.Assets.Add(asset.Object);
            
            containerFactory.Create(new[] { bundle });

            externalBundle.WasProcessed.ShouldBeTrue();
        }
    }
}
