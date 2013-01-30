using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    class RegisterTemplateWithHandlebars : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;
        readonly string javaScriptVariableName;
        readonly IHtmlTemplateIdStrategy idStrategy;

        public RegisterTemplateWithHandlebars(HtmlTemplateBundle bundle, string javaScriptVariableName, IHtmlTemplateIdStrategy idStrategy)
        {
            this.bundle = bundle;
            this.javaScriptVariableName = javaScriptVariableName;
            this.idStrategy = idStrategy;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var compiledTemplate = openSourceStream().ReadToEnd();

                var id = idStrategy.HtmlTemplateId(bundle, asset);
                var template = javaScriptVariableName + "['" + id + "']";

                var output = template + "=new Handlebars.template();" + 
                             template + ".r=" + compiledTemplate + ";";
                return output.AsStream();
            };
        }
    }
}