using System;
using System.Collections.Generic;
using System.Web;
using System.Xml.Linq;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetModule : StylesheetModule, IModuleSource<StylesheetModule>
    {
        public ExternalStylesheetModule(string url)
            : base(url)
        {
            this.url = url;
        }

        readonly string url;

        public override IEnumerable<XElement> CreateCacheManifest()
        {
            var element = new XElement("ExternalModule",
                new XAttribute("Url", url),
                new XAttribute("ContentType", ContentType)
            );
            if (string.IsNullOrEmpty(Media) == false)
            {
                element.Add(new XAttribute("Media", Media));
            }
            yield return element;
        }

        public override void Process(ICassetteApplication application)
        {
            // No processing required.
        }

        public override IHtmlString Render(ICassetteApplication application)
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
