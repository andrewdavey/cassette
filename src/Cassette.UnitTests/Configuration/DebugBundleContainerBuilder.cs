using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

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

            var builder = new DebugBundleContainerBuilder(application, new Dictionary<Type, IBundleFactory<Bundle>>());
            var container = builder.Build(bundles);

            container.FindBundleContainingPath("~/test1").ShouldBeSameAs(bundle1);
            container.FindBundleContainingPath("~/test2").ShouldBeSameAs(bundle2);
        }

        [Fact]
        public void WhenBuildWithBundle_ThenBundleIsProcessed()
        {
            var bundle = new Mock<Bundle>("~/test");
            var application = Mock.Of<ICassetteApplication>();

            var builder = new DebugBundleContainerBuilder(application, new Dictionary<Type, IBundleFactory<Bundle>>());
            builder.Build(new[] { bundle.Object });

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

            var builder = new DebugBundleContainerBuilder(application, factories);
            var container = builder.Build(new[] { bundle });

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

            var builder = new DebugBundleContainerBuilder(application, factories);
            var container = builder.Build(new[] { bundle1, bundle2 });

            container.Bundles.Count().ShouldEqual(3);
        }
    }

    class DebugBundleContainerBuilder
    {
        readonly ICassetteApplication application;
        readonly Dictionary<Type, IBundleFactory<Bundle>> factories;

        public DebugBundleContainerBuilder(ICassetteApplication application, Dictionary<Type, IBundleFactory<Bundle>> factories)
        {
            this.application = application;
            this.factories = factories;
        }

        public IBundleContainer Build(IEnumerable<Bundle> bundles)
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
}
