using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
            DefaultFileSources = CreateDefaultFileSources();
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

        IDictionary<Type, IFileSource> CreateDefaultFileSources()
        {
            return new Dictionary<Type, IFileSource>
            {
                { typeof(ScriptBundle), CreateScriptFileSource() },
                { typeof(StylesheetBundle), CreateStylesheetFileSource() },
                { typeof(HtmlTemplateBundle), CreateHtmlTemplateFileSource() }
            };
        }

        FileSearch CreateScriptFileSource()
        {
            return new FileSearch
            {
                Pattern = "*.js;*.coffee",
                Exclude = new Regex("-vsdoc\\.js"),
                SearchOption = SearchOption.AllDirectories
            };
        }

        FileSearch CreateStylesheetFileSource()
        {
            return new FileSearch
            {
                Pattern = "*.css;*.less",
                SearchOption = SearchOption.AllDirectories
            };
        }

        FileSearch CreateHtmlTemplateFileSource()
        {
            return new FileSearch
            {
                Pattern = "*.htm;*.html",
                SearchOption = SearchOption.AllDirectories
            };
        }
    }
}