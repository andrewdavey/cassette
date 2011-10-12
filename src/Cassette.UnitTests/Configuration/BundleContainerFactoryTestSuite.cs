using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public abstract class BundleContainerFactoryTestSuite<T>
        where T : BundleContainerFactoryBase
    {
        protected abstract T CreateFactory(ICassetteApplication application, IDictionary<Type, IBundleFactory<Bundle>> factories);

        [Fact]
        public void WhenBuildWithBundles_ThenItReturnsContainerWithBundles()
        {
            var bundle1 = new Bundle("~/test1");
            var bundle2 = new Bundle("~/test2");
            var bundles = new[] { bundle1, bundle2 };
            var application = Mock.Of<ICassetteApplication>();

            var builder = CreateFactory(application, new Dictionary<Type, IBundleFactory<Bundle>>());
            var container = builder.Create(bundles);

            container.FindBundleContainingPath("~/test1").ShouldBeSameAs(bundle1);
            container.FindBundleContainingPath("~/test2").ShouldBeSameAs(bundle2);
        }

        [Fact]
        public void WhenBuildWithBundle_ThenBundleIsProcessed()
        {
            var bundle = new Mock<Bundle>("~/test");
            var application = Mock.Of<ICassetteApplication>();

            var builder = CreateFactory(application, new Dictionary<Type, IBundleFactory<Bundle>>());
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

            var builder = CreateFactory(application, factories);
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

            var builder = CreateFactory(application, factories);
            var container = builder.Create(bundles);

            container.Bundles.Count().ShouldEqual(3);
        }
    }
}