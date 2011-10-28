using System;
using System.Collections.Generic;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.Configuration
{
    public class CassetteSettings
    {
        public CassetteSettings()
        {
            DefaultFileSources = new Dictionary<Type, IFileSource>();
            BundleFactories = CreateBundleFactories();
            CacheVersion = "";
        }

        public bool IsDebuggingEnabled { get; set; }
        public bool IsHtmlRewritingEnabled { get; set; }
        public IDirectory SourceDirectory { get; set; }
        public IDirectory CacheDirectory { get; set; }
        public IUrlModifier UrlModifier { get; set; }
        public string CacheVersion { get; set; }

        internal IDictionary<Type, IBundleFactory<Bundle>> BundleFactories { get; private set; }
        internal IDictionary<Type, IFileSource> DefaultFileSources { get; private set; }

        static Dictionary<Type, IBundleFactory<Bundle>> CreateBundleFactories()
        {
            return new Dictionary<Type, IBundleFactory<Bundle>>
            {
                { typeof(ScriptBundle), new ScriptBundleFactory() },
                { typeof(StylesheetBundle), new StylesheetBundleFactory() },
                { typeof(HtmlTemplateBundle), new HtmlTemplateBundleFactory() }
            };
        }
    }
}