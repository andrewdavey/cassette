using System;
using System.IO;
using System.Text;

namespace Cassette.HtmlTemplates
{
    public class JavaScriptHtmlTemplateBuilder : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;
        readonly IJsonSerializer serializer;
        readonly IHtmlTemplateIdStrategy idStrategy;

        public JavaScriptHtmlTemplateBuilder(HtmlTemplateBundle bundle, IJsonSerializer serializer, IHtmlTemplateIdStrategy idStrategy)
        {
            this.bundle = bundle;
            this.serializer = serializer;
            this.idStrategy = idStrategy;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return () =>
            {
                using (var reader = new StreamReader(openSourceStream()))
                {
                    var template = reader.ReadToEnd();
                    var idString = CreateJavaScriptString(TemplateId(asset));
                    var templateString = CreateJavaScriptString(template);
                    var output = string.Format("addTemplate({0},{1});", idString, templateString);
                    return new MemoryStream(Encoding.UTF8.GetBytes(output));
                }
            };
        }

        string TemplateId(IAsset asset)
        {
            return idStrategy.HtmlTemplateId(bundle, asset);
        }

        string CreateJavaScriptString(string template)
        {
            return serializer.Serialize(template);
        }
    }
}