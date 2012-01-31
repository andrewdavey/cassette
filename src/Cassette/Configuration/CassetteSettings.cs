using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Manifests;

namespace Cassette.Configuration
{
    /// <summary>
    /// Settings that control Cassette's behavior.
    /// </summary>
    public class CassetteSettings
    {
        readonly Lazy<ICassetteManifestCache> bundleCache;
        ICassetteManifestCache customCassetteManifestCache;

        public CassetteSettings(string cacheVersion)
        {
            DefaultFileSearches = CreateDefaultFileSearches();
            BundleFactories = CreateBundleFactories();
            bundleCache = new Lazy<ICassetteManifestCache>(() => new CassetteManifestCache(CacheDirectory.GetFile("cassette.xml")));
        }

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
        public IDictionary<Type, IFileSearch> DefaultFileSearches { get; private set; }

        public IUrlGenerator UrlGenerator { get; set; }

        internal IDictionary<Type, IBundleFactory<Bundle>> BundleFactories { get; private set; }

        internal bool AllowRemoteDiagnostics { get; set; }

        static Dictionary<Type, IBundleFactory<Bundle>> CreateBundleFactories()
        {
            return new Dictionary<Type, IBundleFactory<Bundle>>
            {
                { typeof(ScriptBundle), new ScriptBundleFactory() },
                { typeof(StylesheetBundle), new StylesheetBundleFactory() },
                { typeof(HtmlTemplateBundle), new HtmlTemplateBundleFactory() }
            };
        }

        IDictionary<Type, IFileSearch> CreateDefaultFileSearches()
        {
            return new Dictionary<Type, IFileSearch>
            {
                { typeof(ScriptBundle), CreateScriptFileSearch() },
                { typeof(StylesheetBundle), CreateStylesheetFileSearch() },
                { typeof(HtmlTemplateBundle), CreateHtmlTemplateFileSearch() }
            };
        }

        FileSearch CreateScriptFileSearch()
        {
            return new FileSearch
            {
                Pattern = "*.js;*.coffee",
                Exclude = new Regex("-vsdoc\\.js"),
                SearchOption = SearchOption.AllDirectories
            };
        }

        FileSearch CreateStylesheetFileSearch()
        {
            return new FileSearch
            {
                Pattern = "*.css;*.less",
                SearchOption = SearchOption.AllDirectories
            };
        }

        FileSearch CreateHtmlTemplateFileSearch()
        {
            return new FileSearch
            {
                Pattern = "*.htm;*.html",
                SearchOption = SearchOption.AllDirectories
            };
        }

        internal ICassetteManifestCache CassetteManifestCache
        {
            get { return customCassetteManifestCache ?? bundleCache.Value; }
            set { customCassetteManifestCache = value; }
        }

        internal IBundleContainerFactory GetBundleContainerFactory()
        {
            if (IsDebuggingEnabled)
            {
                return new BundleContainerFactory(this);
            }
            else
            {
                return new CachedBundleContainerFactory(CassetteManifestCache, this);
            }
        }
    }
}