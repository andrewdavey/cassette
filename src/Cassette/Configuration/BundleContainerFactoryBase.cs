using System.Collections.Generic;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

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


        protected Bundle ProcessSingleBundle(Bundle bundle, AssignHash hasher) 
        {
            Trace.Source.TraceInformation("Processing {0} {1}", bundle.GetType().Name, bundle.Path);
            hasher.Process(bundle, settings);
            var bundleKey = CassetteSettings.bundles.GetCachebleString(bundle.Url);
            if (CassetteSettings.bundles.ContainsKey(bundleKey, bundle))
            {
                bundle = CassetteSettings.bundles.GetBundle(bundleKey, bundle);
            }
            else
            {
                var unprocessedAssetPaths = CassetteSettings.bundles.getAssetPaths(bundle);
                bundle.Process(settings);
                CassetteSettings.bundles.AddBundle(bundleKey, bundle, unprocessedAssetPaths);
            }
            return bundle;
        }

        protected void ProcessAllBundles(IList<Bundle> bundles)
        {
            Trace.Source.TraceInformation("Processing bundles...");
            if (!settings.IsDebuggingEnabled)
            {
                foreach (var bundle in bundles)
                {
                    bundle.Process(settings);
                }
            }
            else
            {
                var hasher = new AssignHash();
                for (var i = 0; i < bundles.Count; i++)
                {
                    bundles[i] = ProcessSingleBundle(bundles[i], hasher);
                    if (settings.IsDebuggingEnabled && typeof(StylesheetBundle).IsAssignableFrom(bundles[i].GetType()))
                    {
                        ((StylesheetBundle)bundles[i]).Renderer = new DebugStylesheetHtmlRenderer(settings.UrlGenerator);
                    }
                    else if (settings.IsDebuggingEnabled && typeof(ScriptBundle).IsAssignableFrom(bundles[i].GetType()))
                    {
                        ((ScriptBundle)bundles[i]).Renderer = new DebugScriptBundleHtmlRenderer(settings.UrlGenerator);
                    }
                }
                CassetteSettings.bundles.FixReferences(bundles);
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