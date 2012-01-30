using System;
using System.IO;
using System.Text;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    class RegisterTemplateWithHogan : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;
        readonly string ns;

        public RegisterTemplateWithHogan(HtmlTemplateBundle bundle, string ns = null)
        {
            this.bundle = bundle;
            this.ns = ns ?? "JST";
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var id = bundle.GetTemplateId(asset);
                var compiled = openSourceStream().ReadToEnd();
                var template = ns + "['" + id + "']";
                var sb = new StringBuilder();
                
                sb.AppendLine("var " + ns + " = " + ns + "|| {};");
                sb.AppendLine(template + "= new HoganTemplate();");
                sb.AppendLine(template + ".render = " + compiled);
                
                return sb.ToString().AsStream();
            };
        }
    }
}