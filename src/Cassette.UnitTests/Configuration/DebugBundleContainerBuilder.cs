using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;
using Cassette.Persistence;

namespace Cassette.Configuration
{
    public class DebugBundleContainerBuilder_Tests
    {
        [Fact]
        public void WhenBuildWithBundles_ThenItReturnsContainerWithBundles()
        {
            var bundle1 = new Bundle("~/test1");
            var bundle2 = new Bundle("~/test2");
            var bundles = new [] { bundle1, bundle2 };
            var application = Mock.Of<ICassetteApplication>();

            var builder = new BundleContainerFactory(application, new Dictionary<Type, IBundleFactory<Bundle>>());
            var container = builder.Create(bundles);

            container.FindBundleContainingPath("~/test1").ShouldBeSameAs(bundle1);
            container.FindBundleContainingPath("~/test2").ShouldBeSameAs(bundle2);
        }

        [Fact]
        public void WhenBuildWithBundle_ThenBundleIsProcessed()
        {
            var bundle = new Mock<Bundle>("~/test");
            var application = Mock.Of<ICassetteApplication>();

            var builder = new BundleContainerFactory(application, new Dictionary<Type, IBundleFactory<Bundle>>());
            builder.Create(new[] { bundle.Object });

            bundle.Verify(b => b.Process(application));
        }

        [Fact]
        public void WhenBuildWithBundleHavingExternalReference_ThenAnExternalBundleIsAlsoAddedToContainer()
        {
            var externalBundle = new Bundle("http://external.com/api.js");
            var bundle = new Bundle("~/test");
            bundle.AddReferences(new[] { "http://external.com/api.js" });

            var application = Mock.Of<ICassetteApplication>();
            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateExternalBundle("http://external.com/api.js"))
                   .Returns(externalBundle);
            factories[typeof(Bundle)] = factory.Object;

            var builder = new BundleContainerFactory(application, factories);
            var container = builder.Create(new[] { bundle });

            container.FindBundleContainingPath("http://external.com/api.js").ShouldBeSameAs(externalBundle);
        }

