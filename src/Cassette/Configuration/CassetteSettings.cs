using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.BundleProcessing;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Manifests;
#if NET35
using Cassette.Utilities;
#endif

namespace Cassette.Configuration
{
    /// <summary>
    /// Settings that control Cassette's behavior.
    /// </summary>
    public class CassetteSettings
    {
        readonly IDictionary<Type, Func<IFileSearch>> defaultFileSearchFactories;
        readonly Dictionary<Type, string> defaultFileSearchPatterns = new Dictionary<Type, string>();
        readonly Dictionary<Type, IFileSearch> applicationProvidedDefaultFileSearches = new Dictionary<Type, IFileSearch>();
        readonly Lazy<ICassetteManifestCache> cassetteManifestCache;
        readonly Dictionary<Type, object> defaultBundleProcessors;
        readonly List<Func<string, bool>> allowPathPredicates = new List<Func<string, bool>>();
 
        public CassetteSettings(string cacheVersion)
        {
            Version = cacheVersion;

            defaultBundleProcessors = new Dictionary<Type, object>
            {
                { typeof(ScriptBundle), new ScriptPipeline() },
                { typeof(StylesheetBundle), new StylesheetPipeline() },
                { typeof(HtmlTemplateBundle), new HtmlTemplatePipeline() },
            };

            AddDefaultFileSearchPattern<ScriptBundle>("*.js");
            AddDefaultFileSearchPattern<StylesheetBundle>("*.css");
            AddDefaultFileSearchPattern<HtmlTemplateBundle>("*.htm;*.html");

            defaultFileSearchFactories = new Dictionary<Type, Func<IFileSearch>>
            {
                { typeof(ScriptBundle), CreateScriptFileSearch },
                { typeof(StylesheetBundle), CreateStylesheetFileSearch },
                { typeof(HtmlTemplateBundle), CreateHtmlTemplateFileSearch }
            };

            BundleFactories = CreateBundleFactories();
            cassetteManifestCache = new Lazy<ICassetteManifestCache>(
                () => new CassetteManifestCache(CacheDirectory.GetFile("cassette.xml"))
            );
        }

        /// <summary>
        /// When true, Cassette has already loaded bundles from a compile-time generated manifest file.
        /// The application's Cassette configuration MUST NOT add bundles to the bundle collection.
        /// </summary>
        public bool IsUsingPrecompiledManifest { get; internal set; }

        /// <summary>
        /// When this property is true, Cassette will output debug-friendly assets. When false, combined, minified bundles are used instead.
        /// </summary>
        public bool IsDebuggingEnabled { get; set; }

        /// <summary>
        /// When true (the default), Cassette will buffer page output and rewrite to allow bundle references to be inserted into &lt;head&gt;
        /// after it has already been rendered. Disable this when &lt;system.webServer&gt;/&lt;urlCompression dynamicCompressionBeforeCache="true"&gt;
        /// is in Web.config.
        /// </summary>
        public bool IsHtmlRewritingEnabled { get; set; }

        /// <summary>
        /// The directory containing the original bundle asset files.
        /// </summary>
        public IDirectory SourceDirectory { get; set; }

        /// <summary>
        /// The directory used to cache combined, minified bundles.
        /// </summary>
        public IDirectory CacheDirectory { get; set; }

        /// <summary>
        /// The <see cref="IUrlModifier"/> used to convert application relative URLs into absolute URLs.
        /// </summary>
        public IUrlModifier UrlModifier { get; set; }

        /// <summary>
        /// The default <see cref="IFileSearch"/> object for each type of <see cref="Bundle"/>, used to find asset files to include.
        /// </summary>
        public IDictionary<Type, IFileSearch> DefaultFileSearches
        {
            get { return applicationProvidedDefaultFileSearches; }
        }

        public IUrlGenerator UrlGenerator { get; set; }

        internal IDictionary<Type, IBundleFactory<Bundle>> BundleFactories { get; private set; }

        internal bool AllowRemoteDiagnostics { get; set; }

        internal string Version { get; private set; }

        internal IFileSearch GetDefaultFileSearch<T>()
            where T : Bundle
        {
            IFileSearch fileSearch;
            if (applicationProvidedDefaultFileSearches.TryGetValue(typeof(T), out fileSearch))
            {
                return fileSearch;
            }
            return defaultFileSearchFactories[typeof(T)]();
        }

        public void SetDefaultBundleProcessor<T>(IBundleProcessor<T> processor)
            where T : Bundle
        {
            defaultBundleProcessors[typeof(T)] = processor;
        }

        public IBundleProcessor<T> GetDefaultBundleProcessor<T>()
            where T : Bundle
        {
            return (IBundleProcessor<T>)defaultBundleProcessors[typeof(T)];
        }

        Dictionary<Type, IBundleFactory<Bundle>> CreateBundleFactories()
        {
            return new Dictionary<Type, IBundleFactory<Bundle>>
            {
                { typeof(ScriptBundle), new ScriptBundleFactory(this) },
                { typeof(StylesheetBundle), new StylesheetBundleFactory(this) },
                { typeof(HtmlTemplateBundle), new HtmlTemplateBundleFactory(this) }
            };
        }

        FileSearch CreateScriptFileSearch()
        {
            return new FileSearch
            {
                Pattern = defaultFileSearchPatterns[typeof(ScriptBundle)],
                Exclude = new Regex("-vsdoc\\.js"),
                SearchOption = SearchOption.AllDirectories
            };
        }

        FileSearch CreateStylesheetFileSearch()
        {
            return new FileSearch
            {
                Pattern = defaultFileSearchPatterns[typeof(StylesheetBundle)],
                SearchOption = SearchOption.AllDirectories
            };
        }

        FileSearch CreateHtmlTemplateFileSearch()
        {
            return new FileSearch
            {
                Pattern = defaultFileSearchPatterns[typeof(HtmlTemplateBundle)],
                SearchOption = SearchOption.AllDirectories
            };
        }

        internal ICassetteManifestCache CassetteManifestCache
        {
            get { return cassetteManifestCache.Value; }
        }

        internal IBundleContainerFactory GetBundleContainerFactory(IEnumerable<ICassetteConfiguration> cassetteConfigurations)
        {
            var bundles = ExecuteCassetteConfiguration(cassetteConfigurations);
            if (IsDebuggingEnabled)
            {
                return new BundleContainerFactory(bundles, this);
            }
            else
            {
                return new CachedBundleContainerFactory(bundles, CassetteManifestCache, this);
            }
        }

        BundleCollection ExecuteCassetteConfiguration(IEnumerable<ICassetteConfiguration> cassetteConfigurations)
        {
            var bundles = new BundleCollection(this);
            foreach (var configuration in cassetteConfigurations)
            {
                Trace.Source.TraceInformation("Executing configuration {0}", configuration.GetType().AssemblyQualifiedName);
                configuration.Configure(bundles, this);
            }
            return bundles;
        }

        internal bool CanRequestRawFile(string filePath)
        {
            return allowPathPredicates.Any(predicate => predicate(filePath));
        }

        public void AllowRawFileRequest(Func<string, bool> pathIsAllowed)
        {
            allowPathPredicates.Add(pathIsAllowed);
        }

        internal void AddDefaultFileSearchPattern<T>(string fileSearchPattern)
            where T : Bundle
        {
            string existingPattern;
            if (defaultFileSearchPatterns.TryGetValue(typeof(T), out existingPattern))
            {
                defaultFileSearchPatterns[typeof(T)] = existingPattern + ";" + fileSearchPattern;
            }
            else
            {
                defaultFileSearchPatterns[typeof(T)] = fileSearchPattern;
            }
        }
    }
}