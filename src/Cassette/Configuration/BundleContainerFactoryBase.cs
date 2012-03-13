using System.Collections.Generic;
using System.Linq;

namespace Cassette.Configuration
{
    abstract class BundleContainerFactoryBase : IBundleContainerFactory
    {
        readonly CassetteSettings settings;

        protected BundleContainerFactoryBase(CassetteSettings settings)
        {
            this.settings = settings;
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

        protected IEnumerable<Bundle> CreateExternalBundlesUrlReferences(BundleCollection bundles)
        {
            var existingUrls = bundles.OfType<IExternalBundle>().Select(b => b.ExternalUrl);
            var generator = new ExternalBundleGenerator(existingUrls, settings);
            foreach (var bundle in bundles)
            {
                bundle.Accept(generator);
            }
            return generator.ExternalBundles;
        }
    }
}