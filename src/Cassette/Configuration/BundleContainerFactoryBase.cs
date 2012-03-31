using System.Collections.Generic;
using System.Linq;

namespace Cassette.Configuration
{
    abstract class BundleContainerFactoryBase : IBundleContainerFactory
    {
        readonly CassetteSettings settings;
        readonly IBundleFactoryProvider bundleFactoryProvider;

        protected BundleContainerFactoryBase(CassetteSettings settings, IBundleFactoryProvider bundleFactoryProvider)
        {
            this.settings = settings;
            this.bundleFactoryProvider = bundleFactoryProvider;
        }

        public abstract IBundleContainer CreateBundleContainer();

        protected void ProcessAllBundles(IEnumerable<Bundle> bundles)
        {
            Trace.Source.TraceInformation("Processing bundles...");
            foreach (var bundle in bundles)
            {
                Trace.Source.TraceInformation("Processing {0} {1}", bundle.GetType().Name, bundle.Path);
                bundle.Process(settings);
            }
            Trace.Source.TraceInformation("Bundle processing completed.");
        }

        protected IEnumerable<Bundle> CreateExternalBundlesUrlReferences(IEnumerable<Bundle> bundles)
        {
            var existingUrls = bundles.OfType<IExternalBundle>().Select(b => b.ExternalUrl);
            var generator = new ExternalBundleGenerator(existingUrls, bundleFactoryProvider, settings);
            foreach (var bundle in bundles)
            {
                bundle.Accept(generator);
            }
            return generator.ExternalBundles;
        }
    }
}