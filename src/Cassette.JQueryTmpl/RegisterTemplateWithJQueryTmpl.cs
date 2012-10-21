using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    public class RegisterTemplateWithJQueryTmpl : IAssetTransformer
    {
        public delegate RegisterTemplateWithJQueryTmpl Factory(HtmlTemplateBundle bundle);

        readonly HtmlTemplateBundle bundle;
        readonly IHtmlTemplateIdStrategy idStrategy;

        public RegisterTemplateWithJQueryTmpl(HtmlTemplateBundle bundle, IHtmlTemplateIdStrategy idStrategy)
        {
            this.bundle = bundle;
            this.idStrategy = idStrategy;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var id = idStrategy.GetHtmlTemplateId(asset, bundle);
                var template = openSourceStream().ReadToEnd();
                return string.Format("jQuery.template('{0}', {1});{2}", id, template, Environment.NewLine).AsStream();
            };
        }
    }
}