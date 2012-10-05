using System;
using System.IO;
using System.Text;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    public class WrapJavaScriptHtmlTemplatesTransformer : IAssetTransformer
    {
        readonly IHtmlTemplateScriptStrategy scriptStrategy;

        public WrapJavaScriptHtmlTemplatesTransformer(IHtmlTemplateScriptStrategy scriptStrategy)
        {
            this.scriptStrategy = scriptStrategy;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return () =>
            {
                var templateDefinitions = openSourceStream().ReadToEnd();
                var output = scriptStrategy.WrapTemplates(templateDefinitions);
                return new MemoryStream(Encoding.UTF8.GetBytes(output));
            };
        }
    }
}