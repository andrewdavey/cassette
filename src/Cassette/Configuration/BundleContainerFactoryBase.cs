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


        protected T ProcessSingleBundle<T>(T bundle, AssignHash hasher) 
            where T : Bundle
        {
            Trace.Source.TraceInformation("Processing {0} {1}", bundle.GetType().Name, bundle.Path);
            bool loadedFromCache = false;
            if (settings.IsDebuggingEnabled || true)
            {
                hasher.Process(bundle, settings);
                var bundleKey = CassetteSettings.bundles.GetCachebleString(bundle.Url);
                if (CassetteSettings.bundles.ContainsKey(bundleKey, bundle))
                {
                    bundle = CassetteSettings.bundles.GetBundle(bundleKey, bundle);
                    loadedFromCache = true;
                } 
                else
                {
                    var unprocessedAssetPaths = CassetteSettings.bundles.getAssetPaths(bundle);
                    bundle.Process(settings);
                    CassetteSettings.bundles.AddBundle(bundleKey, bundle, unprocessedAssetPaths);
                }
            }
            else
           {
                bundle.Process(settings);
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
                    var type = bundles[i].GetType();
                    if (type == typeof(HtmlTemplateBundle))
                    {
                        bundles[i] = ProcessSingleBundle((HtmlTemplateBundle)bundles[i], hasher);
                    }
                    else if (type == typeof(ScriptBundle))
                    {
                        bundles[i] = ProcessSingleBundle((ScriptBundle)bundles[i], hasher);
                    }
                    else if (type == typeof(StylesheetBundle))
                    {
                        bundles[i] = ProcessSingleBundle((StylesheetBundle)bundles[i], hasher);
                    }
                    else if (type == typeof(ExternalScriptBundle))
                    {
                        bundles[i] = ProcessSingleBundle((ExternalScriptBundle)bundles[i], hasher);
                    }
                    else if (type == typeof(ExternalStylesheetBundle))
                    {
                        bundles[i] = ProcessSingleBundle((ExternalStylesheetBundle)bundles[i], hasher);
                    }
                    else if (type == typeof(InlineScriptBundle))
                    {
                        bundles[i] = ProcessSingleBundle((InlineScriptBundle)bundles[i], hasher);
                    }
                    else if (type == typeof(PageDataScriptBundle))
                    {
                        bundles[i] = ProcessSingleBundle((PageDataScriptBundle)bundles[i], hasher);
                    }

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