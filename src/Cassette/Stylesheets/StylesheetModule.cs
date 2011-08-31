using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassette.ModuleProcessing;
using System.Xml.Linq;

namespace Cassette.Stylesheets
{
    public class StylesheetModule : Module
    {
        public StylesheetModule(string directory)
            : base(directory)
        {
            ContentType = "text/css";
            Processor = new StylesheetPipeline();
        }

        protected static readonly string LinkHtml = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\"/>";
        protected static readonly string LinkHtmlWithMedia = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" media=\"{1}\"/>";

        public string Media { get; set; }
        public IModuleProcessor<StylesheetModule> Processor { get; set; }

        public override void Process(ICassetteApplication application)
        {
            Processor.Process(this, application);
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            if (application.IsOutputOptimized)
            {
                var url = application.UrlGenerator.CreateModuleUrl(this);
                if (string.IsNullOrEmpty(Media))
                {
                    return new HtmlString(string.Format(LinkHtml, url));
                }
                else
                {
                    return new HtmlString(string.Format(LinkHtmlWithMedia, url, Media));
                }
            }
            else
            {
                var hasMedia = string.IsNullOrEmpty(Media);
                var scripts = string.Join(Environment.NewLine,
                    from asset in Assets
                    let url = IsCompiledAsset(asset)
                        ? application.UrlGenerator.CreateAssetCompileUrl(this, asset)
                        : application.UrlGenerator.CreateAssetUrl(asset)
                    select hasMedia ? string.Format(LinkHtml, url) 
                                    : string.Format(LinkHtmlWithMedia, url, Media)
                );
                return new HtmlString(scripts);
            }
        }

        public override IEnumerable<XElement> CreateCacheManifest()
        {
            var element = base.CreateCacheManifest().Single();
            if (string.IsNullOrEmpty(Media) == false)
            {
                element.Add(new XAttribute("Media", Media));
            }
            yield return element;
        }

        public override void InitializeFromManifest(XElement moduleElement)
        {
            var media = moduleElement.Attribute("Media");
            if (media != null)
            {
                Media = media.Value;
            }
        }
    }
}