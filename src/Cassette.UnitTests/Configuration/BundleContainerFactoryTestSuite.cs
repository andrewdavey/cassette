using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public abstract class BundleContainerFactoryTestSuite
    {
        internal abstract IBundleContainerFactory CreateFactory(IDictionary<Type, IBundleFactory<Bundle>> factories);

        [Fact]
        public void WhenCreateWithBundles_ThenItReturnsContainerWithBundles()
        {
            var bundle1 = new TestableBundle("~/test1");
            var bundle2 = new TestableBundle("~/test2");
            var bundles = new[] { bundle1, bundle2 };
            var application = Mock.Of<ICassetteApplication>();

            var builder = CreateFactory(new Dictionary<Type, IBundleFactory<Bundle>>());
            var container = builder.Create(bundles, StubApplication());

            container.FindBundleContainingPath("~/test1").ShouldBeSameAs(bundle1);
            container.FindBundleContainingPath("~/test2").ShouldBeSameAs(bundle2);
        }

        [Fact]
        public void WhenCreateWithBundle_ThenBundleIsProcessed()
        {
            var bundle = new Mock<TestableBundle>("~/test");
            var application = StubApplication();

            var builder = CreateFactory(new Dictionary<Type, IBundleFactory<Bundle>>());
            builder.Create(new[] { bundle.Object }, application);

            bundle.Verify(b => b.Process(application));
        }

        [Fact]
        public void WhenCreateWithBundleHavingExternalReference_ThenAnExternalBundleIsAlsoAddedToContainer()
        {
            var externalBundle = new TestableBundle("http://external.com/api.js");
            var bundle = new TestableBundle("~/test");
            bundle.AddReferences(new[] { "http://external.com/api.js" });

            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://external.com/api.js", null))
                   .Returns(externalBundle);
            factories[typeof(TestableBundle)] = factory.Object;

            var builder = CreateFactory(factories);
            var container = builder.Create(new[] { bundle }, StubApplication());

            container.FindBundleContainingPath("http://external.com/api.js").ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void WhenExternalModuleReferencedTwice_ThenContainerOnlyHasTheExternalModuleOnce()
        {
            var externalBundle = new TestableBundle("http://external.com/api.js");
            var bundle1 = new TestableBundle("~/test1");
            bundle1.AddReferences(new[] { "http://external.com/api.js" });
            var bundle2 = new TestableBundle("~/test2");
            bundle2.AddReferences(new[] { "http://external.com/api.js" });
            var bundles = new[] { bundle1, bundle2 };

            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://external.com/api.js", null))
                   .Returns(externalBundle);
            factories[typeof(TestableBundle)] = factory.Object;

            var builder = CreateFactory(factories);
            var container = builder.Create(bundles, StubApplication());

            container.Bundles.Count().ShouldEqual(3);
        }

        [Fact]
        public void WhenCreate_ThenBundleAssetSourcesAreUsedToInitializeBundle()
        {
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            var containerFactory = CreateFactory(factories);
            var initializer = new Mock<IBundleInitializer>();

            var bundle = new TestableBundle("~/test");
            bundle.BundleInitializers.Add(initializer.Object);

            var application = StubApplication();
            containerFactory.Create(new[] { bundle }, application);

            initializer.Verify(s => s.InitializeBundle(bundle, application));
        }

        [Fact]
        public void GivenAssetWithUrlReference_WhenCreate_ThenExternalBundleInContainer()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            factories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory(factories);

            var bundle = new TestableBundle("~/test");
            bundle.Assets.Add(asset.Object);
            var application = StubApplication();
            var container = containerFactory.Create(new[] { bundle }, application);

            container.FindBundleContainingPath("http://test.com/").ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void GivenAssetWithUrlReferenceAndSameBundleLevelUrlReference_WhenCreate_ThenExternalBundleInContainer()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            factories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory(factories);

            var bundle = new TestableBundle("~/test");
            bundle.AddReferences(new[] { "http://test.com/" });
            bundle.Assets.Add(asset.Object);
            var application = StubApplication();
            var container = containerFactory.Create(new[] { bundle }, application);

            container.FindBundleContainingPath("http://test.com/").ShouldBeSameAs(externalBundle);
        }

        protected ICassetteApplication StubApplication()
        {
            var appMock = new Mock<ICassetteApplication>();
            appMock.SetupGet(a => a.SourceDirectory).Returns(Mock.Of<IDirectory>());
            return appMock.Object;
        }
    }
}