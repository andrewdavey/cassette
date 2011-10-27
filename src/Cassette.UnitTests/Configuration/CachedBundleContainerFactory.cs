using System;
using System.Collections.Generic;
using Cassette.Persistence;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class CachedBundleContainerFactory_Tests : BundleContainerFactoryTestSuite
    {
        readonly Mock<IBundleCache> cache = new Mock<IBundleCache>();
        
        [Fact]
        public void GivenCacheIsUpToDate_WhenCreateWithBundle_ThenContainerCreatedWithTheGivenBundle()
        {
            var bundles = new[] { new TestableBundle("~/test") };
            CacheIsUpToDate(bundles);

            var factory = CreateFactory();
            var container = factory.Create(bundles, StubApplication());

            container.FindBundleContainingPath("~/test").ShouldBeSameAs(bundles[0]);
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenCreateWithBundle_ThenBundleIsNotProcessed()
        {
            var bundle = new TestableBundle("~/test");
            var bundles = new[] { bundle };
            var application = StubApplication();
            CacheIsUpToDate(bundles);

            var factory = CreateFactory();
            factory.Create(bundles, application);

            bundle.WasProcessed.ShouldBeFalse();
        }

        [Fact]
        public void GivenCacheOutOfDate_WhenCreateWithBundle_ThenBundleIsProcessed()
        {
            var bundle = new TestableBundle("~/test");
            var bundles = new[] { bundle };
            var application = StubApplication();
            CacheIsOutOfDate(bundles);

            var factory = CreateFactory();
            factory.Create(bundles, application);

            bundle.WasProcessed.ShouldBeTrue();
        }

        [Fact]
        public void GivenCacheOutOfDate_WhenCreateWithBundle_ThenContainerSavedToCache()
        {
            var bundles = new TestableBundle[0];
            var application = StubApplication();
            CacheIsOutOfDate(bundles);

            var factory = CreateFactory();
            factory.Create(bundles, application);

            cache.Verify(c => c.SaveBundleContainer(It.IsAny<IBundleContainer>()));
        }

        [Fact]
        public void GivenCacheOutOfDate_WhenCreateWithBundle_ThenBundlesInitializedFromNewCache()
        {
            var bundles = new TestableBundle[0];
            var application = StubApplication();
            CacheIsOutOfDate(bundles);
            
            var factory = CreateFactory();
            factory.Create(bundles, application);

            // InitializeBundlesFromCacheIfUpToDate should be called a second time.
            // This is to make the bundles get their content from the cached, optimized, data.
            // Otherwise the first time the app runs the bundles are running from their original sources.
            cache.Verify(c => c.InitializeBundlesFromCacheIfUpToDate(bundles), Times.Exactly(2));
        }

        internal override IBundleContainerFactory CreateFactory(IDictionary<Type, IBundleFactory<Bundle>> factories)
        {
            return new CachedBundleContainerFactory(cache.Object, factories);
        }

        IBundleContainerFactory CreateFactory()
        {
            return CreateFactory(new Dictionary<Type, IBundleFactory<Bundle>>());
        }

        void CacheIsUpToDate(IEnumerable<Bundle> bundles)
        {
            cache.Setup(c => c.InitializeBundlesFromCacheIfUpToDate(bundles))
                .Returns(true);
        }

        void CacheIsOutOfDate(IEnumerable<Bundle> bundles)
        {
            cache.Setup(c => c.InitializeBundlesFromCacheIfUpToDate(bundles))
                 .Returns(false);
        }
    }
}