        [Fact]
        public void WhenExternalModuleReferencedTwice_ThenContainerOnlyHasTheExternalModuleOnce()
        {
            var externalBundle = new Bundle("http://external.com/api.js");
            var bundle1 = new Bundle("~/test1");
            bundle1.AddReferences(new[] { "http://external.com/api.js" });
            var bundle2 = new Bundle("~/test2");
            bundle2.AddReferences(new[] { "http://external.com/api.js" });
            var bundles = new[] { bundle1, bundle2 };
            var application = Mock.Of<ICassetteApplication>();

            var factories = new Dictionary<Type, IBundleFactory<Bundle>>();
            var factory = new Mock<IBundleFactory<Bundle>>();
            factory.Setup(f => f.CreateExternalBundle("http://external.com/api.js"))
                   .Returns(externalBundle);
            factories[typeof(Bundle)] = factory.Object;

            var builder = new BundleContainerFactory(application, factories);
            var container = builder.Create(new[] { bundle1, bundle2 });

            container.Bundles.Count().ShouldEqual(3);
        }
    }

    class BundleContainerFactory
    {
        readonly ICassetteApplication application;
        readonly Dictionary<Type, IBundleFactory<Bundle>> factories;

        public BundleContainerFactory(ICassetteApplication application, Dictionary<Type, IBundleFactory<Bundle>> factories)
        {
            this.application = application;
            this.factories = factories;
        }

        public IBundleContainer Create(IEnumerable<Bundle> bundles)
        {
            var externalPaths = new HashSet<string>();
            var externalBundles = new List<Bundle>();

            foreach (var bundle in bundles)
            {
                bundle.Process(application);
                
                foreach (var reference in bundle.References)
                {
                    if (reference.IsUrl() == false) continue;
                    if (externalPaths.Contains(reference)) continue;

                    var bundleFactory = GetBundleFactory(bundle.GetType());
                    var externalBundle = bundleFactory.CreateExternalBundle(reference);
                    externalBundles.Add(externalBundle);
                    externalPaths.Add(externalBundle.Path);
                }
            }
            return new BundleContainer(bundles.Concat(externalBundles));
        }

        IBundleFactory<Bundle> GetBundleFactory(Type bundleType)
        {
            IBundleFactory<Bundle> factory;
            if (factories.TryGetValue(bundleType, out factory))
            {
                return factory;
            }
            throw new ArgumentException(string.Format("Cannot find bundle factory for {0}", bundleType.FullName));
        }
    }

    public class CachedBundleContainerFactory_Create_Tests
    {
        [Fact]
        public void GivenCacheIsUpToDate_WhenCreateWithBundle_ThenContainerCreatedWithTheGivenBundle()
        {
            var bundles = new[] { new Bundle("~/test") };
            var cache = new Mock<IBundleCache>();
            cache.Setup(c => c.InitializeBundlesFromCacheIfUpToDate(bundles))
                 .Returns(true);
            var application = Mock.Of<ICassetteApplication>();

            var factory = new CachedBundleContainerFactory(cache.Object, application);
            var container = factory.Create(bundles);

            container.FindBundleContainingPath("~/test").ShouldBeSameAs(bundles[0]);
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenCreateWithBundle_ThenBundleIsNotProcessed()
        {
            var bundle = new Mock<Bundle>("~/test");
            var bundles = new[] { bundle.Object };
            var cache = new Mock<IBundleCache>();
            cache.Setup(c => c.InitializeBundlesFromCacheIfUpToDate(bundles))
                 .Returns(true);
            var application = Mock.Of<ICassetteApplication>();

            var factory = new CachedBundleContainerFactory(cache.Object, application);
            factory.Create(bundles);

            bundle.Verify(b => b.Process(application), Times.Never());
        }

        [Fact]
        public void GivenCacheOutOfDate_WhenCreateWithBundle_ThenBundleIsProcessed()
        {
            var bundle = new Mock<Bundle>("~/test");
            var bundles = new[] { bundle.Object };
            var cache = new Mock<IBundleCache>();
            cache.Setup(c => c.InitializeBundlesFromCacheIfUpToDate(bundles))
                 .Returns(false);
            var application = Mock.Of<ICassetteApplication>();

            var factory = new CachedBundleContainerFactory(cache.Object, application);
            factory.Create(bundles);

            bundle.Verify(b => b.Process(application));
        }

        [Fact]
        public void GivenCacheOutOfDate_WhenCreateWithBundle_ThenContainerSavedToCache()
        {
            var bundles = new Bundle[0];
            var cache = new Mock<IBundleCache>();
            var application = Mock.Of<ICassetteApplication>();

            var factory = new CachedBundleContainerFactory(cache.Object, application);
            factory.Create(bundles);

            cache.Verify(c => c.SaveBundleContainer(It.IsAny<IBundleContainer>()));
        }
    }

    class CachedBundleContainerFactory
    {
        readonly IBundleCache cache;
        readonly ICassetteApplication application;

        public CachedBundleContainerFactory(IBundleCache cache, ICassetteApplication application)
        {
            this.cache = cache;
            this.application = application;
        }

        public IBundleContainer Create(IEnumerable<Bundle> bundles)
        {
            // The bundles may get altered, so force the evaluation of the enumerator first.
            var bundlesArray = bundles.ToArray();

            if (cache.InitializeBundlesFromCacheIfUpToDate(bundlesArray))
            {
                return new BundleContainer(bundlesArray);
            }
            else
            {
                ProcessAll(bundlesArray);
                var container = new BundleContainer(bundlesArray);
                cache.SaveBundleContainer(container);
                return container;
            }
        }

        void ProcessAll(IEnumerable<Bundle> bundlesArray)
        {
            foreach (var bundle in bundlesArray)
            {
                bundle.Process(application);
            }
        }
    }
}
