using System;
using System.IO;
using System.Text;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateToJavaScriptTransformer : IAssetTransformer
    {
        public delegate HtmlTemplateToJavaScriptTransformer Factory(string javaScriptTemplate, HtmlTemplateBundle bundle);

        readonly string javaScriptTemplate;
        readonly HtmlTemplateBundle bundle;
        readonly IJsonSerializer serializer;
        readonly IHtmlTemplateIdStrategy idStrategy;

        public HtmlTemplateToJavaScriptTransformer(string javaScriptTemplate, HtmlTemplateBundle bundle, IJsonSerializer serializer, IHtmlTemplateIdStrategy idStrategy)
        {
            this.javaScriptTemplate = javaScriptTemplate;
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
                    var output = string.Format(javaScriptTemplate, idString, templateString);
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