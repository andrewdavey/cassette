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


        protected Bundle ProcessSingleBundle(IFileHelper fileHelper, Dictionary<string, string> uncachedToCachedFiles, 
            Bundle bundle, AssignHash hasher) 
        {
            Trace.Source.TraceInformation("Processing {0} {1}", bundle.GetType().Name, bundle.Path);
            hasher.Process(bundle, settings);
            var bundleKey = fileHelper.GetCachebleString(bundle.Url);
            if (CassetteSettings.bundles.ContainsKey(fileHelper, uncachedToCachedFiles, bundleKey, bundle))
            {
                bundle = CassetteSettings.bundles.GetBundle(fileHelper, uncachedToCachedFiles, bundleKey, bundle);
            }
            else
            {
                var unprocessedAssetPaths = CassetteSettings.bundles.getAssetPaths(bundle);
                bundle.Process(settings);
                CassetteSettings.bundles.AddBundle(fileHelper, uncachedToCachedFiles, bundleKey, bundle, unprocessedAssetPaths);
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
                var diskBacker = new FileHelper();
                for (var i = 0; i < bundles.Count; i++)
                {
                    bundles[i] = ProcessSingleBundle(diskBacker, CassetteSettings.uncachedToCachedFiles, bundles[i], hasher);
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