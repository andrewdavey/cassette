using System.Collections.Generic;
using System.Web;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetModule : StylesheetModule, IModuleSource<StylesheetModule>, IExternalModule
    {
        public ExternalStylesheetModule(string url)
            : base(url)
        {
            this.url = url;
        }

        public ExternalStylesheetModule(string name, string url) 
            : base(PathUtilities.AppRelative(name))
        {
            this.url = url;
        }

        readonly string url;

        public override IEnumerable<XElement> CreateCacheManifest()
        {
            // External modules do not require caching.
            yield break;
        }

        public override void Process(ICassetteApplication application)
        {
            // No processing required.
        }

        public override IHtmlString Render()
        {
            if (string.IsNullOrEmpty(Media))
            {
                return new HtmlString(string.Format(LinkHtml, url));
            }
            else
            {
                return new HtmlString(string.Format(LinkHtmlWithMedia, url, Media));
            }
        }

        IEnumerable<StylesheetModule> IModuleSource<StylesheetModule>.GetModules(IModuleFactory<StylesheetModule> moduleFactory, ICassetteApplication application)
        {
            yield return this;
        }
    }
}
