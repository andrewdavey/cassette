using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Cassette.IO;
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


        protected Bundle ProcessSingleBundle(IFileHelper fileHelper, IDirectory directory, List<Bundle> bundlesToSort,
            Dictionary<string, string> uncachedToCachedFiles, Bundle bundle, AssignHash hasher) 
        {
            Trace.Source.TraceInformation("Processing {0} {1}", bundle.GetType().Name, bundle.Path);
            hasher.Process(bundle, settings);
            var bundleKey = CassetteSettings.bundles.GetSafeString(Encoding.Default.GetString(bundle.Hash));
            if (CassetteSettings.bundles.ContainsKey(fileHelper, directory, uncachedToCachedFiles, bundleKey, bundle))
            {
                bundle = CassetteSettings.bundles.GetBundle(fileHelper, directory, uncachedToCachedFiles, bundleKey, bundle);
                bundlesToSort.Add(bundle);
            }
            else
            {
                var unprocessedAssetPaths = CassetteSettings.bundles.GetAssetPaths(bundle);
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
                var bundlesToSort = new List<Bundle>();
                var directory = new FileSystemDirectory(DiskBackedBundleCache.CacheDirectory);
                for (var i = 0; i < bundles.Count; i++)
                {
                    bundles[i] = ProcessSingleBundle(diskBacker, directory, bundlesToSort,
                        CassetteSettings.uncachedToCachedFiles, bundles[i], hasher);
                    if (typeof(StylesheetBundle).IsAssignableFrom(bundles[i].GetType()))
                    {
                        ((StylesheetBundle)bundles[i]).Renderer = new DebugStylesheetHtmlRenderer(settings.UrlGenerator);
                    }
                    else if (typeof(ScriptBundle).IsAssignableFrom(bundles[i].GetType()))
                    {
                        ((ScriptBundle)bundles[i]).Renderer = new DebugScriptBundleHtmlRenderer(settings.UrlGenerator);
                    }
                    else if (typeof(HtmlTemplateBundle).IsAssignableFrom(bundles[i].GetType()))
                    {  
                        ((HtmlTemplateBundle)bundles[i]).Renderer = new DebugHtmlTemplateBundleRenderer(settings.UrlGenerator);
                        bundles[i].ContentType = "text/javascript";
                    }
                }
                CassetteSettings.bundles.FixReferences(CassetteSettings.uncachedToCachedFiles, bundles);
                bundlesToSort.ForEach(b => b.SortAssetsByDependency());
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