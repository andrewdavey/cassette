using System;
using System.Collections.Generic;
using System.Web;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette.Scripts
{
    public class ExternalScriptModule : ScriptModule, IModuleSource<ScriptModule>
    {
        public ExternalScriptModule(string url)
            : this(url, url)
        {
        }

        public ExternalScriptModule(string name, string url)
            : base(PathUtilities.AppRelative(name))
        {
            if (url == null) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", "url");

            this.url = url;
        }

        public ExternalScriptModule(string name, string url, string javaScriptCondition, string fallbackUrl)
            : base(PathUtilities.AppRelative(name))
        {
            if (url == null) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", "url");
            if (javaScriptCondition == null) throw new ArgumentNullException("javaScriptCondition");
            if (string.IsNullOrWhiteSpace(javaScriptCondition)) throw new ArgumentException("JavaScript condition is required.", "javaScriptCondition");
            if (fallbackUrl == null) throw new ArgumentNullException("fallbackUrl");
            if (string.IsNullOrWhiteSpace(fallbackUrl)) throw new ArgumentException("Fallback URL is required.", "fallbackUrl");
            
            this.url = url;
            this.fallbackUrl = fallbackUrl;
            this.javaScriptCondition = javaScriptCondition;
        }

        string url;
        string fallbackUrl;
        string javaScriptCondition;

        static readonly string fallbackHtml = "<script type=\"text/javascript\">{0} && document.write(unescape('%3Cscript src=\"{1}\"%3E%3C/script%3E'))</script>";

        public override IEnumerable<XElement> CreateCacheManifest()
        {
            // External modules do not require caching.
            yield break;
        }

        public override void Process(ICassetteApplication application)
        {
            // No processing required.
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            if (string.IsNullOrEmpty(fallbackUrl))
            {
                return new HtmlString(
                    string.Format(ScriptHtml, url)
                );
            }
            else
            {
                return new HtmlString(
                    string.Format(ScriptHtml, url) + 
                    Environment.NewLine + 
                    string.Format(
                        fallbackHtml,
                        javaScriptCondition,
                        fallbackUrl
                    )
                );
            }
        }

        IEnumerable<ScriptModule> IModuleSource<ScriptModule>.GetModules(IModuleFactory<ScriptModule> moduleFactory, ICassetteApplication application)
        {
            yield return this;
        }
    }
}