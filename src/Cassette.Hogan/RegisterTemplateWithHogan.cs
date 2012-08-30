using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    class RegisterTemplateWithHogan : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;
        readonly string javaScriptVariableName;

        public RegisterTemplateWithHogan(HtmlTemplateBundle bundle, string javaScriptVariableName)
        {
            this.bundle = bundle;
            this.javaScriptVariableName = javaScriptVariableName;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var compiledTemplate = openSourceStream().ReadToEnd();

                var id = bundle.GetTemplateId(asset);
                var template = javaScriptVariableName + "['" + id + "']";

                var output = template + "=new Hogan.Template();" + 
                             template + ".r=" + compiledTemplate + ";";
                return output.AsStream();
            };
        }
    }
}