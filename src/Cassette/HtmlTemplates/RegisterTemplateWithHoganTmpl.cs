using System;
using System.IO;
using System.Text;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    class RegisterTemplateWithHoganTmpl : IAssetTransformer
    {
        readonly HtmlTemplateBundle bundle;

        public RegisterTemplateWithHoganTmpl(HtmlTemplateBundle bundle)
        {
            this.bundle = bundle;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var id = bundle.GetTemplateId(asset);
                var compiled = openSourceStream().ReadToEnd();
                var ns = "JST";
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