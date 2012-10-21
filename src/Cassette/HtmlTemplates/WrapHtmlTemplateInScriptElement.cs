using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    class WrapHtmlTemplateInScriptElement : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;
        readonly IHtmlTemplateIdStrategy idStrategy;

        public WrapHtmlTemplateInScriptElement(HtmlTemplateBundle bundle, IHtmlTemplateIdStrategy idStrategy)
        {
            this.bundle = bundle;
            this.idStrategy = idStrategy;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var template = openSourceStream().ReadToEnd();
                var scriptElement = String.Format(
                    "<script id=\"{0}\" type=\"{1}\"{2}>{3}</script>",
                    idStrategy.GetHtmlTemplateId(asset, bundle),
                    bundle.ContentType,
                    bundle.HtmlAttributes.CombinedAttributes,
                    template
                );
                return scriptElement.AsStream();
            };
        }
    }
}