using System;
using System.IO;
using System.Text;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateToJavaScriptTransformer : IAssetTransformer
    {
        public delegate HtmlTemplateToJavaScriptTransformer Factory(HtmlTemplateBundle bundle);

        readonly HtmlTemplateBundle bundle;
        readonly IHtmlTemplateScriptStrategy scriptStrategy;
        readonly IHtmlTemplateIdStrategy idStrategy;

        public HtmlTemplateToJavaScriptTransformer(HtmlTemplateBundle bundle, IHtmlTemplateScriptStrategy scriptStrategy, IHtmlTemplateIdStrategy idStrategy)
        {
            this.bundle = bundle;
            this.scriptStrategy = scriptStrategy;
            this.idStrategy = idStrategy;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return () =>
            {
                using (var reader = new StreamReader(openSourceStream()))
                {
                    var template = reader.ReadToEnd();
                    var idString = TemplateId(asset);
                    var output = scriptStrategy.DefineTemplate(idString, template);
                    return new MemoryStream(Encoding.UTF8.GetBytes(output));
                }
            };
        }

        string TemplateId(IAsset asset)
        {
            return idStrategy.GetHtmlTemplateId(asset, bundle);
        }
    }
}