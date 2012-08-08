using System.Collections.Generic;
using System.Linq;
using Cassette.BundleProcessing;

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

        protected void ProcessAllBundles(IList<Bundle> bundles)
        {
            var hasher = new AssignHash();
            Trace.Source.TraceInformation("Processing bundles...");
            for (var i = 0; i < bundles.Count; i++)
            {
                Trace.Source.TraceInformation("Processing {0} {1}", bundles[i].GetType().Name, bundles[i].Path);
                if (settings.IsDebuggingEnabled)
                {
                    hasher.Process(bundles[i], settings);
                    var bundleUrl = bundles[i].Url;
                    if (CassetteSettings.bundles.ContainsKey(bundleUrl))
                    {
                        Bundle tempBundle;
                        CassetteSettings.bundles.TryGetValue(bundleUrl, out tempBundle);
                        bundles[i] = tempBundle;
                    }
                    else
                    {
                        bundles[i].Process(settings);
                        CassetteSettings.bundles.Add(bundleUrl, bundles[i]);
                    }
                }
                else
                {
                    bundles[i].Process(settings);
                }
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