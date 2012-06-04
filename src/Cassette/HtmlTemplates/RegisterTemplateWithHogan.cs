using System;
using System.IO;
using System.Text;
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
            this.javaScriptVariableName = javaScriptVariableName ?? "JST";
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var id = bundle.GetTemplateId(asset);
                var compiled = openSourceStream().ReadToEnd();
                var template = javaScriptVariableName + "['" + id + "']";
                var sb = new StringBuilder();

                sb.AppendLine("var " + javaScriptVariableName + " = " + javaScriptVariableName + "|| {};");
                sb.AppendLine(template + "= new HoganTemplate();");
                sb.AppendLine(template + ".r = " + compiled);
                
                return sb.ToString().AsStream();
            };
        }
    }
}