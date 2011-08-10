using System;
using System.Linq;
using System.Web;

namespace Cassette
{
    public class StylesheetModule : Module
    {
        public StylesheetModule(string directory, IFileSystem fileSystem)
            : base(directory, fileSystem)
        {
        }

        static readonly string linkHtml = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\"/>";
        static readonly string linkHtmlWithMedia = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" media=\"{1}\"/>";

        public string Media { get; set; }

        public override string ContentType
        {
            get { return "text/css"; }
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            if (application.IsOutputOptimized)
            {
                var url = application.CreateModuleUrl(this);
                if (string.IsNullOrEmpty(Media))
                {
                    return new HtmlString(string.Format(linkHtml, url));
                }
                else
                {
                    return new HtmlString(string.Format(linkHtmlWithMedia, url, Media));
                }
            }
            else
            {
                var hasMedia = string.IsNullOrEmpty(Media);
                var scripts = string.Join(Environment.NewLine,
                    from asset in Assets
                    let url = application.CreateAssetUrl(this, asset)
                    select hasMedia ? string.Format(linkHtml, url) 
                                    : string.Format(linkHtmlWithMedia, url, Media)
                );
                return new HtmlString(scripts);
            }
        }
    }
}