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
        protected CassetteSettings settings = new CassetteSettings("")
        {
            SourceDirectory = Mock.Of<IDirectory>()
        };
        internal abstract IBundleContainerFactory CreateFactory(IDictionary<Type, IBundleFactory<Bundle>> factories);

        [Fact]
        public void WhenCreateWithBundles_ThenItReturnsContainerWithBundles()
        {
            var bundle1 = new TestableBundle("~/test1");
            var bundle2 = new TestableBundle("~/test2");
            var bundles = new[] { bundle1, bundle2 };

            var builder = CreateFactory(new Dictionary<Type, IBundleFactory<Bundle>>());
            var container = builder.Create(bundles);

            container.FindBundlesContainingPath("~/test1").First().ShouldBeSameAs(bundle1);
            container.FindBundlesContainingPath("~/test2").First().ShouldBeSameAs(bundle2);
        }

        [Fact]
        public void WhenCreateWithBundle_ThenBundleIsProcessed()
        {
            var bundle = new TestableBundle("~/test");

            var builder = CreateFactory(new Dictionary<Type, IBundleFactory<Bundle>>());
            builder.Create(new[] { bundle });

            bundle.WasProcessed.ShouldBeTrue();
        }

        [Fact]
        public void WhenCreateWithBundleHavingExternalReference_ThenAnExternalBundleIsAlsoAddedToContainer()
        {
            var externalBundle = new TestableBundle("http://external.com/api.js");
            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://external.com/api.js");

            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://external.com/api.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            factories[typeof(TestableBundle)] = factory.Object;

            var builder = CreateFactory(factories);
            var container = builder.Create(new[] { bundle });

            container.FindBundlesContainingPath("http://external.com/api.js").First().ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void WhenExternalModuleReferencedTwice_ThenContainerOnlyHasTheExternalModuleOnce()
        {
            var externalBundle = new TestableBundle("http://external.com/api.js");
            var bundle1 = new TestableBundle("~/test1");
            bundle1.AddReference("http://external.com/api.js");
            var bundle2 = new TestableBundle("~/test2");
            bundle2.AddReference("http://external.com/api.js");
            var bundles = new[] { bundle1, bundle2 };

            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://external.com/api.js", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            factories[typeof(TestableBundle)] = factory.Object;

            var builder = CreateFactory(factories);
            var container = builder.Create(bundles);

            container.Bundles.Count().ShouldEqual(3);
        }

        [Fact]
        public void GivenAssetWithUrlReference_WhenCreate_ThenExternalBundleInContainer()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            factories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory(factories);

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

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            factories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory(factories);

            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://test.com/");
            bundle.Assets.Add(asset.Object);
            var container = containerFactory.Create(new[] { bundle });

            container.FindBundlesContainingPath("http://test.com/").First().ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void GivenAssetWithAdHocUrlReference_WhenCreate_ThenExternalBundleIsCreatedAndProcessed()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.References)
                 .Returns(new[] { new AssetReference("http://test.com/", asset.Object, -1, AssetReferenceType.Url) });

            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            factories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory(factories);

            var bundle = new TestableBundle("~/test");
            bundle.Assets.Add(asset.Object);
            
            containerFactory.Create(new[] { bundle });

            externalBundle.WasProcessed.ShouldBeTrue();
        }


        [Fact]
        public void GivenBundleWithAdHocUrlReference_WhenCreate_ThenExternalBundleIsCreatedAndProcessed()
        {
            var externalBundle = new TestableBundle("http://test.com/");
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateBundle("http://test.com/", It.IsAny<IEnumerable<IFile>>(), It.IsAny<BundleDescriptor>()))
                   .Returns(externalBundle);
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            factories[typeof(TestableBundle)] = factory.Object;
            var containerFactory = CreateFactory(factories);

            var bundle = new TestableBundle("~/test");
            bundle.AddReference("http://test.com/");

            containerFactory.Create(new[] { bundle });

            externalBundle.WasProcessed.ShouldBeTrue();
        }
    }
}
