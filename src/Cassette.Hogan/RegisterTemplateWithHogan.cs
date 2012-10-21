using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    class RegisterTemplateWithHogan : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;
        readonly string javaScriptVariableName;
        readonly IHtmlTemplateIdStrategy idStrategy;

        public RegisterTemplateWithHogan(HtmlTemplateBundle bundle, string javaScriptVariableName, IHtmlTemplateIdStrategy idStrategy)
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

                var id = idStrategy.GetHtmlTemplateId(asset, bundle);
                var template = javaScriptVariableName + "['" + id + "']";

                var output = template + "=new Hogan.Template();" + 
                             template + ".r=" + compiledTemplate + ";";
                return output.AsStream();
            };
        }
    }
}