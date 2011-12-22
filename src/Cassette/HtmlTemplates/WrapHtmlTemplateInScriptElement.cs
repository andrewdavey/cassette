using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    class WrapHtmlTemplateInScriptElement : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;

        public WrapHtmlTemplateInScriptElement(HtmlTemplateBundle bundle)
        {
            this.bundle = bundle;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var template = openSourceStream().ReadToEnd();
                var scriptElement = String.Format(
                    "<script id=\"{0}\" type=\"{1}\">{2}</script>",
                    bundle.GetTemplateId(asset),
                    bundle.ContentType,
                    template
                    );
                return scriptElement.AsStream();
            };
        }
    }
}
