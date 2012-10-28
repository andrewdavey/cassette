using System;

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

        public string Transform(string template, IAsset asset)
        {
            var id = idStrategy.HtmlTemplateId(bundle, asset);
            return string.Format(
                "jQuery.template('{0}', {1});{2}",
                id,
                template,
                Environment.NewLine
            );
        }
    }
}