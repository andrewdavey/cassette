using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    class RegisterTemplateWithJQueryTmpl : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;

        public RegisterTemplateWithJQueryTmpl(HtmlTemplateBundle bundle)
        {
            this.bundle = bundle;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var id = bundle.GetTemplateId(asset);
                var template = openSourceStream().ReadToEnd();
                return string.Format("jQuery.template('{0}', {1});{2}", id, template, Environment.NewLine).AsStream();
            };
        }
    }
}