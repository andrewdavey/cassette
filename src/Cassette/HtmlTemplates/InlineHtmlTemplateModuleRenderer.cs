using System;
using System.Linq;
using System.Web;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    public class InlineHtmlTemplateModuleRenderer : IModuleHtmlRenderer<HtmlTemplateModule>
    {
        public IHtmlString Render(HtmlTemplateModule module)
        {
            var elements = 
                from asset in module.Assets
                select string.Format(
                    "<script id=\"{0}\" type=\"{1}\">{2}</script>",
                    module.GetTemplateId(asset),
                    module.ContentType,
                    GetTemplateContent(asset)
                );

            return new HtmlString(
                string.Join(Environment.NewLine, elements)
            );
        }

        string GetTemplateContent(IAsset asset)
        {
            using (var stream = asset.OpenStream())
            {
                return stream.ReadToEnd();
            }
        }
    }
}